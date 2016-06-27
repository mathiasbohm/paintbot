using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

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
            return "value " + id;
        }

    }
}
