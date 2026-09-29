using UnityEngine;

public class Tower : MonoBehaviour
{
	public Transform arrow;

	private Transform mytransform;

	private Transform c_arrow;

	private Collider mycollider;

	private Vector3 arrowdir;

	private float reload_delay;

	private short cur_stage_index;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
	}

	private void Start()
	{
		cur_stage_index = (short)Crypto.Load_int_key("cur_stage_index");
		c_arrow = (Transform)Object.Instantiate(arrow, mytransform.position, Quaternion.identity);
		c_arrow.GetComponent<WeaponDamage>().PressDamage(cur_stage_index + 1);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 15)
		{
			mycollider.enabled = false;
			arrowdir = other.transform.position - mytransform.position;
			c_arrow.position = mytransform.position;
			c_arrow.rotation = Quaternion.LookRotation(arrowdir);
			c_arrow.gameObject.active = true;
			arrowdir[1] = 0f;
			mytransform.rotation = Quaternion.LookRotation(arrowdir);
			reload_delay = 2f;
		}
	}

	private void Update()
	{
		if (reload_delay > 0f)
		{
			reload_delay -= Time.deltaTime;
			if (reload_delay < 0f)
			{
				reload_delay = 0f;
				mycollider.enabled = true;
			}
		}
	}
}
