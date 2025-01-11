using System;
using Snowship.NState;
using UnityEngine;

namespace Snowship.NTime {

	public class TimeManager : IManager {

		public static readonly int dayLengthSeconds = 1440; // Number of seconds in 1 in-game day

		public static readonly int permanentDeltaTimeMultiplier = 2;
		public static readonly int permanentTimerMultiplier = 2;

		public float deltaTime;

		private float timer = 0;

		private bool paused;
		private int pauseTimeModifier = 0;
		private int timeModifier = 0;

		public bool IsDay { get; private set; }

		public bool minuteChanged;

		public float tileBrightnessTime = 0;

		// Events

		public event Action OnTimeChanged;

		public void Update() {
			tileBrightnessTime = CalculateTileBrightnessTime();
			if (GameManager.tileM.mapState == TileManager.MapState.Generated) {
				if (Input.GetKeyDown(KeyCode.Alpha1) && GameManager.stateM.State != EState.Paused && !GameManager.uiMOld.playerTyping) {
					if (timeModifier > 0) {
						timeModifier -= 1;
						if (timeModifier <= 0) {
							timeModifier = 1;
						}
					}
				}
				if (Input.GetKeyDown(KeyCode.Alpha2) && GameManager.stateM.State != EState.Paused && !GameManager.uiMOld.playerTyping) {
					timeModifier += 1;
				}
				timeModifier = Mathf.Clamp(timeModifier, 0, 5);
				if (timeModifier > 0 && paused) {
					TogglePause();
				}
				pauseTimeModifier = Mathf.Clamp(pauseTimeModifier, 0, 5);
				if (Input.GetKeyDown(KeyCode.Space) && GameManager.stateM.State != EState.Paused && !GameManager.uiMOld.playerTyping) {
					TogglePause();
				}
				deltaTime = UnityEngine.Time.deltaTime * timeModifier * permanentDeltaTimeMultiplier;

				timer += deltaTime * permanentTimerMultiplier;
				minuteChanged = false;
				if (timer >= 1) {
					Time.Minute += 1;
					timer = 0;
					if (Time.Minute % 10 == 0) {
						GameManager.colonyM.colony.map.SetTileBrightness(tileBrightnessTime, false);
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
				IsDay = (Time.Hour >= 6 && Time.Hour <= 18);

				if (minuteChanged)
				{
					OnTimeChanged?.Invoke();
				}
				GameManager.uiMOld.UpdateDateTimeInformation();
			}
		}

		public void TogglePause() {
			paused = !paused;
			if (paused) {
				pauseTimeModifier = timeModifier;
				timeModifier = 0;
			} else {
				if (pauseTimeModifier == 0) {
					timeModifier = 1;
				} else {
					timeModifier = pauseTimeModifier;
				}
			}
		}

		public int GetTimeModifier() {
			return timeModifier;
		}

		public void SetPaused(bool newPausedState) {
			paused = !newPausedState;
			TogglePause();
		}

		public bool GetPaused() {
			return paused;
		}

		public int Get24HourTime() {
			return Time.Hour;
		}

		public int Get12HourTime() {
			return Mathf.FloorToInt(1 + (12 - (1 - Time.Hour)) % 12);
		}

		public string Get12HourTimeString() {
			return $"{Get12HourTime()}:{(Time.Minute < 10 ? ($"0{Time.Minute}") : Time.Minute)} {(Time.Hour < 12 || Time.Hour > 23 ? "AM" : "PM")}";
		}

		private float CalculateTileBrightnessTime() {
			return (float)Math.Round(Time.Hour + (Time.Minute / 60f), 2);
		}

		public void SetTime(float time) {
			Time.Hour = Mathf.FloorToInt(time);
			Time.Minute = Mathf.RoundToInt((time - Time.Hour) * 60);
			Update();
		}

		public string GetDateString() {
			return ($"{Time.Day},{Time.Season},{Time.Year}");
		}

		public string GetTimeString() {
			return ($"{Time.Day}:{Time.Hour}:{Time.Minute}");
		}

		public string GetDayWithSuffix() {
			int dayLastDigit = Time.Day % 10;
			string daySuffix = "th";
			if (Time.Day is not 11 and not 12 and not 13) {
				if (dayLastDigit == 1) {
					daySuffix = "st";
				} else if (dayLastDigit == 2) {
					daySuffix = "nd";
				} else if (dayLastDigit == 3) {
					daySuffix = "rd";
				}
			}
			return $"{Time.Day}{daySuffix}";
		}

		public string GetDayNightString() {
			if (Time.Hour is >= 5 and < 7) {
				return "Dawn";
			} else if (Time.Hour is >= 7 and < 17) {
				return "Day";
			} else if (Time.Hour is >= 17 and < 19) {
				return "Dusk";
			} else {
				return "Night";
			}
		}
	}
}
