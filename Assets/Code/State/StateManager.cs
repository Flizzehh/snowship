using System;
using Cysharp.Threading.Tasks;

namespace Snowship.NState {
	public partial class StateManager : IManager {

		private State state;

		public EState State => state.Type;

		public bool StateChangeLocked { get; private set; }

		public event Action<(EState previousState, EState newState)> OnStateChanged;

		public void OnCreate() {
			UniTask.WhenAll(TransitionToState(EState.Boot));
			UniTask.WhenAll(TransitionToState(EState.MainMenu));
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
					GameManager.uiM.ToggleAllViews();
					break;
				case ETransitionUIAction.Close:
					GameManager.uiM.CloseAllViews();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(transitionUIAction), transitionUIAction, null);
			}

			foreach (Func<UniTask> action in state.ActionsOnOpen) {
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
