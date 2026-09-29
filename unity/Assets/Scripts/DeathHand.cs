using UnityEngine;

public class DeathHand : MonoBehaviour
{
	private float finish_delay = 1.2f;

	private float start_delay = 0.4f;

	private float current_time;

	private Collider mycollider;

	private Transform mytransform;

	private Transform myparent;

	private Renderer myrenderer;

	private bool dash;

	private Transform pt;

	private Cha_Control script_cha;

	private float fogdelay = 0.2f;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		myrenderer = base.GetComponent<Renderer>();
		mycollider.enabled = false;
	}

	private void Start()
	{
		myrenderer.enabled = false;
		pt = mytransform.GetChild(0);
		myparent = mytransform.parent;
		script_cha = myparent.GetComponent<Cha_Control>();
	}

	private void Update()
	{
		current_time += Time.deltaTime;
		if (current_time > finish_delay)
		{
			base.gameObject.active = false;
			current_time = 0f;
			mycollider.enabled = false;
			myrenderer.enabled = false;
			dash = false;
			{
				var emission = pt.GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
		}
		else if (current_time > start_delay)
		{
			if (!dash)
			{
				dash = true;
				myparent.GetComponent<Rigidbody>().AddForce(myparent.forward * 200f);
				myrenderer.enabled = true;
				mycollider.enabled = true;
				{
					var emission = pt.GetComponent<ParticleSystem>().emission;
					emission.enabled = true;
				}
			}
			if (fogdelay >= 0.2f)
			{
				script_cha.FogOn(1);
				fogdelay = 0f;
			}
			else
			{
				fogdelay += Time.deltaTime;
			}
		}
	}
}
