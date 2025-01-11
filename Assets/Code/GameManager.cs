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
	public static readonly StateManager stateM = new StateManager();
	public static readonly TileManager tileM = new TileManager();
	public static readonly TimeManager timeM = new TimeManager();
	public static readonly UIManager uiM = new UIManager();
	public static readonly UIManagerOld uiMOld = new UIManagerOld();
	public static readonly UniverseManager universeM = new UniverseManager();

	public static readonly SelectableManager selectableManager = new SelectableManager();

	public static readonly List<IManager> managers = new List<IManager>() {
		stateM,

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

		selectableManager,
		inputM
	};

	public void Awake() {

		// Initializations

		uiM.Initialize(uiParent);

		// Awakes

		tileM.SetStartCoroutineReference(this);
		persistenceM.SetStartCoroutineReference(this);
		uiMOld.SetStartCoroutineReference(this);

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

		colonistM.CreateColonistSkills(); // Skills must currently be ahead of professions to determine skill-profession relationship
		ProfessionPrefab.CreateProfessionPrefabs();
		colonistM.CreateColonistNeeds();
		colonistM.CreateMoodModifiers();

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
