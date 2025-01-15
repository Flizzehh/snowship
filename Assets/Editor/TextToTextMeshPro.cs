using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

// From: https://discussions.unity.com/t/replacing-text-with-textmesh-pro/690623

public class TextToTextMeshPro : Editor {

	public class TextMeshProSettings {

		public bool Enabled;
		public FontStyles FontStyle;
		public float FontSize;
		public float FontSizeMin;
		public float FontSizeMax;
		public float LineSpacing;
		public bool EnableRichText;
		public bool EnableAutoSizing;
		public TextAlignmentOptions TextAlignmentOptions;
		public bool WrappingEnabled;
		public TextOverflowModes TextOverflowModes;
		public string Text;
		public Color Color;
		public bool RayCastTarget;

	}

	private static bool recursive = false;
	private static bool applyOnDisabled = false;
	private static bool removeEffectComponents = false;

	[MenuItem("Tools/Text To TextMeshPro", false, 4000)]
	private static void DoIt() {
		if (TMP_Settings.defaultFontAsset == null) {
			EditorUtility.DisplayDialog("ERROR!", "Assign a default font asset in project settings!", "OK", "");
			return;
		}

		recursive = EditorUtility.DisplayDialog("Text To TextMeshPro", "Process all the children of the selected objects recursively?", "Yes", "No");
		if (recursive) {
			applyOnDisabled = EditorUtility.DisplayDialog("Text To TextMeshPro", "Process disabled objects too?", "Yes", "No");
		}

		removeEffectComponents = EditorUtility.DisplayDialog("Text To TextMeshPro", "Remove obsolete component effects Shadow and Outline?\n\nThis components have no effect on TextMeshPro and can be safely removed.\nTo apply shadows and outlines on TextMeshPro components, use different materials instead.", "Yes", "No");

		foreach (GameObject gameObject in Selection.gameObjects) {
			if (recursive) {
				Transform[] childs = gameObject.GetComponentsInChildren<Transform>(applyOnDisabled);
				foreach (Transform ch in childs) {
					ConvertTextToTextMeshPro(ch.gameObject);
				}
			} else {
				ConvertTextToTextMeshPro(gameObject);
			}
		}
	}

	private static void ConvertTextToTextMeshPro(GameObject target) {
		if (removeEffectComponents) {
			Shadow[] sha = target.GetComponents<Shadow>();
			foreach (Shadow cmp in sha) {
				Undo.DestroyObjectImmediate(cmp);
			}
			Outline[] outl = target.GetComponents<Outline>();
			foreach (Outline cmp in outl) {
				Undo.DestroyObjectImmediate(cmp);
			}
		}

		Text uiText = target.GetComponent<Text>();
		if (uiText == null) {
			return;
		}

		TextMeshProSettings settings = GetTextMeshProSettings(uiText);

		Undo.RecordObject(target, "Text To TextMeshPro");
		Undo.DestroyObjectImmediate(uiText);

		TextMeshProUGUI tmp = target.AddComponent<TextMeshProUGUI>();
		tmp.enabled = settings.Enabled;
		tmp.fontStyle = settings.FontStyle;
		tmp.fontSize = settings.FontSize;
		tmp.fontSizeMin = settings.FontSizeMin;
		tmp.fontSizeMax = settings.FontSizeMax;
		tmp.lineSpacing = settings.LineSpacing;
		tmp.richText = settings.EnableRichText;
		tmp.enableAutoSizing = settings.EnableAutoSizing;
		tmp.alignment = settings.TextAlignmentOptions;
		tmp.textWrappingMode = settings.WrappingEnabled ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
		tmp.overflowMode = settings.TextOverflowModes;
		tmp.text = settings.Text;
		tmp.color = settings.Color;
		tmp.raycastTarget = settings.RayCastTarget;

		Debug.Log(target.name + " converted to TextMeshProUGUI");
	}

	private static TextMeshProSettings GetTextMeshProSettings(Text uiText) {
		if (uiText == null) {
			EditorUtility.DisplayDialog("ERROR!", "You must select a Unity UI Text Object to convert.", "OK", "");
			return null;
		}

		return new TextMeshProSettings { Enabled = uiText.enabled, FontStyle = FontStyleToFontStyles(uiText.fontStyle), FontSize = uiText.fontSize, FontSizeMin = uiText.resizeTextMinSize, FontSizeMax = uiText.resizeTextMaxSize, LineSpacing = uiText.lineSpacing, EnableRichText = uiText.supportRichText, EnableAutoSizing = uiText.resizeTextForBestFit, TextAlignmentOptions = TextAnchorToTextAlignmentOptions(uiText.alignment), WrappingEnabled = HorizontalWrapModeToBool(uiText.horizontalOverflow), TextOverflowModes = VerticalWrapModeToTextOverflowModes(uiText.verticalOverflow), Text = uiText.text, Color = uiText.color, RayCastTarget = uiText.raycastTarget };
	}

	private static bool HorizontalWrapModeToBool(HorizontalWrapMode overflow) {
		return overflow == HorizontalWrapMode.Wrap;
	}

	private static TextOverflowModes VerticalWrapModeToTextOverflowModes(VerticalWrapMode verticalOverflow) {
		return verticalOverflow == VerticalWrapMode.Truncate ? TextOverflowModes.Truncate : TextOverflowModes.Overflow;
	}

	private static FontStyles FontStyleToFontStyles(FontStyle fontStyle) {
		switch (fontStyle) {
			case FontStyle.Normal:
				return FontStyles.Normal;

			case FontStyle.Bold:
				return FontStyles.Bold;

			case FontStyle.Italic:
				return FontStyles.Italic;

			case FontStyle.BoldAndItalic:
				return FontStyles.Bold | FontStyles.Italic;
		}

		Debug.LogWarning("Unhandled font style " + fontStyle);
		return FontStyles.Normal;
	}

	private static TextAlignmentOptions TextAnchorToTextAlignmentOptions(TextAnchor textAnchor) {
		switch (textAnchor) {
			case TextAnchor.UpperLeft:
				return TextAlignmentOptions.TopLeft;

			case TextAnchor.UpperCenter:
				return TextAlignmentOptions.Top;

			case TextAnchor.UpperRight:
				return TextAlignmentOptions.TopRight;

			case TextAnchor.MiddleLeft:
				return TextAlignmentOptions.Left;

			case TextAnchor.MiddleCenter:
				return TextAlignmentOptions.Center;

			case TextAnchor.MiddleRight:
				return TextAlignmentOptions.Right;

			case TextAnchor.LowerLeft:
				return TextAlignmentOptions.BottomLeft;

			case TextAnchor.LowerCenter:
				return TextAlignmentOptions.Bottom;

			case TextAnchor.LowerRight:
				return TextAlignmentOptions.BottomRight;
		}

		Debug.LogWarning("Unhandled text anchor " + textAnchor);
		return TextAlignmentOptions.TopLeft;
	}

}
