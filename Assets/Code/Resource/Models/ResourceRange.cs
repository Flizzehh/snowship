namespace Snowship.NResource
{
	public class ResourceRange
	{
		public Resource resource;
		public int min;
		public int max;

		public ResourceRange(Resource resource, int min, int max) {
			this.resource = resource;
			this.min = min;
			this.max = max;
		}
	}
}
