using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NColonist {
	public class MoodModifierPrefab {

		public MoodModifierEnum type;
		public string name = string.Empty;

		public MoodModifierGroup group = null;

		public int effectAmount = 0;

		public int effectLengthSeconds = 0;

		public bool infinite = false;

		public MoodModifierPrefab(string stringMoodModifier, MoodModifierGroup group) {
			this.group = group;

			List<string> stringMoodModifierList = stringMoodModifier.Split('\n').ToList();
			foreach (string stringMoodModifierSingle in stringMoodModifierList.Skip(1)) {

				if (!string.IsNullOrEmpty(stringMoodModifierSingle)) {

					string label = stringMoodModifierSingle.Split('>')[0].Replace("<", string.Empty);
					string value = stringMoodModifierSingle.Split('>')[1];

					switch (label) {
						case "Type":
							type = (MoodModifierEnum)Enum.Parse(typeof(MoodModifierEnum), value);
							name = StringUtilities.SplitByCapitals(type.ToString());
							break;
						case "EffectAmount":
							effectAmount = int.Parse(value);
							break;
						case "EffectLengthSeconds":
							infinite = StringUtilities.RemoveNonAlphanumericChars(value) == "UntilNot";
							if (infinite) {
								effectLengthSeconds = int.MaxValue;
							} else {
								effectLengthSeconds = int.Parse(value);
							}
							break;
						default:
							MonoBehaviour.print("Unknown mood modifier label: \"" + stringMoodModifierSingle + "\"");
							break;
					}
				}
			}

			if (string.IsNullOrEmpty(name) || effectAmount == 0 || effectLengthSeconds == 0) {
				MonoBehaviour.print("Potential issue parsing mood modifier: " + stringMoodModifier);
			}
		}

	}
}
