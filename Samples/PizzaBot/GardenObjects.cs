using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

#pragma warning disable 649

namespace Microsoft.Bot.Sample.PizzaBot
{
	[Serializable]
	public class GardenObject
	{
		public static int Ebene  { get; set; }

		public string Identifier { get; set; }
		public string Tagname { get; set; }
		public int Amount { get; set; }
		public List<string> MetaInfos { get; set; }
		public Tuple<double, double, double> Position { get; set; }
		public Tuple<double, double> Scale { get; set; }
		public string Color { get; set; }

		public GardenObject()
		{
			Amount = 1;
			Position = new Tuple<double, double, double>(0.5, 0.5, Ebene++);
			Scale = new Tuple<double, double>(1, 1);
			Color = "#FFFFFF";
			MetaInfos = new List<string>();
			Identifier = Guid.NewGuid().ToString();
			Tagname = "Kind";
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			return builder.ToString();
		}
	};

}