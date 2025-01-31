using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NCamera;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NInput;
using Snowship.NLife;
using UnityEngine;

namespace Snowship.NHuman
{
	public class HumanManager : IManager
	{
		private readonly List<string> maleNames = new();
		private readonly List<string> femaleNames = new();

		public List<List<Sprite>> humanMoveSprites = new();

		public event Action<Human> OnHumanSelected;

		public void CreateNames() {
			foreach (string name in Resources.Load<TextAsset>(@"Data/names-male").text.Split(' ').ToList()) {
				maleNames.Add(name.ToLower());
			}
			foreach (string name in Resources.Load<TextAsset>(@"Data/names-female").text.Split(' ').ToList()) {
				femaleNames.Add(name.ToLower());
			}
		}

		public void CreateHumanSprites() {
			for (int i = 0; i < 3; i++) {
				List<Sprite> innerHumanMoveSprites = Resources.LoadAll<Sprite>(@"Sprites/Colonists/colonists-body-base-" + i).ToList();
				humanMoveSprites.Add(innerHumanMoveSprites);
			}
		}

		public Human selectedHuman;

		public void OnUpdate() {
			if (GameManager.Get<TileManager>().mapState == TileManager.MapState.Generated) {
				SetSelectedHumanFromClick();
			}
			if (Input.GetKey(KeyCode.F) && selectedHuman != null) {
				GameManager.Get<CameraManager>().SetCameraPosition(selectedHuman.obj.transform.position);
			}
		}

		public string GetName(Life.Gender gender) {
			string chosenName = "NAME";
			List<string> names = gender == Life.Gender.Male ? maleNames : femaleNames;
			List<string> validNames = names.Where(name => humans.Find(human => human.Name.ToLower() == name.ToLower()) == null).ToList();
			if (validNames.Count > 0) {
				chosenName = validNames[UnityEngine.Random.Range(0, validNames.Count)];
			} else {
				chosenName = names[UnityEngine.Random.Range(0, names.Count)];
			}
			string tempName = chosenName.Take(1).ToList()[0].ToString().ToUpper();
			foreach (char character in chosenName.Skip(1)) {
				tempName += character;
			}
			chosenName = tempName;
			return chosenName;
		}

		public List<Human> humans = new();

		private void SetSelectedHumanFromClick() {
			if (Input.GetMouseButtonDown(0) && !GameManager.Get<InputManager>().IsPointerOverUI()) {
				Vector2 mousePosition = GameManager.Get<CameraManager>().camera.ScreenToWorldPoint(Input.mousePosition);
				Human newSelectedHuman = humans.Find(human => Vector2.Distance(human.obj.transform.position, mousePosition) < 0.5f);
				if (newSelectedHuman != null) {
					SetSelectedHuman(newSelectedHuman);
				} else if (selectedHuman != null && selectedHuman is Colonist colonist) {
					colonist.PlayerMoveToTile(GameManager.Get<ColonyManager>().colony.map.GetTileFromPosition(mousePosition));
				}
			}
			if (Input.GetMouseButtonDown(1)) {
				SetSelectedHuman(null);
			}
		}

		public void SetSelectedHuman(Human human) {
			selectedHuman = human;

			SetSelectedHumanIndicator();

			OnHumanSelected?.Invoke(selectedHuman);
		}

		private GameObject selectedHumanIndicator;

		private void SetSelectedHumanIndicator() {
			if (selectedHumanIndicator != null) {
				MonoBehaviour.Destroy(selectedHumanIndicator);
			}
			if (selectedHuman != null) {
				selectedHumanIndicator = MonoBehaviour.Instantiate(GameManager.Get<ResourceManager>().tilePrefab, selectedHuman.obj.transform, false);
				selectedHumanIndicator.name = "SelectedHumanIndicator";
				selectedHumanIndicator.transform.localScale = new Vector2(1f, 1f) * 1.2f;

				SpriteRenderer sCISR = selectedHumanIndicator.GetComponent<SpriteRenderer>();
				sCISR.sprite = GameManager.Get<ResourceManager>().selectionCornersSprite;
				sCISR.sortingOrder = 20; // Selected Colonist Indicator Sprite
				sCISR.color = new Color(1f, 1f, 1f, 0.75f);
			}
		}
	}
}