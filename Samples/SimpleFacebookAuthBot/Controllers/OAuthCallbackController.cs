﻿using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Bot.Sample.SimpleFacebookAuthBot.Controllers
{
    public class OAuthCallbackController : ApiController
    {
        private static Lazy<string> botId = new Lazy<string>(() => ConfigurationManager.AppSettings["AppId"]);

        /// <summary>
        /// OAuth call back that is called by Facebook. Read https://developers.facebook.com/docs/facebook-login/manually-build-a-login-flow for more details.
        /// </summary>
        /// <param name="userId"> The Id for the user that is getting authenticated.</param>
        /// <param name="conversationId"> The Id of the conversation.</param>
        /// <param name="code"> The Authentication code returned by Facebook.</param>
        /// <param name="state"> The state returned by Facebook.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/OAuthCallback")]
        public async Task<HttpResponseMessage> OAuthCallback([FromUri] string userId, [FromUri] string conversationId, [FromUri] string channelId, [FromUri] string language, [FromUri] string code, [FromUri] string state)
        {
            // Get the resumption cookie
            var resumptionCookie = new ResumptionCookie(userId, botId.Value, conversationId, channelId, language);

            // Exchange the Facebook Auth code with Access token
            var token = await FacebookHelpers.ExchangeCodeForAccessToken(resumptionCookie, code, SimpleFacebookAuthDialog.FacebookOauthCallback.ToString());

            // Create the message that is send to conversation to resume the login flow
            var msg = resumptionCookie.GetMessage();
            msg.Text = $"token:{token.AccessToken}";
            
            // Resume the conversation to SimpleFacebookAuthDialog
            var reply = await Conversation.ResumeAsync(resumptionCookie, msg);

            // Remove the pending message because login flow is complete
            IBotData dataBag = new JObjectBotData(reply);
            ResumptionCookie pending;
            if (dataBag.PerUserInConversationData.TryGetValue("persistedCookie", out pending))
            {
                dataBag.PerUserInConversationData.RemoveValue("persistedCookie");

                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, reply))
                {
                    // make sure that we have the right Channel info for the outgoing message
                    var persistedCookie = pending.GetMessage();
                    reply.To = persistedCookie.From;
                    reply.From = persistedCookie.To;

                    // Send the login success asynchronously to user
                    var client = scope.Resolve<IConnectorClient>();
                    await client.Messages.SendMessageAsync(reply);
                }

                return Request.CreateResponse("You are now logged in! Continue talking to the bot.");
            }
            else
            {
                // Callback is called with no pending message as a result the login flow cannot be resumed.
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new InvalidOperationException("Cannot resume!"));
            }
        }
    }
}
