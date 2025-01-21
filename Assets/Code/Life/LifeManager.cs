using Snowship.Selectable;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class LifeManager : IManager {

	public List<Life> life = new List<Life>();

	public class Life : ISelectable {
		public float Health { get; protected set; }

		public GameObject obj;

		public bool overTileChanged = false;
		public TileManager.Tile overTile;
		public List<Sprite> moveSprites = new List<Sprite>();

		public Gender gender;

		public bool dead = false;

		public bool visible;

		public enum Gender { Male, Female };

		public event Action<float> OnHealthChanged;

		public Life(TileManager.Tile spawnTile, float startingHealth) {
			gender = GetRandomGender();

			overTile = spawnTile;
			obj = MonoBehaviour.Instantiate(GameManager.resourceM.tilePrefab, overTile.obj.transform.position, Quaternion.identity);
			obj.GetComponent<SpriteRenderer>().sortingOrder = 10; // Life Sprite

			previousPosition = obj.transform.position;

			Health = startingHealth;

			GameManager.lifeM.life.Add(this);
		}

		public static Gender GetRandomGender() {
			return (Gender)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Gender)).Length);
		}

		public virtual void Update() {
			overTileChanged = false;
			TileManager.Tile newOverTile = GameManager.colonyM.colony.map.GetTileFromPosition(obj.transform.position);
			if (overTile != newOverTile) {
				overTileChanged = true;
				overTile = newOverTile;
				SetColour(overTile.sr.color);
				SetVisible(overTile.visible);
			}
			MoveToTile(null, false);
			SetMoveSprite();
		}

		private static readonly List<int> moveSpriteIndices = new List<int>() {
			1,
			2,
			0,
			3,
			1,
			0,
			0,
			1
		};
		private float moveTimer;
		public int startPathLength = 0;
		public List<TileManager.Tile> path = new List<TileManager.Tile>();
		public float moveSpeedMultiplier = 1f;
		public Vector2 previousPosition;
		private float moveSpeedRampingMultiplier;

		public bool MoveToTile(TileManager.Tile tile, bool allowEndTileNonWalkable) {
			if (tile != null) {
				startPathLength = 0;
				path = PathManager.FindPathToTile(overTile, tile, allowEndTileNonWalkable);
				startPathLength = path.Count;
				moveTimer = 0;
				previousPosition = obj.transform.position;
			}
			if (path.Count > 0) {

				obj.transform.position = Vector2.Lerp(previousPosition, path[0].obj.transform.position, moveTimer);

				if (moveTimer >= 1f) {
					previousPosition = obj.transform.position;
					obj.transform.position = path[0].obj.transform.position;
					moveTimer = 0;
					path.RemoveAt(0);
				} else {
					moveSpeedRampingMultiplier = Mathf.Clamp01(moveSpeedRampingMultiplier + GameManager.timeM.Time.DeltaTime);
					moveTimer += 2 * GameManager.timeM.Time.DeltaTime * overTile.walkSpeed * moveSpeedMultiplier * moveSpeedRampingMultiplier;
				}
			} else {
				path.Clear();
				if (!Mathf.Approximately(Vector2.Distance(obj.transform.position, overTile.obj.transform.position), 0f)) {
					path.Add(overTile);
					moveTimer = 0;
				}
				moveSpeedRampingMultiplier = 0;
				return true;
			}
			return false;
		}

		public int CalculateMoveSpriteIndex() {
			int moveSpriteIndex = 0;
			if (path.Count > 0) {
				TileManager.Tile previousTile = GameManager.colonyM.colony.map.GetTileFromPosition(previousPosition);
				if (previousTile != path[0]) {
					moveSpriteIndex = previousTile.surroundingTiles.IndexOf(path[0]);
					if (moveSpriteIndex == -1) {
						moveSpriteIndex = 0;
					}
					moveSpriteIndex = moveSpriteIndices[moveSpriteIndex];
				}
			}
			return moveSpriteIndex;
		}

		private void SetMoveSprite() {
			obj.GetComponent<SpriteRenderer>().sprite = moveSprites[CalculateMoveSpriteIndex()];
		}

		public virtual void SetColour(Color newColour) {
			obj.GetComponent<SpriteRenderer>().color = new Color(newColour.r, newColour.g, newColour.b, 1f);
		}

		public void ChangeHealthValue(float amount) {
			dead = false;
			Health += amount;
			if (Health > 1f) {
				Health = 1f;
			} else if (Health <= 0f) {
				Health = 0f;
				dead = true;
			}
			OnHealthChanged?.Invoke(Health);
		}

		public virtual void Die() {
			// TODO Implement death (instance still exists but will be in a death-state)
		}

		public virtual void Remove() {
			Object.Destroy(obj);
			GameManager.lifeM.life.Remove(this);
		}

		public void SetVisible(bool visible) {
			this.visible = visible;

			obj.SetActive(visible);
		}

		void ISelectable.Select() {

		}

		void ISelectable.Deselect() {

		}
	}
}
