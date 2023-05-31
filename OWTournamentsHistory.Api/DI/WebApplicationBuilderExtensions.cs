using OWTournamentsHistory.Common.Settings;

namespace OWTournamentsHistory.Api.DI
{
    public static class WebApplicationBuilderExtensions
    {
        public static void AddConfigurations(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<OWTournamentsHistoryDatabaseSettings>(
                builder.Configuration.GetRequiredSection(nameof(OWTournamentsHistoryDatabaseSettings)));

            builder.Services.Configure<DropboxApiSettings>(
                builder.Configuration.GetRequiredSection(nameof(DropboxApiSettings)));

            builder.Services.Configure<TwitchApiSettings>(
                builder.Configuration.GetRequiredSection(nameof(TwitchApiSettings)));

            builder.Services.Configure<ApplicationAuthenticationSettings>(
                builder.Configuration.GetRequiredSection(nameof(ApplicationAuthenticationSettings)));
        }
    }
}
