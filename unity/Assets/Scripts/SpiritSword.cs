using UnityEngine;

public class SpiritSword : MonoBehaviour
{
	public Transform sword;

	private Transform mytransform;

	private Collider mycollider;

	private Transform[] c_sword = new Transform[7];

	private int swordindex;

	private int creatindex;

	private float power;

	private float start_delay;

	private Transform cha1;

	private bool homing = true;

	private bool creatfinish;

	private Transform oldtarget;

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
	}

	public void Init()
	{
		mycollider.enabled = false;
		start_delay = 0f;
		creatindex = 0;
		swordindex = 0;
		creatfinish = false;
		homing = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 8 && !homing && !(oldtarget == other.transform))
		{
			c_sword[swordindex].GetComponent<SpiritSword_p>().FireSword(other.transform);
			oldtarget = other.transform;
			swordindex++;
			mycollider.enabled = false;
			start_delay = 0f;
			homing = true;
			if (swordindex >= 7)
			{
				base.gameObject.active = false;
				mytransform.position = Vector3.up * 31f;
			}
		}
	}

	private void Update()
	{
		if (!creatfinish)
		{
			if (start_delay > 0.1f)
			{
				start_delay = 0f;
				if (c_sword[creatindex] == null)
				{
					c_sword[creatindex] = (Transform)Object.Instantiate(sword, mytransform.position, Quaternion.Euler(0f, (float)(30 * creatindex) + mytransform.eulerAngles.y - 270f, 0f));
					c_sword[creatindex].GetComponent<Rigidbody>().mass = power;
					c_sword[creatindex].position += c_sword[creatindex].forward * 0.1f;
					c_sword[creatindex].forward = Vector3.up + mytransform.forward * 0.6f;
				}
				else
				{
					c_sword[creatindex].position = mytransform.position;
					c_sword[creatindex].rotation = Quaternion.Euler(0f, (float)(30 * creatindex) + mytransform.eulerAngles.y - 270f, 0f);
					c_sword[creatindex].gameObject.active = true;
					c_sword[creatindex].position += c_sword[creatindex].forward * 0.1f;
					c_sword[creatindex].forward = Vector3.up + mytransform.forward * 0.6f;
				}
				c_sword[creatindex].parent = mytransform;
				if (creatindex < 6)
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
		else if (start_delay > 0.4f)
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
		mytransform.position = cha1.position + Vector3.up * 0.2f;
		mytransform.rotation = Quaternion.Lerp(mytransform.rotation, cha1.rotation, Time.deltaTime * 3f);
	}
}
