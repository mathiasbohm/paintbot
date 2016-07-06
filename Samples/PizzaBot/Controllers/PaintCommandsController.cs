using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;

namespace Microsoft.Bot.Sample.PizzaBot.Controllers
{
	public class PaintCommandsController : ApiController
	{
		// GET: api/PaintCommands
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}

		// GET: api/PaintCommands/5
		public string Get(int id)
		{
			var commands = new List<CCommand>();
			var len = GardenDialog.CommandStack.Count;

			if (len <= id)
			{
				return "";
			}

			//for (int i = id; i < len; i++)
			//{
			//	commands.Add(GardenDialog.CommandStack[id]);
			//}

			CCommand c = GardenDialog.CommandStack[id];
			GardenObject go = c.gardenObject;


			var x = JsonConvert.SerializeObject(go);
			return x;
		}
	}
}
	