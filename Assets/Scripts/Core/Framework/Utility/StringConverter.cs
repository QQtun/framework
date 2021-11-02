using System.Globalization;
using UnityEngine;

namespace Core.Framework.Utility
{
    public class StringConverter
    {
        public const long Billion = 1000000000;
        public const long TenBillion = Billion * 10;
        public const int Million = 1000000;
        public const int TenMillion = Million * 10;
        public const int Kilo = 1000;
        public const int TenKilo = Kilo * 10;

        private const string TenBFormate = "{0:##.#}B";
        private const string BFormate = "{0:#.##}B";
        private const string TenMFormate = "{0:##.#}M";
        private const string MFormate = "{0:#.##}M";
        private const string TenKFormate = "{0:##.#}K";
        private const string KFormate = "{0:#.##}K";

        public static string ConvertToKmb(int value)
        {
            return ConvertToKmb((long)value);
        }
        public static string ConvertToKmb(long value)
        {
#if UNITY_5_3_OR_NEWER
            if (value >= TenBillion)
            {
                return string.Format(TenBFormate, Mathf.Floor(value * 10 / Billion) / 10);  // *10 取底 在 /10 是在做無條件捨去
            }
            if (value >= Billion)
            {
                return string.Format(BFormate, Mathf.Floor(value * 100 / Billion) / 100);
            }
            if (value >= TenMillion)
            {
                return string.Format(TenMFormate, Mathf.Floor(value * 10 / Million) / 10);
            }
            if (value >= Million)
            {
                return string.Format(MFormate, Mathf.Floor(value * 100 / Million) / 100);
            }
            if (value >= TenKilo)
            {
                return string.Format(TenKFormate, Mathf.Floor(value * 10 / Kilo) / 10);
            }
            if (value >= Kilo)
            {
                return string.Format(KFormate, Mathf.Floor(value * 100 / Kilo) / 100);
            }
#endif
            return value.ToString();
        }
        public static string ConvertToKmb(uint value)
        {
            return ConvertToKmb((ulong)value);
        }
        public static string ConvertToKmb(ulong value)
        {
#if UNITY_5_3_OR_NEWER
            if (value >= TenBillion)
            {
                return string.Format(TenBFormate, Mathf.Floor(value / (float)Billion * 10) / 10);  // *10 取底 在 /10 是在做無條件捨去
            }
            if (value >= Billion)
            {
                return string.Format(BFormate, Mathf.Floor(value / (float)Billion * 100) / 100);
            }
            if (value >= TenMillion)
            {
                return string.Format(TenMFormate, Mathf.Floor(value / (float)Million * 10) / 10);
            }
            if (value >= Million)
            {
                return string.Format(MFormate, Mathf.Floor(value / (float)Million * 100) / 100);
            }
            if (value >= TenKilo)
            {
                return string.Format(TenKFormate, Mathf.Floor(value / (float)Kilo * 10) / 10);
            }
            if (value >= Kilo)
            {
                return string.Format(KFormate, Mathf.Floor(value / (float)Kilo * 100) / 100);
            }
#endif
            return value.ToString();
        }

        public static string ConvertIntToStringWithCommas(int value)
        {
            return value == 0 ? value.ToString() : value.ToString("#,#");
        }

        public static string ConvertIntToStringWithCommas(long value)
        {
            return value == 0 ? value.ToString() : value.ToString("#,#");
        }

        public static string ConvertIntToStringWithCommas(uint value)
        {
            return value == 0 ? value.ToString() : value.ToString("#,#");
        }

        public static string ConvertIntToStringWithCommas(ulong value)
        {
            return value == 0 ? value.ToString() : value.ToString("#,#");
        }

        public static string ConvertToBOrAddCommas(int value)
        {
#if UNITY_5_3_OR_NEWER
            if (value >= Billion)
            {
                return ConvertToKmb(value);
            }
#endif
            return ConvertIntToStringWithCommas(value);
        }
        public static string ConvertToBOrAddCommas(long value)
        {
#if UNITY_5_3_OR_NEWER
            if (value >= Billion)
            {
                return ConvertToKmb(value);
            }
#endif
            return ConvertIntToStringWithCommas(value);
        }
        public static string ConvertToBOrAddCommas(ulong value)
        {
#if UNITY_5_3_OR_NEWER
            if (value >= Billion)
            {
                return ConvertToKmb(value);
            }
#endif
            return ConvertIntToStringWithCommas(value);
        }
    }
}