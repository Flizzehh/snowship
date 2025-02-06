using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NJob;
using Snowship.NResource;
using Snowship.NUtilities;
using Snowship.Selectable;
using UnityEngine;

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
				UIActionGroupButtonElement button = new(buttonData.buttonName, buttonData.buttonSprite);
				button.Open(View.ButtonsLayoutGroup.transform).Forget();
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
						CreateFarmButtons(button);
						break;
					case "Remove":
						CreateRemoveButtons(button);
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
			CreateGroupButtons(parentButton, new List<IGroupItem> { baseGroupItem }, handleLastChildrenAction).Forget();
		}

		private async UniTaskVoid CreateGroupButtons(ITreeButton parentButton, List<IGroupItem> baseGroupItems, Action<ITreeButton, IGroupItem> handleLastChildrenAction) {

			foreach (IGroupItem group in baseGroupItems) {

				ITreeButton groupButton;
				if (baseGroupItems.Count == 1) {
					groupButton = parentButton;
				} else {
					groupButton = await ((UIActionGroupButtonElement)parentButton).AddChildGroupButton(group.Name, group.Icon);
					groupButton.SetChildElementsActive(false);
					groupButton.OnButtonClicked += () => parentButton.SetChildSiblingChildElementsActive(groupButton);
				}

				foreach (IGroupItem subGroup in group.Children) {
					ITreeButton subGroupButton;
					if (group.Children.Count == 1) {
						subGroupButton = groupButton;
					} else {
						subGroupButton = await ((UIActionGroupButtonElement)groupButton).AddChildGroupButton(subGroup.Name, subGroup.Icon);
						subGroupButton.SetChildElementsActive(false);
						subGroupButton.OnButtonClicked += () => groupButton.SetChildSiblingChildElementsActive(subGroupButton);
					}

					foreach (IGroupItem prefab in subGroup.Children) {
						UIActionItemButtonElement prefabButton = await ((UIActionGroupButtonElement)subGroupButton).AddChildItemButton(prefab.Name, prefab.Icon);
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

			CreateGroupButtons(button, groups, SetupIndividualBuildButton).Forget();
		}

		private void SetupIndividualBuildButton(ITreeButton button, IGroupItem item) {

			if (button is not UIActionItemButtonElement prefabButton) {
				return;
			}
			if (item is not ObjectPrefab prefab) {
				return;
			}

			BuildJobParams buildJobParams = new(prefab, prefab.lastSelectedVariation, 0);

			button.OnButtonClicked += () => Debug.Log($"Build Button for {prefab.Name} clicked");
			button.OnButtonClicked += () => GameManager.Get<SelectionManager>().SetSelectedJob<BuildJob, BuildJobDefinition>(buildJobParams);

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
			if (item is not JobDefinition job) {
				return;
			}

			button.OnButtonClicked += () => Debug.Log($"Terraform Button for {job.Name} clicked");
		}

		// Farm Buttons

		private void CreateFarmButtons(ITreeButton button) {
			JobGroup group = GameManager.Get<JobManager>().JobRegistry.GetJobGroup("Farm");

			CreateGroupButtons(button, group, SetupIndividualFarmButton);
		}

		private void SetupIndividualFarmButton(ITreeButton button, IGroupItem item) {
			if (button is not UIActionItemButtonElement jobButton) {
				return;
			}
			if (item is not JobDefinition job) {
				return;
			}

			button.OnButtonClicked += () => Debug.Log($"Farm Button for {job.Name} clicked");
		}

		// Remove Buttons

		private void CreateRemoveButtons(ITreeButton button) {
			JobGroup group = GameManager.Get<JobManager>().JobRegistry.GetJobGroup("Remove");

			CreateGroupButtons(button, group, SetupIndividualRemoveButton);
		}

		private void SetupIndividualRemoveButton(ITreeButton button, IGroupItem item) {
			if (button is not UIActionItemButtonElement jobButton) {
				return;
			}
			if (item is not JobDefinition job) {
				return;
			}

			button.OnButtonClicked += () => Debug.Log($"Remove Button for {job.Name} clicked");
		}
	}
}