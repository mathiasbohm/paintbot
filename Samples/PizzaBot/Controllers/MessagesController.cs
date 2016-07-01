using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Http.Cors;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Sample.PizzaBot
{
    [BotAuthentication]
	[EnableCors("http://localhost:8080", "*", "*")]
    public class MessagesController : ApiController
    {
		private static IForm<GardenObject> BuildForm()
        {
			var builder = new FormBuilder<GardenObject>();

            return builder
                .Build()
                ;
        }

		internal static IDialog<GardenObject> MakeRoot()
        {
			return Chain.From(() => new GardenDialog());
        }

		/// <summary>
		/// POST: api/Messages
		/// receive a message from a user and reply to it
		/// </summary>
		public async Task<Message> Get(string messageString)
		{
			var path = HttpContext.Current.Request.Url.LocalPath.ToLower();
			var query = HttpContext.Current.Request.Url.Query;
			var headers = (HttpContext.Current.Request.Headers.AllKeys).Select(h => h.ToLower()).ToList();

			Message message = new Message();
			message.Text = messageString;

			return await Conversation.SendAsync(message, MakeRoot);
		}

		/// <summary>
		/// POST: api/Messages
		/// receive a message from a user and reply to it
		/// </summary>
		public async Task<Message> Post([FromBody]Message message)
		{
			var path = HttpContext.Current.Request.Url.LocalPath.ToLower();
			var query = HttpContext.Current.Request.Url.Query;
			var headers = (HttpContext.Current.Request.Headers.AllKeys).Select(h => h.ToLower()).ToList();


			return await Conversation.SendAsync(message, MakeRoot);
		}
	}
}