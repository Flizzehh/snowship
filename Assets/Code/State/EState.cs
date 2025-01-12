namespace Snowship.NState {
	public enum EState {
		Boot, // -> MainMenu
		MainMenu, // -> LoadToSimulation, QuitToDesktop
		LoadToSimulation, // -> Simulation
		Simulation, // -> PauseMenu, Saving, QuitToMenu, QuitToDesktop
		PauseMenu, // -> Simulation
		Saving, // -> PauseMenu, Simulation
		QuitToMenu, // -> MainMenu
		QuitToDesktop // -> ...
	}
}
