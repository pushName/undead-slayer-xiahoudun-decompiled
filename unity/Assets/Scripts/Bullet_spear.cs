using UnityEngine;

public class Bullet_spear : MonoBehaviour
{
	private Transform mytransform;

	private Vector3 originscale;

	public float tune;

	private void Awake()
	{
		mytransform = base.transform;
		originscale = mytransform.localScale;
	}

	private void OnEnable()
	{
		mytransform.position += mytransform.forward * tune;
	}

	private void Update()
	{
		mytransform.position += mytransform.forward * Time.deltaTime * (0.2f + tune);
		mytransform.localScale -= Vector3.right * 0.6f * Time.deltaTime;
		if (mytransform.localScale.x < 0f)
		{
			base.gameObject.active = false;
			mytransform.localScale = originscale;
		}
	}
}
