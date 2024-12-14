using System;
using System.Windows.Browser;

namespace Jamesnet.Platform.OpenSilver.Scripts
{
    public class DeviceInfo
    {
        public static bool IsMobile
        {
            get
            {
                var userAgent = HtmlPage.Window.Eval("navigator.userAgent.toLowerCase()").ToString();
                return userAgent.Contains("android") ||
                       userAgent.Contains("webos") ||
                       userAgent.Contains("iphone") ||
                       userAgent.Contains("ipad") ||
                       userAgent.Contains("ipod") ||
                       userAgent.Contains("blackberry") ||
                       userAgent.Contains("windows phone");
            }
        }

        public static double WindowWidth => Convert.ToDouble(HtmlPage.Window.Eval("window.innerWidth"));
    }
}
