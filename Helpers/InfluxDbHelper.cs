using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace MonitoringBackend.Helpers
{

    public class InfluxDbHelper
    {
        private readonly string _bucket = "monitor";          // 改为你自己的 bucket 名称
        private readonly string _org = "binbin";                // 改为你设置的组织名称
        private readonly InfluxDBClient _client;
        private readonly WriteApiAsync _writeApi;
        public InfluxDbHelper(string influxUrl, string token)
        {
            _client = InfluxDBClientFactory.Create(influxUrl, token.ToCharArray());
            _writeApi = _client.GetWriteApiAsync();
        }

        public async Task WriteSensorDataAsync(SensorData data)
        {
            var point = PointData
                .Measurement("sensor_data")
                .Tag("sensor_id", data.SensorId.ToString())
                .Tag("device_id", data.DeviceId.ToString())
                .Tag("gateway_id", data.GatewayId.ToString())
                .Field("temperature", data.Temperature)
                .Field("x_vibration", data.XVibration)
                .Field("y_vibration", data.YVibration)
                .Field("z_vibration", data.ZVibration)
                .Field("vibration", data.Vibration)
                .Field("rpm", data.RPM)
                .Timestamp(data.Timestamp, WritePrecision.Ns);

            await _writeApi.WritePointAsync(point, _bucket, _org);
        }

        public async Task WriteSensorBatchAsync(List<SensorData> dataList)
        {
            var points = new List<PointData>();

            foreach (var data in dataList)
            {
                var point = PointData
                    .Measurement("sensor_data")
                    .Tag("sensor_id", data.SensorId.ToString())
                    .Tag("device_id", data.DeviceId.ToString())
                    .Tag("gateway_id", data.GatewayId.ToString())
                    .Field("temperature", data.Temperature)
                    .Field("x_vibration", data.XVibration)
                    .Field("y_vibration", data.YVibration)
                    .Field("z_vibration", data.ZVibration)
                    .Field("vibration", data.Vibration)
                    .Field("rpm", data.RPM)
                    .Timestamp(data.Timestamp, WritePrecision.Ns);

                points.Add(point);
            }
            try
            {
                await _writeApi.WritePointsAsync(points, _bucket, _org);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"写入 InfluxDB 出错: {ex.Message}");
            }
        }
    }

}
