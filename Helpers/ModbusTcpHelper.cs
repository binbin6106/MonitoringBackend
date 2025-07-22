using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Sockets;
using InfluxDB.Client.Api.Domain;
using Modbus.Device;
using MonitoringBackend.Models; // NModbus4 的命名空间

namespace MonitoringBackend.Helpers
{
    public class ModbusTcpHelper
    {
        private Gateway gateway {  get; set; } = new Gateway();
        private List<SensorData> returnsensorData { get; set; } = new List<SensorData>();
        private int Port { get; set; }
        private byte SlaveId { get; set; }

        private ushort[] ModbusAddress { get; set; } = {0x0002,  0x0012, 0x0022, 0x0032};
        private ushort[] ModbusCoilAddress { get; set; } = { 0x0000, 0x0002, 0x0004, 0x0006 };
        public List<AlarmRecord> alarmRecord { get; set; } = new List<AlarmRecord>();
        private float[] low_threshold = new float[5];
        private float[] up_threshold = new float[5];
        public string[] point_sign = { "温度", "X方向振动", "Y方向振动", "Z方向振动", "振动矢量和" };
        public ModbusTcpHelper(Gateway gatewayData)
        {
            gateway = gatewayData;
            Port = 502;              // Modbus默认端口
            SlaveId = 1;            // 从站ID
        }

        public (List<SensorData>, bool) getData()
        {
            returnsensorData.Clear();
            bool online = false;
            try
            {
                // 创建 TCP 客户端
                using (TcpClient client = new TcpClient(gateway.ip, Port))
                {
                    // 创建 Modbus TCP Master（主站）
                    var master = ModbusIpMaster.CreateIp(client);

                    for (int i = 0; i < gateway.sensors.Count; i++)
                    {
                        if (gateway.sensors[i].type == "IOLink")
                        {
                            // 读取保持寄存器（功能码 0x03）
                            // 例如读取地址 0x0002（对应传统地址 40003），共读取 1 个寄存器
                            ushort startAddress = ModbusAddress[i];
                            ushort numRegisters = 5;
                            ushort[] values = master.ReadHoldingRegisters(SlaveId, startAddress, numRegisters);
                            

                            if (gateway.sensors[i].low_threshold != null && gateway.sensors[i].up_threshold != null)
                            {
                                string[] low_threshold_str = gateway.sensors[i].low_threshold.Split(',');
                                string[] up_threshold_str = gateway.sensors[i].up_threshold.Split(',');

                                for (int p = 0; p < 5; p++)
                                {
                                    low_threshold[p] = float.Parse(low_threshold_str[p]);
                                    up_threshold[p] = float.Parse(up_threshold_str[p]);
                                }

                                for (int q = 0; q < 5; q++)
                                {
                                    if ((values[q] / 10.0f) >= up_threshold[q])
                                    {
                                        alarmRecord.Add(
                                                new AlarmRecord
                                                {
                                                    sensor_id = gateway.sensors[i].id,
                                                    alarmType = "High",
                                                    alarmValue = values[q] / 10.0f, // 假设寄存器值需要除以10转换为实际值
                                                    thresholdMin = low_threshold[q],
                                                    thresholdMax = up_threshold[q],
                                                    alarmLevel = "Danger",
                                                    alarmTime = DateTime.Now,
                                                    handled = false,
                                                    message = $"传感器 {point_sign[q]} 的值 {values[q] / 10.0f} 超过了上限阈值 {up_threshold[q]}"
                                                }
                                            );
                                    }
                                    if ((values[q] / 10.0f) <= low_threshold[q])
                                    {
                                        alarmRecord.Add(
                                                new AlarmRecord
                                                {
                                                    sensor_id = gateway.sensors[i].id,
                                                    alarmType = "Low",
                                                    alarmValue = values[q] / 10.0f, // 假设寄存器值需要除以10转换为实际值
                                                    thresholdMin = low_threshold[q],
                                                    thresholdMax = up_threshold[q],
                                                    alarmLevel = "Danger",
                                                    alarmTime = DateTime.Now,
                                                    handled = false,
                                                    message = $"传感器 {point_sign[q]} 的值 {values[q] / 10.0f} 低于了下限阈值 {low_threshold[q]}"
                                                }
                                            );
                                    }
                                }
                            }
                            else
                            {
                                alarmRecord.Clear();
                            }


                            SensorData singeSensor = new SensorData
                            {
                                SensorId = gateway.sensors[i].id,
                                Name = gateway.sensors[i].name,
                                DeviceId = gateway.sensors[i].device_id,
                                GatewayId = gateway.sensors[i].gateway_id,
                                Temperature = values[0] / 10.0, // 假设温度数据在第一个寄存器
                                XVibration = values[1] / 10.0, // 假设X方向振动数据在第二个寄存器
                                YVibration = values[2] / 10.0, // 假设Y方向振动数据在第三个寄存器
                                ZVibration = values[3] / 10.0, // 假设Z方向振动数据在第四个寄存器
                                Vibration = values[4] / 10.0,   // 假设总振动数据在第五个寄存器
                                AlarmRecord = alarmRecord,
                                Timestamp = DateTime.Now
                            };
                            returnsensorData.Add(singeSensor);
                        }
                        else if (gateway.sensors[i].type == "DI")
                        {
                            ushort startAddress = ModbusCoilAddress[i];
                            ushort numRegisters = 1;
                            bool[] values = master.ReadInputs(SlaveId, startAddress, numRegisters);
                            SensorData singeSensor = new SensorData
                            {
                                SensorId = gateway.sensors[i].id,
                                DeviceId = gateway.sensors[i].device_id,
                                GatewayId = gateway.sensors[i].gateway_id,
                                Temperature = 0.0, // 假设温度数据在第一个寄存器
                                XVibration = 0.0, // 假设X方向振动数据在第二个寄存器
                                YVibration = 0.0, // 假设Y方向振动数据在第三个寄存器
                                ZVibration = 0.0, // 假设Z方向振动数据在第四个寄存器
                                Vibration = values[0] == true ? 1.0 : 0.0,   // 假设总振动数据在第五个寄存器
                                Timestamp = DateTime.Now
                            };
                            returnsensorData.Add(singeSensor);
                        }

                    }
                    online = true;
                }

            }
            catch (SocketException)
            {
                online = false;
            }          
            return (returnsensorData, online);

        }
    }
}
