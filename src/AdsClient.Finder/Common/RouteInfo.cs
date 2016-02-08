namespace Ads.Client.Finder.Common
{
    public class RouteInfo
    {
        public string RouteName = System.Environment.MachineName; // Just a name of the new route
        public AdsNetId LocalAmsNetId;
        public bool IsTemporaryRoute= false;
        public string Login = "Administrator";
        public string Password = "1";
        public string Localhost = System.Environment.MachineName; // IP or machine name
    }
}
