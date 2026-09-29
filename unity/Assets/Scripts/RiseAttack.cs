using UnityEngine;

public class RiseAttack : MonoBehaviour
{
	public float emitfinish_time;

	public float destroy_time;

	public float movespeed;

	private Transform mytransform;

	private Collider mycollider;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
	}

	private void OnEnable()
	{
		Invoke("EmitFinish", emitfinish_time);
		Invoke("DestroyEmitter", destroy_time);
		mycollider.enabled = true;
		{
			var emission = base.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
	}

	public void EmitFinish()
	{
		{
			var emission = base.GetComponent<ParticleSystem>().emission;
			emission.enabled = false;
		}
		mycollider.enabled = false;
	}

	public void DestroyEmitter()
	{
		base.gameObject.active = false;
	}

	private void Update()
	{
		mytransform.position += mytransform.up * Time.deltaTime * movespeed;
	}
}
