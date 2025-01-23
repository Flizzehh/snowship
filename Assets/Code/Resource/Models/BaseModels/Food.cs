using System.Collections.Generic;

namespace Snowship.NResource
{
	public class Food : Resource
	{
		public int nutrition = 0;

		public Food(
			EResource type,
			ResourceGroup.ResourceGroupEnum groupType,
			List<ResourceClassEnum> classes,
			int weight,
			int volume,
			int price,
			int fuelEnergy,
			Dictionary<ObjectPrefabSubGroup.ObjectSubGroupEnum, List<ObjectPrefab.ObjectEnum>> craftingObjects,
			int craftingEnergy,
			int craftingTime,
			int nutrition
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
			this.nutrition = nutrition;
		}
	}
}
