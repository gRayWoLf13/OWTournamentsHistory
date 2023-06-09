﻿using AutoMapper;
using OWTournamentsHistory.Api.MappingProfiles;
using OWTournamentsHistory.Common.Settings;

namespace OWTournamentsHistory.Api.DI
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServices(this IServiceCollection serviceCollection)
        {

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
            });

            services.AddSingleton(mapperConfig.CreateMapper());
        }
    }
}
