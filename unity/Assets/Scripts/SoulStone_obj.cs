using UnityEngine;

public class SoulStone_obj : MonoBehaviour
{
	private Transform mytransform;

	public Transform obj_break;

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
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other == cha1.GetComponent<Collider>())
		{
			Object.Instantiate(obj_break, mytransform.position, mytransform.rotation);
			script_cha.FallDown();
			mytransform.position = -2f * Vector3.forward;
		}
		else if (other.gameObject.layer == 8)
		{
			Object.Instantiate(obj_break, mytransform.position, mytransform.rotation);
			mytransform.position = -2f * Vector3.forward;
		}
	}

	private void Update()
	{
		if (base.transform.position.z > -0.6f)
		{
			base.transform.position += Vector3.forward * -1.8f * Time.deltaTime;
		}
		else
		{
			mytransform.position = -2f * Vector3.forward;
		}
	}
}
