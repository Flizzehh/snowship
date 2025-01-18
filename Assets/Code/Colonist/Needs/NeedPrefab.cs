using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NColonist {
	public class NeedPrefab {

		public static readonly List<NeedPrefab> needPrefabs = new List<NeedPrefab>();

		public ENeed type;
		public string name;

		public Func<NeedInstance, float> CalculateSpecialValueFunction;

		public Func<NeedInstance, bool> DetermineReactionFunction;

		public float baseIncreaseRate;
		public float decreaseRateMultiplier;

		public bool minValueAction;
		public float minValue;

		public bool maxValueAction;
		public float maxValue;

		public bool critValueAction;
		public float critValue;

		public bool canDie;
		public float healthDecreaseRate;

		public int clampValue;

		public int priority;
		public readonly Dictionary<ETrait, float> traitsAffectingThisNeed = new Dictionary<ETrait, float>();

		public readonly List<string> relatedJobs = new List<string>();

		public NeedPrefab(
			ENeed type,
			float baseIncreaseRate,
			float decreaseRateMultiplier,
			bool minValueAction,
			float minValue,
			bool maxValueAction,
			float maxValue,
			bool critValueAction,
			float critValue,
			bool canDie,
			float healthDecreaseRate,
			int clampValue,
			int priority,
			Dictionary<ETrait, float> traitsAffectingThisNeed,
			List<string> relatedJobs,
			Func<NeedInstance, float> calculateSpecialValueFunction,
			Func<NeedInstance, bool> determineReactionFunction) {
			this.type = type;
			name = StringUtilities.SplitByCapitals(type.ToString());

			this.baseIncreaseRate = baseIncreaseRate;
			this.decreaseRateMultiplier = decreaseRateMultiplier;

			this.minValueAction = minValueAction;
			this.minValue = minValue;

			this.maxValueAction = maxValueAction;
			this.maxValue = maxValue;

			this.critValueAction = critValueAction;
			this.critValue = critValue;

			this.canDie = canDie;
			this.healthDecreaseRate = healthDecreaseRate;

			this.clampValue = clampValue;

			this.priority = priority;

			this.traitsAffectingThisNeed = traitsAffectingThisNeed;

			this.relatedJobs = relatedJobs;

			CalculateSpecialValueFunction = calculateSpecialValueFunction;
			DetermineReactionFunction = determineReactionFunction;
		}

		public static NeedPrefab GetNeedPrefabFromEnum(ENeed needsEnumValue) {
			return needPrefabs.Find(needPrefab => needPrefab.type == needsEnumValue);
		}

		public static NeedPrefab GetNeedPrefabFromString(string needTypeString) {
			return needPrefabs.Find(needPrefab => needPrefab.type == (ENeed)Enum.Parse(typeof(ENeed), needTypeString));
		}

		public static void CreateColonistNeeds() {
			List<string> needDataStringList = Resources.Load<TextAsset>(@"Data/colonist-needs").text.Replace("\t", string.Empty).Split(new string[] { "<Need>" }, StringSplitOptions.RemoveEmptyEntries).ToList();
			foreach (string singleNeedDataString in needDataStringList) {

				ENeed type = ENeed.Rest;
				float baseIncreaseRate = 0;
				float decreaseRateMultiplier = 0;
				bool minValueAction = false;
				float minValue = 0;
				bool maxValueAction = false;
				float maxValue = 0;
				bool critValueAction = false;
				float critValue = 0;
				bool canDie = false;
				float healthDecreaseRate = 0;
				int clampValue = 0;
				int priority = 0;
				Dictionary<ETrait, float> traitsAffectingThisNeed = new Dictionary<ETrait, float>();
				List<string> relatedJobs = new List<string>();

				List<string> singleNeedDataLineStringList = singleNeedDataString.Split('\n').ToList();
				foreach (string singleNeedDataLineString in singleNeedDataLineStringList.Skip(1)) {
					if (!string.IsNullOrEmpty(singleNeedDataLineString)) {

						string label = singleNeedDataLineString.Split('>')[0].Replace("<", string.Empty);
						string value = singleNeedDataLineString.Split('>')[1];

						switch (label) {
							case "Type":
								type = (ENeed)Enum.Parse(typeof(ENeed), value);
								break;
							case "BaseIncreaseRate":
								baseIncreaseRate = float.Parse(value);
								break;
							case "DecreaseRateMultiplier":
								decreaseRateMultiplier = float.Parse(value);
								break;
							case "MinValueAction":
								minValueAction = bool.Parse(value);
								break;
							case "MinValue":
								minValue = float.Parse(value);
								break;
							case "MaxValueAction":
								maxValueAction = bool.Parse(value);
								break;
							case "MaxValue":
								maxValue = float.Parse(value);
								break;
							case "CritValueAction":
								critValueAction = bool.Parse(value);
								break;
							case "CritValue":
								critValue = float.Parse(value);
								break;
							case "CanDie":
								canDie = bool.Parse(value);
								break;
							case "HealthDecreaseRate":
								healthDecreaseRate = float.Parse(value);
								break;
							case "ClampValue":
								clampValue = int.Parse(value);
								break;
							case "Priority":
								priority = int.Parse(value);
								break;
							case "TraitsAffectingThisNeed":
								if (!string.IsNullOrEmpty(StringUtilities.RemoveNonAlphanumericChars(value))) {
									foreach (string traitsAffectingThisNeedString in value.Split(',')) {
										ETrait traitEnum = (ETrait)Enum.Parse(typeof(ETrait), traitsAffectingThisNeedString.Split(':')[0]);
										float multiplier = float.Parse(traitsAffectingThisNeedString.Split(':')[1]);
										traitsAffectingThisNeed.Add(traitEnum, multiplier);
									}
								}
								break;
							case "RelatedJobs":
								if (!string.IsNullOrEmpty(StringUtilities.RemoveNonAlphanumericChars(value))) {
									foreach (string relatedJobString in value.Split(',')) {
										string jobTypeEnum = relatedJobString;
										relatedJobs.Add(jobTypeEnum);
									}
								}
								break;
							default:
								MonoBehaviour.print("Unknown need label: \"" + singleNeedDataLineString + "\"");
								break;
						}
					}
				}

				needPrefabs.Add(
					new NeedPrefab(
						type,
						baseIncreaseRate,
						decreaseRateMultiplier,
						minValueAction,
						minValue,
						maxValueAction,
						maxValue,
						critValueAction,
						critValue,
						canDie,
						healthDecreaseRate,
						clampValue,
						priority,
						traitsAffectingThisNeed,
						relatedJobs,
						NeedUtilities.NeedToSpecialValueFunctionMap[type],
						NeedUtilities.NeedToReactionFunctionMap[type]
					)
				);
			}
		}

	}
}
