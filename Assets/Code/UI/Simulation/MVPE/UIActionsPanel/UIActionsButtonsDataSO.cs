using System;
using System.Collections.Generic;
using UnityEngine;

namespace Snowship.NUI
{
	[CreateAssetMenu(fileName = "UIActionsButtonsDataSO", menuName = "Snowship/SOs/UIActionsButtonsDataSO", order = 1)]
	public class UIActionsButtonsDataSO : ScriptableObject
	{
		public List<UIActionButtonData> buttons = new();
	}

	[Serializable]
	public class UIActionButtonData
	{
		public string buttonName;
		public Sprite buttonSprite;
	}
}