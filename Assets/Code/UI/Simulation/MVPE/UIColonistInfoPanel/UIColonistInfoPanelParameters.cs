using Snowship.NColonist;

namespace Snowship.NUI
{
	public class UIColonistInfoPanelParameters : IUIParameters {

		public Colonist Colonist { get; }

		public UIColonistInfoPanelParameters(Colonist colonist) {
			Colonist = colonist;
		}

	}
}