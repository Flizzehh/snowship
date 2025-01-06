using System.Collections.Generic;

namespace Snowship.NState {
	public class State {
		public readonly EState type;
		public readonly List<EState> validNextStates;

		public State(EState type, List<EState> validNextStates) {
			this.type = type;
			this.validNextStates = validNextStates;
		}
	}
}
