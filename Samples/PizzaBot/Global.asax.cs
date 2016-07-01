using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace Microsoft.Bot.Sample.PizzaBot
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			GlobalConfiguration.Configure(WebApiConfig.Register);
			GlobalConfiguration.Configuration.Filters.Add(new AddCustomHeaderFilter());
		}

		protected void Application_BeginRequest()
		{
			if (Request.Headers.AllKeys.Contains("Origin") && Request.HttpMethod == "OPTIONS")
			{
				Response.Flush();
			}
		}
	}
}
