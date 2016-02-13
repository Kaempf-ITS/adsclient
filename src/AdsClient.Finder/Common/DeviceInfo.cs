using System.Net;

namespace Ads.Client.Finder.Common
{
    public class DeviceInfo
    {
        public string Name = "";
        public string Comment = "";
        public IPAddress Address;
        public AdsNetId AmsNetId = new AdsNetId("127.0.0.1.1.1");
        public string OsVersion = "";
        public string TcVersion = "";
        public bool IsRuntime = false;
    }
}
