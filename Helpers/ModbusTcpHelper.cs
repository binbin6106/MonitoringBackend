using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Sockets;
using InfluxDB.Client.Api.Domain;
using NModbus;
using MonitoringBackend.Models;
using Microsoft.AspNetCore.SignalR;

namespace MonitoringBackend.Helpers
{
    public class ModbusTcpHelper
    {
        private Gateway gateway { get; set; } = new Gateway();
        private List<SensorData> returnsensorData { get; set; } = new List<SensorData>();
        private int Port { get; set; }
        private byte SlaveId { get; set; }
        private ushort[] ModbusAddress = { 0x4000, 0x4006, 0x400C, 0x4012, 0x4018, 0x401E, 0x4024, 0x402A };
        private ushort[] ModbusCoilAddress { get; set; } = { 0x0000, 0x0002, 0x0004, 0x0006 };
        public Dictionary<string, AlarmRecord> alarmRecord = new Dictionary<string, AlarmRecord>();
        private float[] low_threshold = new float[5];
        private float[] up_threshold = new float[5];
        public string[] point_sign = { "温度", "X方向振动", "Y方向振动", "Z方向振动", "振动矢量和" };

        public ModbusTcpHelper(Gateway gatewayData)
        {
            gateway = gatewayData;
            Port = 502;
            SlaveId = 1;
        }

        public (List<SensorData>, bool, Dictionary<string, AlarmRecord>) getData()
        {
            returnsensorData.Clear();
            bool online = false;
            try
            {
                using (TcpClient client = new TcpClient(gateway.ip, Port))
                {
                    var factory = new ModbusFactory();
                    var master = factory.CreateMaster(client);
                    ushort numRegisters = 0;

                    for (int i = 0; i < gateway.sensors.Count; i++)
                    {
                        if (gateway.sensors[i].type == "RPM")
                        {
                            numRegisters = 6;
                        }
                        else
                        {
                            numRegisters = 5;
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
                                float nowValue = values[q] / (q == 0 ? 10.0f : 1000.0f); // 温度除以10，振动除以1000
                                string index = i.ToString() + '.' + q.ToString();
                                bool isNowAlarm = alarmRecord.ContainsKey(index);

                                if (!isNowAlarm)
                                {
                                    // 检查是否超出阈值
                                    if (nowValue >= up_threshold[q] || nowValue <= low_threshold[q])
                                    {
                                        string alarmType = nowValue >= up_threshold[q] ? "High" : "Low";
                                        
                                        // 计算报警级别（新增20%逻辑）
                                        string alarmLevel = CalculateAlarmLevel(nowValue, low_threshold[q], up_threshold[q]);

                                        alarmRecord.Add(index, new AlarmRecord
                                        {
                                            sensor_id = gateway.sensors[i].id,
                                            channel_id = q, // 按顺序：0=温度，1=X振动，2=Y振动，3=Z振动，4=振动矢量和
                                            device_id = gateway.sensors[i].device_id,
                                            device_name = gateway.sensors[i].device_name,
                                            sensor_name = gateway.sensors[i].name,
                                            point_name = point_sign[q],
                                            alarmType = alarmType,
                                            alarmValue = nowValue,
                                            thresholdMin = low_threshold[q],
                                            thresholdMax = up_threshold[q],
                                            alarmLevel = alarmLevel, // 使用新的报警级别
                                            alarmTime = DateTime.Now,
                                            handled = false
                                        });
                                    }
                                }
                                else
                                {
                                    // 检查是否恢复正常
                                    if (nowValue < up_threshold[q] && nowValue > low_threshold[q])
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

                        bool isOnline = singeSensor.Temperature > 0;
                        DeviceStatusStore.OnlineStatus[singeSensor.SensorId] = isOnline;

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

        // 新增：计算报警级别（0-20%为一般，>20%为紧急）
        private string CalculateAlarmLevel(float currentValue, float minThreshold, float maxThreshold)
        {
            if (currentValue > maxThreshold)
            {
                var exceedPercentage = (currentValue - maxThreshold) / maxThreshold;
                return exceedPercentage > 0.2f ? "critical" : "warning";
            }
            else if (currentValue < minThreshold)
            {
                var deficitPercentage = (minThreshold - currentValue) / minThreshold;
                return deficitPercentage > 0.2f ? "critical" : "warning";
            }
            return "normal";
        }
    }
}
