using UnityEngine;

public class Pt_hit : MonoBehaviour
{
	private bool pton;

	private float s_delay;

	private ParticleSystem myparticle;

	private ParticleSystem.EmissionModule myemission;

	private void Start()
	{
		myparticle = base.GetComponent<ParticleSystem>();
		myemission = myparticle.emission;
		myemission.enabled = false;
	}

	public void ParticleOn()
	{
		myemission.enabled = true;
		pton = true;
	}

	private void Update()
	{
		if (pton)
		{
			if (s_delay > 0.1f)
			{
				myemission.enabled = false;
				pton = false;
				s_delay = 0f;
			}
			else
			{
				s_delay += Time.deltaTime;
			}
		}
	}
}
