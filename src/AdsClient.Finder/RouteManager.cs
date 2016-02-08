using Ads.Client.Finder.Common;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Ads.Client.Finder
{
    public class RouteManager
    {
        /// <summary>
        /// Add new route to remote PLC.
        /// </summary>
        /// <param name="localhost"></param>
        /// <param name="remoteHost"></param>
        /// <param name="routeInfo"></param>
        /// <param name="timeout"></param>
        /// <returns>True - if route added, False - otherwise</returns>
        public static async Task<bool> AddRemoteRouteAsync(
            IPAddress localhost,
            IPAddress remoteHost,
            RouteInfo routeInfo,
            int timeout = 10000,
            int adsUdpPort = 48899)
        {
            byte[] Segment_AMSNETID = routeInfo.LocalAmsNetId.Bytes;

            byte[] Segment_ROUTENAME = routeInfo.RouteName.GetAdsBytes();
            byte[] Segment_ROUTENAME_LENGTH = Segment.ROUTENAME_L;
            Segment_ROUTENAME_LENGTH[2] = (byte)Segment_ROUTENAME.Length;

            byte[] Segment_USERNAME = routeInfo.Login.GetAdsBytes();
            byte[] Segment_USERNAME_LENGTH = Segment.USERNAME_L;
            Segment_USERNAME_LENGTH[2] = (byte)Segment_USERNAME.Length;

            byte[] Segment_PASSWORD = routeInfo.Password.GetAdsBytes();
            byte[] Segment_PASSWORD_LENGTH = Segment.PASSWORD_L;
            Segment_PASSWORD_LENGTH[2] = (byte)Segment_PASSWORD.Length;

            byte[] Segment_IPADDRESS = routeInfo.Localhost.GetAdsBytes();
            byte[] Segment_IPADDRESS_LENGTH = Segment.LOCALHOST_L;
            Segment_IPADDRESS_LENGTH[2] = (byte)Segment_IPADDRESS.Length;

            Request request = new Request();

            request.Add(Segment.HEADER);
            request.Add(Segment.END);
            request.Add(Segment.REQUEST_ADDROUTE);
            request.Add(Segment_AMSNETID);
            request.Add(Segment.PORT);
            request.Add(routeInfo.IsTemporaryRoute ?
                        Segment.ROUTETYPE_TEMP :
                        Segment.ROUTETYPE_STATIC);
            request.Add(Segment_ROUTENAME_LENGTH);
            request.Add(Segment_ROUTENAME);
            request.Add(Segment.AMSNETID_L);
            request.Add(Segment_AMSNETID);
            request.Add(Segment.USERNAME_L);
            request.Add(Segment_USERNAME);
            request.Add(Segment.PASSWORD_L);
            request.Add(Segment_PASSWORD);
            request.Add(Segment.LOCALHOST_L);
            request.Add(Segment_IPADDRESS);

            if (routeInfo.IsTemporaryRoute)
                request.Add(
                        Segment.TEMPROUTE_TAIL);

            IPEndPoint endpoint = new IPEndPoint(remoteHost, adsUdpPort);
            Response response = await request.SendAsync(endpoint);
            ResponseResult rr = await response.ReceiveAsync();
            bool isAck = ParseResponse(rr);

            return isAck;
        }

        /// <summary>
        /// Parses response early recieved by AdsFinder
        /// </summary>
        /// <param name="rr"></param>
        /// <returns>True - if route added, False - otherwise</returns>
        public static bool ParseResponse(ResponseResult rr)
        {
            if (rr == null)
                return false;

            if (!rr.Buffer.Take(4).ToArray().SequenceEqual(Segment.HEADER))
                    return false;
            if (!rr.Buffer.Skip(4).Take(Segment.END.Length).ToArray().SequenceEqual(Segment.END))
                return false;
            if (!rr.Buffer.Skip(8).Take(Segment.RESPONSE_DISCOVER.Length).ToArray().SequenceEqual(Segment.RESPONSE_ADDROUTE))
                return false;

            rr.Shift =
                Segment.HEADER.Length +
                Segment.END.Length +
                Segment.RESPONSE_ADDROUTE.Length +
                Segment.AMSNETID.Length +
                Segment.PORT.Length +
                Segment.END.Length +
                Segment.END.Length;

            byte[] ack = rr.NextChunk(Segment.L_ROUTEACK);

            return (ack[0] == 0) && (ack[1] == 0);
        }
    }
}
