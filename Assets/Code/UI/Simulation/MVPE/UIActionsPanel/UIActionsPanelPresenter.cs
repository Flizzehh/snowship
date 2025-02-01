using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Snowship.NJob;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NUI
{
	[UsedImplicitly]
	public class UIActionsPanelPresenter : UIPresenter<UIActionsPanelView>
	{
		private readonly List<ITreeButton> childButtons = new();

		public UIActionsPanelPresenter(UIActionsPanelView view) : base(view) {
			CreateButtons();
		}

		public override void OnCreate() {
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
						CreateTerraformButtons(button);
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

		private void CreateGroupButtons(ITreeButton parentButton, IGroupItem baseGroupItem, Action<ITreeButton, IGroupItem> handleLastChildrenAction) {
			CreateGroupButtons(parentButton, new List<IGroupItem> { baseGroupItem }, handleLastChildrenAction);
		}

		private void CreateGroupButtons(ITreeButton parentButton, List<IGroupItem> baseGroupItems, Action<ITreeButton, IGroupItem> handleLastChildrenAction) {

			foreach (IGroupItem group in baseGroupItems) {

				ITreeButton groupButton;
				if (baseGroupItems.Count == 1) {
					groupButton = parentButton;
				} else {
					groupButton = ((UIActionGroupButtonElement)parentButton).AddChildGroupButton(group.Name, group.Icon);
					groupButton.SetChildElementsActive(false);
					groupButton.OnButtonClicked += () => parentButton.SetChildSiblingChildElementsActive(groupButton);
				}

				foreach (IGroupItem subGroup in group.Children) {
					ITreeButton subGroupButton;
					if (group.Children.Count == 1) {
						subGroupButton = groupButton;
					} else {
						subGroupButton = ((UIActionGroupButtonElement)groupButton).AddChildGroupButton(subGroup.Name, subGroup.Icon);
						subGroupButton.SetChildElementsActive(false);
						subGroupButton.OnButtonClicked += () => groupButton.SetChildSiblingChildElementsActive(subGroupButton);
					}

					foreach (IGroupItem prefab in subGroup.Children) {
						UIActionItemButtonElement prefabButton = ((UIActionGroupButtonElement)subGroupButton).AddChildItemButton(prefab.Name, prefab.Icon);
						prefabButton.SetChildElementsActive(false);

						handleLastChildrenAction(prefabButton, prefab);
					}
				}
			}
		}

		// Build Buttons

		private void CreateBuildButtons(ITreeButton button) {
			List<IGroupItem> groups = ObjectPrefabGroup.GetObjectPrefabGroups()
				.Where(group => group.type is not (ObjectPrefabGroup.ObjectGroupEnum.None or ObjectPrefabGroup.ObjectGroupEnum.Command or ObjectPrefabGroup.ObjectGroupEnum.Farm or ObjectPrefabGroup.ObjectGroupEnum.Terraform))
				.Cast<IGroupItem>()
				.ToList();

			CreateGroupButtons(button, groups, SetupIndividualBuildButton);
		}

		private void SetupIndividualBuildButton(ITreeButton button, IGroupItem item) {

			if (button is not UIActionItemButtonElement prefabButton) {
				return;
			}
			if (item is not ObjectPrefab prefab) {
				return;
			}

			Debug.Log($"Create Build Button for {prefab.Name}");
			button.OnButtonClicked += () => Debug.Log($"Build Button for {prefab.Name} clicked");
			// TODO OnButtonClick > SelectionManager > SelectObject(?) > Selection Area > Build Jobs

			foreach (Variation variation in prefab.variations) {
				UIActionItemButtonElement variationButton = prefabButton.AddVariation(prefab, variation);
				variationButton.OnButtonClicked += () => prefabButton.SetVariation(prefab, variation, variationButton);
			}
		}

		// Terraform Buttons

		private void CreateTerraformButtons(ITreeButton button) {
			JobGroup group = GameManager.Get<JobManager>().JobRegistry.GetJobGroup("Terraform");

			CreateGroupButtons(button, group, SetupIndividualTerraformButton);
		}

		private void SetupIndividualTerraformButton(ITreeButton button, IGroupItem item) {
			if (button is not UIActionItemButtonElement jobButton) {
				return;
			}
			if (item is not JobTypeData job) {
				return;
			}

			Debug.Log($"Create Terraform Button for {job.Name}");
			button.OnButtonClicked += () => Debug.Log($"Terraform Button for {job.Name} clicked");
		}
	}
}