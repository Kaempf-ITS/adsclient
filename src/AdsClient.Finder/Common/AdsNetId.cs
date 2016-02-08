using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ads.Client.Finder.Common
{
    public class AdsNetId
    {
        public readonly byte[] Bytes;
        readonly string likeString = null;

        public AdsNetId(string amsNetId)
        {
            List<byte> byteList = new List<byte>();

            string[] parts = amsNetId.Trim().Split('.');
            foreach (string part in parts)
            {
                byteList.Add(
                    byte.Parse(part));
            }

            if (byteList.Count != 6)
                throw new ArgumentOutOfRangeException("amsNetId", "AmsNetId must contain six numbers. Ex.: 192.168.12.10.1.1");

            Bytes = byteList.ToArray();

            likeString = ToString(Bytes);
        }

        public AdsNetId(byte[] bytes)
        {
            if (bytes.Length != 6)
                throw new ArgumentOutOfRangeException("bytes", "The array size must be equal to 6 items.");

            Bytes = bytes;
            likeString = ToString(Bytes);
        }

        private string ToString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                sb.Append(Bytes[i]);
                if (i < 5)
                    sb.Append('.');
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return likeString;
        }
    }
}
