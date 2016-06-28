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
	class GardenDialog : LuisDialog<GardenObjects>
	{
		private readonly List<GardenObjects> ExistingObjects = new List<GardenObjects>();

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
			addEntityToList(entities);
			await context.PostAsync("Auf dem Bild sind: " + answer());
			context.Wait(MessageReceived);
		}

		[LuisIntent("MoveActivity")]
		public async Task MoveObject(IDialogContext context, LuisResult result)
		{
			var entities = new List<EntityRecommendation>(result.Entities);
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
			await context.PostAsync("Ich verändere " + answer());
			context.Wait(MessageReceived);
		}

		private void addEntityToList(List<EntityRecommendation> entities)
		{
			var gardenObject = new GardenObjects
			{
				Characteristics = new List<string>()
			};

			foreach (var entity in entities)
			{
				switch (entity.Type)
				{
					case "Object":
						gardenObject.Object = entity.Entity;
						break;
					case "Property":
						gardenObject.Characteristics.Add(entity.Entity);
						break;
					case "Amount":
						int a = 1;
						int.TryParse(entity.Entity, out a);
						gardenObject.Amount = a;
						break;
				}
			}
			ExistingObjects.Add(gardenObject);
		}

		private void removeEntityFromList(List<EntityRecommendation> entities)
		{
			var gardenObject = new GardenObjects
			{
				Characteristics = new List<string>()
			};

			foreach (var entity in entities)
			{
				switch (entity.Type)
				{
					case "Object":
						gardenObject.Object = entity.Entity;
						break;
					case "Property":
						gardenObject.Characteristics.Add(entity.Entity);
						break;
					case "Amount":
						int a = 1;
						int.TryParse(entity.Entity, out a);
						gardenObject.Amount = a;
						break;
				}
			}

			if (gardenObject.Object == null)
			{
			}
			else if (gardenObject.Object.Contains("all"))
			{
				ExistingObjects.Clear();
			}
			else
			{
				for (int i = ExistingObjects.Count - 1; i >= 0; i--)
				{
					if (ExistingObjects[i].Object.Contains(gardenObject.Object) ||
						gardenObject.Object.Contains(ExistingObjects[i].Object))
					{
						ExistingObjects[i].Amount -= gardenObject.Amount;
					}
					if (ExistingObjects[i].Amount < 1)
					{
						ExistingObjects.RemoveAt(i);
					}
				}
			}
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
				result = entity.Characteristics.Aggregate(result, (current, c) => current + (c + " "));
				result += entity.Object;
			}

			return result;
		}
	}
}