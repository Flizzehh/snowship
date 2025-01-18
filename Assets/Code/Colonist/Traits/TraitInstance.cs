namespace Snowship.NColonist {
	public class TraitInstance {

		public Colonist colonist;
		public TraitPrefab prefab;

		public TraitInstance(Colonist colonist, TraitPrefab prefab) {
			this.colonist = colonist;
			this.prefab = prefab;
		}

	}
}
