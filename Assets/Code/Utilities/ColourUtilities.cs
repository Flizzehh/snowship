using System;
using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NUtilities {
	public static class ColourUtilities {

		public static Color HexToColor(string hexString) {
			int r = int.Parse("" + hexString[0] + hexString[1], System.Globalization.NumberStyles.HexNumber);
			int g = int.Parse("" + hexString[2] + hexString[3], System.Globalization.NumberStyles.HexNumber);
			int b = int.Parse("" + hexString[4] + hexString[5], System.Globalization.NumberStyles.HexNumber);
			return new Color(r, g, b, 255f) / 255f;
		}

		public enum EColour {
			Clear,
			WhiteAlpha128,
			WhiteAlpha64,
			WhiteAlpha20,
			White,
			DarkRed,
			DarkGreen,
			LightRed,
			LightRed100,
			LightGreen,
			LightGreen100,
			LightGrey220,
			LightGrey200,
			LightGrey180,
			Grey150,
			Grey120,
			DarkGrey50,
			LightBlue,
			LightOrange,
			DarkOrange,
			DarkYellow,
			LightYellow,
			LightPurple,
			LightPurple100,
			DarkPurple
		};

		private static readonly Dictionary<EColour, Color> colourMap = new Dictionary<EColour, Color>() {
			{ EColour.Clear, new Color(255f, 255f, 255f, 0f) / 255f },
			{ EColour.WhiteAlpha128, new Color(255f, 255f, 255f, 128f) / 255f },
			{ EColour.WhiteAlpha64, new Color(255f, 255f, 255f, 64f) / 255f },
			{ EColour.WhiteAlpha20, new Color(255f, 255f, 255f, 20f) / 255f },
			{ EColour.White, new Color(255f, 255f, 255f, 255f) / 255f },
			{ EColour.DarkRed, new Color(192f, 57f, 43f, 255f) / 255f },
			{ EColour.DarkGreen, new Color(39f, 174f, 96f, 255f) / 255f },
			{ EColour.LightRed, new Color(231f, 76f, 60f, 255f) / 255f },
			{ EColour.LightRed100, new Color(231f, 76f, 60f, 100f) / 255f },
			{ EColour.LightGreen, new Color(46f, 204f, 113f, 255f) / 255f },
			{ EColour.LightGreen100, new Color(46f, 204f, 113f, 100f) / 255f },
			{ EColour.LightGrey220, new Color(220f, 220f, 220f, 255f) / 255f },
			{ EColour.LightGrey200, new Color(200f, 200f, 200f, 255f) / 255f },
			{ EColour.LightGrey180, new Color(180f, 180f, 180f, 255f) / 255f },
			{ EColour.Grey150, new Color(150f, 150f, 150f, 255f) / 255f },
			{ EColour.Grey120, new Color(120f, 120f, 120f, 255f) / 255f },
			{ EColour.DarkGrey50, new Color(50f, 50f, 50f, 255f) / 255f },
			{ EColour.LightBlue, new Color(52f, 152f, 219f, 255f) / 255f },
			{ EColour.LightOrange, new Color(230f, 126f, 34f, 255f) / 255f },
			{ EColour.DarkOrange, new Color(211f, 84f, 0f, 255f) / 255f },
			{ EColour.DarkYellow, new Color(216f, 176f, 15f, 255f) / 255f },
			{ EColour.LightYellow, new Color(241f, 196f, 15f, 255f) / 255f },
			{ EColour.LightPurple, new Color(155f, 89f, 182f, 255f) / 255f },
			{ EColour.LightPurple100, new Color(155f, 89f, 182f, 100f) / 255f },
			{ EColour.DarkPurple, new Color(142f, 68f, 173f, 255f) / 255f }
		};

		public static Color GetColour(EColour colourKey) {
			return colourMap[colourKey];
		}

		public static Color ChangeAlpha(Color colour, float alpha) {
			return new Color(colour.r, colour.g, colour.b, alpha);
		}

		public enum Gradients {
			LightRedYellowGreen,
			LightGreenYellowRed,
			DarkRedYellowGreen,
			DarkGreenYellowRed
		};

		private static readonly Dictionary<Gradients, Gradient> gradientMap = new Dictionary<Gradients, Gradient>() {
			{
				Gradients.LightRedYellowGreen, new Func<Gradient>(() => {
					Gradient gradient = new Gradient();
					GradientColorKey[] colourKey = new GradientColorKey[3];
					GradientAlphaKey[] alphaKey = new GradientAlphaKey[3];
					colourKey[0].color = GetColour(EColour.LightRed);
					colourKey[0].time = 0.0f;
					alphaKey[0].alpha = 1.0f;
					alphaKey[0].time = 0.0f;
					colourKey[1].color = GetColour(EColour.LightYellow);
					colourKey[1].time = 0.5f;
					alphaKey[1].alpha = 1.0f;
					alphaKey[1].time = 0.0f;
					colourKey[2].color = GetColour(EColour.LightGreen);
					colourKey[2].time = 1.0f;
					alphaKey[2].alpha = 1.0f;
					alphaKey[2].time = 0.0f;
					gradient.SetKeys(colourKey, alphaKey);
					return gradient;
				}).Invoke()
			}, {
				Gradients.LightGreenYellowRed, new Func<Gradient>(() => {
					Gradient gradient = new Gradient();
					GradientColorKey[] colourKey = new GradientColorKey[3];
					GradientAlphaKey[] alphaKey = new GradientAlphaKey[3];
					colourKey[0].color = GetColour(EColour.LightGreen);
					colourKey[0].time = 0.0f;
					alphaKey[0].alpha = 1.0f;
					alphaKey[0].time = 0.0f;
					colourKey[1].color = GetColour(EColour.LightYellow);
					colourKey[1].time = 0.5f;
					alphaKey[1].alpha = 1.0f;
					alphaKey[1].time = 0.0f;
					colourKey[2].color = GetColour(EColour.LightRed);
					colourKey[2].time = 1.0f;
					alphaKey[2].alpha = 1.0f;
					alphaKey[2].time = 0.0f;
					gradient.SetKeys(colourKey, alphaKey);
					return gradient;
				}).Invoke()
			}, {
				Gradients.DarkRedYellowGreen, new Func<Gradient>(() => {
					Gradient gradient = new Gradient();
					GradientColorKey[] colourKey = new GradientColorKey[3];
					GradientAlphaKey[] alphaKey = new GradientAlphaKey[3];
					colourKey[0].color = GetColour(EColour.DarkRed);
					colourKey[0].time = 0.0f;
					alphaKey[0].alpha = 1.0f;
					alphaKey[0].time = 0.0f;
					colourKey[1].color = GetColour(EColour.DarkYellow);
					colourKey[1].time = 0.5f;
					alphaKey[1].alpha = 1.0f;
					alphaKey[1].time = 0.0f;
					colourKey[2].color = GetColour(EColour.DarkGreen);
					colourKey[2].time = 1.0f;
					alphaKey[2].alpha = 1.0f;
					alphaKey[2].time = 0.0f;
					gradient.SetKeys(colourKey, alphaKey);
					return gradient;
				}).Invoke()
			}, {
				Gradients.DarkGreenYellowRed, new Func<Gradient>(() => {
					Gradient gradient = new Gradient();
					GradientColorKey[] colourKey = new GradientColorKey[3];
					GradientAlphaKey[] alphaKey = new GradientAlphaKey[3];
					colourKey[0].color = GetColour(EColour.DarkGreen);
					colourKey[0].time = 0.0f;
					alphaKey[0].alpha = 1.0f;
					alphaKey[0].time = 0.0f;
					colourKey[1].color = GetColour(EColour.DarkYellow);
					colourKey[1].time = 0.5f;
					alphaKey[1].alpha = 1.0f;
					alphaKey[1].time = 0.0f;
					colourKey[2].color = GetColour(EColour.DarkRed);
					colourKey[2].time = 1.0f;
					alphaKey[2].alpha = 1.0f;
					alphaKey[2].time = 0.0f;
					gradient.SetKeys(colourKey, alphaKey);
					return gradient;
				}).Invoke()
			}
		};

		public static Gradient GetGradient(Gradients gradientKey) {
			return gradientMap[gradientKey];
		}
	}
}
