using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Text;
#pragma warning disable 649

namespace Microsoft.Bot.Sample.PizzaBot
{
	[Serializable]
	class GardenObject
	{
		public string Object { get; set; }
		public int Amount;
		public List<string> Characteristics;
		public Tuple<double, double, double> Position;
		public Tuple<double, double> Scale;
		public string Color { get; set; }

		public override string ToString()
		{
			var builder = new StringBuilder();
			return builder.ToString();
		}
	};

}