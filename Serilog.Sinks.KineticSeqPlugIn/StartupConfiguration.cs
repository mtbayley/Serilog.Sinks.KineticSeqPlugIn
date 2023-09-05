using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Serilog.Sinks.KineticSeqPlugIn.StartupConfiguration))]

namespace Serilog.Sinks.KineticSeqPlugIn;

public class StartupConfiguration : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        //var stringBuilder = new StringBuilder($"Starting {DateTime.Now}");
        //System.IO.File.WriteAllText(@"C:\Temp\test-hosting.log", stringBuilder.ToString());
    }
}
