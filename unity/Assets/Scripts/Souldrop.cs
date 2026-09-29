using UnityEngine;

public class Souldrop : MonoBehaviour
{
	private Transform cha1;

	private Transform mytransform;

	private Vector3 directionVector;

	private Vector3 targetVector;

	private Collider mycollider;

	private Quaternion rotate;

	private float finish_delay = 4f;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
	}

	private void Start()
	{
		cha1 = GameObject.FindWithTag("Player").transform;
		base.gameObject.active = false;
	}

	private void OnEnable()
	{
		directionVector = Vector3.up * 2f;
		{
			var emission = base.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		finish_delay = 4f;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 15)
		{
			mytransform.position = cha1.position;
			mycollider.enabled = false;
			{
				var emission = base.GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
			Invoke("Finish", 0.5f);
		}
	}

	public void Finish()
	{
		base.gameObject.active = false;
		mytransform.position = Vector3.one * 34f;
	}

	private void Update()
	{
		if (finish_delay < 0f)
		{
			Finish();
			finish_delay = 4f;
		}
		else if (finish_delay < 3f)
		{
			mycollider.enabled = true;
		}
		else
		{
			finish_delay -= Time.deltaTime;
		}
		targetVector = Vector3.Normalize(cha1.position - mytransform.position + Vector3.up * 0.05f);
		directionVector = Vector3.MoveTowards(directionVector, targetVector, Time.deltaTime * 5f);
		mytransform.position += directionVector * Time.deltaTime;
	}
}
