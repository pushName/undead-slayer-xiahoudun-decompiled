using UnityEngine;

public class Snake_base : MonoBehaviour
{
	private Transform mytransform;

	private float finish_delay;

	private Collider mycollider;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
	}

	private void OnEnable()
	{
		finish_delay = 0f;
		mytransform.localScale = Vector3.zero;
		mytransform.GetChild(0).gameObject.active = true;
		mytransform.GetChild(1).gameObject.active = true;
		{
			var emission = base.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		InvokeRepeating("ColliderOn", 0.3f, 0.5f);
	}

	private void ColliderOn()
	{
		mycollider.enabled = false;
		mycollider.enabled = true;
	}

	private void Update()
	{
		finish_delay += Time.deltaTime;
		if (finish_delay > 8f)
		{
			base.gameObject.active = false;
			mytransform.GetChild(0).gameObject.active = false;
			mytransform.GetChild(1).gameObject.active = false;
			CancelInvoke();
		}
		else if (finish_delay > 6f)
		{
			{
				var emission = base.GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
			mytransform.localScale = Vector3.MoveTowards(mytransform.localScale, Vector3.zero, Time.deltaTime * 3f);
		}
		else
		{
			mytransform.localScale = Vector3.MoveTowards(mytransform.localScale, Vector3.one, Time.deltaTime * 3f);
		}
	}
}
