using System;
using Snowship.NJob;
using Snowship.NProfession;
using Snowship.Selectable;
using Snowship.NTime;
using System.Collections.Generic;
using Snowship;
using Snowship.NCamera;
using Snowship.NCaravan;
using Snowship.NColonist;
using Snowship.NColony;
using Snowship.NInput;
using Snowship.NPersistence;
using Snowship.NPlanet;
using Snowship.NState;
using Snowship.NUI;
using UnityEngine;

public class GameManager : MonoBehaviour {
	public static SharedReferences SharedReferences { get; private set; }

	private static readonly Dictionary<Type, IManager> managersMap = new();
	private static readonly List<IManager> managers = new();

	public void Awake() {

		SharedReferences = GetComponent<SharedReferences>();

		// Awakes
		CreateManagers();

		foreach (IManager manager in managers) {
			manager.OnCreate();
		}

		SpecialManagerSetups();
	}

	private void CreateManagers() {
		Create<InputManager>();
		Create<TimeManager>();
		Create<CameraManager>();
		Create<ResourceManager>();
		Create<LifeManager>();
		Create<HumanManager>();
		Create<ColonistManager>();
		Create<CaravanManager>();
		Create<JobManager>();
		Create<TileManager>();
		Create<ColonyManager>();
		Create<PlanetManager>();
		Create<UniverseManager>();
		Create<PersistenceManager>();
		Create<UIManager>();
		Create<UIManagerOld>();
		Create<SelectableManager>();
		Create<DebugManager>();
		Create<StateManager>();
	}

	private void SpecialManagerSetups() {
		// TODO Move these to be called from their respective files
		TileManager.TileType.InitializeTileTypes();
		TileManager.Biome.InitializeBiomes();
		TileManager.ResourceVein.InitializeResourceVeins();

		Get<HumanManager>().CreateNames();
		Get<HumanManager>().CreateHumanSprites();

		SkillPrefab.CreateColonistSkills(); // TODO (Solution: Use string references which can be converted to the correct Prefab obj when needed) Skills must currently be ahead of professions to determine skill-profession relationship
		ProfessionPrefab.CreateProfessionPrefabs();
		NeedPrefab.CreateColonistNeeds();
		MoodModifierGroup.CreateMoodModifiers();

		Get<UIManagerOld>().SetupUI();

		Get<PersistenceManager>().PSettings.CreateSettingsState();
	}

	public void Start() {
		foreach (IManager manager in managers) {
			manager.OnGameSetupComplete();
		}
	}

	public void Update() {
		foreach (IManager manager in managers) {
			manager.OnUpdate();
		}
	}

	private static void Create<TManager>() where TManager : class, IManager, new() {
		TManager manager = new();
		managersMap.Add(typeof(TManager), manager);
		managers.Add(manager);
	}

	public static TManager Get<TManager>() where TManager : class, IManager {
		return managersMap[typeof(TManager)] as TManager;
	}
}