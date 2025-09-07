using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using NodaTime;
namespace MonitoringBackend;

public class InfluxService
{
    private readonly InfluxDBClient _client;
    private readonly string _bucket;
    private readonly string _org;
   
    public InfluxService(IOptions<InfluxSettings> settings)
    {
        var opts = settings.Value;
        _client = InfluxDBClientFactory.Create(opts.Url, opts.Token.ToCharArray());
        _bucket = opts.Bucket;
        _org = opts.Org;
    }

    public async Task WriteSensorDataAsync(SensorData data)
    {
        if (data != null)
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
            .Timestamp(data.Timestamp.ToUniversalTime(), WritePrecision.Ms);

            await _client.GetWriteApiAsync().WritePointAsync(point, _bucket, _org);
        }

    }

    public async Task WriteSensorBatchAsync(List<SensorData> dataList)
    {
        var writeApi = _client.GetWriteApiAsync();

        //using var writeApi = _client.GetWriteApiAsync();
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
                .Timestamp(data.Timestamp, WritePrecision.Ns);

            points.Add(point);
        }
        try
        {
            await writeApi.WritePointsAsync(points, _bucket, _org);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"写入 InfluxDB 出错: {ex.Message}");
        }
    }

    public async Task<List<SensorData>> QuerySensorDataAsync(int sensorId, DateTime from, DateTime to)
    {
        var flux = $@"
        from(bucket: ""{_bucket}"")
        |> range(start: {from:O}, stop: {to:O})
        |> filter(fn: (r) => r._measurement == ""sensor_data"")
        |> filter(fn: (r) => r[""sensor_id""] == ""{sensorId}"")
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
                        SensorId = sensorId,
                        Timestamp = time.ToDateTimeUtc().ToLocalTime(),
                        Temperature = Convert.ToDouble(values["temperature"]),
                        Vibration = Convert.ToDouble(values["vibration"])
                    });
                }
            }
        }

        return result;
    }
}
