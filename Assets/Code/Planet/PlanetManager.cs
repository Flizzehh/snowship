namespace Snowship.NPlanet {
	public class PlanetManager : BaseManager {

		public Planet planet;
		public PlanetTile selectedPlanetTile;
		private readonly PlanetMapDataValues planetMapDataValues = new PlanetMapDataValues();

		public void SetPlanet(Planet planet) {
			this.planet = planet;
		}

		public void SetSelectedPlanetTile(PlanetTile planetTile) {
			selectedPlanetTile = planetTile;
		}

		public Planet CreatePlanet(CreatePlanetData createPlanetData) {
			Planet planet = new Planet(
				createPlanetData.Name,
				new TileManager.MapData(
					null,
					createPlanetData.Seed,
					createPlanetData.Size,
					PlanetMapDataValues.ActualMap,
					PlanetMapDataValues.PlanetTemperature,
					createPlanetData.TemperatureRange,
					createPlanetData.Distance,
					PlanetMapDataValues.AverageTemperature,
					PlanetMapDataValues.AveragePrecipitation,
					planetMapDataValues.terrainTypeHeights,
					planetMapDataValues.surroundingPlanetTileHeightDirections,
					PlanetMapDataValues.River,
					planetMapDataValues.surroundingPlanetTileRivers,
					PlanetMapDataValues.PreventEdgeTouching,
					createPlanetData.WindDirection,
					planetMapDataValues.planetTilePosition
				)
			);

			this.planet = planet;

			return planet;
		}


	}
}
