using Snowship.NTime;
using UnityEngine;

public class CameraManager : BaseManager {

	private const int MIN_ZOOM = 1;
	private const int MAX_ZOOM = 20;

	public GameObject cameraGO;
	public Camera cameraComponent;

	private float currentZoom;
	private float targetZoom;
	private float zoomTimer;

	private float cameraSpeedMultiplier;

	public override void Awake() {
		cameraGO = GameObject.Find("Camera");
		cameraComponent = cameraGO.GetComponent<Camera>();
	}

	public Vector2 GetCameraPosition() {
		return cameraGO.transform.position;
	}

	public void SetCameraPosition(Vector2 position) {
		cameraGO.transform.position = position;
	}

	public float GetCameraZoom() {
		return currentZoom;
	}

	public void SetCameraZoom(float newZoom) {
		cameraComponent.orthographicSize = newZoom;

		currentZoom = newZoom;
		targetZoom = newZoom;
	}

	public override void Update() {
		if (GameManager.tileM.mapState != TileManager.MapState.Generated || GameManager.uiM.pauseMenu.activeSelf || GameManager.uiM.playerTyping) {
			return;
		}

		UpdateCameraPosition();
		UpdateCameraZoom();
	}

	private void UpdateCameraPosition() {

		cameraSpeedMultiplier = currentZoom * UnityEngine.Time.deltaTime * TimeManager.permanentDeltaTimeMultiplier;

		cameraGO.transform.Translate(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * cameraSpeedMultiplier);

		if (Input.GetMouseButton(2)) {
			cameraGO.transform.Translate(new Vector2(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")) * cameraSpeedMultiplier);
		}

		cameraGO.transform.position = new Vector2(
			Mathf.Clamp(cameraGO.transform.position.x, 0, GameManager.colonyM.colony.map.mapData.mapSize),
			Mathf.Clamp(cameraGO.transform.position.y, 0, GameManager.colonyM.colony.map.mapData.mapSize)
		);
	}

	private void UpdateCameraZoom() {

		if (Mathf.Approximately(currentZoom, targetZoom)) {
			zoomTimer = 0;
		} else {
			zoomTimer += 1 * UnityEngine.Time.deltaTime;
			zoomTimer = Mathf.Clamp(zoomTimer, 0, 1);
		}

		currentZoom = Mathf.Clamp(
			Mathf.Lerp(currentZoom, targetZoom, zoomTimer),
			MIN_ZOOM,
			MAX_ZOOM * (GameManager.debugM.debugMode ? 25 : 1)
		);

		if (!GameManager.uiM.IsPointerOverUI()) {

			//cameraComponent.orthographicSize -= Mathf.Clamp(Input.GetAxis("Mouse ScrollWheel") + (Input.GetAxis("KeyboardZoom") / 10f), -1f, 1f) * cameraComponent.orthographicSize * Time.deltaTime * 100;

			targetZoom -= Mathf.Clamp(
				Input.GetAxis("Mouse ScrollWheel") + (Input.GetAxis("KeyboardZoom") / 10f),
				-1f,
				1f
			) * currentZoom * UnityEngine.Time.deltaTime * 100;

			float zoomDifference = targetZoom - currentZoom;
			float maxZoomDifference = Mathf.Clamp(currentZoom / 3f, MIN_ZOOM, MAX_ZOOM);
			if (Mathf.Abs(zoomDifference) >= maxZoomDifference) {
				if (zoomDifference >= 0) {
					targetZoom = currentZoom + maxZoomDifference;
				} else {
					targetZoom = currentZoom - maxZoomDifference;
				}
			}

			targetZoom = Mathf.Clamp(
				targetZoom,
				MIN_ZOOM,
				MAX_ZOOM * (GameManager.debugM.debugMode ? 25 : 1)
			);
		}

		cameraComponent.orthographicSize = currentZoom;
	}
}
