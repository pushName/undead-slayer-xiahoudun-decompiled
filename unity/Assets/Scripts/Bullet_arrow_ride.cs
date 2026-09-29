using UnityEngine;

public class Bullet_arrow_ride : MonoBehaviour
{
	public float bullet_speed;

	private Transform mytransform;

	private Transform cha1;

	private Cha_Control_ride_cha script_cha;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
		cha1 = GameObject.FindWithTag("Player").transform;
		script_cha = cha1.GetComponent<Cha_Control_ride_cha>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.IsChildOf(cha1))
		{
			base.gameObject.active = false;
			mytransform.position = Vector3.one * 12f;
			script_cha.Damaged();
		}
	}

	private void Update()
	{
		mytransform.position += mytransform.forward * Time.deltaTime * bullet_speed;
	}
}
