using Highlander.Core.Common;
using Highlander.Core.Common.CommsInterfaces;
using Highlander.Core.Common.Services;
using Highlander.Core.Server;
using Highlander.GrpcService.Data;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.RefCounting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;

namespace Highlander.GrpcService
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddDbContext<HighlanderContext>(opt => opt.UseSqlite(configuration.GetConnectionString("Storage")), ServiceLifetime.Singleton);

            //logging
            ProcessModule pm = Process.GetCurrentProcess().MainModule;
            string moduleName = pm?.ModuleName.Split('.')[0];
            var logger = Reference<ILogger>.Create(new FileLogger(@"logs/" + moduleName + ".{dddd}.log"));

            const EnvId env = EnvId.Dev_Development; // hack EnvId.Dev_Development
            //var settings = new NamedValueSet(EnvHelper.GetAppSettings(env, EnvHelper.SvcPrefix(SvcId.CoreServer), true));
            var settings = new NamedValueSet();
            services.AddSingleton(sp =>
            {
                return new CoreServer(logger, settings, sp.GetService<HighlanderContext>());
            });

            services.AddSingleton<ITransferV341>(sp => sp.GetService<CoreServer>().CommsEngine);
            services.AddSingleton<ISessCtrlV131>(sp => sp.GetService<CoreServer>().CommsEngine);
            services.AddSingleton<IDiscoverV111>(sp => sp.GetService<CoreServer>().CommsEngine);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<TransferReceiverV341>();
                endpoints.MapGrpcService<DiscoverReceiverV111>();
                endpoints.MapGrpcService<SessCtrlReceiverV131>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            //start core server
            Console.WriteLine("Starting server...");
            var coreServer = app.ApplicationServices.GetService<CoreServer>();
            coreServer.Start();
        }
    }
}
