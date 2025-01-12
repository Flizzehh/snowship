﻿using Snowship.NState;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Snowship.NCamera {
	public partial class CameraManager {

		private Vector3 moveVector = Vector2.zero;
		private float zoomAxis = 0;

		private void OnInputSystemEnabled(InputSystemActions actions) {
			actions.Simulation.MoveCamera.performed += OnMoveCameraPerformed;
			actions.Simulation.MoveCamera.canceled += OnMoveCameraCancelled;

			actions.Simulation.ZoomCamera.performed += OnZoomCameraPerformed;
			actions.Simulation.ZoomCamera.canceled += OnZoomCameraCancelled;
		}

		private void OnInputSystemDisabled(InputSystemActions actions) {
			actions.Simulation.MoveCamera.performed -= OnMoveCameraPerformed;
			actions.Simulation.MoveCamera.canceled -= OnMoveCameraCancelled;

			actions.Simulation.ZoomCamera.performed -= OnZoomCameraPerformed;
			actions.Simulation.ZoomCamera.canceled -= OnZoomCameraCancelled;
		}

		private void OnMoveCameraPerformed(InputAction.CallbackContext callbackContext) {

			// TODO Need to disable moving when player is typing

			if (GameManager.stateM.State != EState.Simulation) {
				return;
			}

			moveVector = callbackContext.ReadValue<Vector2>() * CameraMoveSpeedMultiplier;
			MoveCamera();
		}

		private void OnMoveCameraCancelled(InputAction.CallbackContext callbackContext) {
			moveVector = Vector2.zero;
		}

		private void OnZoomCameraPerformed(InputAction.CallbackContext callbackContext) {

			if (eventSystem.IsPointerOverGameObject()) {
				return;
			}

			if (GameManager.stateM.State != EState.Simulation) {
				return;
			}

			zoomAxis = callbackContext.ReadValue<float>();
			ZoomCamera();
		}

		private void OnZoomCameraCancelled(InputAction.CallbackContext callbackContext) {
			zoomAxis = 0;
		}
	}
}
