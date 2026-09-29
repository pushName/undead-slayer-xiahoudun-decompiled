using UnityEngine;

public class Bullet_delay2 : MonoBehaviour
{
	public float show_delay;

	public float disable_delay;

	public bool moveleft;

	private Transform mytransform;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void OnEnable()
	{
		Invoke("StartObj", show_delay);
		Invoke("finishObj", disable_delay);
		base.GetComponent<Collider>().enabled = false;
	}

	private void StartObj()
	{
		base.gameObject.active = true;
		base.GetComponent<Collider>().enabled = true;
	}

	private void finishObj()
	{
		base.gameObject.active = false;
		base.GetComponent<Collider>().enabled = false;
	}

	private void Update()
	{
		if (moveleft)
		{
			mytransform.position -= mytransform.right * Time.deltaTime * 0.2f;
		}
	}
}
