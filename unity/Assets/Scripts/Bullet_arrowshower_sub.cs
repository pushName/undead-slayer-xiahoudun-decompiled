using UnityEngine;

public class Bullet_arrowshower_sub : MonoBehaviour
{
	private Transform mytransform;

	private void Awake()
	{
		mytransform = base.transform;
		Object.Destroy(base.gameObject, 1f);
	}

	private void Update()
	{
		if (mytransform.position.y > 0.2f)
		{
			mytransform.position -= Vector3.up * Time.deltaTime * 3f;
		}
		else if (mytransform.localScale.y > 0f)
		{
			mytransform.position -= Vector3.up * Time.deltaTime;
			mytransform.localScale -= Vector3.up * Time.deltaTime;
		}
	}
}
