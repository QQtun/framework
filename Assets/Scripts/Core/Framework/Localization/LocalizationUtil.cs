using I2.Loc;

namespace Core.Framework.Localization
{
    public class LocalizationUtil
    {
        private static object[] sArray1 = new object[1];
        private static object[] sArray2 = new object[2];
        private static object[] sArray3 = new object[3];
        private static object[] sArray4 = new object[4];
        private static object[] sArray5 = new object[5];
        private static object[] sArray6 = new object[6];
        private static object[] sArray7 = new object[7];
        private static object[] sArray8 = new object[8];
        private static object[] sArray9 = new object[9];
        private static object[] sArray10 = new object[10];

        public static bool IsKey(string str)
        {
            return str.StartsWith("[") && str.EndsWith("]");
        }

        /// <summary>
        ///     key 可以帶[] 也可以不帶
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetTranslation(string key)
        {
            string newKey = key;
            if (IsKey(newKey))
                newKey = newKey.Substring(1, newKey.Length - 2);

            if (!LocalizationManager.TryGetTranslation(newKey, out var value))
            {
                return key;
            }
            return value;
        }

        /// <summary>
        ///     key 和 args 都會做翻譯 再format
        /// </summary>
        /// <param name="key">可以帶[] 也可以不帶</param>
        /// <param name="args">可以帶[] 也可以不帶</param>
        /// <returns></returns>
        public static string GetTranslationFormat(string key, params object[] args)
        {
            string newKey = key;
            if (IsKey(newKey))
                newKey = newKey.Substring(1, newKey.Length - 2);

            if (!LocalizationManager.TryGetTranslation(newKey, out var value))
            {
                return key;
            }

            if (args != null && args.Length > 0)
            {
                var newArgs = GetCacheArray(args.Length);
                for (int i = 0; i < args.Length; i++)
                {
                    newArgs[i] = args[i];
                    if (args[i] is string)
                    {
                        string str = (string)args[i];
                        newArgs[i] = GetTranslation(str);
                    }
                }
                return string.Format(value, newArgs);
            }
            return value;
        }

        private static object[] GetCacheArray(int length)
        {
            object[] ret;
            switch (length)
            {
                case 1:
                {
                    ret = sArray1;
                    break;
                }
                case 2:
                {
                    ret = sArray2;
                    break;
                }
                case 3:
                {
                    ret = sArray3;
                    break;
                }
                case 4:
                {
                    ret = sArray4;
                    break;
                }
                case 5:
                {
                    ret = sArray5;
                    break;
                }
                case 6:
                {
                    ret = sArray6;
                    break;
                }
                case 7:
                {
                    ret = sArray7;
                    break;
                }
                case 8:
                {
                    ret = sArray8;
                    break;
                }
                case 9:
                {
                    ret = sArray9;
                    break;
                }
                case 10:
                {
                    ret = sArray10;
                    break;
                }
                default:
                {
                    ret = new object[length];
                    break;
                }
            }

            System.Array.Clear(ret, 0, ret.Length);
            return ret;
        }
    }
}