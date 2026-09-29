using UnityEngine;

public class Ef_block : MonoBehaviour
{
	private Transform mytransform;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Update()
	{
		if (mytransform.localScale.x < 1.4f)
		{
			mytransform.localScale += Vector3.one * 2f * Time.deltaTime;
			return;
		}
		base.gameObject.active = false;
		mytransform.localScale = Vector3.one;
	}
}
