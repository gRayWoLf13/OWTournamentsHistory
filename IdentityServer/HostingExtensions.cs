using OWTournamentsHistory.Common.Settings;
using Serilog;

namespace IdentityServer
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            //var identityServerBuilder = builder.Environment.IsDevelopment()
            //    ? builder.Services.AddIdentityServer(x =>
            //    {
            //        x.IssuerUri = "https://identity-server:5001";
            //    })
            //    : builder.Services.AddIdentityServer();

            var identityServerSettings = configuration
                 .GetRequiredSection(nameof(IdentityServerSettings))
                 .Get<IdentityServerSettings>()
                 ?? throw new Exception($"{nameof(IdentityServerSettings)} not found in application configuration");

            var config = new Config(identityServerSettings);

            var identityServerBuilder = builder.Services.AddIdentityServer(x =>
            {
                x.Logging = new Duende.IdentityServer.Configuration.LoggingOptions { AuthorizeRequestSensitiveValuesFilter = Array.Empty<string>(), BackchannelAuthenticationRequestSensitiveValuesFilter = Array.Empty<string>(), TokenRequestSensitiveValuesFilter = Array.Empty<string>() };
                x.IssuerUri = "https://identity-server:443";
                x.Events.RaiseErrorEvents = true;
                x.Events.RaiseInformationEvents = true;
                x.Events.RaiseFailureEvents = true;
                x.Events.RaiseSuccessEvents = true;
            });

            identityServerBuilder
             .AddInMemoryApiScopes(config.ApiScopes)
             .AddInMemoryClients(config.Clients);

            // not recommended for production - you need to store your key material somewhere secure
            //identityServerBuilder.AddDeveloperSigningCredential();

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
            //app.UseStaticFiles();
            //app.UseRouting();

            app.UseIdentityServer();

            // uncomment if you want to add a UI
            //app.UseAuthorization();
            //app.MapRazorPages().RequireAuthorization();

            return app;
        }
    }
}