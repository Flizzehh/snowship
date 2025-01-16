using System;
using Snowship.NState;
using UnityEngine;

namespace Snowship.NTime {

	public class TimeManager : IManager {

		public SimulationDateTime Time { get; } = new SimulationDateTime();
		private float timer = 0;

		// Events

		public event Action<SimulationDateTime> OnTimeChanged;

		public void Update() {
			if (GameManager.stateM.State != EState.Simulation) {
				return;
			}

			if (Input.GetKeyDown(KeyCode.Alpha1) && GameManager.stateM.State != EState.PauseMenu && !GameManager.uiMOld.playerTyping) {
				if (Time.TimeModifier > 0) {
					Time.TimeModifier -= 1;
					if (Time.TimeModifier <= 0) {
						Time.TimeModifier = 1;
					}
				}
			}

			if (Input.GetKeyDown(KeyCode.Alpha2) && GameManager.stateM.State != EState.PauseMenu && !GameManager.uiMOld.playerTyping) {
				Time.TimeModifier += 1;
			}
			Time.TimeModifier = Mathf.Clamp(Time.TimeModifier, 0, 3);
			if (Time.TimeModifier > 0 && Time.Paused) {
				TogglePause();
			}
			Time.PreviousTimeModifier = Mathf.Clamp(Time.PreviousTimeModifier, 0, 3);
			if (Input.GetKeyDown(KeyCode.Space) && GameManager.stateM.State != EState.PauseMenu && !GameManager.uiMOld.playerTyping) {
				TogglePause();
			}
			Time.DeltaTime = UnityEngine.Time.deltaTime * Time.TimeModifier * SimulationDateTime.PermanentDeltaTimeMultiplier;

			timer += Time.DeltaTime * SimulationDateTime.PermanentTimerMultiplier;
			bool minuteChanged = false;
			if (timer >= 1) {
				Time.Minute += 1;
				timer = 0;
				if (Time.Minute % 10 == 0) {
					GameManager.colonyM.colony.map.SetTileBrightness(Time.TileBrightnessTime, false);
				}
				minuteChanged = true;
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
				}
			}
			Time.IsDay = Time.Hour is >= 6 and <= 18;

			if (minuteChanged) {
				OnTimeChanged?.Invoke(Time);
			}
		}

		public void SetTime(float time) {
			Time.Hour = Mathf.FloorToInt(time);
			Time.Minute = Mathf.RoundToInt((time - Time.Hour) * 60);
			Update();
		}

		public void TogglePause() {
			Time.Paused = !Time.Paused;
			if (Time.Paused) {
				Time.PreviousTimeModifier = Time.TimeModifier;
				Time.TimeModifier = 0;
			} else {
				Time.TimeModifier = Time.PreviousTimeModifier == 0 ? 1 : Time.PreviousTimeModifier;
			}
		}

		public void SetPaused(bool newPausedState) {
			Time.Paused = !newPausedState;
			TogglePause();
		}
	}
}
