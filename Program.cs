using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using NLog.Web;

namespace demo_close_stdout
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                var builder = new HostBuilder();

                builder.UseContentRoot(Directory.GetCurrentDirectory());
                builder.ConfigureHostConfiguration(config =>
                {
                    config.AddEnvironmentVariables(prefix: "DOTNET_");
                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                });

                builder.ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        var env = hostingContext.HostingEnvironment;

                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                        if (env.IsDevelopment() && !string.IsNullOrEmpty(env.ApplicationName))
                        {
                            var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                            if (appAssembly != null)
                            {
                                config.AddUserSecrets(appAssembly, optional: true);
                            }
                        }

                        config.AddEnvironmentVariables();

                        if (args != null)
                        {
                            config.AddCommandLine(args);
                        }
                    })
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                        // IMPORTANT: This needs to be added *before* configuration is loaded, this lets
                        // the defaults be overridden by the configuration.
                        if (isWindows)
                        {
                            // Default the EventLogLoggerProvider to warning or above
                            logging.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Warning);
                        }

                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        //在container環境要記得把這的log關掉
                        logging.AddConsole();
                        logging.AddDebug();
                        logging.AddEventSourceLogger();

                        if (isWindows)
                        {
                            // Add the EventLogLoggerProvider on windows machines
                            logging.AddEventLog();
                        }
                    })
                    .UseDefaultServiceProvider((context, options) =>
                    {
                        var isDevelopment = context.HostingEnvironment.IsDevelopment();
                        options.ValidateScopes = isDevelopment;
                        options.ValidateOnBuild = isDevelopment;
                    })
                    .ConfigureServices((hostContext, services) => { services.AddHostedService<DemoBackgroundService>(); });

                await builder.UseNLog().Build().RunAsync();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Stopped program because of exception");
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
    }
}