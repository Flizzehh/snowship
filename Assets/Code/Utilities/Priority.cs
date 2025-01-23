namespace Snowship.NUtilities
{
	public class Priority
	{
		public readonly int min;
		public readonly int max;

		private int priority = 0;

		public Priority(int priority = 0, int min = 0, int max = 9) {
			this.priority = priority;
			this.min = min;
			this.max = max;
		}

		public int Set(int priority) {
			if (priority > max) {
				priority = min;
			} else if (priority < min) {
				priority = max;
			}

			this.priority = priority;

			return this.priority;
		}

		public int Change(int amount) {
			return Set(Get() + amount);
		}

		public int Get() {
			return priority;
		}
	}
}
