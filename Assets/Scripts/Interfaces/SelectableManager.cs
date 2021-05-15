using System.Collections.Generic;
using UnityEngine;

public class SelectableManager : BaseManager {

	public static readonly List<ISelectable> selected = new List<ISelectable>();

	public interface ISelectable {
		void Select();

		void Deselect();
	}

	public static void AddSelectable(ISelectable selectable) {
		selected.Add(selectable);
		selectable.Select();
	}

	public static void RemoveSelectable(ISelectable selectable) {
		selected.Remove(selectable);
		selectable.Deselect();
	}

	public override void Update() {
		if (Input.GetMouseButtonUp(0)) {

		}

		if (Input.GetMouseButtonUp(1)) {
			if (selected.Count > 0) {
				RemoveSelectable(selected[selected.Count - 1]);
			}
		}
	}
}
