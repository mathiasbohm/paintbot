using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Luis.Models;

namespace Microsoft.Bot.Sample.PizzaBot
{
	[LuisModel("4ed5a5de-6461-4f62-8724-e3953282ab1d", "df42d05539ff446db4dcba16c2ac3a05")]
	[Serializable]
	class GardenDialog : LuisDialog<GardenObject>
	{
		private static List<GardenObject> ExistingObjects = new List<GardenObject>();
		public static GardenObject CurrentGardenObject;
		public static List<Command> CommandStack = new List<Command>();

		private Dictionary<string, List<string>> TagList = new Dictionary<string, List<string>>
		{
			{"baum", new List<string>{"baum", "bäume", "kirschbaum","kirschbäume",  "apfelbaum", "apfelbäume", "eiche", "eichen", "tanne", "tannen", "linde", "linden", "birnbaum", "birnbäume"}},  
			{"apfel", new List<string>{"apfel","äpfel", "boskop", "braeburn", "elstar", "gala", "rot", "grün"}}, 
			{"eis", new List<string>{"eis", "vanille", "schoko", "erdbeere", "kirsche", "zitrone", "haselnuss", "magnum", "softeis"}},
			{"kanne", new List<string>{"kanne", "kannen", "teekanne", "teekannen", "plastikkanne", "plastikkannen", "glaskanne", "glaskannen", "kaffeekanne", "kaffeekannen"}},
			{"grill", new List<string>{"grill", "grille", "grillrost", "grillroste", "kühlergrill", "kühlergrille", "grillen"}},
			{"gras", new List<string>{"gras", "gräser", "grüngras", "grüngräser"}},
			{"hase", new List<string>{"hase", "hasen"}},
			{"igel", new List<string>{"igel"}},
			{"kirsche", new List<string>{"kirsche", "kirschen", "sauerkirsche", "sauerkirschen", "süsskirsche", "süsskirschen"}},
			{"kreis", new List<string>{"kreis", "kreise", "oval", "unrund", "eiförmig"}},
			{"kreuz", new List<string>{"kreuz", "kreuze"}},
			{"vertikal", new List<string>{"vertikal", "senkrecht"}},
			{"horizontal", new List<string>{"horizontal", "waagerecht"}},
			{"raupe", new List<string>{"raupe", "raupen"}},
			{"regen", new List<string>{"regen", "regentropfen", "starkregen", "wasser"}},
			{"rose", new List<string>{"rose", "rosen", "stachel", "stacheln"}},
			{"vogel", new List<string>{"vogel", "vögel", "zaunkönig", "zaukönige", "spatz", "spatze", "krähe", "krähen", "raabe", "raaben"}},
			{"tomate", new List<string>{"tomate", "tomaten", "cherrytomate", "cherrytomaten", "strauchtomate", "strauchtomaten", "romatomate", "romatomaten"}},
			{"tanne", new List<string>{"tanne", "tannen"}},
			{"brunnen", new List<string>{"brunnen", "rundbrunnen"}},
			{"sonne", new List<string>{"sonne", "sonnen", "sonennschein", "sonnenscheine", "sonnenbrille", "sonnenbrillen"}},
			{"schmetterling", new List<string>{"schmetterling", "schmetterlinge", "tagfalter"}},
			{"schippe", new List<string>{"schippe", "schippen", "gartengeräte", "gartengerät"}},
			{"schaf", new List<string>{"schaf", "schafe", "tier", "tiere", "herde", "herden"}},
			{"wolke", new List<string>{"wolke", "wolken", "unwetter", "regen"}},
			{"karotte", new List<string>{"karotte", "möhre", "möhren", "karotten"}}		
		};
			
		[LuisIntent("")]
		public async Task None(IDialogContext context, LuisResult result)
		{
			await context.PostAsync("Das habe ich nicht verstanden");
			context.Wait(MessageReceived);
		}

		[LuisIntent("CreateActivity")]
		public async Task CreateObject(IDialogContext context, LuisResult result)
		{
			var entities = new List<EntityRecommendation>(result.Entities);
			createEntityToList(entities);
			await context.PostAsync("Auf dem Bild sind: " + answer());
			context.Wait(MessageReceived);
		}

		[LuisIntent("MoveActivity")]
		public async Task MoveObject(IDialogContext context, LuisResult result)
		{
			var entities = new List<EntityRecommendation>(result.Entities);
			moveEntity(entities);
			await context.PostAsync("Ich verschiebe " + answer());
			context.Wait(MessageReceived);
		}

		[LuisIntent("DeleteActivity")]
		public async Task DeleteObject(IDialogContext context, LuisResult result)
		{
			var entities = new List<EntityRecommendation>(result.Entities);
			removeEntityFromList(entities);
			await context.PostAsync("Auf dem Bild sind: " + answer());
			context.Wait(MessageReceived);
		}

		[LuisIntent("TransformActivity")]
		public async Task TransformObject(IDialogContext context, LuisResult result)
		{
			var entities = new List<EntityRecommendation>(result.Entities);
			transformEntity(entities);
			await context.PostAsync("Ich verändere " + answer());
			context.Wait(MessageReceived);
		}

		public void moveEntity(List<EntityRecommendation> entities)
		{
			var gardenObject = createGardenObject(entities);
			CurrentGardenObject = findAndReplaceGardenObject(gardenObject);
			CurrentGardenObject = moveGardenObject(CurrentGardenObject);
			sendCommand(CurrentGardenObject);
		}

