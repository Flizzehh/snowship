using System;
using System.Diagnostics.CodeAnalysis;
using Snowship.NColony;
using Snowship.NInput;
using Snowship.NState;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Snowship.NTime {

	public class TimeManager : IManager {

		public SimulationDateTime Time { get; } = new SimulationDateTime();
		private float timer = 0;

		public event Action<SimulationDateTime> OnTimeChanged;

		public void OnGameSetupComplete() {
			GameManager.Get<StateManager>().OnStateChanged += OnStateChanged;

			OnTimeChanged?.Invoke(Time);
		}

		[SuppressMessage("ReSharper", "InvertIf")]
		private void OnStateChanged((EState previousState, EState newState) state) {
			if (state is { previousState: EState.LoadToSimulation, newState: EState.Simulation }) {
				GameManager.Get<InputManager>().InputSystemActions.Simulation.TimeSpeedUp.performed += TimeSpeedUp;
				GameManager.Get<InputManager>().InputSystemActions.Simulation.TimeSlowDown.performed += TimeSlowDown;
				GameManager.Get<InputManager>().InputSystemActions.Simulation.Pause.performed += TogglePause;
			}

			if (state is { previousState: EState.Simulation, newState: EState.QuitToMenu }) {
				GameManager.Get<InputManager>().InputSystemActions.Simulation.TimeSpeedUp.performed -= TimeSpeedUp;
				GameManager.Get<InputManager>().InputSystemActions.Simulation.TimeSlowDown.performed -= TimeSlowDown;
				GameManager.Get<InputManager>().InputSystemActions.Simulation.Pause.performed -= TogglePause;
			}
		}

		public void OnUpdate() {
			UpdateTime();
		}

		private void TimeSpeedUp(InputAction.CallbackContext callbackContext) {
			ChangeTimeModifier(1);
		}

		private void TimeSlowDown(InputAction.CallbackContext callbackContext) {
			ChangeTimeModifier(-1);
		}

		private void ChangeTimeModifier(int direction) {
			if (GameManager.Get<StateManager>().State != EState.Simulation) {
				return;
			}

			if (GameManager.Get<InputManager>().IsPlayerTyping()) {
				return;
			}

			Time.TimeModifier += direction;

			Time.TimeModifier = Mathf.Clamp(Time.TimeModifier, 0, SimulationDateTime.TimeModifierMax);

			if (Time.TimeModifier <= 0 && !Time.Paused) {
				Time.TimeModifier = 1;
			}

			if (Time.TimeModifier > 0 && Time.Paused) {
				TogglePause();
			}

			OnTimeChanged?.Invoke(Time);
		}

		private bool UpdateTime() {

			if (GameManager.Get<StateManager>().State != EState.Simulation) {
				return false;
			}

			Time.DeltaTime = UnityEngine.Time.deltaTime * Time.TimeModifier * SimulationDateTime.PermanentDeltaTimeMultiplier;

			timer += Time.DeltaTime * SimulationDateTime.PermanentTimerMultiplier;
			if (!(timer >= 1)) {
				return false;
			}

			Time.Minute += 1;
			timer = 0;
			if (Time.Minute % 10 == 0) {
				GameManager.Get<ColonyManager>().colony.map.SetTileBrightness(Time.TileBrightnessTime, false);
			}
			if (Time.Minute >= 60) {
				Time.Hour += 1;
				Time.Minute = 0;
				if (Time.Hour >= 24) {
					Time.Day += 1;
					Time.Hour = 0;
					if (Time.Day > 30) {
						Time.Season += 1;
						Time.Day = 1;
						if (Time.Season > Season.Winter) {
							Time.Year += 1;
							Time.Season = Season.Spring;
						}
					}
				}
				Time.IsDay = Time.Hour is >= 6 and <= 18;
			}
			OnTimeChanged?.Invoke(Time);
			return true;
		}

		public void SetTime(float time) {
			Time.Hour = Mathf.FloorToInt(time);
			Time.Minute = Mathf.RoundToInt((time - Time.Hour) * 60);
			if (!UpdateTime()) {
				OnTimeChanged?.Invoke(Time);
			}
		}

		public void TogglePause() {

			if (GameManager.Get<StateManager>().State != EState.Simulation) {
				return;
			}

			if (GameManager.Get<InputManager>().IsPlayerTyping()) {
				return;
			}

			Time.Paused = !Time.Paused;
			if (Time.Paused) {
				Time.PreviousTimeModifier = Time.TimeModifier;
				Time.TimeModifier = 0;
			} else {
				Time.TimeModifier = Time.PreviousTimeModifier == 0 ? 1 : Time.PreviousTimeModifier;
			}
			OnTimeChanged?.Invoke(Time);
		}

		private void TogglePause(InputAction.CallbackContext callbackContext) {
			TogglePause();
		}
	}
}