using System.Net;

namespace CSharpDemo.Utils
{
    public class SystemHelper
    {
        public static string GetHostAddress()
        {
            var host = "";
            var hostName = Dns.GetHostName();
            var addresses = Dns.GetHostAddresses(hostName);
            foreach (var ip in addresses)
            {
                if (ip.AddressFamily.ToString() != "InterNetwork") continue;
                //只找一个IPV4地址
                host = ip.ToString();
            }

            return host;
        }
    }
}