using UnityEngine;

public class SpiritSword_p : MonoBehaviour
{
	private bool fireon;

	private Transform mytransform;

	private Transform target;

	private Vector3 targetpos;

	private Vector3 directionVector;

	private float dt;

	private Collider mycollider;

	private TrailRenderer mytrail;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		mytrail = GetComponent<TrailRenderer>();
	}

	private void OnEnable()
	{
		mytransform.localScale = Vector3.right + Vector3.up;
		mycollider.enabled = false;
		dt = 0f;
	}

	public void FireSword(Transform enemy)
	{
		mycollider.enabled = true;
		mytransform.parent = null;
		fireon = true;
		target = enemy;
		mytrail.enabled = true;
	}

	private void Update()
	{
		if (fireon)
		{
			if (mytransform.position.y > 0f)
			{
				if (dt < 10f)
				{
					dt += Time.deltaTime * 5f;
				}
				if ((bool)target)
				{
					targetpos = target.position;
				}
				directionVector = targetpos - mytransform.position;
				if (directionVector != Vector3.zero)
				{
					mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.LookRotation(directionVector), 4f * dt * Time.deltaTime);
				}
				mytransform.position += mytransform.forward * Time.deltaTime * 1.8f;
			}
			else
			{
				mycollider.enabled = false;
				mytrail.enabled = false;
				fireon = false;
				base.gameObject.active = false;
			}
		}
		else if (mytransform.localScale.z < 1f)
		{
			mytransform.localScale += Vector3.forward * Time.deltaTime * 6f;
		}
		else
		{
			mytransform.localScale = Vector3.one;
		}
	}
}
