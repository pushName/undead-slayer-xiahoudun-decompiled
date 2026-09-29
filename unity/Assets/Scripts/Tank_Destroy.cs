using UnityEngine;

public class Tank_Destroy : MonoBehaviour
{
	private float delay;

	private bool fogon;

	private Animation myanimation;

	public Transform pt_fog;

	private void Awake()
	{
		myanimation = base.GetComponent<Animation>();
		myanimation["tank_destroy"].speed = 0.25f;
	}

	private void OnEnable()
	{
		{
			var emission = pt_fog.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		delay = 0f;
		fogon = true;
		myanimation.Stop();
		myanimation.Play("tank_destroy");
	}

	private void Update()
	{
		if (delay > 2.5f)
		{
			base.transform.position = Vector3.up * 25f;
			base.gameObject.active = false;
		}
		if (delay > 0.5f && fogon)
		{
			{
				var emission = pt_fog.GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
			fogon = false;
		}
		delay += Time.deltaTime;
	}
}
