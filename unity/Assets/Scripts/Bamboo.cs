using UnityEngine;

public class Bamboo : MonoBehaviour
{
	private Transform mytransform;

	public int collideroff;

	public float finishdelay = 4.2f;

	private float delay;

	private Collider mycollider;

	private Vector3 originpos;

	private bool next;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
	}

	private void OnEnable()
	{
		originpos = mytransform.position;
		originpos[1] = 0f;
		delay = 0f;
		mycollider.enabled = true;
		mycollider.isTrigger = true;
	}

	private void Update()
	{
		delay += Time.deltaTime;
		if (delay > finishdelay)
		{
			mytransform.position -= Vector3.up * Time.deltaTime * 2f;
			if (mytransform.position.y < -1f)
			{
				next = false;
				base.gameObject.active = false;
				mytransform.position = Vector3.one * 5f;
			}
		}
		else if (delay > 0.5f)
		{
			if (!next)
			{
				if (collideroff == 0)
				{
					mycollider.enabled = false;
					mytransform.position = originpos;
				}
				else if (collideroff == 1)
				{
					mycollider.isTrigger = false;
					mytransform.position = originpos;
				}
				next = true;
			}
		}
		else if (mytransform.position.y >= 0f)
		{
			mytransform.position = originpos;
		}
		else
		{
			mytransform.position += Vector3.up * Time.deltaTime * 3f;
		}
	}
}
