namespace MonitoringBackend.Models
{
    public class ResPage<T>
    {
        public List<T> List { get; set; } = new();
        public int PageNum { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
    }

}
