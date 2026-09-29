using UnityEngine;

public class Trap_Extreme : MonoBehaviour
{
	private Transform mytransform;

	private Collider mycollider;

	public float firerate = 4f;

	public float duration = 1f;

	public float startdelay = 0.5f;

	public bool rndRotation;

	public float startPosY;

	public Vector3 startScale;

	public float growScaleSpeed;

	public Vector3 movedir;

	public float accuracy = 0.1f;

	public bool playAutomatically;

	public Transform liveObj;

	private bool trapOn;

	private Vector3 originTrapScale;

	private Transform target;

	private bool scaleUp;

	private bool scaleDown;

	private bool moveOn;

	private int damage = 1;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		originTrapScale = mytransform.localScale;
		target = GameObject.FindWithTag("Player").transform;
		mytransform.position = Vector3.up * 56f;
		mytransform.rotation = Quaternion.identity;
		mycollider.enabled = false;
	}

	public void StopActive()
	{
		if ((bool)liveObj)
		{
			liveObj.SendMessage("FisnishShoot");
		}
		CancelInvoke();
		trapOn = false;
		scaleUp = false;
		scaleDown = false;
		moveOn = false;
		mycollider.enabled = false;
		mytransform.position = Vector3.up * 45f;
	}

	public void SetDamage(int _damage)
	{
		if ((bool)liveObj)
		{
			InvokeRepeating("LiveObjOn", 3f, firerate);
		}
		damage = _damage;
		Invoke("Ready_trap", firerate + Random.Range(0f, 2f) + 3f);
		trapOn = true;
	}

	private void LiveObjOn()
	{
		liveObj.SendMessage("StartShoot");
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 15)
		{
			Vector3 vector = Vector3.Normalize(other.transform.position - mytransform.position);
			vector[1] = 0f;
			other.transform.root.SendMessage("Damaged", mytransform.forward + Vector3.up * damage);
		}
	}

	public void DirectFire(Vector3 _pos)
	{
		if (trapOn)
		{
			mytransform.position = _pos;
			Emit_trap();
			if (rndRotation)
			{
				mytransform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
			}
		}
	}

	private void Emit_trap()
	{
		if (trapOn)
		{
			mytransform.localScale = originTrapScale;
			if (growScaleSpeed != 0f)
			{
				scaleUp = true;
			}
			if (movedir != Vector3.zero)
			{
				moveOn = true;
			}
			mycollider.enabled = true;
			Invoke("Disappear_trap", duration);
		}
	}

	private void Disappear_trap()
	{
		if (trapOn)
		{
			scaleUp = false;
			scaleDown = true;
			moveOn = false;
			if (playAutomatically)
			{
				Invoke("Ready_trap", firerate + Random.Range(0f, 2f));
			}
			mycollider.enabled = false;
		}
	}

	private void Ready_trap()
	{
		if (trapOn)
		{
			Vector3 position = target.position + Random.onUnitSphere * accuracy;
			if (startPosY != 0f)
			{
				position.y = startPosY;
			}
			else
			{
				position.y = 0f;
			}
			mytransform.position = position;
			mytransform.localScale = startScale;
			if (rndRotation)
			{
				mytransform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
			}
			Invoke("Emit_trap", startdelay);
		}
	}

	private void Update()
	{
		if (scaleUp)
		{
			mytransform.localScale = Vector3.MoveTowards(mytransform.localScale, originTrapScale, growScaleSpeed * Time.deltaTime);
		}
		else if (scaleDown)
		{
			mytransform.localScale = Vector3.MoveTowards(mytransform.localScale, Vector3.zero, Time.deltaTime);
			if (mytransform.localScale.x < 0.01f)
			{
				mytransform.position = Vector3.up * 45f;
				scaleDown = false;
			}
		}
		if (moveOn && mytransform.position.y > 0f)
		{
			mytransform.position += movedir * Time.deltaTime;
		}
	}
}
