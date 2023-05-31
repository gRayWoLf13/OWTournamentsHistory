using Duende.IdentityServer.Models;
using OWTournamentsHistory.Common.Settings;

namespace IdentityServer
{
    public class Config
    {
        private readonly IdentityResource[] _identityResources = new[]
        {
            new IdentityResources.OpenId()
        };

        private readonly ApiScope[] _apiScopes;
        private readonly Client[] _clients;

        public Config(IdentityServerSettings serverSettings)
        {
            (_apiScopes, _clients) = InitConfig(serverSettings);
        }

        private static (ApiScope[], Client[]) InitConfig(IdentityServerSettings serverSettings)
        {
            var clients = new Client[]
            {
                new Client
                {
                    ClientId = serverSettings.AdminClientId,
                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret(serverSettings.AdminClientSecret.Sha256())
                    },
                     // scopes that client has access to
                    AllowedScopes = serverSettings.AdminScopes.Split(',')
                }
            };

            var scopes = clients
                .SelectMany(client => client.AllowedScopes)
                .Select(scope => new ApiScope(name: scope, displayName: scope))
                .ToArray();

            return (scopes, clients);
        }

        public IEnumerable<IdentityResource> IdentityResources => _identityResources;
        public IEnumerable<ApiScope> ApiScopes => _apiScopes;
        public IEnumerable<Client> Clients => _clients;
    }
}