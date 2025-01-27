using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Snowship.NState {
	public class State<TStateEnum> where TStateEnum : Enum
	{
		public readonly TStateEnum Type;
		public readonly List<TStateEnum> ValidNextStates;
		public readonly List<Func<UniTask>> ActionsOnTransition;

		public State(
			TStateEnum type,
			List<TStateEnum> validNextStates,
			List<Func<UniTask>> actionsOnTransition
		) {
			Type = type;
			ValidNextStates = validNextStates;
			ActionsOnTransition = actionsOnTransition;
		}
	}
}