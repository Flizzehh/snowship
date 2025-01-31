using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Needs", "Clothing", "WearClothes", false)]
	public class WearClothesJob : Job
	{
		private readonly Container container;
		private readonly Clothing clothing;

		public WearClothesJob(TileManager.Tile tile, Container container, Clothing clothing) : base(tile) {
			this.container = container;
			this.clothing = clothing;

			Description = $"Wearing {clothing.name}.";

			Returnable = false;
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