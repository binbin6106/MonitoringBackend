using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Sockets;
using InfluxDB.Client.Api.Domain;
using NModbus;
using MonitoringBackend.Models; // NModbus4 的命名空间

namespace MonitoringBackend.Helpers
{
    public class ModbusTcpHelper
    {
        private Gateway gateway {  get; set; } = new Gateway();
        private List<SensorData> returnsensorData { get; set; } = new List<SensorData>();
        private int Port { get; set; }
        private byte SlaveId { get; set; }
        private ushort[] ModbusAddress = {0x4000, 0x4006, 0x400C, 0x4012, 0x4018, 0x401E, 0x4024, 0x402A};
        private ushort[] ModbusCoilAddress { get; set; } = { 0x0000, 0x0002, 0x0004, 0x0006 };
        public Dictionary<string, AlarmRecord> alarmRecord = new Dictionary<string, AlarmRecord>();
        private float[] low_threshold = new float[5];
        private float[] up_threshold = new float[5];
        public string[] point_sign = { "温度", "X方向振动", "Y方向振动", "Z方向振动", "振动矢量和" };

        public ModbusTcpHelper(Gateway gatewayData)
        {
            gateway = gatewayData;
            Port = 502;              // Modbus默认端口
            SlaveId = 1;            // 从站ID
        }

        public (List<SensorData>, bool, Dictionary<string, AlarmRecord>) getData()
        {
            returnsensorData.Clear();
            bool online = false;
            try
            {
                // 创建 TCP 客户端
                using (TcpClient client = new TcpClient(gateway.ip, Port))
                {
                    var factory = new ModbusFactory();
                    // 创建 Modbus TCP Master（主站）
                    var master = factory.CreateMaster(client);
                    ushort numRegisters = 0;
                    for (int i = 0; i < gateway.sensors.Count; i++)
                    {                
                        if (gateway.sensors[i].type == "RPM")
                        {
                            numRegisters = 6; // 读取6个寄存器
                        }
                        else
                        {
                            numRegisters = 5; // 读取5个寄存器
                        }
                        ushort[] values = new ushort[6];
                        ushort startAddress = ModbusAddress[i];
                        ushort[] tempValues = master.ReadHoldingRegisters(SlaveId, startAddress, numRegisters);
                        Array.Copy(tempValues, values, Math.Min(numRegisters, values.Length));

                        if (gateway.sensors[i].low_threshold != null && gateway.sensors[i].up_threshold != null)
                        {
                            string[] low_threshold_str = gateway.sensors[i].low_threshold.Split(',');
                            string[] up_threshold_str = gateway.sensors[i].up_threshold.Split(',');

                            low_threshold = low_threshold_str.Select(p => float.Parse(p)).ToArray();
                            up_threshold = up_threshold_str.Select(p => float.Parse(p)).ToArray();

                            for (int q = 0; q < 5; q++)
                            {
                                float nowVaule = values[q] / 10.0f;
                                string index = i.ToString() + '.' + q.ToString();
                                bool isNowAlarm = alarmRecord.TryGetValue(index, out AlarmRecord prevAlarm) ? true : false;
                                //不处于报警状态时，才会去判断阈值
                                if (!isNowAlarm)
                                {
                                    if (nowVaule >= up_threshold[q])
                                    {
                                        alarmRecord.Add(index,
                                            new AlarmRecord
                                            {
                                                sensor_id = gateway.sensors[i].id,
                                                channel_id = q,
                                                device_id = gateway.sensors[i].device_id,
                                                device_name = gateway.sensors[i].device_name,
                                                sensor_name = gateway.sensors[i].name,
                                                point_name = point_sign[q],
                                                alarmType = "High",
                                                alarmValue = nowVaule, // 假设寄存器值需要除以10转换为实际值
                                                thresholdMin = low_threshold[q],
                                                thresholdMax = up_threshold[q],
                                                alarmLevel = "Danger",
                                                alarmTime = DateTime.Now,
                                                handled = false
                                            }
                                        );
                                    }
                                    if (nowVaule <= low_threshold[q])
                                    {
                                        alarmRecord.Add(index,
                                                new AlarmRecord
                                                {
                                                    sensor_id = gateway.sensors[i].id,
                                                    channel_id = q,
                                                    device_id = gateway.sensors[i].device_id,
                                                    device_name = gateway.sensors[i].device_name,
                                                    sensor_name = gateway.sensors[i].name,
                                                    point_name = point_sign[q],
                                                    alarmType = "Low",
                                                    alarmValue = nowVaule, // 假设寄存器值需要除以10转换为实际值
                                                    thresholdMin = low_threshold[q],
                                                    thresholdMax = up_threshold[q],
                                                    alarmLevel = "Danger",
                                                    alarmTime = DateTime.Now,
                                                    handled = false
                                                }
                                            );
                                    }
                                }
                                //处于报警状态时，判断值是否恢复正常
                                else
                                {
                                    if (nowVaule < up_threshold[q] && nowVaule > low_threshold[q])
                                    {
                                        alarmRecord.Remove(index);
                                    }
                                }
                            }
                        }

                        SensorData singeSensor = new SensorData
                        {
                            SensorId = gateway.sensors[i].id,
                            SensorType = gateway.sensors[i].type,
                            Name = gateway.sensors[i].name,
                            DeviceId = gateway.sensors[i].device_id,
                            GatewayId = gateway.sensors[i].gateway_id,
                            Temperature = values[0] / 10.0,
                            XVibration = values[4] / 1000.0,
                            YVibration = values[3] / 1000.0,
                            ZVibration = values[2] / 1000.0,
                            Vibration = values[1] / 1000.0,
                            RPM = values[5],
                            Timestamp = DateTime.Now
                        };
                        returnsensorData.Add(singeSensor);
                    }
                    online = true;
                }

            }
            catch (SocketException)
            {
                online = false;
            }          
            return (returnsensorData, online, alarmRecord);

        }
        // 检查是否需要报警（仅在状态发生变化时返回 true）
        //private (bool needAlarm, AlarmType type) CheckAndUpdateAlarm(int sensorId, float value, float lowThreshold, float highThreshold)
        //{
        //    var previousState = _sensorAlarmStates.TryGetValue(sensorId, out var prev) ? prev : AlarmState.Normal;
        //    AlarmState currentState;

        //    if (value > highThreshold)
        //    {
        //        currentState = AlarmState.HighAlarm;
        //    }
        //    else if (value < lowThreshold)
        //    {
        //        currentState = AlarmState.LowAlarm;
        //    }
        //    else
        //    {
        //        currentState = AlarmState.Normal;
        //    }

        //    // 状态未变 → 不报警
        //    if (previousState == currentState)
        //    {
        //        return (false, AlarmType.None);
        //    }

        //    // 状态变了 → 更新状态并报警
        //    _sensorAlarmStates[sensorId] = currentState;

        //    return currentState switch
        //    {
        //        AlarmState.HighAlarm => (true, AlarmType.High),
        //        AlarmState.LowAlarm => (true, AlarmType.Low),
        //        _ => (false, AlarmType.None) // 状态恢复正常可以触发“恢复”事件
        //    };
        //}
    }
}
