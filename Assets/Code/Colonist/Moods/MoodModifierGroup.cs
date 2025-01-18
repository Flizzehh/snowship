using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NColonist {
	public class MoodModifierGroup {

		public static readonly List<MoodModifierGroup> moodModifierGroups = new List<MoodModifierGroup>();

		public MoodModifierGroupEnum type;
		public string name;

		public readonly List<MoodModifierPrefab> prefabs = new List<MoodModifierPrefab>();

		public MoodModifierGroup(string stringMoodModifierGroup) {
			List<string> stringMoodModifiers = stringMoodModifierGroup.Split(new string[] { "<MoodModifier>" }, StringSplitOptions.RemoveEmptyEntries).ToList();

			type = (MoodModifierGroupEnum)Enum.Parse(typeof(MoodModifierGroupEnum), stringMoodModifiers[0]);
			name = StringUtilities.SplitByCapitals(type.ToString());

			foreach (string stringMoodModifier in stringMoodModifiers.Skip(1)) {
				prefabs.Add(new MoodModifierPrefab(stringMoodModifier, this));
			}
		}

		public static void CreateMoodModifiers() {
			List<string> stringMoodModifierGroups = Resources.Load<TextAsset>(@"Data/mood-modifiers").text.Replace("\t", string.Empty).Split(new string[] { "<MoodModifierGroup>" }, StringSplitOptions.RemoveEmptyEntries).ToList();
			foreach (string stringMoodModifierGroup in stringMoodModifierGroups) {
				MoodModifierGroup moodModifierGroup = new MoodModifierGroup(stringMoodModifierGroup);
				moodModifierGroups.Add(moodModifierGroup);
			}
		}

		public static MoodModifierGroup GetMoodModifierGroupFromEnum(MoodModifierGroupEnum moodModifierGroupEnum) {
			return moodModifierGroups.Find(mmiGroup => mmiGroup.type == moodModifierGroupEnum);
		}

		public static MoodModifierPrefab GetMoodModifierPrefabFromEnum(MoodModifierEnum moodModifierEnum) {
			foreach (MoodModifierGroup moodModifierGroup in moodModifierGroups) {
				MoodModifierPrefab prefab = moodModifierGroup.prefabs.Find(moodModifierPrefab => moodModifierPrefab.type == moodModifierEnum);
				if (prefab != null) {
					return prefab;
				}
			}
			return null;
		}

		public static MoodModifierPrefab GetMoodModifierPrefabFromString(string moodModifierTypeString) {
			return GetMoodModifierPrefabFromEnum((MoodModifierEnum)Enum.Parse(typeof(MoodModifierEnum), moodModifierTypeString));
		}

	}
}
