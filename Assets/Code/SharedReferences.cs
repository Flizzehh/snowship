using UnityEngine;

namespace Snowship
{
	public class SharedReferences : MonoBehaviour
	{
		[SerializeField] private new Camera camera;
		public Camera Camera => camera;

		[SerializeField] private Transform canvas;
		public Transform Canvas => canvas;

		[SerializeField] private Transform tileParent;
		public Transform TileParent => tileParent;

		[SerializeField] private Transform lifeParent;
		public Transform LifeParent => lifeParent;

		[SerializeField] private Transform selectionParent;
		public Transform SelectionParent => selectionParent;

		[SerializeField] private Transform jobParent;
		public Transform JobParent => jobParent;
	}
}