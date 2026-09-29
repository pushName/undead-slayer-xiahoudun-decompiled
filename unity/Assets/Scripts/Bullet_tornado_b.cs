using UnityEngine;

public class Bullet_tornado_b : MonoBehaviour
{
	private Transform mytransform;

	private Vector3 originscale;

	private void Awake()
	{
		mytransform = base.transform;
		originscale = mytransform.localScale;
	}

	private void OnEnable()
	{
		mytransform.localScale = originscale;
	}

	private void Start()
	{
		base.GetComponent<Animation>()["tornado"].speed = 0.3f;
		mytransform.position += mytransform.forward * 0.1f;
	}

	private void Update()
	{
		float num = 0.5f * Time.deltaTime;
		mytransform.localScale += new Vector3(-5f * num, num, -5f * num);
		mytransform.position += mytransform.forward * num;
		if (mytransform.localScale.y > 4f)
		{
			base.gameObject.active = false;
			mytransform.position = Vector3.one * 5f;
		}
	}
}
