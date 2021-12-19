using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using Serilog;
using Web.UI.MVC;

var configuration = GetConfiguration();

Log.Logger = CreateSerilogLogger(configuration);

try
{
    Log.Information("Configuring web host ({ApplicationContext})...", "Web.UI.MVC.Program.AppName");
    var host = BuildWebHost(configuration, args);

    Log.Information("Starting web host ({ApplicationContext})...", "Web.UI.MVC.Program.AppName");
    host.Run();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", "Web.UI.MVC.Program.AppName");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
    Microsoft.AspNetCore.WebHost.CreateDefaultBuilder(args)
        .CaptureStartupErrors(false)
        .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
        .UseStartup<Startup>()
        .UseSerilog()
        .Build();

Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var logstashUrl = configuration["Serilog:LogstashgUrl"];
    var cfg = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .Enrich.WithProperty("ApplicationContext", "Web.UI.MVC.Program.AppName")
        .Enrich.FromLogContext()
        .WriteTo.Console();
    if (!string.IsNullOrWhiteSpace(seqServerUrl))
    {
        cfg.WriteTo.Seq(seqServerUrl);
    }

    if (!string.IsNullOrWhiteSpace(logstashUrl))
    {
        cfg.WriteTo.Http(logstashUrl);
    }

    return cfg.CreateLogger();
}

IConfiguration GetConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    return builder.Build();
}
