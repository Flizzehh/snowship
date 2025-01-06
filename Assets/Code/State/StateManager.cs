using System;
using System.Collections.Generic;

namespace Snowship.NState {
	public class StateManager : BaseManager {

		private State state = states[EState.Boot];
		public EState State => state.type;

		public bool StateChangeLocked { get; private set; }

		public event Action<(EState previousState, EState newState)> OnStateChanged;

		private static readonly Dictionary<EState, State> states = new() {
			{ EState.Boot, new State(EState.Boot, new List<EState> { EState.MainMenu }) },
			{ EState.MainMenu, new State(EState.MainMenu, new List<EState> { EState.LoadToSimulation, EState.QuitToDesktop }) },
			{ EState.LoadToSimulation, new State(EState.LoadToSimulation, new List<EState> { EState.Simulation }) },
			{ EState.Simulation, new State(EState.Simulation, new List<EState> { EState.Paused, EState.Saving, EState.QuitToMenu, EState.QuitToDesktop }) },
			{ EState.Paused, new State(EState.Paused, new List<EState> { EState.Simulation }) },
			{ EState.Saving, new State(EState.Saving, new List<EState> { EState.Paused, EState.Simulation }) },
			{ EState.QuitToMenu, new State(EState.QuitToMenu, new List<EState> { EState.MainMenu }) },
			{ EState.QuitToDesktop, new State(EState.QuitToDesktop, null) }
		};

		public void TransitionToState(EState newState) {
			if (StateChangeLocked) {
				return;
			}

			if (state.type == newState) {
				return;
			}

			if (!state.validNextStates.Contains(newState)) {
				return;
			}

			EState previousState = state.type;
			state = states[newState];
			OnStateChanged?.Invoke((previousState, state.type));
		}

		public void LockStateChange() {
			StateChangeLocked = true;
		}

		public void UnlockStateChange() {
			StateChangeLocked = false;
		}
	}

}
