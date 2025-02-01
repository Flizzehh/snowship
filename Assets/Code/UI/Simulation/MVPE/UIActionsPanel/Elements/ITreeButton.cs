using System;

namespace Snowship.NUI
{
	public interface ITreeButton
	{
		bool ChildElementsActiveState { get; }
		event Action OnButtonClicked;
		void SetChildSiblingChildElementsActive(ITreeButton childButtonToBeActive);
		void SetChildElementsActive(bool active);
		void Close();
	}
}