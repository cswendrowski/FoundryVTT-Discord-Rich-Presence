using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DiscordSdk;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddControllers();

            //var discord = new Discord(long.Parse("635971834499563530"), (ulong)CreateFlags.Default);

            //RegisterActivityManager(discord);

            //RunCallbacks(discord);

            //services.AddSingleton(discord);
        }

        private static void RunCallbacks(Discord discord)
        {
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        discord.RunCallbacks();
                        Thread.Sleep(1000 / 60);
                    }
                }
                finally
                {
                    discord.Dispose();
                }
            });
        }

        private static void RegisterActivityManager(Discord discord)
        {
            var activityManager = discord.GetActivityManager();

            // Received when someone accepts a request to join or invite.
            // Use secrets to receive back the information needed to add the user to the group/party/match
            activityManager.OnActivityJoin += secret =>
            {
                Console.WriteLine("OnJoin {0}", secret);
            };

            // Received when someone accepts a request to spectate
            activityManager.OnActivitySpectate += secret =>
            {
                Console.WriteLine("OnSpectate {0}", secret);
            };

            // A join request has been received. Render the request on the UI.
            activityManager.OnActivityJoinRequest += (ref User user) =>
            {
                Console.WriteLine("OnJoinRequest {0} {1}", user.Id, user.Username);
            };

            // An invite has been received. Consider rendering the user / activity on the UI.
            activityManager.OnActivityInvite += (ActivityActionType Type, ref User user, ref Activity activity2) =>
            {
                Console.WriteLine("OnInvite {0} {1} {2}", Type, user.Username, activity2.Name);
                // activityManager.AcceptInvite(user.Id, result =>
                // {
                //     Console.WriteLine("AcceptInvite {0}", result);
                // });
            };
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
