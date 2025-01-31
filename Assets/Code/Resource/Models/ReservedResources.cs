using System.Collections.Generic;
using Snowship.NHuman;

namespace Snowship.NResource
{
	public class ReservedResources
	{
		public List<ResourceAmount> resources = new();
		public Human human;

		public ReservedResources(List<ResourceAmount> resourcesToReserve, Human humanReservingResources) {
			resources.AddRange(resourcesToReserve);
			human = humanReservingResources;
		}
	}
}
