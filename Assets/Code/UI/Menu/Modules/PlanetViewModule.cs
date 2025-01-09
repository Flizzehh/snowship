using System;
using System.Collections.Generic;
using Snowship.NPlanet;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Snowship.NUI.Modules {
	public class PlanetViewModule {

		private readonly GridLayoutGroup planetGrid;
		private readonly RectTransform planetGridRectTransform;
		private readonly GameObject planetTilePrefab;

		private Planet currentPlanet;
		private readonly List<GameObject> planetTileGameObjects = new List<GameObject>();

		public event Action<PlanetTile> OnPlanetTileClicked;
		public event Action<PersistenceManager.PersistenceColony> OnColonyTileClicked;

		public PlanetViewModule(GridLayoutGroup planetGrid, GameObject planetTilePrefab) {
			this.planetGrid = planetGrid;
			this.planetTilePrefab = planetTilePrefab;
			planetGridRectTransform = planetGrid.GetComponent<RectTransform>();
		}

		public void DestroyPlanet() {
			foreach (GameObject planetTileGameObject in planetTileGameObjects) {
				Object.Destroy(planetTileGameObject);
			}

			planetTileGameObjects.Clear();
		}

		public void DisplayPlanet(
			Planet planet,
			List<PersistenceManager.PersistenceColony> persistenceColonies,
			bool destroyCurrentPlanet
		) {
			bool instantiateNewPlanetTiles = destroyCurrentPlanet || planetTileGameObjects.Count == 0 || currentPlanet.mapData.mapSize != planet.mapData.mapSize;
			if (instantiateNewPlanetTiles) {
				DestroyPlanet();
			}

			planetGrid.cellSize = planetGridRectTransform.rect.size / planet.mapData.mapSize;
			planetGrid.constraintCount = planet.mapData.mapSize;

			for (int i = 0; i < planet.planetTiles.Count; i++) {
				PlanetTile planetTile = planet.planetTiles[i];

				GameObject planetTileGameObject;
				if (instantiateNewPlanetTiles) {
					planetTileGameObject = Object.Instantiate(planetTilePrefab, planetGridRectTransform, false);
					planetTileGameObject.name = $"PlanetTile {planetTile.tile.position}";
					planetTileGameObject.GetComponent<Button>().onClick.AddListener(() => OnPlanetTileClicked?.Invoke(planetTile));
				} else {
					planetTileGameObject = planetTileGameObjects[i];
				}
				planetTileGameObject.GetComponent<Image>().sprite = planetTile.sprite;

				PersistenceManager.PersistenceColony colony = persistenceColonies?.Find(colony => colony.planetPosition == planetTile.tile.position);
				if (colony != null) {
					GameObject colonyObj = Object.Instantiate(GameManager.resourceM.colonyObj, planetTileGameObject.transform, false);
					colonyObj.name = $"Colony {colony.name}";
					colonyObj.GetComponent<Button>().onClick.AddListener(() => OnColonyTileClicked?.Invoke(colony));
				}

				planetTileGameObjects.Add(planetTileGameObject);
			}
		}
	}
}
