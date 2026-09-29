using UnityEngine;

public class Bullet_trigger : MonoBehaviour
{
	public float bullet_speed;

	private Transform mytransform;

	public Transform bullet_splash;

	private Transform c_splash;

	private float delay_finish;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
		c_splash = (Transform)Object.Instantiate(bullet_splash, mytransform.position, Quaternion.identity);
		c_splash.gameObject.active = false;
		c_splash.GetComponent<Rigidbody>().mass = base.GetComponent<Rigidbody>().mass;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 8)
		{
			c_splash.position = mytransform.position + mytransform.forward * 0.1f;
			c_splash.gameObject.active = true;
			base.gameObject.active = false;
			mytransform.position = Vector3.one * 4f;
		}
	}

	private void Update()
	{
		if (delay_finish > 0.4f)
		{
			base.gameObject.active = false;
			delay_finish = 0f;
		}
		mytransform.position += mytransform.forward * Time.deltaTime * bullet_speed;
	}
}
