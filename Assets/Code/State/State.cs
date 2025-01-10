using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Snowship.NState {
	public class State {
		public readonly EState Type;
		public readonly List<EState> ValidNextStates;
		public readonly List<Func<UniTask>> ActionsOnOpen;

		public State(
			EState type,
			List<EState> validNextStates,
			List<Func<UniTask>> actionsOnOpen
		) {
			Type = type;
			ValidNextStates = validNextStates;
			ActionsOnOpen = actionsOnOpen;
		}
	}
}
