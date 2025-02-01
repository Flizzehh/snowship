using System.Linq;
using Snowship.NColonist;
using Snowship.NResource;
using Snowship.NTime;

namespace Snowship.NJob
{
	[RegisterJob("Needs", "Rest", "Sleep")]
	public class SleepJob : Job
	{
		private Colonist colonist;
		private NeedInstance sleepNeed;
		private float startSleepNeedValue;
		private float sleepTime;
		private Bed bed;

		public SleepJob(TileManager.Tile tile) : base(tile) {
			Description = "Sleeping.";
			Returnable = false;
		}

		protected override void OnJobTaken() {
			base.OnJobTaken();

			bed = Bed.Beds
				.Where(b => b.Occupant == null && b.tile.region == Worker.overTile.region)
				.OrderByDescending(b => b.prefab.restComfortAmount)
				.ThenByDescending(b => PathManager.RegionBlockDistance(Worker.overTile.regionBlock, b.tile.regionBlock, true, true, false))
				.FirstOrDefault();

			if (bed != null) {
				ChangeTile(bed.tile);
			}

			colonist = (Colonist)Worker; // TODO Remove cast when Humans have Job ability
			sleepNeed = colonist.needs.Find(n => n.prefab.name == "Sleep");
			startSleepNeedValue = sleepNeed.GetValue();
			sleepTime = SimulationDateTime.DayLengthSeconds * (8 / 24f) - (bed?.prefab.restComfortAmount).GetValueOrDefault(0);
		}

		protected override void OnJobStarted() {
			base.OnJobStarted();

			bed.StartSleeping(Worker);
		}

		protected override void OnJobInProgress() {
			base.OnJobInProgress();

			sleepNeed?.ChangeValue(-(startSleepNeedValue / sleepTime));
		}

		protected override void OnJobReturned() {
			base.OnJobReturned();

			bed?.StopSleeping();
		}

		protected override void OnJobFinished() {
			base.OnJobFinished();

			if (bed != null) {
				bed.StopSleeping();
				colonist.Moods.AddMoodModifier(bed.prefab.restComfortAmount >= 10 ? MoodModifierEnum.WellRested : MoodModifierEnum.Rested);
			} else {
				colonist.Moods.AddMoodModifier(MoodModifierEnum.SleptOnTheGround);
			}
		}
	}
}