using Snowship.NUtilities;

namespace Snowship.NResource
{
	public class PriorityResourceInstance
	{
		public Resource resource;

		public static readonly int priorityMax = 9;
		public Priority priority;

		public PriorityResourceInstance(Resource resource, int priority) {
			this.resource = resource;

			this.priority = new Priority(priority);
		}
	}
}
