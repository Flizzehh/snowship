using System.Collections.Generic;
using JetBrains.Annotations;
using Snowship.NResource;
using UnityEngine.UI;

namespace Snowship.NUI
{
	[UsedImplicitly]
	public class UIActionsPanelPresenter : UIPresenter<UIActionsPanelView>
	{
		private readonly List<ITreeButton> childButtons = new();

		public UIActionsPanelPresenter(UIActionsPanelView view) : base(view) {
		}

		public override void OnCreate() {
			CreateButtons();
		}

		public override void OnClose() {

		}

		private void CreateButtons() {
			foreach (UIActionButtonData buttonData in View.ButtonsData.buttons) {
				UIActionGroupButtonElement button = new(View.ButtonsLayoutGroup.transform, buttonData.buttonName, buttonData.buttonSprite);
				button.OnButtonClicked += () => SetButtonChildrenActive(button);
				childButtons.Add(button);

				switch (buttonData.buttonName) {
					case "Build":
						CreateBuildButtons(button);
						break;
					case "Terraform":
						break;
					case "Farm":
						break;
					case "Remove":
						break;
				}
			}
		}

		private void SetButtonChildrenActive(ITreeButton buttonToBeActive) {
			foreach (ITreeButton childButton in childButtons) {
				childButton.SetChildElementsActive(childButton == buttonToBeActive && !buttonToBeActive.ChildElementsActiveState);
			}
		}

		private void CreateBuildButtons(UIActionGroupButtonElement button) {
			foreach (ObjectPrefabGroup group in ObjectPrefabGroup.GetObjectPrefabGroups()) {
				if (group.type is ObjectPrefabGroup.ObjectGroupEnum.None or ObjectPrefabGroup.ObjectGroupEnum.Command or ObjectPrefabGroup.ObjectGroupEnum.Farm or ObjectPrefabGroup.ObjectGroupEnum.Terraform) {
					continue;
				}

				UIActionGroupButtonElement groupButton = button.AddChildGroupButton(group.name, null);
				groupButton.SetChildElementsActive(false);
				groupButton.OnButtonClicked += () => button.SetChildSiblingChildElementsActive(groupButton);

				foreach (ObjectPrefabSubGroup subGroup in group.subGroups) {
					UIActionGroupButtonElement subGroupButton = groupButton.AddChildGroupButton(subGroup.name, null);
					subGroupButton.SetChildElementsActive(false);
					subGroupButton.OnButtonClicked += () => groupButton.SetChildSiblingChildElementsActive(subGroupButton);

					foreach (ObjectPrefab prefab in subGroup.prefabs) {
						UIActionItemButtonElement prefabButton = subGroupButton.AddChildItemButton(prefab.name, prefab.GetBaseSpriteForVariation(prefab.lastSelectedVariation));
						prefabButton.SetChildElementsActive(false);

						foreach (Variation variation in prefab.variations) {
							prefabButton.AddVariation(prefab, variation);
						}
					}
				}
			}
		}
	}
}