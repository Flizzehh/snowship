using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Snowship.NHuman;
using Snowship.NResource;
using UnityEngine;
using PU = Snowship.NPersistence.PersistenceUtilities;

namespace Snowship.NPersistence {
	public class PHuman : Human
	{

		private readonly PInventory pInventory = new PInventory();

		public enum HumanProperty {
			Human,
			Name,
			SkinIndex,
			HairIndex,
			Clothes,
			Inventory
		}

		public void WriteHumanLines(StreamWriter file, Human human, int startLevel) {
			file.WriteLine(PU.CreateKeyValueString(HumanProperty.Human, string.Empty, startLevel));

			file.WriteLine(PU.CreateKeyValueString(HumanProperty.Name, human.Name, startLevel + 1));
			file.WriteLine(PU.CreateKeyValueString(HumanProperty.SkinIndex, human.bodyIndices[BodySection.Skin], startLevel + 1));
			file.WriteLine(PU.CreateKeyValueString(HumanProperty.HairIndex, human.bodyIndices[BodySection.Hair], startLevel + 1));

			if (human.clothes.Any(kvp => kvp.Value != null)) {
				file.WriteLine(PU.CreateKeyValueString(HumanProperty.Clothes, string.Empty, startLevel + 1));
				foreach (KeyValuePair<BodySection, Clothing> appearanceToClothing in human.clothes) {
					if (appearanceToClothing.Value != null) {
						file.WriteLine(PU.CreateKeyValueString(appearanceToClothing.Key, appearanceToClothing.Value.prefab.clothingType + ":" + appearanceToClothing.Value.colour, startLevel + 2));
					}
				}
			}

			pInventory.WriteInventoryLines(file, human.Inventory, startLevel + 1);
		}

		public class PersistenceHuman {
			public string name;
			public int? skinIndex;
			public int? hairIndex;
			public Dictionary<BodySection, Clothing> clothes;
			public PInventory.PersistenceInventory persistenceInventory;

			public PersistenceHuman(string name, int? skinIndex, int? hairIndex, Dictionary<BodySection, Clothing> clothes, PInventory.PersistenceInventory persistenceInventory) {
				this.name = name;
				this.skinIndex = skinIndex;
				this.hairIndex = hairIndex;
				this.clothes = clothes;
				this.persistenceInventory = persistenceInventory;
			}
		}

		public List<PersistenceHuman> LoadHumans(string path) {
			List<PersistenceHuman> persistenceHumans = new List<PersistenceHuman>();

			List<KeyValuePair<string, object>> properties = PU.GetKeyValuePairsFromFile(path);
			foreach (KeyValuePair<string, object> property in properties) {
				switch ((HumanProperty)Enum.Parse(typeof(HumanProperty), property.Key)) {
					case HumanProperty.Human:
						persistenceHumans.Add(LoadPersistenceHuman((List<KeyValuePair<string, object>>)property.Value));
						break;
					default:
						Debug.LogError("Unknown human property: " + property.Key + " " + property.Value);
						break;
				}
			}

			return persistenceHumans;
		}

		public PersistenceHuman LoadPersistenceHuman(List<KeyValuePair<string, object>> properties) {
			string name = null;
			int? skinIndex = null;
			int? hairIndex = null;
			Dictionary<BodySection, Clothing> clothes = new();
			PInventory.PersistenceInventory persistenceInventory = null;

			foreach (KeyValuePair<string, object> humanProperty in properties) {
				switch ((HumanProperty)Enum.Parse(typeof(HumanProperty), humanProperty.Key)) {
					case HumanProperty.Name:
						name = (string)humanProperty.Value;
						break;
					case HumanProperty.SkinIndex:
						skinIndex = int.Parse((string)humanProperty.Value);
						break;
					case HumanProperty.HairIndex:
						hairIndex = int.Parse((string)humanProperty.Value);
						break;
					case HumanProperty.Clothes:
						foreach (KeyValuePair<string, object> clothingProperty in (List<KeyValuePair<string, object>>)humanProperty.Value) {
							BodySection clothingPropertyKey = (BodySection)Enum.Parse(typeof(BodySection), clothingProperty.Key);
							Clothing.ClothingEnum clothingType = (Clothing.ClothingEnum)Enum.Parse(typeof(Clothing.ClothingEnum), ((string)clothingProperty.Value).Split(':')[0]);
							string colour = ((string)clothingProperty.Value).Split(':')[1];
							clothes.Add(clothingPropertyKey, Clothing.GetClothesByAppearance(clothingPropertyKey).Find(c => c.prefab.clothingType == clothingType && c.colour == colour));
						}
						break;
					case HumanProperty.Inventory:
						persistenceInventory = pInventory.LoadPersistenceInventory((List<KeyValuePair<string, object>>)humanProperty.Value);
						break;
					default:
						Debug.LogError("Unknown human property: " + humanProperty.Key + " " + humanProperty.Value);
						break;
				}
			}

			return new PersistenceHuman(name, skinIndex, hairIndex, clothes, persistenceInventory);
		}

	}
}