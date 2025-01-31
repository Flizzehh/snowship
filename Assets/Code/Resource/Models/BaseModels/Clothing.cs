using System.Collections.Generic;
using System.Linq;
using Snowship.NHuman;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NResource
{
	public class Clothing : Resource
	{
		public ClothingPrefab prefab;
		public string colour;

		public List<Sprite> moveSprites = new();

		public Clothing(
			EResource type,
			ResourceGroup.ResourceGroupEnum groupType,
			HashSet<ResourceClassEnum> classes,
			int weight,
			int volume,
			int price,
			int fuelEnergy,
			Dictionary<ObjectPrefabSubGroup.ObjectSubGroupEnum, List<ObjectPrefab.ObjectEnum>> craftingObjects,
			int craftingEnergy,
			int craftingTime,
			ClothingPrefab prefab,
			string colour
		) : base(
			type,
			groupType,
			classes,
			weight,
			volume,
			price,
			fuelEnergy,
			craftingObjects,
			craftingEnergy,
			craftingTime
		) {
			this.prefab = prefab;
			this.colour = colour;

			name = StringUtilities.SplitByCapitals(colour + prefab.clothingType);

			for (int i = 0; i < 4; i++) {
				//moveSprites.Add(prefab.moveSprites[i][prefab.colours.IndexOf(colour)]); // TODO Fix when adding ResourceInstance
				moveSprites.Add(prefab.moveSprites[i][0]);
			}

			if (prefab.BodySection == BodySection.Backpack) {
				image = moveSprites[1];
			} else {
				image = moveSprites[0];
			}
		}

		public static List<Clothing> GetClothesByAppearance(BodySection bodySection) {
			return GetResourcesInClass(ResourceClassEnum.Clothing).Select(r => (Clothing)r).Where(c => c.prefab.BodySection == bodySection).ToList();
		}

		public enum ClothingEnum
		{
			Sweater, Shirt, SmallBackpack
		}
	}
}