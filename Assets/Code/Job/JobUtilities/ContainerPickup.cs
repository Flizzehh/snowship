using System.Collections.Generic;
using Snowship.NResource;

namespace Snowship.NJob
{
	public class ContainerPickup
	{
		public Container container;
		public List<ResourceAmount> resourcesToPickup;

		public ContainerPickup(Container container, List<ResourceAmount> resourcesToPickup) {
			this.container = container;
			this.resourcesToPickup = resourcesToPickup;
		}
	}
}