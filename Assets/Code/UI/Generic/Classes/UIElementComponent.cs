using System;
using UnityEngine;

namespace Snowship.NUI.Generic {
	public abstract class UIElementComponent : MonoBehaviour {
		private void OnEnable() {
			OnCreate();
		}

		private void OnDisable() {
			OnClose();
		}

		protected abstract void OnCreate();
		public abstract void OnClose();
		public void Close() => OnClose();
	}
}
