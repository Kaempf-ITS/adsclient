using System;

namespace Ads.Client.Helpers
{
    /// <summary>
    /// System Manager to .NET Type
    /// https://infosys.beckhoff.com/content/1033/tcsystemmanager/basics/tcsysmgr_datatypecomparison.htm
    /// </summary>
    public sealed class SmType
    {
        public static readonly Type BIT = typeof(Boolean);
        public static readonly Type BIT8 = typeof(Boolean);
        public static readonly Type BITARR8 = typeof(Byte);
        public static readonly Type BITARR16 = typeof(UInt16);
        public static readonly Type BITARR32 = typeof(UInt32);
        public static readonly Type INT8 = typeof(SByte);
        public static readonly Type INT16 = typeof(Int16);
        public static readonly Type INT32 = typeof(Int32);
        public static readonly Type INT64 = typeof(Int64);
        public static readonly Type UINT8 = typeof(Byte);
        public static readonly Type UINT16 = typeof(UInt16);
        public static readonly Type UINT32 = typeof(UInt32);
        public static readonly Type UINT64 = typeof(UInt64);
        public static readonly Type FLOAT = typeof(Single);
        public static readonly Type DOUBLE = typeof(Double);
    }
}
