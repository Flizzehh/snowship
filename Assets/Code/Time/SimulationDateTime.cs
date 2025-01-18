using System;
using UnityEngine;

namespace Snowship.NTime {

	public class SimulationDateTime {

		// Standard Date/Time Values
		public int Minute { get; set; } = 0; // 0
		public int Hour { get; set; } = 8; // 8
		public int Day { get; set; } = 1; // 1
		public Season Season { get; set; } = Season.Spring; // Season.Spring
		public int Year { get; set; } = 1; // 1

		// Extra
		public int Hour12 => Mathf.FloorToInt(1 + (12 - (1 - Hour)) % 12);
		public bool IsDay { get; set; }
		public float TileBrightnessTime => (float)Math.Round(Hour + Minute / 60f, 2);

		// Time Modifier
		public bool Paused = true;
		public float DeltaTime;
		public int TimeModifier { get; set; } = 0;
		public int PreviousTimeModifier { get; set; } = 0;

		// Constants
		public const int DayLengthSeconds = 1440; // Number of seconds in 1 in-game day
		public const int PermanentDeltaTimeMultiplier = 2;
		public const int PermanentTimerMultiplier = 2;
		public const int TimeModifierMax = 3;

		// Date/Time Strings
		public string Hour12String => $"{Hour12}:{(Minute < 10 ? $"0{Minute}" : Minute)} {(Hour is < 12 or > 23 ? "AM" : "PM")}";
		public string DateString => $"{Day},{Season},{Year}";
		public string TimeString => $"{Day}:{Hour}:{Minute}";

		public string GetDayWithSuffix() {
			int dayLastDigit = Day % 10;
			string daySuffix = "th";
			if (Day is not 11 and not 12 and not 13) {
				daySuffix = dayLastDigit switch {
					1 => "st",
					2 => "nd",
					3 => "rd",
					_ => daySuffix
				};
			}
			return $"{Day}{daySuffix}";
		}

		public string GetDayNightString() {
			return Hour switch {
				>= 5 and < 7 => "Dawn",
				>= 7 and < 17 => "Day",
				>= 17 and < 19 => "Dusk",
				_ => "Night"
			};
		}

	}
}
