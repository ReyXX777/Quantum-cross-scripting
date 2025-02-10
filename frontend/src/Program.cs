using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using YourNamespace.Services; // Added namespace for services
using Blazored.LocalStorage; // Added for local storage
using Tewr.Blazors.FileReader; // Added for file uploads
using MudBlazor; // Added for MudBlazor
using MudBlazor.Services; // Added for MudBlazor services


namespace YourNamespace
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // Added Component 1: Register ApiService
            builder.Services.AddScoped<ApiService>();

            // Added Component 2: Register Local Storage Service
            builder.Services.AddBlazoredLocalStorage();

            // Added Component 3: Register File Reader Service
            builder.Services.AddFileReaderService();

            // Added Component 4: Register MudBlazor services
            builder.Services.AddMudServices();

            // Added Component 5: Example of registering a custom service (assuming you have a CounterService)
            builder.Services.AddScoped<CounterService>();


            await builder.Build().RunAsync();
        }
    }
}
