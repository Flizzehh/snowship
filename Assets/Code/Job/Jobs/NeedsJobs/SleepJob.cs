using Snowship.NColonist;
using Snowship.NResource;

namespace Snowship.NJob
{
	[RegisterJob("Needs", "Sleep")]
	public class SleepJob : Job
	{
		private readonly SleepSpot sleepSpot;

		protected SleepJob(JobPrefab jobPrefab, TileManager.Tile tile, SleepSpot sleepSpot) : base(jobPrefab, tile) {
			this.sleepSpot = sleepSpot;

			Description = "Sleeping.";

			Returnable = false;
		}

		public override void OnJobFinished() {
			base.OnJobFinished();

			Colonist colonist = (Colonist)Worker; // TODO Remove cast when Humans have Job ability

			if (sleepSpot != null) {
				sleepSpot.StopSleeping();
				if (sleepSpot.prefab.restComfortAmount >= 10) {
					colonist.Moods.AddMoodModifier(MoodModifierEnum.WellRested);
				} else {
					colonist.Moods.AddMoodModifier(MoodModifierEnum.Rested);
				}
			} else {
				colonist.Moods.AddMoodModifier(MoodModifierEnum.Rested);
			}
			foreach (SleepSpot sleepSpot in SleepSpot.sleepSpots) {
				if (sleepSpot.occupyingColonist == colonist) {
					sleepSpot.StopSleeping();
				}
			}
		}
	}
}