		private GardenObject moveGardenObject(GardenObject CurrentGardenObject)
		{
			for (int i = CurrentGardenObject.MetaInfos.Count - 1; i >= 0; i--)
			{
				var info = CurrentGardenObject.MetaInfos[i];
				if (info == "links")
				{
					var tmp = CurrentGardenObject.Position;
					CurrentGardenObject.Position = new Tuple<double, double, double>(tmp.Item1 / 4, tmp.Item2, tmp.Item3);
					CurrentGardenObject.MetaInfos.RemoveAt(i);
				}
				if (info == "rechts")
				{
					var tmp = CurrentGardenObject.Position;
					CurrentGardenObject.Position = new Tuple<double, double, double>(1 - ((1 - tmp.Item1) / 4), tmp.Item2, tmp.Item3);
					CurrentGardenObject.MetaInfos.RemoveAt(i);
				}

				if (info == "oben")
				{
					var tmp = CurrentGardenObject.Position;
					CurrentGardenObject.Position = new Tuple<double, double, double>(tmp.Item1, tmp.Item2 / 4, tmp.Item3);
					CurrentGardenObject.MetaInfos.RemoveAt(i);
				}
				if (info == "unten")
				{
					var tmp = CurrentGardenObject.Position;
					CurrentGardenObject.Position = new Tuple<double, double, double>(tmp.Item1, 1 - ((1 - tmp.Item2) / 4), tmp.Item3);
					CurrentGardenObject.MetaInfos.RemoveAt(i);
				}
			}
			return CurrentGardenObject;
		}

		private void transformEntity(List<EntityRecommendation> entities)
		{
			var gardenObject = createGardenObject(entities);
			CurrentGardenObject = findAndReplaceGardenObject(gardenObject);
			CurrentGardenObject = transformGardenObject(CurrentGardenObject);
			sendCommand(CurrentGardenObject);
		}

		private GardenObject transformGardenObject(GardenObject CurrentGardenObject)
		{
			foreach (var info in CurrentGardenObject.MetaInfos)
			{
				if (info.Contains("klein"))
				{
					var tmp = CurrentGardenObject.Scale;
					CurrentGardenObject.Scale = new Tuple<double, double>(tmp.Item1 * 0.8, tmp.Item2 * 0.8);
				}
				if (info.Contains("größer"))
				{
					var tmp = CurrentGardenObject.Scale;
					CurrentGardenObject.Scale = new Tuple<double, double>(tmp.Item1 * 1.2, tmp.Item2 * 0.8);
				}
			}
			return CurrentGardenObject;
		}

		public void createEntityToList(List<EntityRecommendation> entities)
		{
			var gardenObject = createGardenObject(entities);
			ExistingObjects.Add(gardenObject);
			CurrentGardenObject = gardenObject;
			sendCommand(CurrentGardenObject);
		}

		private GardenObject findAndReplaceGardenObject(GardenObject gardenObject)
		{
			var meta = gardenObject.MetaInfos;
			foreach (var existingObject in ExistingObjects)
			{
				if (existingObject.Identifier == gardenObject.Identifier)
				{
					gardenObject = existingObject;
					gardenObject.MetaInfos.AddRange(meta);
				}
				else if (existingObject.Tagname == gardenObject.Tagname)
				{
					gardenObject = existingObject;
					gardenObject.MetaInfos.AddRange(meta);
				}
			}
			return gardenObject;
		}

		public void removeEntityFromList(List<EntityRecommendation> entities)
		{
			CurrentGardenObject = createGardenObject(entities);

			if (CurrentGardenObject.Tagname != null && CurrentGardenObject.Tagname.Contains("all"))
			{
				ExistingObjects.Clear();
				CommandStack.Clear();
			}
			else
			{
				CurrentGardenObject = findAndReplaceGardenObject(CurrentGardenObject);
				for (int i = ExistingObjects.Count - 1; i >= 0; i--)
				{
					if (ExistingObjects[i].Tagname.Contains(CurrentGardenObject.Tagname) ||
						CurrentGardenObject.Tagname.Contains(ExistingObjects[i].Tagname))
					{
						ExistingObjects[i].Amount -= CurrentGardenObject.Amount;
					}
					if (ExistingObjects[i].Amount < 1)
					{
						ExistingObjects.RemoveAt(i);
					}
				}
			}
			sendCommand(CurrentGardenObject);
			CurrentGardenObject = null;
		}

		private GardenObject createGardenObject(List<EntityRecommendation> entities)
		{
			var gardenObject = new GardenObject();

			foreach (var entity in entities)
			{
				switch (entity.Type)
				{
					case "Object":
						gardenObject.Tagname = tagOfEntity(entity.Entity);
						break;
					case "Identifier":
						gardenObject.Identifier = entity.Entity;
						break;
					case "Property":
						gardenObject.MetaInfos.Add(entity.Entity);
						break;
					case "Amount":
						int a = 1;
						int.TryParse(entity.Entity, out a);
						gardenObject.Amount = a;
						break;
				}
			}
			return gardenObject;
		}

		private string tagOfEntity(string tag)
		{
			string key = "unknowen";//tag;
			foreach (var kvp in TagList.Keys)
			{
				var tmp = TagList[kvp];
				if (tmp.Contains(tag))
				{
					key = kvp;
				}
			}
			return key;
		}

		private void sendCommand(GardenObject CurrentGardenObject)
		{
			var cmd = new Command(CommandStack.Count + 1, CurrentGardenObject);		
	
			CommandStack.Add(cmd);
		}

		private string answer()
		{
			string result = "";

			bool first = true;
			foreach (var entity in ExistingObjects)
			{
				if (!first)
				{
					result += " und ";
				}
				else
				{
					first = false;
				}

				result += entity.Amount + " ";
				result = entity.MetaInfos.Aggregate(result, (current, c) => current + (c + " "));
				result += entity.Identifier;
			}

			return result;
		}
	}
}