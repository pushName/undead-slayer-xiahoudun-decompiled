using UnityEngine;

public class SpiritSword_p3 : MonoBehaviour
{
	private bool fireon;

	private Transform mytransform;

	private Transform target;

	private Transform myparent;

	private Vector3 targetpos;

	private Vector3 directionVector;

	private float f_speed;

	private Collider mycollider;

	private TrailRenderer mytrail;

	private Vector3 n_pos;

	private bool finish;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		mytrail = GetComponent<TrailRenderer>();
	}

	private void OnEnable()
	{
		finish = false;
		mycollider.enabled = false;
		f_speed = 0.4f;
		mytrail.enabled = true;
	}

	private void Start()
	{
		myparent = mytransform.parent;
		mytransform.parent = null;
	}

	public void FireSword(Transform _enemy)
	{
		mycollider.enabled = true;
		target = _enemy;
		targetpos = target.position;
		directionVector = targetpos - mytransform.position;
		directionVector[1] = 0f;
		if (directionVector != Vector3.zero)
		{
			mytransform.forward = directionVector;
		}
		f_speed = 0.4f;
		fireon = true;
	}

	public void FinishSword()
	{
		mycollider.enabled = false;
		fireon = false;
		f_speed = 0f;
		finish = true;
	}

	public void SetPos(int _index)
	{
		switch (_index)
		{
		case 0:
			n_pos = new Vector3(1f, 0f, 1f) * 0.13f;
			break;
		case 1:
			n_pos = new Vector3(1f, 0f, -1f) * 0.13f;
			break;
		case 2:
			n_pos = new Vector3(-1f, 0f, 1f) * 0.13f;
			break;
		case 3:
			n_pos = new Vector3(-1f, 0f, -1f) * 0.13f;
			break;
		}
	}

	private void Update()
	{
		if (finish)
		{
			mytransform.position += Vector3.up * 3f * Time.deltaTime;
			if (mytransform.position.y > 1f)
			{
				finish = false;
				mytrail.enabled = false;
				base.gameObject.active = false;
				CancelInvoke();
			}
		}
		else if (fireon)
		{
			f_speed -= Time.deltaTime;
			if (f_speed < 0f)
			{
				fireon = false;
				return;
			}
			mytransform.up = directionVector;
			mytransform.position += directionVector * Time.deltaTime * f_speed * 20f;
		}
		else
		{
			mycollider.enabled = false;
			directionVector = myparent.position - mytransform.position + myparent.forward * n_pos.z + myparent.right * n_pos.x;
			if (directionVector != Vector3.zero)
			{
				mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.LookRotation(directionVector), Time.deltaTime * 12f);
			}
			mytransform.position += directionVector * 3f * Time.deltaTime;
			f_speed = 0f;
		}
	}
}
