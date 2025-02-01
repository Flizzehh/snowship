using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RecentAssetsWindow : EditorWindow
{
	private List<string> recentAssets = new();
	private const string PrefsKey = "RecentAssetsHistory";
	private Vector2 scrollPos;

	[MenuItem("Window/Recent Assets Tracker")]
	public static void ShowWindow() {
		GetWindow<RecentAssetsWindow>("Recent Assets");
	}

	private void OnEnable() {
		LoadHistory();
		EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
		EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
	}

	private void OnDisable() {
		SaveHistory();
		EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
		EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemGUI;
	}

	private void OnGUI() {
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		for (int i = recentAssets.Count - 1; i >= 0; i--) {
			string assetPath = recentAssets[i];
			Texture icon = AssetDatabase.GetCachedIcon(assetPath);

			EditorGUILayout.BeginHorizontal();
			if (icon) {
				GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
			}
			if (GUILayout.Button(Path.GetFileName(assetPath))) {
				SelectAsset(i);
			}
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();

		if (GUILayout.Button("Clear")) {
			ClearHistory();
		}
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
			if (obj) {
				string path = AssetDatabase.GetAssetPath(obj);
				if (!string.IsNullOrEmpty(path)) {
					RegisterAsset(path);
				}
			}
		}
	}

	private void RegisterAsset(string path) {
		if (!string.IsNullOrEmpty(path)) {
			recentAssets.Remove(path);
			recentAssets.Add(path);
			if (recentAssets.Count > 20) // Limit to last 20 items
			{
				recentAssets.RemoveAt(0);
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
			}
		}
	}

	private void ClearHistory() {
		recentAssets.Clear();
		SaveHistory();
		Repaint();
	}

	private void LoadHistory() {
		string data = EditorPrefs.GetString(PrefsKey, "");
		if (!string.IsNullOrEmpty(data)) {
			recentAssets = data.Split('|').ToList();
		}
	}

	private void SaveHistory() {
		string data = string.Join("|", recentAssets);
		EditorPrefs.SetString(PrefsKey, data);
	}
}