using System.Net;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.Reflection;

namespace FlowBlox.Core.Util
{
    internal class WebBrowserCookieContainer
    {
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetGetCookieEx(
            string url,
            string cookieName,
            StringBuilder cookieData,
            ref int size,
            int dwFlags,
            IntPtr lpReserved);

        private const int InternetCookieHttponly = 0x2000;

        /// <summary>
        /// Liefert alle Cookies zu einem <c>CookieContainer</c> zurück.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static IEnumerable<Cookie> GetAllCookiesFromCookieContainer(CookieContainer c)
        {
            Hashtable k = (Hashtable)c.GetType().GetField("m_domainTable", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(c);
            foreach (DictionaryEntry element in k)
            {
                SortedList l = (SortedList)element.Value.GetType().GetField("m_list", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(element.Value);
                foreach (var e in l)
                {
                    var cl = (CookieCollection)((DictionaryEntry)e).Value;
                    foreach (Cookie fc in cl)
                    {
                        yield return fc;
                    }
                }
            }
        }

        /// <summary>
        /// Übernimmt die Cookies aus dem übergebenen <c>CookieContainer</c> in die aktuelle <c>WebBrowser</c> Session.
        /// </summary>
        /// <param name="cookieContainer"></param>
        public static void SetCookieContainer(CookieContainer cookieContainer)
        {
            if (cookieContainer != null)
            {
                foreach (Cookie cookie in GetAllCookiesFromCookieContainer(cookieContainer))
                {
                    InternetSetCookie(cookie.Domain, cookie.Name, cookie.Value);
                }
            }
        }

        /// <summary>
        /// Gets the URI cookie container.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        public static CookieContainer GetUriCookieContainer(Uri uri)
        {
            CookieContainer cookies = null;
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if (datasize < 0)
                    return null;
                // Allocate StringBuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(
                    uri.ToString(),
                    null, cookieData,
                    ref datasize,
                    InternetCookieHttponly,
                    IntPtr.Zero))
                    return null;
            }
            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }
            return cookies;
        }
    }
}
