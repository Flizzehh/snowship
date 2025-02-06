using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Snowship.NColonist;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob
{
	[UsedImplicitly]
	[RegisterJob("Build", "Build", "Build")]
	public class BuildJobDefinition : JobDefinition
	{
		public override Func<TileManager.Tile, int, bool>[] SelectionConditions { get; protected set; } = {
			Selectable.SelectionConditions.WalkableIncludingFences,
			Selectable.SelectionConditions.Buildable,
			Selectable.SelectionConditions.NoPlant,
			Selectable.SelectionConditions.NoSameLayerObject,
			Selectable.SelectionConditions.NoSameLayerJobs
		};

		public BuildJobDefinition(IGroupItem group, IGroupItem subGroup, string name, Sprite icon) : base(group, subGroup, name, icon) {
		}
	}

	public class BuildJobParams : IJobParams
	{
		public List<Func<TileManager.Tile, int, bool>> SelectionConditions { get; }
		public Sprite SelectedJobPreviewSprite { get; }

		public ObjectPrefab ObjectPrefab;
		public Variation Variation;
		public int Rotation;

		public BuildJobParams(ObjectPrefab objectPrefab, Variation variation, int rotation) {
			ObjectPrefab = objectPrefab;
			Variation = variation;
			Rotation = rotation;

			// SelectionConditions.AddRange(ObjectPrefab.SelectionConditions); // TODO
			SelectedJobPreviewSprite = ObjectPrefab.canRotate
				? ObjectPrefab.GetBitmaskSpritesForVariation(Variation)[Rotation]
				: ObjectPrefab.GetBaseSpriteForVariation(Variation);
		}
	}

	public class BuildJob : Job<BuildJobDefinition>
	{
		public ObjectPrefab ObjectPrefab { get; }
		public Variation Variation { get; }
		public int Rotation { get; }

		public BuildJob(TileManager.Tile tile, BuildJobParams args) : base(tile) {
			Variation = args.Variation;
			ObjectPrefab = args.ObjectPrefab;
			Rotation = args.Rotation;

			Description = $"Building a {ObjectPrefab.Name}.";
			RequiredResources.AddRange(ObjectPrefab.commonResources);
			if (Variation != null) {
				RequiredResources.AddRange(Variation.uniqueResources);
			}
			SetTimeToWork(ObjectPrefab.timeToBuild);

			JobPreviewObject.GetComponent<SpriteRenderer>().sprite = ObjectPrefab.GetBitmaskSpritesForVariation(Variation)[Rotation];
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			Colonist colonist = (Colonist)Worker; // TODO Remove when Human has Job ability

			if (ObjectPrefab.subGroupType == ObjectPrefabSubGroup.ObjectSubGroupEnum.Roofs) {
				Tile.SetRoof(true);
				return;
			}

			ObjectInstance objectInstance = ObjectInstance.CreateObjectInstance(ObjectPrefab, Variation, Tile, Rotation, true);
			Tile.SetObject(objectInstance);
			objectInstance.sr.color = Color.white; // TODO This should be in FinishCreation or SetObject probably?
			objectInstance.FinishCreation();
			if (ObjectPrefab.canRotate) {
				objectInstance.sr.sprite = ObjectPrefab.GetBitmaskSpritesForVariation(Variation)[Rotation];
			}

			// TODO Move this to Job parent class
			SkillInstance skill = ((Colonist)Worker).GetSkillFromJobType(Definition.Name);
			skill?.AddExperience(ObjectPrefab.timeToBuild);

			colonist.MoveToClosestWalkableTile(true);
		}
	}
}