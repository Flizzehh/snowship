using System.Collections.Generic;
using System.Linq;
using Snowship.NHuman;
using Snowship.NResource;
using UnityEngine;

public class ClothingPrefab
{
	public static readonly List<ClothingPrefab> clothingPrefabs = new();

	public BodySection BodySection;
	public Clothing.ClothingEnum clothingType;
	public int insulation;
	public int waterResistance;
	public int weightCapacity;
	public int volumeCapacity;

	public List<string> colours;

	public List<List<Sprite>> moveSprites = new();

	public List<Clothing> clothes = new();

	public ClothingPrefab(
		BodySection bodySection,
		Clothing.ClothingEnum clothingType,
		int insulation,
		int waterResistance,
		int weightCapacity,
		int volumeCapacity,
		List<string> colours,
		int typeIndex
	) {
		BodySection = bodySection;
		this.clothingType = clothingType;
		this.insulation = insulation;
		this.waterResistance = waterResistance;
		this.weightCapacity = weightCapacity;
		this.volumeCapacity = volumeCapacity;
		this.colours = colours;

		for (int i = 0; i < 4; i++) {
			moveSprites.Add(Resources.LoadAll<Sprite>(@"Sprites/Clothes/" + bodySection + "/clothes-" + bodySection.ToString().ToLower() + "-" + i).Skip(typeIndex).Take(colours.Count).ToList());
		}
	}

	public static List<ClothingPrefab> GetClothingPrefabsByAppearance(BodySection bodySection) {
		return clothingPrefabs.FindAll(c => c.BodySection == bodySection);
	}
}
