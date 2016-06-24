using System.Web.Http;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Sample.PizzaBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
		private static IForm<GardenObjects> BuildForm()
        {
			var builder = new FormBuilder<GardenObjects>();

            return builder
                .Build()
                ;
        }

		internal static IDialog<GardenObjects> MakeRoot()
        {
			return Chain.From(() => new GardenDialog());
        }

        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            return await Conversation.SendAsync(message, MakeRoot);
        }
    }
}