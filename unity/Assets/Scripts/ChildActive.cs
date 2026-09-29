using UnityEngine;

public class ChildActive : MonoBehaviour
{
	private Transform firstchild;

	private void Awake()
	{
		firstchild = base.transform.GetChild(0);
	}

	private void OnEnable()
	{
		firstchild.gameObject.active = true;
	}
}
