using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace QuantumCrossScripting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                // Log any unhandled exceptions during startup
                Console.WriteLine($"Application startup failed: {ex.Message}");
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Load environment-specific configuration files
                    var env = context.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    // Add environment variables as a configuration source
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((context, logging) =>
                {
                    // Clear default logging providers
                    logging.ClearProviders();

                    // Add console logging for development
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    }

                    // Add other logging providers (e.g., file, Azure App Insights) for production
                    else
                    {
                        logging.AddApplicationInsights(); // Example: Add Azure Application Insights
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                              .ConfigureKestrel((context, options) =>
                              {
                                  // Configure Kestrel server settings (e.g., timeouts, limits)
                                  options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
                              })
                              .UseUrls("http://*:5000;https://*:5001"); // Specify default URLs
                });
    }
}
