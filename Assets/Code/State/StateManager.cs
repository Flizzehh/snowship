using System;
using Cysharp.Threading.Tasks;
using Snowship.NUI;

namespace Snowship.NState {
	public partial class StateManager : IManager {
		private State<EState> state;

		public EState State => state.Type;

		public bool StateChangeLocked { get; private set; }

		public event Action<(EState previousState, EState newState)> OnStateChanged;

		public void OnCreate() {
			TransitionToState(EState.Boot).Forget();
			TransitionToState(EState.MainMenu).Forget();
		}

		public async UniTask TransitionToState(EState newState, ETransitionUIAction transitionUIAction = ETransitionUIAction.Close) {
			if (StateChangeLocked) {
				return;
			}

			state ??= states[newState];

			if (state.Type == newState) {
				return;
			}

			if (!state.ValidNextStates.Contains(newState)) {
				return;
			}

			EState previousState = state.Type;
			state = states[newState];

			switch (transitionUIAction) {
				case ETransitionUIAction.Nothing:
					break;
				case ETransitionUIAction.Hide:
					GameManager.Get<UIManager>().ToggleAllViews();
					break;
				case ETransitionUIAction.Close:
					GameManager.Get<UIManager>().CloseAllViews();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(transitionUIAction), transitionUIAction, null);
			}

			foreach (Func<UniTask> action in state.ActionsOnTransition) {
				await action();
			}

			OnStateChanged?.Invoke((previousState, state.Type));
		}

		public void LockStateChange() {
			StateChangeLocked = true;
		}

		public void UnlockStateChange() {
			StateChangeLocked = false;
		}
	}

}