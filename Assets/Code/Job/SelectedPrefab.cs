using Snowship.NResource;

namespace Snowship.NJob
{
	public class SelectedPrefab
	{
		public readonly ObjectPrefab prefab;
		public readonly Variation variation;

		public SelectedPrefab(
			// Update the Equals method below whenever adding/removing parameters
			ObjectPrefab prefab,
			Variation variation
		) {
			this.prefab = prefab;
			this.variation = variation;
		}
	}
}