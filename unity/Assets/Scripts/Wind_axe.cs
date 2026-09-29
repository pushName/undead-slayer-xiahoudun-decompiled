using UnityEngine;

public class Wind_axe : MonoBehaviour
{
	private Transform mytransform;

	private Transform cha1;

	private Vector3 originscale;

	private float finish_delay;

	private void Awake()
	{
		mytransform = base.transform;
		cha1 = GameObject.FindWithTag("Player").transform;
		originscale = mytransform.localScale;
		mytransform.GetChild(0).GetComponent<Animation>()["tornado"].speed = 0.2f;
	}

	private void OnEnable()
	{
		mytransform.localScale = Vector3.zero;
		mytransform.GetChild(0).gameObject.active = true;
		finish_delay = 0f;
	}

	private void Update()
	{
		mytransform.position = cha1.position;
		finish_delay += Time.deltaTime;
		if (finish_delay > 1.5f)
		{
			mytransform.localScale = Vector3.MoveTowards(mytransform.localScale, Vector3.zero, Time.deltaTime * 5f);
		}
		else
		{
			mytransform.localScale = Vector3.MoveTowards(mytransform.localScale, originscale, Time.deltaTime * 3f);
		}
	}
}
