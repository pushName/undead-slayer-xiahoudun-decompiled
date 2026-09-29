using UnityEngine;

public class Particle_Emit : MonoBehaviour
{
	public float emitfinish_time;

	public float destroy_time;

	public float show_time;

	public float movespeed;

	public bool ismoveup;

	private bool move;

	private Transform mytransform;

	private Collider mycollider;

	private bool colliderexist;

	private void Awake()
	{
		mytransform = base.transform;
		if (base.GetComponent<Collider>() != null)
		{
			mycollider = base.GetComponent<Collider>();
			colliderexist = true;
		}
	}

	private void OnEnable()
	{
		Invoke("EmitStart", show_time);
		Invoke("EmitFinish", emitfinish_time);
		Invoke("DestroyEmitter", destroy_time);
		move = false;
	}

	public void EmitStart()
	{
		{
			var emission = base.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		move = true;
		if (colliderexist)
		{
			InvokeRepeating("RepeatDamage", 0.1f, 0.3f);
		}
	}

	public void EmitFinish()
	{
		{
			var emission = base.GetComponent<ParticleSystem>().emission;
			emission.enabled = false;
		}
		if (colliderexist)
		{
			CancelInvoke("RepeatDamage");
			mycollider.enabled = false;
		}
	}

	public void DestroyEmitter()
	{
		base.gameObject.active = false;
	}

	private void RepeatDamage()
	{
		mycollider.enabled = false;
		mycollider.enabled = true;
	}

	private void Update()
	{
		if (move)
		{
			if (!ismoveup)
			{
				mytransform.position += mytransform.forward * Time.deltaTime * movespeed;
			}
			else
			{
				mytransform.position += Vector3.up * Time.deltaTime * movespeed;
			}
		}
	}
}
