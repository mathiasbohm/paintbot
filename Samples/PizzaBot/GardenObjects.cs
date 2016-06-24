using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Text;
#pragma warning disable 649

namespace Microsoft.Bot.Sample.PizzaBot
{
	[Serializable]
	class GardenObjects
	{
		public string Object { get; set; }
		public int Amount;
		public List<string> Characteristics;

		public override string ToString()
		{
			var builder = new StringBuilder();
			return builder.ToString();
		}
	};

}