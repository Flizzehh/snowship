using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class RecentAssetsWindow : EditorWindow
{
	private List<string> recentAssets = new();
	private const string PrefsKey = "RecentAssetsHistory";
	private Vector2 scrollPos;
	private int currentIndex = -1;

	[MenuItem("Window/Recent Assets Tracker")]
	public static void ShowWindow() {
		GetWindow<RecentAssetsWindow>("Recent Assets");
	}

	private void OnEnable() {
		LoadHistory();
		EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
		EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
		SceneView.duringSceneGui += OnSceneGUI;
	}

	private void OnDisable() {
		SaveHistory();
		EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
		EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemGUI;
		SceneView.duringSceneGui -= OnSceneGUI;
	}

	private void OnGUI() {
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		for (int i = recentAssets.Count - 1; i >= 0; i--) {
			string assetPath = recentAssets[i];
			Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
			Texture icon = AssetDatabase.GetCachedIcon(assetPath);

			EditorGUILayout.BeginHorizontal();
			if (icon != null) {
				GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
			}
			if (GUILayout.Button(Path.GetFileName(assetPath))) {
				SelectAsset(i);
			}
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();
	}

	private void OnProjectWindowItemGUI(string guid, Rect selectionRect) {
		if (Event.current.type == EventType.MouseDown && selectionRect.Contains(Event.current.mousePosition)) {
			string path = AssetDatabase.GUIDToAssetPath(guid);
			RegisterAsset(path);
		}
	}

	private void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect) {
		if (Event.current.type == EventType.MouseDown && selectionRect.Contains(Event.current.mousePosition)) {
			Object obj = EditorUtility.InstanceIDToObject(instanceID);
			if (obj != null) {
				string path = AssetDatabase.GetAssetPath(obj);
				if (!string.IsNullOrEmpty(path)) {
					RegisterAsset(path);
				}
			}
		}
	}

	private void OnSceneGUI(SceneView sceneView) {
		Event e = Event.current;
		if (e.type == EventType.KeyDown) {
			if (e.keyCode == KeyCode.Mouse3) // Back button
			{
				GoBack();
				e.Use();
			} else if (e.keyCode == KeyCode.Mouse4) // Forward button
			{
				GoForward();
				e.Use();
			}
		}
	}

	private void RegisterAsset(string path) {
		if (!string.IsNullOrEmpty(path)) {
			recentAssets.Remove(path); // Remove previous occurrence if exists
			recentAssets.Add(path);
			currentIndex = recentAssets.Count - 1;
			if (recentAssets.Count > 20) // Limit to last 20 items
			{
				recentAssets.RemoveAt(0);
				currentIndex--;
			}
			SaveHistory();
			Repaint();
		}
	}

	private void SelectAsset(int index) {
		if (index >= 0 && index < recentAssets.Count) {
			string assetPath = recentAssets[index];
			Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
			if (obj) {
				Selection.activeObject = obj;
				EditorGUIUtility.PingObject(obj);
				recentAssets.RemoveAt(index); // Remove previous occurrence
				recentAssets.Add(assetPath); // Add it to the end to reflect most recent selection
				currentIndex = recentAssets.Count - 1;
				SaveHistory();
				Repaint();
			}
		}
	}

	private void GoBack() {
		if (currentIndex > 0) {
			currentIndex--;
			SelectAsset(currentIndex);
		}
	}

	private void GoForward() {
		if (currentIndex < recentAssets.Count - 1) {
			currentIndex++;
			SelectAsset(currentIndex);
		}
	}

	private void LoadHistory() {
		string data = EditorPrefs.GetString(PrefsKey, "");
		if (!string.IsNullOrEmpty(data)) {
			recentAssets = data.Split('|').ToList();
			currentIndex = recentAssets.Count - 1;
		}
	}

	private void SaveHistory() {
		string data = string.Join("|", recentAssets);
		EditorPrefs.SetString(PrefsKey, data);
	}
}