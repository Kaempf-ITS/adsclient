using NUnit.Framework;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Ads.Client.Finder.Common;
using Ads.Client.Finder;

namespace Ads.Client.Test
{
	[TestFixture()]
    public class DeviceFinderTest
    {
        [Test()]
        public async Task DeviceFinder_BroadcastSearchAsync()
        {
            //IPAddress localhost = IPAddress.Parse("192.168.1.10");
            //List<DeviceInfo> devices = await DeviceFinder.BroadcastSearchAsync(localhost, 1000);
            //foreach (DeviceInfo d in devices)
            //{
            //    Console.WriteLine("IP: " + d.Address.ToString());
            //    Console.WriteLine("AmsNetId: " + d.AmsNetId.ToString());
            //    Console.WriteLine("Name: " + d.Name);
            //    Console.WriteLine("Comment: " + d.Comment);
            //    Console.WriteLine("OS version: " + d.OsVersion);
            //    Console.WriteLine("TwinCAT: " + d.TcVersion);
            //    Console.WriteLine("Runtime: " + (d.IsRuntime ? "present" : "not present (engineering)"));
            //    Console.WriteLine(Environment.NewLine);
            //}

            //Assert.IsTrue(devices.Count > 0);
        }

        [Test()]
        public async Task DeviceFinder_GetDeviceInfoAsync()
        {
            //IPAddress localhost = IPAddress.Parse("192.168.1.10");
            //IPAddress remotehost = IPAddress.Parse("192.168.1.14");
            //DeviceInfo d = await DeviceFinder.GetDeviceInfoAsync(localhost, remotehost, 1000);
            //Console.WriteLine("IP: " + d.Address.ToString());
            //Console.WriteLine("AmsNetId: " + d.AmsNetId.ToString());
            //Console.WriteLine("Name: " + d.Name);
            //Console.WriteLine("Comment: " + d.Comment);
            //Console.WriteLine("OS version: " + d.OsVersion);
            //Console.WriteLine("TwinCAT: " + d.TcVersion);
            //Console.WriteLine("Runtime: " + (d.IsRuntime ? "present" : "not present (engineering)"));
            //Console.WriteLine(Environment.NewLine);

            //Assert.Pass();
        }

        [Test()]
        public void IPHelper_GetHostMask()
        {
            //IPAddress address = IPAddress.Parse("192.168.1.10");
            //IPAddress act = IPHelper.GetHostMask(address);
            //IPAddress exp = IPAddress.Parse("255.255.255.0");

            //Assert.AreEqual(exp, act);
        }

        [Test()]
        public void AdsNetId()
        {
            string expString = "192.168.56.3.1.1";
            byte[] expBytes = { 192, 168, 56, 3, 1, 1 };

            AdsNetId byString = new AdsNetId(expString);
            AdsNetId byBytes = new AdsNetId(expBytes);

            Assert.AreEqual(expBytes, byString.Bytes);
            Assert.AreEqual(expString, byString.ToString());

            Assert.AreEqual(expBytes, byBytes.Bytes);
            Assert.AreEqual(expString, byBytes.ToString());
        }
    }
}
