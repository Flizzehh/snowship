using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Snowship.NHuman;
using Snowship.NResource;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UIReservedResourcesElementComponent : UIElementComponent
	{
		[SerializeField] private Transform colonistBodyElementLocation;
		[SerializeField] private TMP_Text reserverNameText;
		[SerializeField] private TMP_Text reservedCountText;
		[SerializeField] private VerticalLayoutGroup reservedResourcesListVerticalLayoutGroup;

		private readonly List<UIResourceAmountElement> resourceAmountElements = new();

		public UIHumanBodyElement SetReserverBodyElement(Human human) {
			UIHumanBodyElement humanBodyElement = new(human);
			humanBodyElement.Open(colonistBodyElementLocation).Forget();
			return humanBodyElement;
		}

		protected override void OnClose() {
			base.OnClose();

			foreach (UIResourceAmountElement resourceAmountElement in resourceAmountElements) {
				resourceAmountElement.Close();
			}
			resourceAmountElements.Clear();
		}

		public void SetReserverNameText(string text) {
			reserverNameText.SetText(text);
		}

		public void SetReservedCountText(string count) {
			reservedCountText.SetText(count);
		}

		public void AddResourceAmountElement(ResourceAmount resourceAmount) {
			UIResourceAmountElement resourceAmountElement = new(resourceAmount);
			resourceAmountElement.Open(reservedResourcesListVerticalLayoutGroup.transform).Forget();
			resourceAmountElements.Add(resourceAmountElement);
		}
	}
}