using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Snowship.NUI;
using Snowship.NUI.LoadingScreen;
using Snowship.NUI.Menu.MainMenu;
using Snowship.NUI.Menu.PauseMenu;
using Snowship.NUI.Simulation.SimulationUI;

namespace Snowship.NState
{
	public partial class StateManager
	{
		public Dictionary<EState, State<EState>> States => states;

		private static readonly Dictionary<EState, State<EState>> states = new() {
			{
				EState.Boot, new State<EState>(
					EState.Boot,
					new List<EState> { EState.MainMenu },
					null
				)
			}, {
				EState.MainMenu, new State<EState>(
					EState.MainMenu,
					new List<EState> { EState.LoadToSimulation, EState.QuitToDesktop },
					new List<Func<UniTask>> {
						async () => await GameManager.Get<UIManager>().OpenViewAsync<UIMainMenu>()
					}
				)
			}, {
				EState.LoadToSimulation, new State<EState>(
					EState.LoadToSimulation,
					new List<EState> { EState.Simulation },
					new List<Func<UniTask>> {
						async () => await GameManager.Get<UIManager>().OpenViewAsync<UILoadingScreen>()
					}
				)
			}, {
				EState.Simulation, new State<EState>(
					EState.Simulation,
					new List<EState> { EState.PauseMenu, EState.Saving, EState.QuitToMenu, EState.QuitToDesktop },
					new List<Func<UniTask>> {
						async () => await GameManager.Get<UIManager>().OpenViewAsync<UISimulation>()
					}
				)
			}, {
				EState.PauseMenu, new State<EState>(
					EState.PauseMenu,
					new List<EState> { EState.Simulation },
					new List<Func<UniTask>> {
						async () => await GameManager.Get<UIManager>().OpenViewAsync<UIPauseMenu>()
					}
				)
			}, {
				EState.Saving, new State<EState>(
					EState.Saving,
					new List<EState> { EState.PauseMenu, EState.Simulation },
					null
				)
			}, {
				EState.QuitToMenu, new State<EState>(
					EState.QuitToMenu,
					new List<EState> { EState.MainMenu },
					null
				)
			}, {
				EState.QuitToDesktop, new State<EState>(
					EState.QuitToDesktop,
					null,
					null
				)
			}
		};
	}
}