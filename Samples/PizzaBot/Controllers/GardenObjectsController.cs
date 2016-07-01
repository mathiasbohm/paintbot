using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Bot.Builder.Luis.Models;

namespace Microsoft.Bot.Sample.PizzaBot
{
	public class GardenObjectsController : ApiController
	{
		public string Get(string action, string item, string property, int amount, string identifier = "")
		{
			var dialog = new GardenDialog();
			List<EntityRecommendation> entities = new List<EntityRecommendation>
			{
				new EntityRecommendation
				{
					Entity = item,
					Type = "Object"
				},
				new EntityRecommendation
				{
					Entity = property,
					Type = "Property"
				},
				new EntityRecommendation
				{
					Entity = amount.ToString(),
					Type = "Amount"
				}
			};
			if (!string.IsNullOrWhiteSpace(identifier))
			{
				entities.Add(new EntityRecommendation
				{
					Entity = identifier,
					Type = "Identifier"
				});
			}

			if (action == "create")
			{
				dialog.createEntityToList(entities);
			}
			else if (action == "move")
			{
				dialog.moveEntity(entities);
			}
			else if (action == "delete")
			{
				dialog.removeEntityFromList(entities);
			}
			return GardenDialog.CurrentGardenObject != null 
				? GardenDialog.CurrentGardenObject.Identifier
				: "null";
		}
	}
}
