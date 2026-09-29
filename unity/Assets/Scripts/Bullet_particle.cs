using UnityEngine;

public class Bullet_particle : MonoBehaviour
{
	public float start_delay;

	public float active_delay;

	public float disable_delay;

	private float current_time;

	private ParticleSystem myemitter;

	private ParticleSystem.EmissionModule myemission;

	private Collider mycollider;

	private bool colliderexist;

	private void Awake()
	{
		myemitter = base.GetComponent<ParticleSystem>();
		myemission = myemitter.emission;
		if ((bool)base.GetComponent<Collider>())
		{
			mycollider = base.GetComponent<Collider>();
			colliderexist = true;
		}
		base.gameObject.active = false;
	}

	private void OnEnable()
	{
		current_time = 0f;
		myemission.enabled = false;
		if (colliderexist)
		{
			mycollider.enabled = false;
		}
	}

	private void Update()
	{
		current_time += Time.deltaTime;
		if (current_time >= disable_delay)
		{
			base.gameObject.active = false;
		}
		if (current_time >= active_delay)
		{
			myemission.enabled = false;
			if (colliderexist)
			{
				mycollider.enabled = false;
			}
		}
		else if (current_time >= start_delay)
		{
			myemission.enabled = true;
			if (colliderexist)
			{
				mycollider.enabled = true;
			}
		}
	}
}
