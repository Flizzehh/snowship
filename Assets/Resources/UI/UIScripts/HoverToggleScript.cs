using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverToggleScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	// List of objects to enable/disable when the mouse enters/exits the parent object
	private List<GameObject> objects = new List<GameObject>();
	
	// If any of the objects in the "objects" list has no children, do not enable it
	private bool disableWithNoChildren;

	// If any of these objects are enabled, the "objects" list objects will be disabled
	private List<GameObject> disablingObjects = new List<GameObject>();

	public void Initialize(GameObject obj, bool disableWithNoChildren, List<GameObject> disablingObjects) {
		Initialize(new List<GameObject>() { obj }, disableWithNoChildren, disablingObjects);
	}

	public void Initialize(List<GameObject> objects, bool disableWithNoChildren, List<GameObject> disablingObjects) {
		this.objects = objects;
		this.disableWithNoChildren = disableWithNoChildren;
		this.disablingObjects = disablingObjects;

		DisableObjects();
	}

	public void SetDisablingObjects(List<GameObject> disablingObjects) {
		this.disablingObjects = disablingObjects;
	}

	public void OnPointerEnter(PointerEventData eventData) {
		EnableObjects();
	}

	public void OnPointerExit(PointerEventData eventData) {
		DisableObjects();
	}

	public void EnableObjects() {
		if (disablingObjects != null) {
			foreach (GameObject dObj in disablingObjects) {
				if (dObj.activeSelf) {
					return;
				}
			}
		}
		foreach (GameObject obj in objects) {
			if (disableWithNoChildren) {
				if (obj.transform.childCount <= 0) {
					continue;
				}
			}
			obj.SetActive(true);
		}
	}

	public void DisableObjects() {
		foreach (GameObject obj in objects) {
			obj.SetActive(false);
		}
	}
}