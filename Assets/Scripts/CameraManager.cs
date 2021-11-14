using System;
using UnityEngine;

public class CameraManager : BaseManager {

	public GameObject cameraGO;
	public Camera cameraComponent;

	public override void Awake() {
		cameraGO = GameObject.Find("Camera");
		cameraComponent = cameraGO.GetComponent<Camera>();
	}

	private readonly int minZoom = 1;
	private readonly int maxZoom = 20;

	public int GetMinZoom() {
		return minZoom;
	}

	public int GetMaxZoom() {
		return maxZoom;
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

	private float currentZoom = 0;
	private float targetZoom = 0;
	private float zoomTimer = 0;

	public override void Update() {
		if (GameManager.tileM.mapState == TileManager.MapState.Generated && !GameManager.uiM.pauseMenu.activeSelf && !GameManager.uiM.playerTyping) {

			// Position Logic
			cameraGO.transform.Translate(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * currentZoom * Time.deltaTime * GameManager.timeM.permanentDeltaTimeMultiplier);
			if (Input.GetMouseButton(2)) {
				cameraGO.transform.Translate(new Vector2(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")) * currentZoom * Time.deltaTime * GameManager.timeM.permanentDeltaTimeMultiplier);
			}
			cameraGO.transform.position = new Vector2(Mathf.Clamp(cameraGO.transform.position.x, 0, GameManager.colonyM.colony.map.mapData.mapSize), Mathf.Clamp(cameraGO.transform.position.y, 0, GameManager.colonyM.colony.map.mapData.mapSize));

			// Zoom Logic
			if (!Mathf.Approximately(currentZoom, targetZoom)) {
				zoomTimer += 1 * Time.deltaTime;
				zoomTimer = Mathf.Clamp(zoomTimer, 0, 1);
			} else {
				zoomTimer = 0;
			}

			currentZoom = Mathf.Clamp(Mathf.Lerp(currentZoom, targetZoom, zoomTimer), minZoom, maxZoom * (GameManager.debugM.debugMode ? 25 : 1));

			if (!GameManager.uiM.IsPointerOverUI()) {
				//cameraComponent.orthographicSize -= Mathf.Clamp(Input.GetAxis("Mouse ScrollWheel") + (Input.GetAxis("KeyboardZoom") / 10f), -1f, 1f) * cameraComponent.orthographicSize * Time.deltaTime * 100;
				targetZoom -= Mathf.Clamp(Input.GetAxis("Mouse ScrollWheel") + (Input.GetAxis("KeyboardZoom") / 10f), -1f, 1f) * currentZoom * Time.deltaTime * 100;

				float zoomDifference = targetZoom - currentZoom;
				float maxZoomDifference = Mathf.Clamp(currentZoom / 3f, minZoom, maxZoom);
				if (Mathf.Abs(zoomDifference) >= maxZoomDifference) {
					if (zoomDifference >= 0) {
						targetZoom = currentZoom + maxZoomDifference;
					} else {
						targetZoom = currentZoom - maxZoomDifference;
					}
				}

				targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom * (GameManager.debugM.debugMode ? 25 : 1));
			}

			cameraComponent.orthographicSize = currentZoom;
		}
	}
}