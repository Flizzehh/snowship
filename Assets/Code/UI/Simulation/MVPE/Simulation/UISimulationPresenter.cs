using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Snowship.NColonist;
using Snowship.NInput;
using Snowship.NPersistence;
using Snowship.NState;
using Snowship.NUI.Simulation.UIColonistInfoPanel;
using UnityEngine.InputSystem;

namespace Snowship.NUI.Simulation.SimulationUI {

	[UsedImplicitly]
	public class UISimulationPresenter : UIPresenter<UISimulationView> {

		public UISimulationPresenter(UISimulationView view) : base(view) {
		}

		public override void OnCreate() {
			GameManager.Get<InputManager>().InputSystemActions.Simulation.Escape.performed += OnEscapePerformed;

			View.SetDisclaimerText($"Lumi Games (Snowship {PersistenceManager.GameVersion.text})");

			SetupChildUIs().Forget();

			GameManager.Get<HumanManager>().OnHumanSelected += human => OnHumanSelected(human).Forget();
		}

		public override void OnClose() {
			GameManager.Get<InputManager>().InputSystemActions.Simulation.Escape.performed -= OnEscapePerformed;
		}

		private void OnEscapePerformed(InputAction.CallbackContext callbackContext) {
			// TODO Once setup, de-select items from SelectionManager in a FIFO format, and if none remain, then open the PauseMenu
			GameManager.Get<StateManager>().TransitionToState(EState.PauseMenu, ETransitionUIAction.Hide).Forget();
		}

		private async UniTask SetupChildUIs() {
			await GameManager.Get<UIManager>().OpenViewAsync<UIDateTime.UIDateTime>(this);
		}

		private async UniTaskVoid OnHumanSelected(HumanManager.Human selectedHuman) {
			if (selectedHuman is not Colonist selectedColonist) {
				GameManager.Get<UIManager>().CloseView<UIColonistInfoPanel.UIColonistInfoPanel>();
				return;
			}
			UIColonistInfoPanelParameters parameters = new(selectedColonist);
			await GameManager.Get<UIManager>().ReopenView<UIColonistInfoPanel.UIColonistInfoPanel>(this, parameters);
		}

	}
}