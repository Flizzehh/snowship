using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Snowship.NPersistence {
	public class PLife : PersistenceHandler {

		public enum LifeProperty {
			Life,
			Health,
			Gender,
			Position,
			PreviousPosition,
			PathEndPosition
		}

		public void WriteLifeLines(StreamWriter file, LifeManager.Life life, int startLevel) {
			file.WriteLine(CreateKeyValueString(LifeProperty.Life, string.Empty, startLevel));

			file.WriteLine(CreateKeyValueString(LifeProperty.Health, life.health, startLevel + 1));
			file.WriteLine(CreateKeyValueString(LifeProperty.Gender, life.gender, startLevel + 1));
			file.WriteLine(CreateKeyValueString(LifeProperty.Position, FormatVector2ToString(life.obj.transform.position), startLevel + 1));
			file.WriteLine(CreateKeyValueString(LifeProperty.PreviousPosition, FormatVector2ToString(life.previousPosition), startLevel + 1));
			if (life.path.Count > 0) {
				file.WriteLine(CreateKeyValueString(LifeProperty.PathEndPosition, FormatVector2ToString(life.path[life.path.Count - 1].obj.transform.position), startLevel + 1));
			}
		}

		public class PersistenceLife {
			public float? health;
			public LifeManager.Life.Gender? gender;
			public Vector2? position;
			public Vector2? previousPosition;
			public Vector2? pathEndPosition;

			public PersistenceLife(float? health, LifeManager.Life.Gender? gender, Vector2? position, Vector2? previousPosition, Vector2? pathEndPosition) {
				this.health = health;
				this.gender = gender;
				this.position = position;
				this.previousPosition = previousPosition;
				this.pathEndPosition = pathEndPosition;
			}
		}

		public List<PersistenceLife> LoadLife(string path) {
			List<PersistenceLife> persistenceLife = new List<PersistenceLife>();

			List<KeyValuePair<string, object>> properties = GetKeyValuePairsFromFile(path);
			foreach (KeyValuePair<string, object> property in properties) {
				switch ((LifeProperty)Enum.Parse(typeof(LifeProperty), property.Key)) {
					case LifeProperty.Life:
						persistenceLife.Add(LoadPersistenceLife((List<KeyValuePair<string, object>>)property.Value));
						break;
					default:
						Debug.LogError("Unknown life property: " + property.Key + " " + property.Value);
						break;
				}
			}

			return persistenceLife;
		}

		public PersistenceLife LoadPersistenceLife(List<KeyValuePair<string, object>> properties) {
			float? health = null;
			LifeManager.Life.Gender? gender = null;
			Vector2? position = null;
			Vector2? previousPosition = null;
			Vector2? pathEndPosition = null;

			foreach (KeyValuePair<string, object> lifeProperty in properties) {
				switch ((LifeProperty)Enum.Parse(typeof(LifeProperty), lifeProperty.Key)) {
					case LifeProperty.Health:
						health = float.Parse((string)lifeProperty.Value);
						break;
					case LifeProperty.Gender:
						gender = (LifeManager.Life.Gender)Enum.Parse(typeof(LifeManager.Life.Gender), (string)lifeProperty.Value);
						break;
					case LifeProperty.Position:
						position = new Vector2(float.Parse(((string)lifeProperty.Value).Split(',')[0]), float.Parse(((string)lifeProperty.Value).Split(',')[1]));
						break;
					case LifeProperty.PreviousPosition:
						previousPosition = new Vector2(float.Parse(((string)lifeProperty.Value).Split(',')[0]), float.Parse(((string)lifeProperty.Value).Split(',')[1]));
						break;
					case LifeProperty.PathEndPosition:
						pathEndPosition = new Vector2(float.Parse(((string)lifeProperty.Value).Split(',')[0]), float.Parse(((string)lifeProperty.Value).Split(',')[1]));
						break;
					default:
						Debug.LogError("Unknown life property: " + lifeProperty.Key + " " + lifeProperty.Value);
						break;
				}
			}

			return new PersistenceLife(health, gender, position, previousPosition, pathEndPosition);
		}

	}
}
