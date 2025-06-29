using System.Security.Cryptography;
using System.Text;

public static class Md5Helper
{
    /// <summary>
    /// 将字符串进行 MD5 加密，返回 32 位小写十六进制字符串
    /// </summary>
    public static string Encrypt(string input)
    {
        using (var md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // 转换为十六进制字符串
            StringBuilder sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2")); // 小写16进制
            }

            return sb.ToString(); // 返回32位的十六进制字符串
        }
    }
}
