﻿using System;
using System.Diagnostics.CodeAnalysis;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NUI.Simulation
{
	public class UIClothingButtonElement : UIElement<UIClothingButtonElementComponent>
	{
		public readonly HumanManager.Human.Appearance Appearance;
		private Clothing clothing;

		public event Action<HumanManager.Human.Appearance> OnButtonClicked;

		public UIClothingButtonElement(
			Transform parent,
			HumanManager.Human.Appearance appearance,
			Clothing clothing
		) : base(
			parent
		) {
			Appearance = appearance;

			Component.SetTypeText(appearance.ToString());
			SetClothing(clothing);

			Component.OnButtonClicked += OnComponentButtonClicked;
		}

		protected override void OnClose() {
			base.OnClose();

			Component.OnButtonClicked -= OnComponentButtonClicked;
			OnButtonClicked = null;
		}

		private void OnComponentButtonClicked() {
			OnButtonClicked?.Invoke(Appearance);
		}

		[SuppressMessage("ReSharper", "ParameterHidesMember")]
		public void SetClothing(Clothing clothing) {
			this.clothing = clothing;
			if (clothing != null) {
				Component.SetNameText(clothing.name);
				Component.SetImage(clothing.image);
			} else {
				Component.SetNameText("None");
				Component.SetImage(null);
			}
		}
	}
}
