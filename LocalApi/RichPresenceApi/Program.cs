using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using RichPresenceApi;
using System;

namespace TestApi
{
    public class Program
    {
        const string UriScheme = "foundryvtt-richpresence";
        const string FriendlyName = "FoundryVTT Discord Rich Presence";

        public static void Main(string[] args)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                RegisterWindowsCustomUriScheme();
            }

            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        private static void RegisterWindowsCustomUriScheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UriScheme))
                {
                    string applicationLocation = typeof(Startup).Assembly.Location.Replace(".dll", ".exe");

                    key.SetValue("", "URL:" + FriendlyName);
                    key.SetValue("URL Protocol", "");

                    using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                    {
                        defaultIcon.SetValue("", applicationLocation + ",1");
                    }

                    using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                    {
                        commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                    }
                    Console.WriteLine($"Registered URI scheme {UriScheme}:// to location {applicationLocation}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't register URI scheme due to: " + e.Message);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            DiscordManager.CreateDiscord();

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://*:2324");
                });
        }
    }
}
