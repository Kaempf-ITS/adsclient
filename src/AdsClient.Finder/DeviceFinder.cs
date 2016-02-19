﻿using Ads.Client.Finder.Common;
using Ads.Client.Finder.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ads.Client.Finder
{
    public static class DeviceFinder
    {
        public static async Task<List<DeviceInfo>> BroadcastSearchAsync(IPAddress localhost, int timeout = 10000, int adsUdpPort = Request.DEFAULT_UDP_PORT)
        {
            Request request = CreateSearchRequest(localhost, timeout);

            IPEndPoint broadcast =
                new IPEndPoint(
                    IPHelper.GetBroadcastAddress(localhost),
                    adsUdpPort);

            Response response = await request.SendAsync(broadcast);
            List<ResponseResult> responses = await response.ReceiveMultipleAsync();

            List<DeviceInfo> devices = new List<DeviceInfo>();
            foreach (var r in responses)
            {
                DeviceInfo device = ParseResponse(r);
                devices.Add(device);
            }

            return devices;
        }

        public static async Task<DeviceInfo> GetDeviceInfoAsync(IPAddress localhost, IPAddress remoteHost, int timeout = 10000, int adsUdpPort = Request.DEFAULT_UDP_PORT)
        {
            Request request = CreateSearchRequest(localhost, timeout);

            IPEndPoint broadcast = new IPEndPoint(remoteHost, adsUdpPort);

            Response response = await request.SendAsync(broadcast);
            ResponseResult rr = await response.ReceiveAsync();
            DeviceInfo device = ParseResponse(rr);

            return device;
        }

        public static Request CreateSearchRequest(IPAddress localhost, int timeout = 10000)
        {
            Request request = new Request(timeout);

            byte[] Segment_AMSNETID = Segment.AMSNETID;
            localhost.GetAddressBytes().CopyTo(Segment_AMSNETID, 0);

            request.Add(Segment.HEADER);
            request.Add(Segment.END);
            request.Add(Segment.REQUEST_DISCOVER);
            request.Add(Segment_AMSNETID);
            request.Add(Segment.PORT);
            request.Add(Segment.END);

            return request;
        }

        public static DeviceInfo ParseResponse(ResponseResult rr)
        {
            DeviceInfo device = new DeviceInfo();

            device.Address = rr.RemoteHost;

            if (!rr.Buffer.Take(4).ToArray().SequenceEqual(Segment.HEADER))
                return device;
            if (!rr.Buffer.Skip(4).Take(Segment.END.Length).ToArray().SequenceEqual(Segment.END))
                return device;
            if (!rr.Buffer.Skip(8).Take(Segment.RESPONSE_DISCOVER.Length).ToArray().SequenceEqual(Segment.RESPONSE_DISCOVER))
                return device;

            rr.Shift = Segment.HEADER.Length + Segment.END.Length + Segment.RESPONSE_DISCOVER.Length;

            // AmsNetId
            // then skip 2 bytes of PORT + 4 bytes of ROUTE_TYPE
            byte[] amsNetId = rr.NextChunk(Segment.AMSNETID.Length, add: Segment.PORT.Length + Segment.ROUTETYPE_STATIC.Length);
            device.AmsNetId = new AdsNetId(amsNetId);

            // PLC NameLength
            byte[] bNameLen = rr.NextChunk(Segment.L_NAMELENGTH);
            int nameLen =
                bNameLen[0] == 5 && bNameLen[1] == 0 ?
                    bNameLen[2] + bNameLen[3] * 256 :
                    0;

            byte[] bName = rr.NextChunk(nameLen - 1, add: 1);
            device.Name = System.Text.ASCIIEncoding.Default.GetString(bName);

            // TCat type
            byte[] tcatType = rr.NextChunk(Segment.TCATTYPE_RUNTIME.Length);
            if (tcatType[0] == Segment.TCATTYPE_RUNTIME[0])
            {
                if(tcatType[2] == Segment.TCATTYPE_RUNTIME[2])
                    device.IsRuntime = true;
            }

            // OS version
            byte[] osVer = rr.NextChunk(Segment.L_OSVERSION);
            ushort osKey = (ushort)(osVer[0] * 256 + osVer[4]);
            device.OsVersion = OS_IDS.ContainsKey(osKey) ? OS_IDS[osKey] : osKey.ToString("X2");
            //??? device.OS += " build " + (osVer[8] + osVer[9] * 256).ToString();

            bool isUnicode = false;

            // looking for packet with tcat version;
            // usually it is in the end of the packet
            byte[] tail = rr.NextChunk(rr.Buffer.Length - rr.Shift, true);

            string tcatV = "";
            int ci = tail.Length - 4;
            for (int i = ci; i > 0; i -= 4)
            {
                if (tail[i + 0] == 3 &&
                    tail[i + 2] == 4)
                {
                    isUnicode = tail[i + 4] > 2; // Tc3 uses unicode
                    tcatV = string.Format("{0}.{1} build {2}",
                        tail[i + 4].ToString(),
                        tail[i + 5].ToString(),
                        (tail[i + 6] + tail[i + 7] * 256).ToString());
                    break;
                }
            }
            device.TcVersion = tcatV;

            // Comment
            byte[] descMarker = rr.NextChunk(Segment.L_DESCRIPTIONMARKER);
            int len = 0;
            int c = rr.Buffer.Length;
            if (descMarker[0] == 2)
            {
                if (isUnicode)
                {
                    for (int i = 0; i < c; i += 2)
                    {
                        if (rr.Buffer[rr.Shift + i] == 0 &&
                            rr.Buffer[rr.Shift + i + 1] == 0)
                            break;
                        len += 2;
                    }
                }
                else
                {
                    for (int i = 0; i < c; i++)
                    {
                        if (rr.Buffer[rr.Shift + i] == 0)
                            break;
                        len++;
                    }
                }

                if (len > 0)
                {
                    byte[] description = rr.NextChunk(len);

                    if (isUnicode)
                    {
                        byte[] asciiBytes = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, description);
                        char[] asciiChars = new char[Encoding.ASCII.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
                        Encoding.ASCII.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
                        device.Comment = new string(asciiChars);
                    }
                    else
                        device.Comment = ASCIIEncoding.Default.GetString(description);
                }
            }

            return device;
        }

        public static readonly Dictionary<ushort, string> OS_IDS =
            new Dictionary<ushort, string>
            {
                {0x0700, "Windows CE 7"},
                {0x0602, "Windows 8/8.1/10"},
                {0x0601, "Windows 7 Embedded Standart"},
                {0x0600, "Windows CE 6"},
                {0x0500, "Windows CE 5"},
                {0x0501, "Windows XP"}
            };
    }
}
