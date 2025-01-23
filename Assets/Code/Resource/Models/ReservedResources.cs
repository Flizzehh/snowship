using System.Collections.Generic;

namespace Snowship.NResource
{
	public class ReservedResources
	{
		public List<ResourceAmount> resources = new();
		public HumanManager.Human human;

		public ReservedResources(List<ResourceAmount> resourcesToReserve, HumanManager.Human humanReservingResources) {
			resources.AddRange(resourcesToReserve);
			human = humanReservingResources;
		}
	}
}
