using UnityEngine;

public class Itemdrop : MonoBehaviour
{
	private Cha_Control script_cha;

	private int itemindex;

	private int itemlevel;

	private Transform mytransform;

	private Renderer myrenderer;

	private Transform cha1;

	private float maxy = 1.4f;

	private float distime;

	private bool drop;

	private void Awake()
	{
		mytransform = base.transform;
		myrenderer = base.GetComponent<Renderer>();
		base.GetComponent<Renderer>().sharedMaterial.renderQueue = 4003;
		cha1 = GameObject.FindWithTag("Player").transform;
		script_cha = cha1.GetComponent<Cha_Control>();
	}

	private void Start()
	{
		base.gameObject.active = false;
	}

	public void Whatsindex(int _index, int _level)
	{
		itemindex = _index;
		itemlevel = _level;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.IsChildOf(cha1))
		{
			script_cha.GetItem(itemindex, itemlevel);
			Disappear();
		}
	}

	public void Disappear()
	{
		mytransform.position = Vector3.one * 5f;
		base.GetComponent<Collider>().enabled = false;
		base.gameObject.active = false;
		maxy = 1.4f;
		distime = 0f;
		drop = false;
	}

	private void Update()
	{
		distime += Time.deltaTime;
		if (distime > 14f)
		{
			Disappear();
		}
		else if (distime > 10f)
		{
			if (distime * 10f % 4f > 2f)
			{
				myrenderer.enabled = true;
			}
			else
			{
				myrenderer.enabled = false;
			}
		}
		if (!drop)
		{
			if (mytransform.position.y >= 0.05f)
			{
				maxy -= 4.5f * Time.deltaTime;
				mytransform.position += Vector3.up * maxy * Time.deltaTime;
				return;
			}
			drop = true;
			script_cha.FindItem(mytransform);
			base.GetComponent<Collider>().enabled = true;
			mytransform.position = Vector3.right * mytransform.position.x + Vector3.forward * mytransform.position.z;
		}
	}
}
