﻿using System;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Menu.LoadSave {
	internal class UniverseElement {

		public static event Action<UniverseElement> OnUniverseElementClicked;

		public PersistenceManager.PersistenceUniverse persistenceUniverse;

		public GameObject obj;

		public UniverseElement(PersistenceManager.PersistenceUniverse persistenceUniverse, Transform parent) {
			this.persistenceUniverse = persistenceUniverse;

			obj = MonoBehaviour.Instantiate(Resources.Load<GameObject>(@"UI/UIElements/UniverseElement-Panel"),
				parent, false);

			obj.transform.Find("UniverseName-Text").GetComponent<Text>().text =
				persistenceUniverse.universeProperties[PersistenceManager.UniverseProperty.Name];

			obj.transform.Find("LastSaveData-Panel/LastSavedDateTime-Text").GetComponent<Text>().text =
				persistenceUniverse.universeProperties[PersistenceManager.UniverseProperty.LastSaveDateTime];

			string saveVersion =
				persistenceUniverse.configurationProperties[PersistenceManager.ConfigurationProperty.SaveVersion];
			string gameVersion =
				persistenceUniverse.configurationProperties[PersistenceManager.ConfigurationProperty.GameVersion];

			string saveVersionColour =
				ColorUtility.ToHtmlStringRGB(ColourUtilities.GetColour(ColourUtilities.EColour.DarkGrey50));
			string gameVersionColour =
				ColorUtility.ToHtmlStringRGB(ColourUtilities.GetColour(ColourUtilities.EColour.DarkGrey50));

			if (saveVersion == PersistenceManager.saveVersion.text) {
				obj.GetComponent<Button>().onClick.AddListener(delegate {
					OnUniverseElementClicked?.Invoke(this);
				});
				if (gameVersion != PersistenceManager.gameVersion.text) {
					obj.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkYellow);
					gameVersionColour = ColorUtility.ToHtmlStringRGB(
						(ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed) +
						ColourUtilities.GetColour(ColourUtilities.EColour.DarkGrey50)) / 2f);
				}
			}
			else {
				obj.GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed);
				obj.GetComponent<Button>().interactable = false;

				if (saveVersion != PersistenceManager.saveVersion.text) {
					saveVersionColour = ColorUtility.ToHtmlStringRGB(
						(ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed) +
						ColourUtilities.GetColour(ColourUtilities.EColour.DarkGrey50)) / 2f);
				}

				if (gameVersion != PersistenceManager.gameVersion.text) {
					gameVersionColour = ColorUtility.ToHtmlStringRGB(
						(ColourUtilities.GetColour(ColourUtilities.EColour.DarkRed) +
						ColourUtilities.GetColour(ColourUtilities.EColour.DarkGrey50)) / 2f);
				}
			}

			obj.transform.Find("LastSaveData-Panel/VersionGameSave-Text").GetComponent<Text>().text = string.Format(
				"{0}{1}{2}{3} {4}{5}{6}{7}",
				"<color=#" + gameVersionColour + ">",
				"G",
				persistenceUniverse.configurationProperties[PersistenceManager.ConfigurationProperty.GameVersion],
				"</color>",
				"<color=#" + saveVersionColour + ">",
				"S",
				persistenceUniverse.configurationProperties[PersistenceManager.ConfigurationProperty.SaveVersion],
				"</color>"
			);
		}

		public void Delete() {
			MonoBehaviour.Destroy(obj);
		}
	}
}
