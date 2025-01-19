using Snowship.NJob;
using Snowship.NProfession;
using Snowship.Selectable;
using Snowship.NTime;
using System.Collections.Generic;
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

	[SerializeField] private Transform uiParent;

	public static readonly CameraManager cameraM = new CameraManager();
	public static readonly CaravanManager caravanM = new CaravanManager();
	public static readonly ColonistManager colonistM = new ColonistManager();
	public static readonly ColonyManager colonyM = new ColonyManager();
	public static readonly DebugManager debugM = new DebugManager();
	public static readonly HumanManager humanM = new HumanManager();
	public static readonly InputManager inputM = new InputManager();
	public static readonly JobManager jobM = new JobManager();
	public static readonly LifeManager lifeM = new LifeManager();
	public static readonly PersistenceManager persistenceM = new PersistenceManager();
	public static readonly PlanetManager planetM = new PlanetManager();
	public static readonly ResourceManager resourceM = new ResourceManager();
	public static readonly SelectableManager selectableManager = new SelectableManager();
	public static readonly StateManager stateM = new StateManager();
	public static readonly TileManager tileM = new TileManager();
	public static readonly TimeManager timeM = new TimeManager();
	public static readonly IUIManager uiM = new UIManager();
	public static readonly UIManagerOld uiMOld = new UIManagerOld();
	public static readonly UniverseManager universeM = new UniverseManager();

	private static readonly List<IManager> managers = new List<IManager>() {
		stateM,
		inputM,

		timeM,
		debugM,

		resourceM,

		lifeM,
		humanM,
		colonistM,
		jobM,
		caravanM,


		tileM,
		planetM,
		universeM,

		persistenceM,

		uiM,
		uiMOld,
		cameraM,

		selectableManager
	};

	public void Awake() {

		// Initializations

		uiM.Initialize(uiParent);

		// Awakes

		resourceM.SetResourceReferences();
		resourceM.SetGameObjectReferences();
		resourceM.CreateJobPrefabs();
		resourceM.CreateResources();
		resourceM.CreatePlantPrefabs();
		resourceM.CreateObjectPrefabs();
		resourceM.LoadLocationNames();

		TileManager.TileType.InitializeTileTypes();
		TileManager.Biome.InitializeBiomes();
		TileManager.ResourceVein.InitializeResourceVeins();

		humanM.CreateNames();
		humanM.CreateHumanSprites();

		SkillPrefab.CreateColonistSkills(); // TODO (Solution: Use string references which can be converted to the correct Prefab obj when needed) Skills must currently be ahead of professions to determine skill-profession relationship
		ProfessionPrefab.CreateProfessionPrefabs();
		NeedPrefab.CreateColonistNeeds();
		MoodModifierGroup.CreateMoodModifiers();

		foreach (IManager manager in managers) {
			manager.Awake();
		}

		uiMOld.SetupUI();

		persistenceM.PSettings.CreateSettingsState();
	}

	public void Start() {
		foreach (IManager manager in managers) {
			manager.Start();
		}
	}

	public void Update() {
		foreach (IManager manager in managers) {
			manager.Update();
		}
	}
}
