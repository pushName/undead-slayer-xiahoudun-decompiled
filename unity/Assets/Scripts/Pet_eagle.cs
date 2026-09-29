using UnityEngine;

public class Pet_eagle : MonoBehaviour
{
	private Transform cha1;

	private Vector3 rndPos;

	private float skilldelay = 2f;

	private int currentmotion;

	private bool skillon;

	private Vector3 directionVector;

	private Vector3 tochaV;

	private Transform finditem;

	private int itembehaviour;

	private Quaternion rotate;

	private Transform mytransform;

	private Vector3 edgepos;

	private bool anispeedchange;

	private float flyspeed = 0.5f;

	private float finishdelay = -3f;

	private float getitem_delay;

	private Animation myanimation;

	private void Awake()
	{
		mytransform = base.transform;
		myanimation = base.GetComponent<Animation>();
		cha1 = GameObject.FindWithTag("Player").transform;
	}

	public void OnEnable()
	{
		InvokeRepeating("LoadDir", 0.1f, 0.1f);
		InvokeRepeating("SetRndPosition", 0.1f, 2f);
		float num = 2 * Random.Range(0, 2) - 1;
		float num2 = 2 * Random.Range(0, 2) - 1;
		finishdelay = 0f;
		edgepos = new Vector3(num * 0.6f, 0.3f, num2 * 0.5f);
		mytransform.position = cha1.position + edgepos;
		flyspeed = 0.5f;
		myanimation.CrossFade("fly_flap");
	}

	private void Start()
	{
		myanimation["fly_glide"].layer = 1;
		myanimation["fly_flap"].layer = 1;
		myanimation["fly_glide"].speed = 0.4f;
		myanimation["fly_flap"].speed = 0.4f;
		finishdelay = -3f;
	}

	public void LoadDir()
	{
		rndPos = cha1.position + edgepos;
		directionVector = rndPos - mytransform.position;
		directionVector[1] = 0f;
		if (directionVector != Vector3.zero)
		{
			rotate = Quaternion.LookRotation(directionVector);
		}
	}

	public void SetRndPosition()
	{
		edgepos = Random.insideUnitSphere * 0.1f;
		edgepos[1] = 0.3f;
		currentmotion = Random.Range(0, 10);
		if (currentmotion < 7 || skillon)
		{
			myanimation.CrossFade("fly_flap");
		}
		else
		{
			myanimation.CrossFade("fly_glide");
		}
	}

	public void SkillOn()
	{
		skillon = true;
		anispeedchange = true;
	}

	public void SkillOff()
	{
		skillon = false;
		anispeedchange = true;
	}

	public void GetItem(Transform _finditem)
	{
		if (itembehaviour == 0 && !skillon)
		{
			finishdelay = 5.5f;
			finditem = _finditem;
			itembehaviour = 1;
			directionVector = finditem.position - mytransform.position;
			directionVector[1] = 0f;
			CancelInvoke("LoadDir");
			CancelInvoke("SetRndPosition");
		}
	}

	private void Update()
	{
		if (skillon)
		{
			if (skilldelay > 0f)
			{
				mytransform.position = Vector3.Lerp(mytransform.position, cha1.position + Vector3.up * 0.3f, Time.deltaTime * 6f);
				skilldelay -= Time.deltaTime;
				tochaV = cha1.position - mytransform.position;
				tochaV[1] = 0f;
				if (tochaV != Vector3.zero)
				{
					mytransform.rotation = Quaternion.LookRotation(tochaV);
				}
			}
			else
			{
				mytransform.position = cha1.position + Vector3.up * 0.3f;
				mytransform.rotation = Quaternion.Lerp(mytransform.rotation, cha1.rotation, Time.deltaTime * 3f);
				if (anispeedchange)
				{
					myanimation["fly_flap"].speed = 0.7f;
					anispeedchange = false;
				}
			}
			return;
		}
		if (itembehaviour == 1)
		{
			if (finditem.gameObject.active)
			{
				myanimation.CrossFade("fly_flap");
				getitem_delay += Time.deltaTime;
				if (getitem_delay > 2.5f)
				{
					getitem_delay = 0f;
					itembehaviour = 2;
				}
				else if (getitem_delay > 2.2f)
				{
					finditem.position = Vector3.MoveTowards(finditem.position, mytransform.position, Time.deltaTime);
				}
				mytransform.position = Vector3.Lerp(mytransform.position, finditem.position, Time.deltaTime * 2f);
				if (directionVector != Vector3.zero)
				{
					mytransform.rotation = Quaternion.LookRotation(directionVector);
				}
			}
			else
			{
				itembehaviour = 0;
			}
			return;
		}
		if (itembehaviour == 2)
		{
			if (finditem.gameObject.active)
			{
				getitem_delay += Time.deltaTime;
				if (getitem_delay > 2f)
				{
					itembehaviour = 0;
					getitem_delay = 0f;
					return;
				}
				if (getitem_delay > 1f)
				{
					finditem.position = Vector3.MoveTowards(finditem.position, cha1.position, Time.deltaTime * 1.2f);
					mytransform.position += mytransform.forward * Time.deltaTime * 0.8f;
					return;
				}
				directionVector = cha1.position - mytransform.position;
				directionVector[1] = 0f;
				if (directionVector != Vector3.zero)
				{
					mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.LookRotation(directionVector), Time.deltaTime * 3f);
				}
				mytransform.position = Vector3.Lerp(mytransform.position, cha1.position + Vector3.up * 0.3f, Time.deltaTime * 2f);
				finditem.position = mytransform.position - Vector3.up * 0.1f;
			}
			else
			{
				itembehaviour = 0;
			}
			return;
		}
		finishdelay += Time.deltaTime;
		if (finishdelay > 8f)
		{
			flyspeed = 1f;
			CancelInvoke("LoadDir");
			CancelInvoke("SetRndPosition");
			if (finishdelay > 9.5f)
			{
				base.gameObject.active = false;
				mytransform.position = Vector3.one * 8f;
			}
		}
		if (anispeedchange)
		{
			myanimation["fly_flap"].speed = 0.4f;
			anispeedchange = false;
		}
		skilldelay = 2f;
		mytransform.rotation = Quaternion.Lerp(mytransform.rotation, rotate, Time.deltaTime * 1.6f);
		mytransform.position += mytransform.forward * Time.deltaTime * flyspeed;
	}
}
