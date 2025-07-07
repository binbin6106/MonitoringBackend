using System;
using System.Diagnostics.Metrics;
using System.Net.Sockets;
using InfluxDB.Client.Api.Domain;
using Modbus.Device;
using MonitoringBackend.Models; // NModbus4 的命名空间

namespace MonitoringBackend.Helpers
{
    public class ModbusTcpHelper
    {
        private Device device {  get; set; } = new Device();
        private List<SensorData> returnsensorData { get; set; } = new List<SensorData>();
        private int Port { get; set; }
        private byte SlaveId { get; set; }

        private ushort[] ModbusAddress { get; set; } = {0x0002,  0x0012, 0x0022, 0x0032};
        private ushort[] ModbusCoilAddress { get; set; } = { 0x0000, 0x0002, 0x0004, 0x0006 };
        public ModbusTcpHelper(Device deviceData)
        {
            device = deviceData;
            Port = 502;              // Modbus默认端口
            SlaveId = 1;            // 从站ID
        }

        public (List<SensorData>, bool) getData()
        {
            returnsensorData.Clear();
            bool online = false;
            // 创建 TCP 客户端
            using (TcpClient client = new TcpClient(device.ip, Port))
            {
                try
                {
                    // 创建 Modbus TCP Master（主站）
                    var master = ModbusIpMaster.CreateIp(client);

                    for (int i = 0; i < device.sensors.Count; i++)
                    {
                        if (device.sensors[i].type == "IOLink")
                        {
                            // 读取保持寄存器（功能码 0x03）
                            // 例如读取地址 0x0002（对应传统地址 40003），共读取 1 个寄存器
                            ushort startAddress = ModbusAddress[i];
                            ushort numRegisters = 5;
                            ushort[] values = master.ReadHoldingRegisters(SlaveId, startAddress, numRegisters);
                            SensorData singeSensor = new SensorData
                            {
                                SensorId = device.sensors[i].id,
                                Temperature = values[0] / 10.0, // 假设温度数据在第一个寄存器
                                XVibration = values[1] / 10.0, // 假设X方向振动数据在第二个寄存器
                                YVibration = values[2] / 10.0, // 假设Y方向振动数据在第三个寄存器
                                ZVibration = values[3] / 10.0, // 假设Z方向振动数据在第四个寄存器
                                Vibration = values[4] / 10.0,   // 假设总振动数据在第五个寄存器
                                Timestamp = DateTime.Now
                            };
                            returnsensorData.Add(singeSensor);
                        }
                        else if (device.sensors[i].type == "DI")
                        {
                            ushort startAddress = ModbusCoilAddress[i];
                            ushort numRegisters = 1;
                            bool[] values = master.ReadInputs(SlaveId, startAddress, numRegisters);
                            SensorData singeSensor = new SensorData
                            {
                                SensorId = device.sensors[i].id,
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
                catch (SocketException)
                {
                    for (int i = 0; i < device.sensors.Count; i++)
                    {
                           SensorData singeSensor = new SensorData
                            {
                                SensorId = device.sensors[i].id,
                                Temperature = 0.0, // 假设温度数据在第一个寄存器
                                XVibration = 0.0, // 假设X方向振动数据在第二个寄存器
                                YVibration = 0.0, // 假设Y方向振动数据在第三个寄存器
                                ZVibration = 0.0, // 假设Z方向振动数据在第四个寄存器
                                Vibration = 0.0,   // 假设总振动数据在第五个寄存器
                                Timestamp = DateTime.Now
                            };
                            returnsensorData.Add(singeSensor);                      
                    }
                    online = false;
                }

            }
            return (returnsensorData, online);

        }
    }
}
