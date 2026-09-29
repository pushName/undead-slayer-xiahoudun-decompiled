using UnityEngine;

public class Ef_swordwind : MonoBehaviour
{
	private bool movestart;

	private Transform mytransform;

	private Renderer myrenderer;

	private float delay;

	private Vector3 originscale;

	private void Awake()
	{
		mytransform = base.transform;
		myrenderer = base.GetComponent<Renderer>();
	}

	private void Start()
	{
		originscale = base.transform.localScale;
		myrenderer.enabled = false;
	}

	private void OnEnable()
	{
		for (int i = 0; i < 2; i++)
		{
			base.transform.GetChild(i).gameObject.active = true;
		}
	}

	private void Update()
	{
		if (!movestart)
		{
			if (delay < 0.8f)
			{
				delay += Time.deltaTime;
				return;
			}
			movestart = true;
			myrenderer.enabled = true;
			delay = 0f;
		}
		else if (mytransform.localScale.z > 1f)
		{
			mytransform.position += mytransform.forward * Time.deltaTime;
			mytransform.localScale -= Vector3.forward * 3f * Time.deltaTime;
		}
		else
		{
			base.transform.localScale = originscale;
			mytransform.position = Vector3.one * 22f;
			base.GetComponent<Renderer>().enabled = false;
			base.gameObject.active = false;
			movestart = false;
		}
	}
}
