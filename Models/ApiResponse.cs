namespace MonitoringBackend.Models
{
    public class ApiResponse<T>
    {
        public int Code { get; set; }
        public T Data { get; set; }
        public string Msg { get; set; }

        public static ApiResponse<T> Success(T data, string msg = "成功")
        {
            return new ApiResponse<T>
            {
                Code = 200,
                Data = data,
                Msg = msg
            };
        }

        public static ApiResponse<T> Fail(string msg, int code = 500)
        {
            return new ApiResponse<T>
            {
                Code = code,
                Data = default,
                Msg = msg
            };
        }
    }

}
