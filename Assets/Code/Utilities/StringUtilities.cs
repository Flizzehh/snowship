using System.Text.RegularExpressions;

namespace Snowship.NUtilities {
	public static class StringUtilities {

		public static string SplitByCapitals(string combinedString) {
			Regex r = new Regex(
				@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])",
				RegexOptions.IgnorePatternWhitespace);
			return r.Replace(combinedString, " ");
		}

		public static string RemoveNonAlphanumericChars(string removeFromString) {
			return new Regex("[^a-zA-Z0-9 -]").Replace(removeFromString, string.Empty);
		}

		public static bool IsAlphanumericWithSpaces(string text) {
			return Regex.IsMatch(text, @"^[A-Za-z0-9 ]*[A-Za-z0-9][A-Za-z0-9 ]*$");
		}
	}
}
