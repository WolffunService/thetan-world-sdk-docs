using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Wolffun.Log;

#if UNITY_2021_3_OR_NEWER
using UnityEngine.Pool;
#endif

namespace Wolffun.RestAPI
{
    public static class Utils
    {
        
        public static class WolffunRequestCommonPool
        {
#if UNITY_2021_3_OR_NEWER
            private static readonly ObjectPool<WolffunRequestCommon> WolffunRequestPool = new(() => new());
            public static WolffunRequestCommon Get() => WolffunRequestPool.Get();

            public static PooledObject<WolffunRequestCommon> Get(out WolffunRequestCommon value) =>
                WolffunRequestPool.Get(out value);

            public static void Release(WolffunRequestCommon toRelease) => WolffunRequestPool.Release(toRelease);
#else
            private static readonly Wolffun.Pooling.ObjectPool<WolffunRequestCommon> WolffunRequestPool = new Wolffun.Pooling.ObjectPool<WolffunRequestCommon>(
                () => new WolffunRequestCommon());
            public static WolffunRequestCommon Get() => WolffunRequestPool.Get();

            public static void Release(WolffunRequestCommon toRelease) => WolffunRequestPool.Release(toRelease);
#endif
        }
        
        public static class StringBuilderPool
        {
#if UNITY_2021_3_OR_NEWER
            private static readonly ObjectPool<StringBuilder> SPool = new(() => new(), null, sb => sb.Clear());
            public static StringBuilder Get() => SPool.Get();
            public static PooledObject<StringBuilder> Get(out StringBuilder value) => SPool.Get(out value);
            public static void Release(StringBuilder toRelease) => SPool.Release(toRelease);
#else
            private static readonly Wolffun.Pooling.ObjectPool<StringBuilder> SPool = new Wolffun.Pooling.ObjectPool<StringBuilder>(
                () => new StringBuilder(), null, sb => sb.Clear());
            public static StringBuilder Get() => SPool.Get();
            public static void Release(StringBuilder toRelease) => SPool.Release(toRelease);
#endif
        }
        
        public static class WolffunResponseCommonPool
        {
#if UNITY_2021_3_OR_NEWER
            private static readonly ObjectPool<WolffunResponseCommon> WolffunRequestPool = new(() => new());
            public static WolffunResponseCommon Get() => WolffunRequestPool.Get();

            public static PooledObject<WolffunResponseCommon> Get(out WolffunResponseCommon value) =>
                WolffunRequestPool.Get(out value);

            public static void Release(WolffunResponseCommon toRelease) => WolffunRequestPool.Release(toRelease);
#else
            private static readonly Wolffun.Pooling.ObjectPool<WolffunResponseCommon> WolffunRequestPool = new Wolffun.Pooling.ObjectPool<WolffunResponseCommon>(
                () => new WolffunResponseCommon());
            public static WolffunResponseCommon Get() => WolffunRequestPool.Get();

            public static void Release(WolffunResponseCommon toRelease) => WolffunRequestPool.Release(toRelease);
#endif
        }

        public static KeyValuePair<string, string>[] GetProperties(this object me)
        {
#if UNITY_2021_3_OR_NEWER
            var result = ListPool<KeyValuePair<string, string>>.Get();
#else
            var result = Wolffun.Pooling.ListPool<KeyValuePair<string, string>>.Get();
#endif
            
            try
            {
                foreach (var property in me.GetType().GetProperties())
                {
                    var value = property.GetValue(me, null);
                    if (value == null)
                        continue;
                    result.Add(new KeyValuePair<string, string>(property.Name, value.ToString()));
                }

                foreach (var field in me.GetType().GetFields())
                {
                    var value = field.GetValue(me);
                    if (value == null)
                        continue;
                    result.Add(new KeyValuePair<string, string>(field.Name, value.ToString()));
                }

                var res = result.ToArray();
                
#if UNITY_2021_3_OR_NEWER
                ListPool<KeyValuePair<string, string>>.Release(result);
#else
                Wolffun.Pooling.ListPool<KeyValuePair<string, string>>.Release(result);
#endif
                return res;
            }
            catch (Exception ex)
            {
                CommonLog.Log(ex.StackTrace);
#if UNITY_2021_3_OR_NEWER
                ListPool<KeyValuePair<string, string>>.Release(result);
#else
                Wolffun.Pooling.ListPool<KeyValuePair<string, string>>.Release(result);
#endif
                return null;
            }
        }

        public static List<IMultipartFormSection> GetForm(this object me)
        {
            try
            {
                List<IMultipartFormSection> form = new List<IMultipartFormSection>();
                var prop = me.GetProperties();
                foreach (KeyValuePair<string, string> keyValuePair in prop)
                {
                    form.Add(new MultipartFormDataSection(keyValuePair.Key, keyValuePair.Value));
                }

                return form;
            }
            catch (Exception ex)
            {
                CommonLog.Log(ex.StackTrace);
                return null;
            }
        }

        public static string GetDefaultDeviceID()
        {
            string tempDeviceId = SystemInfo.deviceUniqueIdentifier.ToString();

#if UNITY_IOS && !UNITY_EDITOR
            return "IOS_" + tempDeviceId;
#elif UNITY_ANDROID
            return "AND_" + tempDeviceId;
#elif UNITY_STANDALONE_WIN
            return "WINDOW_" + tempDeviceId;
#endif
            return tempDeviceId;
        }

        const string EMAIL_REGEX = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)" + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
        public static bool IsEmailValid(string email)
        {
            //var regex = new Regex(EMAIL_REGEX, RegexOptions.IgnoreCase);
            //return regex.IsMatch(email);

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }

        }

        public static ScreenType GetCurrentScreenType()
        {
            if (UnityEngine.Screen.width < UnityEngine.Screen.height)
            {
                return ScreenType.Portrait;
            }
            else
            {
                return ScreenType.Landscape;
            }
        }

        public enum ScreenType
        {
            Landscape = 0,
            Portrait = 1,
        }
    }
}