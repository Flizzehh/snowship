using System;

namespace Snowship.NUtilities {
	public readonly struct Range {
		public int Min { get; }
		public int Start => Min;
		public int Max { get; }
		public int End => Max;

		public int Sum => Max + Min;
		public int Midpoint => Sum / 2;

		public Range(int min, int max) {
			if (min > max) {
				throw new ArgumentException("Min cannot be greater than Max.");
			}

			Min = min;
			Max = max;
		}

		public int RandomExclusive() {
			return UnityEngine.Random.Range(Min, Max);
		}
	}
}
