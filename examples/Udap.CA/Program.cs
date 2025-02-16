#region (c) 2023 Joseph Shook. All rights reserved.
// /*
//  Authors:
//     Joseph Shook   Joseph.Shook@Surescripts.com
// 
//  See LICENSE in the project root for license information.
// */
#endregion

using Google.Cloud.SecretManager.V1;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Udap.CA.DbContexts;
using Udap.CA.Services;
using Udap.CA.Services.State;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(
            outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));


    StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

    var provider = builder.Configuration.GetValue("provider", "SqlServer");
    string dbChoice;

    if (Environment.GetEnvironmentVariable("GCPDeploy") == "true")
    {
        dbChoice = "gcp_udap_ca_sql_db";
    }
    else if (Environment.GetEnvironmentVariable("LocalSqlDb") == "true")
    {
        dbChoice = "local_sqlDb";
    }
    else
    {
        dbChoice = "DefaultConnection";
    }

    string? connectionString;

    if (Environment.GetEnvironmentVariable("GCLOUD_PROJECT") != null)
    {
        // Log.Information("Loading connection string from gcp_db");
        // connectionString = Environment.GetEnvironmentVariable("gcp_db");
        // Log.Information($"Loaded connection string, length:: {connectionString?.Length}");

        Log.Information("Creating client");
        var client = SecretManagerServiceClient.Create();

        const string secretResource = "projects/288013792534/secrets/gcp_udap_ca_sql_db/versions/latest";

        Log.Information("Requesting {secretResource");

        // Call the API.
        var result = client.AccessSecretVersion(secretResource);

        // Convert the payload to a string. Payloads are bytes by default.
        var payload = result.Payload.Data.ToStringUtf8();

        connectionString = payload;
    }
    else
    {
        connectionString = builder.Configuration.GetConnectionString(dbChoice);
    }
    

// Add services to the container.
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    builder.Services.AddMudServices();
    builder.Services.AddAutoMapper(typeof(Program));

    _ = provider switch
    {
        "sqlite" => builder.Services.AddDbContext<IUdapCaContext, UdapCaContext>(
            options => options.UseSqlite(connectionString)
                .LogTo(Console.WriteLine, LogLevel.Information)),

        "SqlServer" => builder.Services.AddDbContext<IUdapCaContext, UdapCaContext>(
            options => options.UseSqlServer(connectionString)
                .LogTo(Console.WriteLine, LogLevel.Information)),

        _ => throw new Exception($"Unsupported provider: {provider}")
    };


    builder.Services.AddSingleton<CommunityState>();
    builder.Services.AddScoped<CommunityService>();
    builder.Services.AddScoped<RootCertificateService>();

    var app = builder.Build();

// Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseRouting();

    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down completed");
    Log.CloseAndFlush();
}