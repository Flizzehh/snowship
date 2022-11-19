using System.Collections.Generic;
using UnityEngine;

namespace Snowship.Selectable {

	public class SelectableManager : BaseManager {

		public static readonly List<ISelectable> selected = new();

		public static void AddSelectable(ISelectable selectable) {
			selected.Add(selectable);
			selectable.Select();

			Debug.Log("Selected " + selectable.ToString());
		}

		public static void RemoveSelectable(ISelectable selectable) {
			selected.Remove(selectable);
			selectable.Deselect();

			Debug.Log("Deselected " + selectable.ToString());
		}

		public override void Update() {
			if (Input.GetMouseButtonUp(0)) {

			}

			if (Input.GetMouseButtonUp(1)) {
				if (selected.Count > 0) {
					RemoveSelectable(selected[^1]);
				}
			}
		}
	}
}