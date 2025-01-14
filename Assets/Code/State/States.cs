﻿using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Snowship.NUI.LoadingScreen;
using Snowship.NUI.Menu.MainMenu;
using Snowship.NUI.Menu.PauseMenu;
using Snowship.NUI.Simulation.SimulationUI;

namespace Snowship.NState {
	public partial class StateManager {

		public Dictionary<EState, State> States => states;

		private static readonly Dictionary<EState, State> states = new() {
			{
				EState.Boot, new State(
					EState.Boot,
					new List<EState> { EState.MainMenu },
					null
				)
			}, {
				EState.MainMenu, new State(
					EState.MainMenu,
					new List<EState> { EState.LoadToSimulation, EState.QuitToDesktop },
					new List<Func<UniTask>> {
						async () => await GameManager.uiM.OpenViewAsync<UIMainMenu>()
					})
			}, {
				EState.LoadToSimulation, new State(
					EState.LoadToSimulation,
					new List<EState> { EState.Simulation },
					new List<Func<UniTask>> {
						async () => await GameManager.uiM.OpenViewAsync<UILoadingScreen>()
					}
				)
			}, {
				EState.Simulation, new State(
					EState.Simulation,
					new List<EState> { EState.PauseMenu, EState.Saving, EState.QuitToMenu, EState.QuitToDesktop },
					new List<Func<UniTask>> {
						async () => await GameManager.uiM.OpenViewAsync<UISimulation>()
					}
				)
			}, {
				EState.PauseMenu, new State(
					EState.PauseMenu,
					new List<EState> { EState.Simulation },
					new List<Func<UniTask>> {
						async () => await GameManager.uiM.OpenViewAsync<UIPauseMenu>()
					}
				)
			}, {
				EState.Saving, new State(
					EState.Saving,
					new List<EState> { EState.PauseMenu, EState.Simulation },
					null
				)
			}, {
				EState.QuitToMenu, new State(
					EState.QuitToMenu,
					new List<EState> { EState.MainMenu },
					null
				)
			}, {
				EState.QuitToDesktop, new State(
					EState.QuitToDesktop,
					null,
					null
				)
			}
		};
	}
}
