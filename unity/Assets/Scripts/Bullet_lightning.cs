using UnityEngine;

public class Bullet_lightning : MonoBehaviour
{
	private Transform cha1;

	private Transform mytransform;

	private void Awake()
	{
		mytransform = base.transform;
		cha1 = GameObject.FindWithTag("Player").transform;
	}

	private void OnEnable()
	{
		mytransform.position = cha1.transform.position + new Vector3(Random.Range(-0.02f, 0.04f), 1.5f, Random.Range(-0.02f, 0.04f));
	}

	private void Update()
	{
		if (base.GetComponent<Renderer>().enabled)
		{
			if (mytransform.position.y > 0f)
			{
				mytransform.position -= Vector3.up * 5f * Time.deltaTime;
			}
			else
			{
				mytransform.position = Vector3.right * mytransform.position.x + Vector3.forward * mytransform.position.z;
			}
		}
	}
}
