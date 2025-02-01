using Snowship.NColonist;
using Snowship.NResource;
using UnityEngine;

namespace Snowship.NJob
{
	[RegisterJob("Task", "Build", "Build")]
	public class BuildJob : Job
	{
		public ObjectPrefab ObjectPrefab { get; }
		public Variation Variation { get; }
		public int Rotation { get; }

		public BuildJob(TileManager.Tile tile, ObjectPrefab objectPrefab, Variation variation, int rotation) : base(tile) {
			Variation = variation;
			ObjectPrefab = objectPrefab;
			Rotation = rotation;

			Description = $"Building a {objectPrefab.Name}.";
			RequiredResources.AddRange(objectPrefab.commonResources);
			RequiredResources.AddRange(variation.uniqueResources);
			SetTimeToWork(ObjectPrefab.timeToBuild);

			JobPreviewObject = Object.Instantiate(
				GameManager.Get<ResourceManager>().tilePrefab, // TODO Create separate Class/Prefab for JobPreviewObject
				Tile.obj.transform,
				false
			);
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
			SkillInstance skill = ((Colonist)Worker).GetSkillFromJobType(Name);
			skill?.AddExperience(ObjectPrefab.timeToBuild);

			colonist.MoveToClosestWalkableTile(true);
		}
	}
}