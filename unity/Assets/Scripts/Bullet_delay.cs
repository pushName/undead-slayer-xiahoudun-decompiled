using UnityEngine;

public class Bullet_delay : MonoBehaviour
{
	public float show_delay;

	public float disable_delay;

	private float current_time;

	private Collider mycollider;

	private bool renderOn;

	private Renderer myrenderer;

	private Transform mytransform;

	private Vector3 originscale;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		originscale = mytransform.localScale;
		mytransform.localScale = originscale * 0.1f;
		if ((bool)base.GetComponent<Renderer>())
		{
			myrenderer = base.GetComponent<Renderer>();
		}
		else
		{
			renderOn = true;
		}
	}

	private void OnEnable()
	{
		current_time = 0f;
		base.GetComponent<Renderer>().enabled = false;
		mycollider.enabled = false;
	}

	private void Update()
	{
		current_time += Time.deltaTime;
		if (current_time >= disable_delay)
		{
			base.gameObject.active = false;
			current_time = 0f;
			renderOn = false;
			base.GetComponent<Renderer>().enabled = false;
			mycollider.enabled = false;
			mytransform.localScale = originscale * 0.1f;
		}
		else if (renderOn)
		{
			if (mytransform.localScale.x < originscale.x)
			{
				mytransform.localScale += Vector3.one * Time.deltaTime * 4f;
			}
		}
		else if (current_time >= show_delay)
		{
			mycollider.enabled = true;
			if ((bool)myrenderer)
			{
				myrenderer.enabled = true;
				renderOn = true;
			}
		}
	}
}
