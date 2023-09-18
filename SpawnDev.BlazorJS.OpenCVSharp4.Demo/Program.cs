using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SpawnDev.BlazorJS.OpenCVSharp4.Demo;
using SpawnDev.BlazorJS.OpenCVSharp4.Services;
using SpawnDev.BlazorJS.Toolbox;
using SpawnDev.BlazorJS.WebWorkers;

namespace SpawnDev.BlazorJS.OpenCVSharp4.Demo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddBlazorJSRuntime();
            builder.Services.AddWebWorkerService();
            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton<WebWorkerPool>();
            builder.Services.AddSingleton<OpenCVService>();
            builder.Services.AddSingleton<MediaDevicesService>();


            await builder.Build().BlazorJSRunAsync();
        }
    }
}
