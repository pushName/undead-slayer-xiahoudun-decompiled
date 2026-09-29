using UnityEngine;

public class DunGate : MonoBehaviour
{
	private Transform mytransform;

	private Transform arrow;

	private Transform cha1;

	private float delay_open;

	private void Awake()
	{
		mytransform = base.transform;
		arrow = mytransform.GetChild(0);
		arrow.position = Vector3.one * 22f;
	}

	private void OnEnable()
	{
		base.GetComponent<Collider>().enabled = false;
		delay_open = 1.5f;
	}

	private void Start()
	{
		cha1 = GameObject.FindWithTag("Player").transform;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.root == cha1)
		{
			cha1.GetComponent<Cha_Control>().GateEnter(base.transform.position);
			base.gameObject.active = false;
			arrow.position = Vector3.one * 22f;
		}
	}

	private void Update()
	{
		if (delay_open > 0f)
		{
			delay_open -= Time.deltaTime;
			if (delay_open <= 0f)
			{
				base.GetComponent<Collider>().enabled = true;
			}
		}
		Vector3 vector = Vector3.Normalize(mytransform.position - cha1.position);
		if (vector != Vector3.zero)
		{
			arrow.rotation = Quaternion.LookRotation(vector);
		}
		vector = cha1.position + Vector3.up * 0.02f + vector * 0.3f;
		arrow.position = vector;
	}
}
