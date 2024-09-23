using System.Collections.Generic;
using Snowship.NUI.Menu.LoadSave;
using Snowship.NUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI.Menu.LoadSave {

	/*
	public class CreateUniverseView : IUIView {

		[Header("Load Universe")]
		[SerializeField] private Transform loadUniversePanel;

		[Header("Create Universe")]
		[SerializeField] private GameObject createUniversePanel;
		[SerializeField] private InputField universeNameInputField;

		private Transform universesListPanel;

		private Button loadUniverseButton;

		private readonly List<UniverseElement> universeElements = new List<UniverseElement>();

		private UniverseElement selectedUniverseElement;





		private void SetSelectedUniverseElement(UniverseElement universeElement) {
			selectedUniverseElement = universeElement;

			loadUniverseButton.interactable = selectedUniverseElement != null;
			if (selectedUniverseElement != null) {
				loadUniverseButton.transform.Find("Text").GetComponent<Text>().text = $"Load {selectedUniverseElement.persistenceUniverse.universeProperties[PersistenceManager.UniverseProperty.Name]}";
			}
			else {
				loadUniverseButton.transform.Find("Text").GetComponent<Text>().text = "Select a Universe to Load";
			}
		}

		private void SetupLoadUniverseUI(Transform mainMenu) {
			loadUniversePanel = mainMenu.Find("LoadUniverse-Panel");

			Button backButton = loadUniversePanel.transform.Find("Back-Button").GetComponent<Button>();
			backButton.onClick.AddListener(delegate {
				GameManager.universeM.SetUniverse(null);
				GameManager.planetM.SetPlanet(null);
				GameManager.colonyM.SetColony(null);

				SetSelectedUniverseElement(null);

				SetLoadUniverseActive(false);
			});

			universesListPanel = loadUniversePanel.transform.Find("Universes-ScrollPanel/UniversesList-Panel");

			loadUniverseButton = loadUniversePanel.transform.Find("LoadUniverse-Button").GetComponent<Button>();
			loadUniverseButton.onClick.AddListener(delegate {
				if (selectedUniverseElement != null) {
					GameManager.persistenceM.ApplyLoadedConfiguration(selectedUniverseElement.persistenceUniverse);
					GameManager.persistenceM.ApplyLoadedUniverse(selectedUniverseElement.persistenceUniverse);

					SetSelectedUniverseElement(null);

					SetLoadUniverseActive(false);
					SetLoadPlanetActive(true);
				}
			});
			SetSelectedUniverseElement(null);
		}

		private void SetLoadUniverseActive(bool active) {
			loadUniversePanel.gameObject.SetActive(active);
			ToggleMainMenuButtons(loadUniversePanel);

			foreach (UniverseElement universeElement in universeElements) {
				universeElement.Delete();
			}

			universeElements.Clear();

			if (loadUniversePanel.gameObject.activeSelf) {
				foreach (PersistenceManager.PersistenceUniverse persistenceUniverse in GameManager.persistenceM.GetPersistenceUniverses()) {
					universeElements.Add(new UniverseElement(persistenceUniverse, universesListPanel));
				}
			}
		}



		private void SetupCreateUniverseUI(Transform mainMenu) {
			createUniversePanel = mainMenu.transform.Find("CreateUniverse-Panel").gameObject;

			Button backButton = createUniversePanel.transform.Find("Back-Button").GetComponent<Button>();
			backButton.onClick.AddListener(delegate {
				GameManager.universeM.SetUniverse(null);
				GameManager.planetM.SetPlanet(null);
				GameManager.colonyM.SetColony(null);

				universeNameInputField.text = UniverseManager.GetRandomUniverseName();

				SetCreateUniverseActive(false);
			});

			universeNameInputField = createUniversePanel.transform.Find("UniverseName-Panel/InputField").GetComponent<InputField>();

			Button saveUniverseButton = createUniversePanel.transform.Find("SaveUniverse-Button").GetComponent<Button>();
			saveUniverseButton.onClick.AddListener(delegate {
				GameManager.universeM.CreateUniverse(universeNameInputField.text);
				GameManager.planetM.SetPlanet(null);
				GameManager.colonyM.SetColony(null);

				universeNameInputField.text = UniverseManager.GetRandomUniverseName();

				SetCreateUniverseActive(false);
				SetCreatePlanetActive(true);
			});
			saveUniverseButton.interactable = false;
			saveUniverseButton.transform.Find("Image").GetComponent<Image>().color = ColourUtilities.GetColour(ColourUtilities.Colours.Grey120);

			universeNameInputField.onValueChanged.AddListener(delegate {
				ValidateUniverseName(universeNameInputField, saveUniverseButton);
			});

			universeNameInputField.text = UniverseManager.GetRandomUniverseName();
			ValidateUniverseName(universeNameInputField, saveUniverseButton);
		}

		private void ValidateUniverseName(InputField universeNameInputField, Button saveUniverseButton) {
			bool validUniverseName = !string.IsNullOrEmpty(universeNameInputField.text) && StringUtilities.IsAlphanumericWithSpaces(universeNameInputField.text);
			saveUniverseButton.interactable = validUniverseName;
			saveUniverseButton.transform.Find("Image").GetComponent<Image>().color = validUniverseName
				? ColourUtilities.GetColour(ColourUtilities.Colours.LightGrey220)
				: ColourUtilities.GetColour(ColourUtilities.Colours.Grey120);
		}

		private void SetCreateUniverseActive(bool active) {
			createUniversePanel.SetActive(active);
			ToggleMainMenuButtons(createUniversePanel);
		}

		private void SubscribeEvents() {
			UniverseElement.OnUniverseElementClicked += SetSelectedUniverseElement;
		}

		private void UnsubscribeEvents() {
			UniverseElement.OnUniverseElementClicked -= SetSelectedUniverseElement;
		}
	}
	*/
}
