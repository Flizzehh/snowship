using System.Linq;
using JetBrains.Annotations;
using Snowship.NPersistence;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Menu.Settings {

	[UsedImplicitly]
	public class UISettingsPresenter : UIPresenter<UISettingsView> {
		private readonly PSettings pSettings = GameManager.Get<PersistenceManager>().PSettings;

		public UISettingsPresenter(UISettingsView view) : base(view) {
		}

		public override void OnCreate() {
			View.OnCancelButtonClicked += OnCancelButtonClicked;
			View.OnApplyButtonClicked += OnApplyButtonClicked;
			View.OnAcceptButtonClicked += OnAcceptButtonClicked;

			View.OnResolutionSliderChanged += OnResolutionSliderChanged;
			View.OnFullscreenToggled += OnFullscreenToggled;
			View.OnUIScaleToggled += OnUIScaleToggled;

			SetResolutionSlider();
			SetFullscreenToggle();
			SetUIScaleToggle();

			Debug.Log(pSettings.SettingsState.ToString());
		}

		public override void OnClose() {
			View.OnCancelButtonClicked -= OnCancelButtonClicked;
			View.OnApplyButtonClicked -= OnApplyButtonClicked;
			View.OnAcceptButtonClicked -= OnAcceptButtonClicked;

			View.OnResolutionSliderChanged -= OnResolutionSliderChanged;
			View.OnFullscreenToggled -= OnFullscreenToggled;
			View.OnUIScaleToggled -= OnUIScaleToggled;
		}

		private void OnCancelButtonClicked() {
			GameManager.Get<UIManager>().GoBack(this);
		}

		private void OnApplyButtonClicked() {
			GameManager.Get<PersistenceManager>().PSettings.ApplySettings();
		}

		private void OnAcceptButtonClicked() {
			GameManager.Get<PersistenceManager>().PSettings.ApplySettings();
			GameManager.Get<UIManager>().GoBack(this);
		}

		private void SetResolutionSlider() {
			View.SetResolutionSlider(
				0,
				Screen.resolutions.Length - 1,
				Screen.resolutions.ToList().IndexOf(pSettings.SettingsState.Resolution)
			);
		}

		private void SetFullscreenToggle() {
			View.SetFullscreenToggle(pSettings.SettingsState.Fullscreen);
		}

		private void SetUIScaleToggle() {
			View.SetUIScaleToggle(pSettings.SettingsState.ScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize);
		}

		private void OnResolutionSliderChanged(float sliderValue) {
			Resolution resolution = Screen.resolutions[Mathf.FloorToInt(sliderValue)];
			pSettings.NewSettingsState.SetResolution(resolution);
			View.SetResolutionText(resolution.ToString());
			UpdateButtonsForChanges();
		}

		private void OnFullscreenToggled(bool isOn) {
			pSettings.NewSettingsState.SetFullscreen(isOn);
			UpdateButtonsForChanges();
		}

		private void OnUIScaleToggled(bool isOn) {
			pSettings.NewSettingsState.SetScaleMode(isOn ? CanvasScaler.ScaleMode.ScaleWithScreenSize : CanvasScaler.ScaleMode.ConstantPixelSize);
			UpdateButtonsForChanges();
		}

		private void UpdateButtonsForChanges() {
			bool changesExist = !pSettings.SettingsState.Equals(pSettings.NewSettingsState);
			if (changesExist) {
				View.SetButtonColours(
					ColourUtilities.GetColour(ColourUtilities.EColour.LightRed),
					ColourUtilities.GetColour(ColourUtilities.EColour.LightGreen),
					ColourUtilities.GetColour(ColourUtilities.EColour.DarkGreen)
				);
			} else {
				View.SetButtonColours(
					ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey200),
					ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey200),
					ColourUtilities.GetColour(ColourUtilities.EColour.LightGrey200)
				);
			}
		}
	}
}