using UnityEngine;

public class Bullet_tornado : MonoBehaviour
{
	private bool movestart;

	private int rnddir;

	private int monmovestat;

	private Vector3 targetpod;

	private AI_Enemy01 script_mon;

	private Transform mytransform;

	private Transform myparent;

	private Vector3 originscale;

	private float sintime;

	private float movedelay;

	private void Awake()
	{
		mytransform = base.transform;
		myparent = mytransform.parent;
		originscale = mytransform.localScale;
	}

	private void Start()
	{
		base.GetComponent<Animation>()["tornado"].speed = 0.3f;
		script_mon = mytransform.root.GetComponent<AI_Enemy01>();
	}

	private void OnEnable()
	{
		rnddir = Random.Range(0, 2);
		rnddir = rnddir * 2 - 1;
		mytransform.localScale = originscale;
		targetpod = Vector3.zero;
		movestart = false;
		movedelay = 0f;
		sintime = 0f;
		mytransform.parent = myparent;
		mytransform.position += Vector3.up * 0.1f;
	}

	private void Update()
	{
		movedelay += Time.deltaTime;
		if (!movestart)
		{
			monmovestat = script_mon.monmovestat;
			if (movedelay < 1.2f)
			{
				if (monmovestat <= 0)
				{
					Disappear();
				}
				mytransform.localScale += Vector3.one * 0.5f * Time.deltaTime;
			}
			else
			{
				mytransform.parent = null;
				movestart = true;
				mytransform.localScale = Vector3.one * 2f;
			}
			return;
		}
		sintime += Time.deltaTime;
		targetpod = mytransform.forward * 0.8f + mytransform.right * 0.1f * Mathf.Cos(sintime * 8f) * rnddir;
		mytransform.rotation = Quaternion.LookRotation(targetpod);
		mytransform.position += targetpod * Time.deltaTime;
		if (mytransform.localScale.y > 2.2f)
		{
			mytransform.localScale += (Vector3.right * -1f + Vector3.forward * -1f) * Time.deltaTime;
			if (mytransform.localScale.x < 1f)
			{
				Disappear();
			}
		}
		else
		{
			mytransform.localScale += Vector3.up * 0.2f;
		}
	}

	public void Disappear()
	{
		base.gameObject.active = false;
		mytransform.position = Vector3.one * 5f;
	}
}
