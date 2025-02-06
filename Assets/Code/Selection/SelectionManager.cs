using System;
using System.Collections.Generic;
using System.Linq;
using Snowship.NCamera;
using Snowship.NColony;
using Snowship.NInput;
using Snowship.NJob;
using Snowship.NState;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Snowship.Selectable
{
	public class SelectionManager : IManager
	{
		private readonly Stack<ISelectable> selectables = new();

		private Type selectedJobType;
		private IJobDefinition selectedJobDefinition;
		private IJobParams selectedJobParams;
		private bool selecting = false;

		private TileManager.Tile firstTile;
		private TileManager.Tile secondTile;
		private TileManager.Tile previousSecondTile;

		private HashSet<TileManager.Tile> selectionArea = new();
		private readonly Dictionary<TileManager.Tile, GameObject> selectionIndicators = new();

		private TileManager.Map map;
		private Camera camera;

		public void OnCreate() {
			GameManager.Get<InputManager>().InputSystemActions.Simulation.Select.performed += OnSelectPerformed;
			GameManager.Get<InputManager>().InputSystemActions.Simulation.Select.canceled += OnSelectCanceled;

			GameManager.Get<InputManager>().InputSystemActions.Simulation.Deselect.canceled += OnDeselectCanceled;

			GameManager.Get<StateManager>().OnStateChanged += OnStateChanged;
		}

		private void OnStateChanged((EState previousState, EState newState) state) {
			if (state is { previousState: EState.LoadToSimulation, newState: EState.Simulation }) {
				map = GameManager.Get<ColonyManager>().colony.map;
				camera = GameManager.Get<CameraManager>().camera;
			}
		}

		public void AddSelectable(ISelectable selectable) {
			selectables.Push(selectable);
			selectable.Select();

			Debug.Log("Selected " + selectable);
		}

		private void RemoveSelectable() {

			if (selectables.Count <= 0) {
				return;
			}

			ISelectable selectable = selectables.Pop();
			selectable.Deselect();

			Debug.Log("Deselected " + selectable);
		}

		private void OnDeselectCanceled(InputAction.CallbackContext callbackContext) {
			if (selectedJobDefinition != null) {
				ClearSelectedJob();
				return;
			}
			RemoveSelectable();
		}

		public void SetSelectedJob<TJob, TJobDefinition>()
			where TJob : class, IJob
			where TJobDefinition : class, IJobDefinition {

			selectedJobType = typeof(TJob);
			selectedJobDefinition = GameManager.Get<JobManager>().JobRegistry.GetJobDefinition<TJobDefinition>() as TJobDefinition;
			selectedJobParams = null;
		}

		public void SetSelectedJob<TJob, TJobDefinition>(IJobParams jobParams)
			where TJob : class, IJob
			where TJobDefinition : class, IJobDefinition {

			SetSelectedJob<TJob, TJobDefinition>();
			selectedJobParams = jobParams;
		}

		public void ClearSelectedJob() {
			selectedJobType = null;
			selectedJobDefinition = null;
			selectedJobParams = null;
		}

		private void OnSelectPerformed(InputAction.CallbackContext callbackContext) {

			if (selectedJobDefinition == null) {
				return;
			}

			selecting = true;
			firstTile = GetTileFromMouseScreenPosition(Input.mousePosition);
		}

		private void OnSelectCanceled(InputAction.CallbackContext callbackContext) {
			selecting = false;

			if (selectionArea is { Count: > 0 }) {
				foreach (TileManager.Tile tile in selectionArea) {
					if (selectedJobParams == null) {
						GameManager.Get<JobManager>().AddJob(Activator.CreateInstance(selectedJobType, tile) as IJob);
					} else {
						GameManager.Get<JobManager>().AddJob(Activator.CreateInstance(selectedJobType, tile, selectedJobParams) as IJob);
					}
				}
				selectionArea.Clear();
			}
			SetSelectionIndicators(null);

			firstTile = null;
			secondTile = null;
			previousSecondTile = null;
		}

		public void OnUpdate() {
			UpdateSelectedJobPreview();
			if (selecting) {
				UpdateSelectionArea();
			}
		}

		private void UpdateSelectedJobPreview() {

			SpriteRenderer selectedJobPreview = GameManager.SharedReferences.SelectedJobPreview;

			if (selectedJobDefinition == null || selecting) {
				selectedJobPreview.gameObject.SetActive(false);
				return;
			}

			selectedJobPreview.gameObject.SetActive(true);

			selectedJobPreview.sprite = selectedJobParams?.SelectedJobPreviewSprite ?? GameManager.Get<ResourceManager>().selectionCornersSprite;
			selectedJobPreview.transform.position = GetTileFromMouseScreenPosition(Input.mousePosition).obj.transform.position;
		}

		private void UpdateSelectionArea() {
			previousSecondTile = secondTile;
			secondTile = GetTileFromMouseScreenPosition(Input.mousePosition);
			if (previousSecondTile == secondTile) {
				return;
			}

			selectionArea = GetSelectionArea(selectedJobDefinition);

			SetSelectionIndicators(selectedJobParams);
		}

		private HashSet<TileManager.Tile> GetSelectionArea(IJobDefinition jobDefinition, IJobParams jobParams = null) {
			if (!selecting) {
				return null;
			}
			if (map == null) {
				return null;
			}
			if (jobDefinition == null) {
				return null;
			}

			Vector2Int smallerPosition = new(
				Mathf.FloorToInt(Mathf.Min(firstTile.position.x, secondTile.position.x)),
				Mathf.FloorToInt(Mathf.Min(firstTile.position.y, secondTile.position.y))
			);
			Vector2Int largerPosition = new(
				Mathf.FloorToInt(Mathf.Max(firstTile.position.x, secondTile.position.x)),
				Mathf.FloorToInt(Mathf.Max(firstTile.position.y, secondTile.position.y))
			);

			HashSet<TileManager.Tile> selection = new();

			for (int y = smallerPosition.y; y <= largerPosition.y; y++) {
				for (int x = smallerPosition.x; x <= largerPosition.x; x++) {
					bool positionMatchesSelectionType = false;
					switch (jobDefinition.SelectionType) {
						case SelectionType.Full:
							positionMatchesSelectionType = true;
							break;
						case SelectionType.Outline:
							if (x == smallerPosition.x || x == largerPosition.x || y == smallerPosition.y || y == largerPosition.y) {
								positionMatchesSelectionType = true;
							}
							break;
						case SelectionType.Single:
							if (x == smallerPosition.x && y == smallerPosition.y) {
								positionMatchesSelectionType = true;
							}
							break;
						default:
							throw new ArgumentOutOfRangeException(nameof(jobDefinition.SelectionType), jobDefinition.SelectionType, null);
					}

					if (!positionMatchesSelectionType) {
						continue;
					}

					TileManager.Tile tile = map.GetTileFromPosition(x, y);

					if (!(jobDefinition.SelectionConditions?.All(condition => condition(tile, jobDefinition.Layer)) ?? true)) {
						continue;
					}

					if (!(jobParams?.SelectionConditions?.All(condition => condition(tile, jobDefinition.Layer)) ?? true)) {
						continue;
					}

					selection.Add(tile);
				}
			}
			return selection;
		}

		private TileManager.Tile GetTileFromMouseScreenPosition(Vector2 mousePosition) {
			return map?.GetTileFromPosition(camera.ScreenToWorldPoint(mousePosition));
		}

		private void SetSelectionIndicators(IJobParams jobParams) {
			List<TileManager.Tile> selectionIndicatorsToRemove = new();

			if (selectionArea == null || selectionArea.Count == 0) {
				foreach (TileManager.Tile tile in selectionIndicators.Keys) {
					Object.Destroy(selectionIndicators[tile]);
				}
				selectionIndicators.Clear();
				return;
			}

			foreach (TileManager.Tile tile in selectionIndicators.Keys) {
				if (!selectionArea.Contains(tile)) {
					selectionIndicatorsToRemove.Add(tile);
				}
			}
			foreach (TileManager.Tile tile in selectionIndicatorsToRemove) {
				Object.Destroy(selectionIndicators[tile]);
				selectionIndicators.Remove(tile);
			}
			foreach (TileManager.Tile tile in selectionArea) {
				if (!selectionIndicators.ContainsKey(tile)) {
					GameObject selectionIndicator = Object.Instantiate(GameManager.Get<ResourceManager>().tilePrefab, tile.obj.transform, false);
					selectionIndicator.GetComponent<SpriteRenderer>().sprite = jobParams?.SelectedJobPreviewSprite ?? GameManager.Get<ResourceManager>().selectionCornersSprite;
					selectionIndicators.Add(tile, selectionIndicator);
				}
			}
		}
	}
}