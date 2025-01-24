using Snowship.NPlanet;
using Snowship.NUtilities;

namespace Snowship.NColony {
	public class CreateColonyData {

		public string Name { get; private set; }
		public int Seed;
		public PlanetTile PlanetTile;

		public int SizeIndex { get; private set; }
		public int Size { get; private set; }

		public static readonly Range MapSizeIndexRange = new Range(1, 4);

		public CreateColonyData() {
			SetDefaultValues();
		}

		public CreateColonyData(
			string name,
			int seed,
			int size,
			PlanetTile planetTile
		) {
			Name = name;
			Seed = seed;
			Size = size;
			PlanetTile = planetTile;
		}

		private void SetDefaultValues() {
			Name = GenerateRandomColonyName();
			Seed = TileManager.Map.GetRandomMapSeed();

			SetSize(2);
			PlanetTile = null;
		}

		public bool SetName(string name) {
			bool colonyNameValid = IsColonyNameValid(name);
			Name = colonyNameValid ? name : string.Empty;
			return colonyNameValid;
		}

		public bool SetPlanetTile(PlanetTile planetTile) {
			bool planetTileValid = IsPlanetTileValid(planetTile);
			PlanetTile = planetTileValid ? planetTile : null;
			return planetTileValid;
		}

		public void SetSize(int sizeIndex) {
			SizeIndex = sizeIndex;
			Size = CalculateColonySizeByIndex(SizeIndex);
		}

		public static string GenerateRandomColonyName() {
			return GameManager.Get<ResourceManager>().GetRandomLocationName();
		}

		public bool CanCreateColony() {
			return IsColonyNameValid(Name) && IsPlanetTileValid(PlanetTile);
		}

		private bool IsColonyNameValid(string colonyName) {
			return StringUtilities.IsAlphanumericWithSpaces(colonyName);
		}

		private bool IsPlanetTileValid(PlanetTile planetTile) {
			return planetTile != null;
		}

		private static int CalculateColonySizeByIndex(int sizeIndex) {
			return sizeIndex * 50;
		}
	};
}