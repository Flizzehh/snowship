using System;
using System.Collections.Generic;
using Snowship.NColonist;
using Snowship.NResource;
using Snowship.NUtilities;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Build", "Build", "Build")]
	public class BuildJobDefinition : JobDefinition<BuildJob>
	{
		public override Func<TileManager.Tile, int, bool>[] SelectionConditions { get; protected set; } = {
			Selectable.SelectionConditions.WalkableIncludingFences,
			Selectable.SelectionConditions.Buildable,
			Selectable.SelectionConditions.NoPlant,
			Selectable.SelectionConditions.NoSameLayerObject,
			Selectable.SelectionConditions.NoSameLayerJobs
		};

		public override int Layer { get; protected set; } = 100;

		public BuildJobDefinition(IGroupItem group, IGroupItem subGroup, string name) : base(group, subGroup, name) {
		}
	}

	public class BuildJobParams : IJobParams
	{
		public List<Func<TileManager.Tile, int, bool>> SelectionConditions { get; }
		public Sprite JobPreviewSprite { get; }

		public readonly ObjectPrefab ObjectPrefab;
		public readonly Variation Variation;
		public readonly int Layer;
		public readonly int Rotation;

		public BuildJobParams(ObjectPrefab objectPrefab, Variation variation, int rotation) {
			ObjectPrefab = objectPrefab;
			Variation = variation;
			Layer += ObjectPrefab.layer;
			Rotation = rotation;

			// SelectionConditions.AddRange(ObjectPrefab.SelectionConditions); // TODO
			JobPreviewSprite = ObjectPrefab.canRotate
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
			Layer = args.Layer;

			Description = $"Building a {ObjectPrefab.Name}.";
			RequiredResources.AddRange(ObjectPrefab.commonResources);
			if (Variation != null) {
				RequiredResources.AddRange(Variation.uniqueResources);
			}
			SetTimeToWork(ObjectPrefab.timeToBuild);

			JobPreviewObject.GetComponent<SpriteRenderer>().sprite = ObjectPrefab.GetBitmaskSpritesForVariation(Variation)[Rotation];
			JobPreviewObject.GetComponent<SpriteRenderer>().sortingOrder = (int)SortingOrder.Job + Layer;
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

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

			Worker.MoveToClosestWalkableTile(true);
		}
	}
}