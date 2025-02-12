using Snowship.NResource;
using Snowship.NUtilities;

namespace Snowship.NJob
{
	[RegisterJob("Needs", "Clothing", "WearClothes")]
	public class WearClothesJobDefinition : JobDefinition<WearClothesJob>
	{
		public WearClothesJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
			Returnable = false;
		}
	}

	public class WearClothesJob : Job<WearClothesJobDefinition>
	{
		private readonly Container container;
		private readonly Clothing clothing;

		public WearClothesJob(TileManager.Tile tile, Container container, Clothing clothing) : base(tile) {
			this.container = container;
			this.clothing = clothing;

			Description = $"Wearing {clothing.name}.";
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			container?.Inventory.TakeReservedResources(Worker);
			ResourceAmount clothingFromInventory = Worker.Inventory.TakeResourceAmount(new ResourceAmount(clothing, 1));
			if (clothingFromInventory == null) {
				return;
			}
			Worker.ChangeClothing(clothing.prefab.BodySection, clothing);
		}
	}
}