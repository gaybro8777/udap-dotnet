#region (c) 2022 Joseph Shook. All rights reserved.
// /*
//  Authors:
//     Joseph Shook   Joseph.Shook@Surescripts.com
// 
//  See LICENSE in the project root for license information.
// */
#endregion

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Udap.Server;
using Udap.Server.Extensions;
using Udap.Server.Registration;

namespace Udap.Idp;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        if (! int.TryParse(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT"), out int sslPort))
        {
            sslPort = 5002;
        }

        //
        // Running localhost:5002 when UseKestrel confiratio below is commented
        // Uncomment and to run your own cert.  
        //

        // builder.WebHost.UseKestrel((webHostBuilderContext, kestrelServerOptions) =>
        // {
        //     kestrelServerOptions.ListenAnyIP(sslPort, listenOpt =>
        //     {
        //         listenOpt.UseHttps(
        //             Path.Combine(
        //                 Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? string.Empty,
        //                 webHostBuilderContext.Configuration["SslFileLocation"]),
        //             webHostBuilderContext.Configuration["CertPassword"]);
        //     });
        // });


        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // uncomment if you want to add a UI
        builder.Services.AddRazorPages();

        var migrationsAssembly = typeof(Program).Assembly.GetName().Name;
        builder.Services.AddIdentityServer(options =>
            {
                // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
                options.EmitStaticAudienceClaim = true;
            })
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlite(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlite(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            //TODO remove
            .AddTestUsers(TestUsers.Users)
            .AddUdapDiscovery()
            .AddUdapServerConfiguration()
            .AddUdapConfigurationStore(options =>
            {
                options.UdapDbContext = b => b.UseSqlite(connectionString,
                    sql => sql.MigrationsAssembly(typeof(UdapDiscoveryEndpoint).Assembly.FullName));
            });

        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();
            
        app.UseIdentityServer();

        app.MapPost("/connect/register", async (HttpContext httpContext, [FromServices] UdapDynamicClientRegistrationEndpoint endpoint) =>
        {
            //TODO:  Tests and response codes needed...    httpContext.Response
            await endpoint.Process(httpContext);
        })
        .AllowAnonymous()
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status401Unauthorized);

        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
