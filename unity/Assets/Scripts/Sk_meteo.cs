using UnityEngine;

public class Sk_meteo : MonoBehaviour
{
	private Transform mytransform;

	private bool falldown;

	private Vector3 currentpos;

	private Vector3 directionVector;

	private Collider mycollider;

	private float finish_delay;

	private Ef_boom script_boom;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		script_boom = GameObject.FindWithTag("skill").GetComponent<Ef_boom>();
	}

	private void OnEnable()
	{
		{
			var emission = mytransform.GetChild(0).GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		{
			var emission = mytransform.GetChild(1).GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		falldown = true;
		finish_delay = 0f;
		directionVector = mytransform.forward - Vector3.up * 0.7f;
		mytransform.up = -directionVector;
		currentpos = mytransform.position;
	}

	private void Update()
	{
		if (falldown)
		{
			if (mytransform.position.y > 0f)
			{
				currentpos += directionVector * 0.08f;
			}
			else
			{
				mycollider.enabled = true;
				falldown = false;
				currentpos[1] = 0f;
				script_boom.SetTex(0, currentpos, _collider: true);
			}
			mytransform.position = currentpos;
		}
		else if (finish_delay > 2f)
		{
			{
				var emission = mytransform.GetChild(0).GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
			{
				var emission = mytransform.GetChild(1).GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
			base.gameObject.active = false;
			mycollider.enabled = false;
		}
		else
		{
			finish_delay += Time.deltaTime;
		}
	}
}
