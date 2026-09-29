using UnityEngine;

public class SpiritSword_p2 : MonoBehaviour
{
	private bool fireon;

	private Transform mytransform;

	private Transform target;

	private Transform myparent;

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
		mycollider.enabled = false;
		dt = 0f;
		mytrail.enabled = true;
		fireon = true;
	}

	private void Start()
	{
		myparent = mytransform.parent;
		FireSword(myparent);
		mytransform.parent = null;
	}

	public void FireSword(Transform enemy)
	{
		mycollider.enabled = true;
		target = enemy;
	}

	public void FinishSword()
	{
		mycollider.enabled = false;
		fireon = false;
		dt = 0f;
	}

	private void Update()
	{
		if (fireon)
		{
			if (mytransform.position.y > 0f)
			{
				if (target == myparent)
				{
					dt = 2f;
				}
				else if (dt < 10f)
				{
					dt += Time.deltaTime * 5f;
				}
				if (target != null)
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
				target = myparent;
				mytransform.forward = Vector3.up;
				mytransform.position += mytransform.forward * Time.deltaTime * 1.8f;
				dt = 0f;
			}
		}
		else
		{
			mytransform.forward = Vector3.up;
			mytransform.position += mytransform.forward * Time.deltaTime * 1.8f;
			if (mytransform.position.y > 1f)
			{
				base.gameObject.active = false;
				mytrail.enabled = false;
			}
		}
	}
}
