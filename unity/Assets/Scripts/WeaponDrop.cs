using UnityEngine;

public class WeaponDrop : MonoBehaviour
{
	private Transform mytransform;

	private float maxy = 1.7f;

	private bool drop;

	private Vector3 rotateaxis;

	private void Awake()
	{
		mytransform = base.transform;
		rotateaxis = mytransform.right;
	}

	public void DropCancel()
	{
		CancelInvoke("Disappear");
		drop = false;
	}

	public void Drop(bool _destroy)
	{
		CancelInvoke("Disappear");
		if (_destroy)
		{
			Object.Destroy(base.gameObject, 2.5f);
		}
		else
		{
			Invoke("Disappear", 3.5f);
		}
		drop = true;
		maxy = 1.7f;
	}

	private void Disappear()
	{
		mytransform.position = Vector3.one * 4f;
		base.gameObject.active = false;
	}

	private void Update()
	{
		if (drop)
		{
			if (mytransform.position.y > 0f)
			{
				mytransform.Rotate(rotateaxis * Time.deltaTime * 720f);
				maxy -= 3.5f * Time.deltaTime;
				mytransform.position += Vector3.up * maxy * Time.deltaTime;
			}
			else
			{
				mytransform.position = Vector3.right * mytransform.position.x + Vector3.forward * mytransform.position.z;
				drop = false;
			}
		}
	}
}
