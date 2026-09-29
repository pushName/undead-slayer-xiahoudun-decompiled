using UnityEngine;

public class Bullet_chaarrow : MonoBehaviour
{
	private Transform mytransform;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
	}

	private void Update()
	{
		mytransform.position += 4f * base.transform.forward * Time.deltaTime;
		if (mytransform.position.y < 0f)
		{
			base.gameObject.active = false;
		}
	}
}
