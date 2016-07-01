using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Bot.Sample.PizzaBot
{
	public class Command
	{
		public int index { get; set; }
		public GardenObject gardenObject { get; set; }

		public Command(int index, GardenObject go)
		{
			index = index;
			gardenObject = go;
		}
	}
}