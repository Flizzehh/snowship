using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Menu.LoadSave {

	internal class CraftableResourceElement : PriorityResourceElement {

		public CraftableResourceElement(
			Type type,
			ResourceManager.Resource resource,
			Transform parent,
			ResourceManager.CraftingObject craftingObject
		) : base(
			type,
			resource,
			parent,
			craftingObject
		) {

		}

		public override void Update() {
			base.Update();

			panel.transform.Find("ActiveIndicator-Panel").GetComponent<Image>().color =
				craftingObject.GetCraftableResourceFromResource(resource) != null
					? (resource.GetAvailableAmount() > 0
						? ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen)
						: ColourUtilities.GetColour(ColourUtilities.EColour.LightOrange))
					: ColourUtilities.GetColour(ColourUtilities.EColour.Clear);
		}

		public override void ChangePriority(int amount) {

			ResourceManager.CraftableResourceInstance craftableResource = craftingObject.GetCraftableResourceFromResource(resource);
			if (craftableResource == null) {
				craftableResource = craftingObject.ToggleResource(resource, 0);
			}

			int priority = craftableResource.priority.Change(amount);

			if (priority <= craftableResource.priority.min && craftableResource != null) {
				craftingObject.ToggleResource(resource, 0);
			}

			UpdatePriorityButtonText(priority);

			Update();
		}
	}
}
