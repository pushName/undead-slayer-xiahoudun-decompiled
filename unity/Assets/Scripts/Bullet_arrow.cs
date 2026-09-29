using UnityEngine;

public class Bullet_arrow : MonoBehaviour
{
	public float bullet_speed;

	private Transform mytransform;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Update()
	{
		mytransform.position += mytransform.forward * Time.deltaTime * bullet_speed;
	}
}
