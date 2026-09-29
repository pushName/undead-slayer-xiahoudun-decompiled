using UnityEngine;

public class Dragonhead : MonoBehaviour
{
	private Transform mytransform;

	private SphereCollider mycollider;

	private Animation myanimation;

	private float active_delay;

	private int impact;

	public Transform pt_body;

	public Transform zwater;

	private AnimationState impact_ani;

	public AudioClip snd_water;

	public AudioClip snd_bite;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = (SphereCollider)base.GetComponent<Collider>();
		myanimation = base.GetComponent<Animation>();
		myanimation["bite_ready"].speed = 0.15f;
	}

	private void OnEnable()
	{
		mycollider.enabled = false;
		myanimation.Play("bite_ready");
		impact_ani = myanimation.PlayQueued("bite");
		impact_ani.speed = 0.15f;
		impact_ani = myanimation.PlayQueued("bite_i");
		impact_ani.speed = 0.25f;
		impact = 0;
		{
			var emission = pt_body.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		zwater.localScale = Vector3.one * 0.32f;
	}

	private void Update()
	{
		if (myanimation.IsPlaying("bite_i"))
		{
			if (impact != 2)
			{
				base.GetComponent<Rigidbody>().mass *= 4f;
				mycollider.center = Vector3.forward * 0.1f;
				mycollider.enabled = true;
				impact = 2;
				active_delay = 0f;
				base.GetComponent<AudioSource>().clip = snd_bite;
				base.GetComponent<AudioSource>().Play();
			}
			else
			{
				active_delay += Time.deltaTime;
				if (active_delay > 3f)
				{
					mycollider.enabled = false;
					base.gameObject.active = false;
					mytransform.position = Vector3.one * 16f;
					{
						var emission = pt_body.GetComponent<ParticleSystem>().emission;
						emission.enabled = false;
					}
					active_delay = 0f;
				}
				else if (active_delay > 2f)
				{
					zwater.localScale -= Vector3.right * Time.deltaTime * 0.3f;
				}
				else if (active_delay > 0.5f)
				{
					mycollider.enabled = false;
				}
			}
			mytransform.position += mytransform.forward * Time.deltaTime * 0.1f;
		}
		else if (myanimation.IsPlaying("bite"))
		{
			if (impact != 1)
			{
				base.GetComponent<AudioSource>().clip = snd_water;
				base.GetComponent<AudioSource>().Play();
				active_delay = 0f;
				mycollider.center = Vector3.zero;
				mycollider.enabled = true;
				impact = 1;
			}
			else
			{
				active_delay += Time.deltaTime;
				if (active_delay > 0.5f)
				{
					mycollider.enabled = false;
				}
			}
		}
		else if (myanimation.IsPlaying("bite_ready"))
		{
			mytransform.position += mytransform.forward * Time.deltaTime * 0.8f;
		}
	}
}
