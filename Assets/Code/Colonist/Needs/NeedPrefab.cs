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

		/*

		1440 seconds per in-game day
		@ 0.01 BRI ... 14.4 points per in-game day
		@ 0.05 BRI ... 72 points per in-game day
		@ 0.1 BRI ... 144 points per in-game day

		Convert days to points between 0-100
		(days * 1440) * BRI = pointsAfterDays

		Convert points between 0-100 to days
		(pointsAfterDays / BRI) / 1440 = days

		0	NeedName/
		1	0.01/		BRI			Base rate of increase per second while conditions met
		2	false/		AMinVB		Whether there is any action taken at MinV
		3	0/			MinV		No action
		4	false/		AMaxVB		Whether there is any action taken at MaxV
		5	0/			MaxV		No action
		6	false/		ACVB		Whether there is any action taken at CV
		7	0/			CV			Value until they begin dying from the need not being fulfilled
		8	false/		DB			Whether they can die from this need not being fulfilled
		9	0.0/		DR			Base rate of health loss due to the need not being fulfilled
		10	100/		ClampV		Value above which the food value will be clamped
		11	0/			Priority	The priority of fulfilling the requirements of this need over others
		12	2/			Number of Affected Traits
		13	TraitName,TraitName/	Names of the affected traits
		14	1.1,1.1`	Multiplier of the BRI for each trait

		*/

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
