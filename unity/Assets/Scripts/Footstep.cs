using UnityEngine;

public class Footstep : MonoBehaviour
{
	private Cha_Control script_cha;

	private int chamovestat;

	private bool turnon;

	private float s_delay;

	private ParticleSystem myparticle;

	private ParticleSystem.EmissionModule myemission;

	private void Start()
	{
		myparticle = base.GetComponent<ParticleSystem>();
		myemission = myparticle.emission;
		script_cha = base.transform.parent.GetComponent<Cha_Control>();
		myemission.enabled = false;
	}

	private void Update()
	{
		if (s_delay > 0.5f)
		{
			chamovestat = script_cha.chamovestat;
			if (chamovestat == 2)
			{
				if (!turnon)
				{
					turnon = true;
					myemission.enabled = true;
				}
			}
			else
			{
				turnon = false;
				myemission.enabled = false;
			}
		}
		else
		{
			s_delay += Time.deltaTime;
		}
	}
}
