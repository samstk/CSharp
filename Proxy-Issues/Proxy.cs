using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32;
//Pseudo
namespace Solutions
{
    /// <summary>
    /// Proxy-event class for web requests
    /// </summary>
    public static class Proxy
    {
        private static WebProxy proxy;
        /// <summary>
        /// Checks the web client and installs basic proxy settings
        /// </summary>
        /// <param name="client">web client</param>
        public static void CheckClient(WebClient client)
        {
            if(proxy==null)
            {
                string host = "";
                IWebProxy proxy2 = WebRequest.GetSystemWebProxy();
				//High-availability server
                host = proxy2.GetProxy(new Uri("http://www.google.com")).Host;
                proxy = new WebProxy(host, true);
                try
                {
					//Low-data return, checks for error when downloading
                    client.DownloadData("www.google.com");
                }
                catch
                {
                    if (Username == "" && Password == "")
                        //Always returns proxy server if there is a proxy - which does not equal the initial proxy host 
						if (proxy.Address.Host != "www.google.com")
                        {
                            PromptCredientials(proxy.Address.Host);
                            proxy.Credentials = new NetworkCredential(Username, Password);
                        }
                    else
                        {
                            proxy = null;
                        }
                }

            }
            client.Proxy = proxy;
        }
        private static void PromptCredientials(string server)
        {
			//Shows form with ShowPrompt(string server) function, which initialises proxy text, and form visibility
            /*PROMPTFORM*/.ShowPrompt(server);
        }
        private static ProxySetWindow win = new ProxySetWindow();
        private static string Username = "";
        private static string Password = "";
        /// <summary>
        /// Sets the credientials of the proxy
        /// </summary>
        /// <param name="user">username</param>
        /// <param name="pass">password</param>
        public static void SetCredientials(string user, string pass)
        {
            Username = user;
            Password = pass;
        }
        /// <summary>
        /// Sets the registry settings for the proxy
        /// </summary>
        /// <param name="server">server ip/address</param>
        /// <param name="username">userkey</param>
        /// <param name="password">passkey</param>
        public static void SetControl(string server, string username, string password)
        {
            RegistryKey RegKey = Registry.CurrentUser.OpenSubKey(subkey, true);
            RegKey.SetValue("ProxyServer", username+":"+password+"@"+server);
            RegKey.SetValue("ProxyEnable", 1);
            RegKey.Close();
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }
        /// <summary>
        /// Prompts the user to set the control henceafter
        /// </summary>
        public static void PromptControl()
        {
            IWebProxy proxy2 = WebRequest.GetSystemWebProxy();
			//Connects to a URL with high availability
            string server = proxy2.GetProxy(new Uri("http://www.google.com")).Host;
            Username = "";
            Password = "";
            PromptCredientials(server);
            if (Username == "")
                return;
            SetControl(server, Username, Password);
        }
        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;

        const string userRoot = "HKEY_CURRENT_USER";
        const string subkey = "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings";
        const string keyName = userRoot + "\\" + subkey;
        /// <summary>
        /// Resets the registry settings for the proxy
        /// </summary>
        public static void ReleaseControl()
        {
            RegistryKey RegKey = Registry.CurrentUser.OpenSubKey(subkey, true);
            RegKey.SetValue("ProxyServer", "");
            RegKey.SetValue("ProxyEnable", 0);
            RegKey.Close();
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }
    }
}

