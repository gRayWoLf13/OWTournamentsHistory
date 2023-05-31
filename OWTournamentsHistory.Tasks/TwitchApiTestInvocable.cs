using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OWTournamentsHistory.Common.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.HttpCallHandlers;
using TwitchLib.Api.Core.RateLimiter;

namespace OWTournamentsHistory.Tasks
{
    public class TwitchApiTestInvocable : BaseInvocable<TwitchApiTestInvocable>
    {
        private readonly ILogger<TwitchHttpClient> _twitchLogger;
        private readonly TwitchApiSettings _apiSettings;

        public TwitchApiTestInvocable(ILogger<TwitchApiTestInvocable> logger, ILogger<TwitchHttpClient> twitchLogger, IOptions<TwitchApiSettings> apiSettings) 
            : base(logger)
        {
            _twitchLogger = twitchLogger;
            _apiSettings = apiSettings.Value;
        }



        //OAUTH2 flow:
        //generate link for user to authenticate an app by using GetAuthorizationCodeUrl with our redirect_url (includes app id and scopes)
        //redirect user to a generated link
        //twitch checks app permissions and redirects back to redirect_url passing authorization_code as query parameter
        //app needs to have a valid redirect url (backend controller) that can accept authorization_code
        //app generates an access token using GetAccessTokenFromCodeAsync method
        //token lasts for ~13000 seconds
        //profit!

        private async Task GetAccessToken()
        {
            var scopes = new List<TwitchLib.Api.Core.Enums.AuthScopes>()
            {
                TwitchLib.Api.Core.Enums.AuthScopes.Channel_Check_Subscription,
                TwitchLib.Api.Core.Enums.AuthScopes.Helix_User_Read_Email,
                TwitchLib.Api.Core.Enums.AuthScopes.Helix_Channel_Read_Subscriptions,
                TwitchLib.Api.Core.Enums.AuthScopes.Helix_User_Read_Subscriptions,
                TwitchLib.Api.Core.Enums.AuthScopes.Helix_User_Edit,
            };

            var rateLimiter = TimeLimiter.GetFromMaxCountByInterval(10, TimeSpan.FromSeconds(10));

            var httpClient = new TwitchHttpClient(_twitchLogger);

            var auth = new TwitchLib.Api.Auth.Auth(new ApiSettings { ClientId = _apiSettings.ClientId, Secret = _apiSettings.ClientSecret, Scopes = scopes }, rateLimiter, httpClient);

            //var authCodeUrl = auth.GetAuthorizationCodeUrl(_redirectUri, scopes, clientId: _clientId);
            //Debug.WriteLine(authCodeUrl);
            //var accessTokenResult = await auth.GetAccessTokenFromCodeAsync(authCode, clientId: _clientId, clientSecret: _clientSecret, redirectUri: _redirectUri);
        }

        protected override async Task InvokeInternal()
        {
            await GetAccessToken();
            var scopes = new List<TwitchLib.Api.Core.Enums.AuthScopes>()
            {
                //The only scope needed to check subscriptions?
                TwitchLib.Api.Core.Enums.AuthScopes.Helix_User_Read_Subscriptions,


                TwitchLib.Api.Core.Enums.AuthScopes.Channel_Check_Subscription,
                TwitchLib.Api.Core.Enums.AuthScopes.Helix_User_Read_Email,
                TwitchLib.Api.Core.Enums.AuthScopes.Helix_Channel_Read_Subscriptions,
                TwitchLib.Api.Core.Enums.AuthScopes.Helix_User_Edit,
            };

            var rateLimiter = TimeLimiter.GetFromMaxCountByInterval(10, TimeSpan.FromSeconds(10));

            var httpClient = new TwitchHttpClient(_twitchLogger);

            var auth = new TwitchLib.Api.Auth.Auth(new ApiSettings { ClientId = _apiSettings.ClientId, Secret = _apiSettings.ClientSecret, Scopes = scopes }, rateLimiter, httpClient);

          

            // var authCodeUrl = auth.GetAuthorizationCodeUrl(_redirectUri, scopes, clientId: _clientId);
            //var accessTokenResult = await auth.GetAccessTokenFromCodeAsync(authCode, clientId: _clientId, clientSecret: _clientSecret, redirectUri: _redirectUri);

            var accessToken = "TODO";
            //var accessToken = accessTokenResult.AccessToken;

            var api = new TwitchAPI();
            api.Settings.ClientId = _apiSettings.ClientId;
            api.Settings.Scopes = scopes;
            api.Settings.Secret = _apiSettings.ClientSecret;
            api.Settings.AccessToken = accessToken;

            var validation = await auth.ValidateAccessTokenAsync(accessToken);


            //var accessToken = await api.Auth.GetAccessTokenAsync();

            //var token = await api.Auth.GetAccessTokenFromCodeAsync(authCode, _clientSecret, "http://localhost", _clientId);

            //var authCodeUrl = api.Auth.GetAuthorizationCodeUrl("http://localhost", scopes, clientId: _clientId);

            //var ids = await api.Helix.Users.GetUsersAsync(logins: new List<string>() { "User name", "Broadcaster name" });
            //var userFollows = await api.Helix.Users.GetUsersFollowsAsync( fromId: ids.Users.First().Id);


            //try
            //{
            //  //  var channelSubscribers = await api.Helix.Subscriptions.GetUserSubscriptionsAsync(broadcasterId: ids.Users.Last().Id, userIds: new List<string> { ids.Users.First().Id });
            //    var channelSubscribers = await api.Helix.Subscriptions.GetUserSubscriptionsAsync(broadcasterId: ids.Users.First().Id, userIds: new List<string> { ids.Users.First().Id });

            //}
            //catch (Exception ex)
            //{

            //}

            try
            {
                 await api.Helix.Users.UpdateUserAsync("Description update test");

            }
            catch (Exception ex)
            {

            }
        }
    }
}
