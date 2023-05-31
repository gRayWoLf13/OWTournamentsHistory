using OWTournamentsHistory.Common.Settings;

namespace IdentityServer.DI
{
    public static class WebApplicationBuilderExtensions
    {
        public static void AddConfigurations(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<IdentityServerSettings>(
                builder.Configuration.GetRequiredSection(nameof(IdentityServerSettings)));
        }
    }
}
