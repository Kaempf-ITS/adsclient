using Ads.Client.Finder;
using Ads.Client.Finder.Common;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

namespace Ads.Client.Test
{
	[TestFixture()]
    public class RouteManagerTest
    {
        [Test()]
        public async Task RouteManager_Static_AddRemoteRouteAsync()
        {
            //IPAddress localhost = IPAddress.Parse("192.168.1.10");
            //IPAddress plcIpAddress = IPAddress.Parse("192.168.1.33");

            //RouteInfo info = new RouteInfo();
            //info.Localhost = localhost.ToString();
            //info.LocalAmsNetId = new AdsNetId("192.168.56.1.1.1");
            //info.IsTemporaryRoute = false;

            //bool act = await RouteManager.AddRemoteRouteAsync(localhost, plcIpAddress, info, 1000);

            //Assert.IsTrue(act);
        }

        [Test()]
        public async Task RouteManager_Temp_AddRemoteRouteAsync()
        {
            //IPAddress localhost = IPAddress.Parse("192.168.1.10");
            //IPAddress plcIpAddress = IPAddress.Parse("192.168.1.14");

            //RouteInfo info = new RouteInfo();
            //info.Localhost = localhost.ToString();
            //info.LocalAmsNetId = new AdsNetId("192.168.56.1.1.1");
            //info.IsTemporaryRoute = true;

            //bool act = await RouteManager.AddRemoteRouteAsync(localhost, plcIpAddress, info, 1000);

            //Assert.IsTrue(act);
        }
    }
}
