using UnityEngine;

public class Ef_swtrail : MonoBehaviour
{
	private Transform mytransform;

	private void Awake()
	{
		mytransform = base.transform;
		mytransform.localScale = Vector3.zero;
	}

	public void OnEnable()
	{
		mytransform.localScale = Vector3.one * 3f;
	}

	private void Update()
	{
		base.transform.localScale -= Vector3.one * 10f * Time.deltaTime;
		if (mytransform.localScale.x < 0.1f)
		{
			base.gameObject.active = false;
			mytransform.localScale = Vector3.zero;
		}
	}
}
