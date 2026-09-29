using UnityEngine;

public class AI_Ride_Enemy2 : MonoBehaviour
{
	private Transform mytransform;

	private Transform cha1;

	private Vector3 targetdir;

	private AnimationState state;

	private float rot_limit;

	private float rot_speed;

	private bool impact;

	private bool live;

	public Transform bullet;

	private Transform c_bullet;

	private AnimationState temp;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
		cha1 = GameObject.FindWithTag("Player").transform;
		base.GetComponent<Animation>()["ride_mon"].speed = 0.25f;
		base.GetComponent<Animation>()["ride_mon_shoot"].speed = 0.25f;
		base.GetComponent<Animation>()["ride_mon_shoot"].layer = 1;
		base.GetComponent<Animation>().Play("ride_mon");
		InvokeRepeating("Attack", 4f, Random.Range(3f, 5f));
		c_bullet = (Transform)Object.Instantiate(bullet, Vector3.one * 10f, Quaternion.identity);
		c_bullet.gameObject.active = false;
	}

	public void Attack()
	{
		if (live)
		{
			base.GetComponent<Animation>().Play("ride_mon_shoot");
			temp = base.GetComponent<Animation>().PlayQueued("ride_mon_shoot_i");
			temp.speed = 0.25f;
			temp.layer = 1;
		}
	}

	public void Wake()
	{
		live = true;
	}

	public void Defeat()
	{
		live = false;
	}

	private void Update()
	{
		if (!live)
		{
			return;
		}
		if (base.GetComponent<Animation>().IsPlaying("ride_mon_shoot_i"))
		{
			rot_limit = -1f;
			rot_speed = 8f;
			if (!impact)
			{
				c_bullet.gameObject.active = true;
				c_bullet.position = mytransform.position + Vector3.up * 0.08f;
				c_bullet.rotation = mytransform.rotation;
				impact = true;
			}
		}
		else if (base.GetComponent<Animation>().IsPlaying("ride_mon_shoot"))
		{
			rot_limit = -1f;
			rot_speed = 8f;
			impact = false;
		}
		else if (base.GetComponent<Animation>().IsPlaying("ride_mon"))
		{
			rot_limit = 0f;
			rot_speed = 4f;
		}
		targetdir = cha1.position - mytransform.position;
		targetdir[1] = 0f;
		targetdir = Vector3.Normalize(targetdir);
		if (targetdir.z > rot_limit)
		{
			mytransform.forward = Vector3.Lerp(mytransform.forward, targetdir, Time.deltaTime * rot_speed);
		}
		else
		{
			mytransform.forward = Vector3.Lerp(mytransform.forward, Vector3.forward, Time.deltaTime * 6f);
		}
	}
}
