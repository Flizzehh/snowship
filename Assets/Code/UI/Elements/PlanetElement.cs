using System;
using Snowship.NPersistence;
using Snowship.NUI.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Snowship.NUI.Menu.LoadSave {
	internal class PlanetElement {
		public PersistencePlanet persistencePlanet;

		public GameObject obj;

		public event Action OnPlanetElementClicked;

		public PlanetElement(PersistencePlanet persistencePlanet, Transform parent) {
			this.persistencePlanet = persistencePlanet;

			obj = Object.Instantiate(
				Resources.Load<GameObject>(@"UI/UIElements/PlanetElement-Panel"),
				parent,
				false
			);

			obj.transform.Find("PlanetName-Text").GetComponent<Text>().text = persistencePlanet.name;

			obj.transform.Find("LastSaveData-Panel/LastSavedDateTime-Text").GetComponent<Text>().text = persistencePlanet.lastSaveDateTime;

			obj.GetComponent<Button>().onClick.AddListener(delegate {
				// TODO GameManager.uiMOld.SetSelectedPlanetElement(this);
			});
		}

		public void Delete() {
			MonoBehaviour.Destroy(obj);
		}
	}
}
