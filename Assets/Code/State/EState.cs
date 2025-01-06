namespace Snowship.NState {
	public enum EState {
		Boot, // -> MainMenu
		MainMenu, // -> LoadToSimulation, QuitToDesktop
		LoadToSimulation, // -> Simulation
		Simulation, // -> Paused, Saving, QuitToMenu, QuitToDesktop
		Paused, // -> Simulation
		Saving, // -> Paused, Simulation
		QuitToMenu, // -> MainMenu
		QuitToDesktop // -> ...
	}
}
