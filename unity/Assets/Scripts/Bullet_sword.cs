using UnityEngine;

public class Bullet_sword : MonoBehaviour
{
	private Transform mytransform;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Update()
	{
		mytransform.position += Vector3.up * Time.deltaTime * 0.1f;
	}
}
