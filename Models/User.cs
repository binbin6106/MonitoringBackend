using System.ComponentModel.DataAnnotations.Schema;
namespace MonitoringBackend.Models
{
    public class User
    {
        public long id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public string password { get; set; }
        public bool isEnabled { get; set; } = true; // 默认启用
        //[Column("created_at")]
        //public DateTime createTime { get; set; }
    }

}
