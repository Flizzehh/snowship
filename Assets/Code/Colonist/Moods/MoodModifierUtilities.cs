using System;
using System.Collections.Generic;

namespace Snowship.NColonist {
	public static class MoodModifierUtilities {

		public static readonly Dictionary<MoodModifierGroupEnum, Action<Colonist>> moodModifierFunctions = new Dictionary<MoodModifierGroupEnum, Action<Colonist>>() {
			{
				MoodModifierGroupEnum.Death, delegate(Colonist colonist) {
					// TODO Implement colonists viewing deaths and being sad
				}
			}, {
				MoodModifierGroupEnum.Rest, delegate(Colonist colonist) {
					NeedInstance restNeed = colonist.needs.Find(ni => ni.prefab.type == ENeed.Rest);
					if (restNeed.GetValue() >= restNeed.prefab.critValue) {
						colonist.Moods.AddMoodModifier(MoodModifierEnum.Exhausted);
					} else if (restNeed.GetValue() >= restNeed.prefab.maxValue) {
						colonist.Moods.AddMoodModifier(MoodModifierEnum.Tired);
					} else {
						colonist.Moods.RemoveMoodModifier(MoodModifierEnum.Exhausted);
						colonist.Moods.RemoveMoodModifier(MoodModifierEnum.Tired);
					}
				}
			}, /* {
				MoodModifierGroupEnum.Water, delegate(Colonist colonist) {
					NeedInstance waterNeed = colonist.needs.Find(ni => ni.prefab.type == ENeed.Water);
					if (waterNeed.GetValue() >= waterNeed.prefab.critValue) {
						colonist.AddMoodModifier(MoodModifierEnum.Dehydrated);
					} else if (waterNeed.GetValue() >= waterNeed.prefab.maxValue) {
						colonist.AddMoodModifier(MoodModifierEnum.Thirsty);
					} else {
						colonist.RemoveMoodModifier(MoodModifierEnum.Dehydrated);
						colonist.RemoveMoodModifier(MoodModifierEnum.Thirsty);
					}
				}
			}, */ {
				MoodModifierGroupEnum.Food, delegate(Colonist colonist) {
					NeedInstance foodNeed = colonist.needs.Find(ni => ni.prefab.type == ENeed.Food);
					if (foodNeed.GetValue() >= foodNeed.prefab.critValue) {
						colonist.Moods.AddMoodModifier(MoodModifierEnum.Starving);
					} else if (foodNeed.GetValue() >= foodNeed.prefab.maxValue) {
						colonist.Moods.AddMoodModifier(MoodModifierEnum.Hungry);
					} else {
						colonist.Moods.RemoveMoodModifier(MoodModifierEnum.Starving);
						colonist.Moods.RemoveMoodModifier(MoodModifierEnum.Hungry);
					}
				}
			}, {
				MoodModifierGroupEnum.Inventory, delegate(Colonist colonist) {
					if (colonist.Inventory.UsedWeight() > colonist.Inventory.maxWeight) {
						colonist.Moods.AddMoodModifier(MoodModifierEnum.Overencumbered);
					} else {
						colonist.Moods.RemoveMoodModifier(MoodModifierEnum.Overencumbered);
					}
				}
			}
		};

	}
}