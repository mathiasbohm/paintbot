using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;

namespace Microsoft.Bot.Sample.PizzaBot
{
	public class AddCustomHeaderFilter : ActionFilterAttribute
	{
		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
		}
	}
}