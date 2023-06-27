using AutoMapper;
using OWTournamentsHistory.Api.GrpcServices;
using OWTournamentsHistory.Api.MappingProfiles;
using OWTournamentsHistory.Api.MappingProfiles.Grpc;
using OWTournamentsHistory.Api.Services;
using OWTournamentsHistory.Common.Settings;

namespace OWTournamentsHistory.Api.DI
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<StatisticsService>();
            serviceCollection.AddScoped<TeamsService>();
            serviceCollection.AddScoped<MatchesService>();
        }

        public static void AddGrpcServices(this WebApplication? app)
        {
            app!.MapGrpcService<StatisticsHandlerService>();
            app!.MapGrpcService<TeamsHandlerService>();
            app!.MapGrpcService<MatchesHandlerService>();
        }

        public static void AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var authenticationConfiguration = configuration
                .GetRequiredSection(nameof(ApplicationAuthenticationSettings))
                .Get<ApplicationAuthenticationSettings>()
                ?? throw new Exception($"{nameof(ApplicationAuthenticationSettings)} not found in application configuration");

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = authenticationConfiguration.AuthenticationAuthority;

                    options.TokenValidationParameters = new()
                    {
                        ValidateAudience = false,
                    };

                    var handler = new HttpClientHandler
                    {
                        ClientCertificateOptions = ClientCertificateOption.Manual,
                        ServerCertificateCustomValidationCallback =
                        (httpRequestMessage, cert, cetChain, policyErrors) =>
                            (cert?.Issuer) == authenticationConfiguration.RootCertificateIssuer
                            && (cert?.MatchesHostname(httpRequestMessage?.RequestUri?.Host ?? string.Empty) ?? false)
                    };
                    options.BackchannelHttpHandler = handler;
                });
        }

        public static void AddAutoMapper(this IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new PlayerMappingProfile());
                mc.AddProfile(new TeamMappingProfile());
                mc.AddProfile(new MatchMappingProfile());
                mc.AddProfile(new StatisticsMappingProfile());
                mc.AddProfile(new TeamsMappingProfile());
                mc.AddProfile(new MatchesMappingProfile());
            });

            services.AddSingleton(mapperConfig.CreateMapper());
        }
    }
}
