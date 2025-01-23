using System.Collections.Generic;
using System.Linq;
using Snowship.NResource;
using UnityEngine;

public class ClothingPrefab
{
	public static readonly List<ClothingPrefab> clothingPrefabs = new();

	public HumanManager.Human.Appearance appearance;
	public Clothing.ClothingEnum clothingType;
	public int insulation;
	public int waterResistance;
	public int weightCapacity;
	public int volumeCapacity;

	public List<string> colours;

	public List<List<Sprite>> moveSprites = new();

	public List<Clothing> clothes = new();

	public ClothingPrefab(
		HumanManager.Human.Appearance appearance,
		Clothing.ClothingEnum clothingType,
		int insulation,
		int waterResistance,
		int weightCapacity,
		int volumeCapacity,
		List<string> colours,
		int typeIndex
	) {
		this.appearance = appearance;
		this.clothingType = clothingType;
		this.insulation = insulation;
		this.waterResistance = waterResistance;
		this.weightCapacity = weightCapacity;
		this.volumeCapacity = volumeCapacity;
		this.colours = colours;

		for (int i = 0; i < 4; i++) {
			moveSprites.Add(Resources.LoadAll<Sprite>(@"Sprites/Clothes/" + appearance + "/clothes-" + appearance.ToString().ToLower() + "-" + i).Skip(typeIndex).Take(colours.Count).ToList());
		}
	}

	public static List<ClothingPrefab> GetClothingPrefabsByAppearance(HumanManager.Human.Appearance appearance) {
		return clothingPrefabs.FindAll(c => c.appearance == appearance);
	}
}
