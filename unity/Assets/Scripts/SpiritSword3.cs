using UnityEngine;

public class SpiritSword3 : MonoBehaviour
{
	public Transform sword;

	private Transform mytransform;

	private Collider mycollider;

	private Transform[] c_sword = new Transform[4];

	private int swordindex;

	private int creatindex;

	private float power;

	private float start_delay;

	private Transform cha1;

	private bool homing = true;

	private bool creatfinish;

	private Transform oldtarget;

	private int hitcount;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		cha1 = GameObject.FindWithTag("Player").transform;
	}

	private void Start()
	{
		power = base.GetComponent<Rigidbody>().mass;
	}

	private void OnEnable()
	{
		mycollider.enabled = false;
		start_delay = 0f;
		creatindex = 0;
		swordindex = 0;
		creatfinish = false;
		homing = false;
		hitcount = 0;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer != 8 || homing || oldtarget == other.transform)
		{
			return;
		}
		c_sword[swordindex].GetComponent<SpiritSword_p3>().FireSword(other.transform);
		oldtarget = other.transform;
		swordindex = (swordindex + 1) % 4;
		hitcount++;
		mycollider.enabled = false;
		start_delay = 0f;
		homing = true;
		if (hitcount >= 24)
		{
			homing = false;
			base.gameObject.active = false;
			mytransform.position = Vector3.up * 31f;
			for (int i = 0; i < 4; i++)
			{
				c_sword[i].GetComponent<SpiritSword_p3>().FinishSword();
			}
		}
	}

	private void Update()
	{
		if (!creatfinish)
		{
			if (start_delay > 0.2f)
			{
				start_delay = 0f;
				if (c_sword[creatindex] == null)
				{
					c_sword[creatindex] = (Transform)Object.Instantiate(sword, mytransform.position - mytransform.forward, mytransform.rotation);
					c_sword[creatindex].GetComponent<Rigidbody>().mass = power;
					c_sword[creatindex].parent = mytransform;
					c_sword[creatindex].GetComponent<SpiritSword_p3>().SetPos(creatindex);
				}
				else
				{
					c_sword[creatindex].position = mytransform.position - mytransform.forward;
					c_sword[creatindex].rotation = mytransform.rotation;
					c_sword[creatindex].gameObject.active = true;
				}
				if (creatindex < 3)
				{
					creatindex++;
				}
				else
				{
					creatfinish = true;
				}
			}
			else
			{
				start_delay += Time.deltaTime;
			}
		}
		else if (start_delay > 0.3f)
		{
			homing = false;
			mycollider.enabled = true;
			start_delay = -1f;
		}
		else if (start_delay > -1f)
		{
			start_delay += Time.deltaTime;
		}
		else
		{
			start_delay -= Time.deltaTime;
			if (start_delay < -2f)
			{
				mycollider.enabled = false;
				mycollider.enabled = true;
				oldtarget = null;
			}
		}
		mytransform.position = cha1.position + Vector3.up * 0.1f;
		mytransform.rotation = Quaternion.Lerp(mytransform.rotation, cha1.rotation, Time.deltaTime * 3f);
	}
}
