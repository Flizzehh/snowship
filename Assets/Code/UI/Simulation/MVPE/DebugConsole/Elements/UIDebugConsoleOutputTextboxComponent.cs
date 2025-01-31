using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Snowship.NUI
{
	public class UIDebugConsoleOutputTextboxComponent : UIElementComponent {

		[Header("Properties")]
		[SerializeField] private int charsPerLine = 70;
		[SerializeField] private int textBoxSizePerLine = 17;

		[Header("Components")]
		[SerializeField] private TMP_Text text;
		[SerializeField] private LayoutElement layoutElement;
		[SerializeField] private RectTransform rectTransform;

		public override void OnCreate() {

		}

		protected override void OnClose() {

		}

		public void OutputToConsole(string outputString) {
			if (string.IsNullOrEmpty(outputString)) {
				return;
			}

			int lines = Mathf.CeilToInt(outputString.Length / (float)charsPerLine);

			layoutElement.minHeight = lines * textBoxSizePerLine;
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, lines * textBoxSizePerLine);

			text.text = outputString;
		}

	}
}