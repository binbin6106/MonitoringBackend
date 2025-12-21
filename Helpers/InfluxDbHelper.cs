using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Options;
using MonitoringBackend.Models;
using Newtonsoft.Json.Linq;
using NodaTime;

namespace MonitoringBackend.Helpers
{

    public class InfluxDbHelper
    {
        private readonly string _bucket = "";          // 改为你自己的 bucket 名称
        private readonly string _org = "";                // 改为你设置的组织名称
        private readonly InfluxDBClient _client;
        private readonly WriteApiAsync _writeApi;
        public InfluxDbHelper(IOptions<InfluxSettings> settings)
        {
            string influxUrl = settings.Value.Url;
            string token = settings.Value.Token;
            _bucket = settings.Value.Bucket;
            _org = settings.Value.Org;

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
                .Tag("sensor_type", data.SensorType.ToString())
                .Tag("name", data.Name.ToString())
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
                    .Tag("sensor_type", data.SensorType.ToString())
                    .Tag("name", data.Name.ToString())
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

        public async Task<List<SensorData>> QuerySensorDataAsync(int device_id, int sensor_id, DateTime from, DateTime to)
        {
            var flux = $@"
        from(bucket: ""{_bucket}"")
        |> range(start: {from:O}, stop: {to:O})
        |> filter(fn: (r) => r._measurement == ""sensor_data"")
        |> filter(fn: (r) => r[""device_id""] == ""{device_id}"")
        |> filter(fn: (r) => r[""sensor_id""] == ""{sensor_id}"")
        |> pivot(rowKey: [""_time""], columnKey: [""_field""], valueColumn: ""_value"")
        |> sort(columns: [""_time""])
        ";

            var result = new List<SensorData>();
            var tables = await _client.GetQueryApi().QueryAsync(flux, _org);

            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    var values = record.Values;

                    if (values.ContainsKey("temperature") && values.ContainsKey("vibration") && values["_time"] is Instant time)
                    {
                        result.Add(new SensorData
                        {
                            SensorId = Convert.ToInt32(values["sensor_id"]),
                            Name = values["name"]?.ToString(),
                            GatewayId = Convert.ToInt32(values["gateway_id"]),
                            DeviceId = device_id,
                            Timestamp = time.ToDateTimeUtc().ToLocalTime(),
                            Temperature = Convert.ToDouble(values["temperature"]),
                            Vibration = Convert.ToDouble(values["vibration"]),
                            XVibration = Convert.ToDouble(values["x_vibration"]),
                            YVibration = Convert.ToDouble(values["y_vibration"]),
                            ZVibration = Convert.ToDouble(values["z_vibration"]),
                            RPM = Convert.ToInt32(values["rpm"])
                        });
                    }
                }
            }

            return result;
        }

        public async Task<List<SensorData>> QuerySensorDataPagedAsync(int device_id, int sensor_id, DateTime from, DateTime to)
        {
            return await QuerySensorDataAsync(device_id, sensor_id, from, to);
        }
    }

}
