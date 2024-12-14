using System;
using System.Windows.Browser;

namespace Jamesnet.Foundation
{
    public class JwtTokenManager
    {
        private const string JwtTokenKey = "JwtToken";

        public string Initialize()
        {
            var token = ExtractTokenFromUrl();
            if (!string.IsNullOrEmpty(token))
            {
                SaveTokenToCookie(token);
                return token;
            }
            return GetTokenFromCookie();
        }

        private string ExtractTokenFromUrl()
        {
            try
            {
                var uri = HtmlPage.Document.DocumentUri.ToString();
                var queryStartIndex = uri.IndexOf("?", StringComparison.Ordinal);
                if (queryStartIndex == -1) return null;

                var queryParams = uri.Substring(queryStartIndex + 1).Split('&');
                foreach (var param in queryParams)
                {
                    var keyValue = param.Split('=');
                    if (keyValue.Length == 2 && keyValue[0] == "token")
                    {
                        return Uri.UnescapeDataString(keyValue[1]); // URL 디코딩 추가
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public void RemoveTokenFromCookie(bool redirect = true)
        {
            var script = @"
               function removeToken() {
                   document.cookie = '" + JwtTokenKey + @"=; path=/; expires=Thu, 01 Jan 1970 00:00:01 GMT; secure';
               }
               removeToken();";
            HtmlPage.Window.Eval(script);

            if (redirect)
            {
                HtmlPage.Window.Eval("window.location.href = '/?session-expired'");
            }
        }

        private void SaveTokenToCookie(string token)
        {
            if (string.IsNullOrEmpty(token)) return;

            RemoveTokenFromCookie(false);

            var expirationDate = DateTime.UtcNow.AddDays(30).ToString("R");
            var script = @"
               function saveToken(token, expiry) {
                   document.cookie = '" + JwtTokenKey + @"=' + token + '; path=/; expires=' + expiry + '; secure';
               }
               saveToken('" + token + "', '" + expirationDate + "');";

            HtmlPage.Window.Eval(script);
            HtmlPage.Window.Eval("window.history.replaceState({}, '', window.location.pathname)");
        }

        public string GetTokenFromCookie()
        {
            var script = @"
               function getToken() {
                   let cookies = document.cookie.split(';');
                   for(let cookie of cookies) {
                       cookie = cookie.trim();
                       if (cookie.startsWith('" + JwtTokenKey + @"=')) {
                           return cookie.substring('" + JwtTokenKey + @"='.length);
                       }
                   }
                   return null;
               }
               getToken();";

            return HtmlPage.Window.Eval(script) as string;
        }
    }
}