using System;

namespace Ads.Client.Helpers
{
    /// <summary>
    /// IEC61131-3 to .NET Type
    /// https://infosys.beckhoff.com/content/1033/tcsystemmanager/basics/tcsysmgr_datatypecomparison.htm
    /// </summary>
    public sealed class StType
    {
        public static readonly Type BOOL = typeof(Boolean);
        public static readonly Type BYTE = typeof(Byte);
        public static readonly Type WORD = typeof(UInt16);
        public static readonly Type DWORD = typeof(UInt32);
        public static readonly Type SINT = typeof(SByte);
        public static readonly Type INT = typeof(Int16);
        public static readonly Type DINT = typeof(Int32);
        public static readonly Type LINT = typeof(Int64);
        public static readonly Type USINT = typeof(Byte);
        public static readonly Type UINT = typeof(UInt16);
        public static readonly Type UDINT = typeof(UInt32);
        public static readonly Type ULINT = typeof(UInt64);
        public static readonly Type REAL = typeof(Single);
        public static readonly Type LREAL = typeof(Double);
    }
}
