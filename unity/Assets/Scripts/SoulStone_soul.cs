using UnityEngine;

public class SoulStone_soul : MonoBehaviour
{
	private Transform mytransform;

	private GameObject cha1;

	private Cha_Control_ride_cha script_cha;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
		cha1 = GameObject.FindWithTag("Player");
		script_cha = cha1.GetComponent<Cha_Control_ride_cha>();
		mytransform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other == cha1.GetComponent<Collider>())
		{
			script_cha.GetSoulStone(1);
			mytransform.position = -2f * Vector3.forward;
		}
	}

	private void Update()
	{
		if (base.transform.position.z > -0.6f)
		{
			base.transform.position += Vector3.forward * -1.8f * Time.deltaTime;
			mytransform.Rotate(Vector3.up * 500f * Time.deltaTime);
		}
		else
		{
			mytransform.position = -2f * Vector3.forward;
		}
	}
}
