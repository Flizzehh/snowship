using Snowship.NResource;

namespace Snowship.NUI
{
	public interface ITreeButton
	{
		bool ChildElementsActiveState { get; }
		void SetChildSiblingChildElementsActive(ITreeButton childButtonToBeActive);
		void SetChildElementsActive(bool active);
		void Close();
	}
}