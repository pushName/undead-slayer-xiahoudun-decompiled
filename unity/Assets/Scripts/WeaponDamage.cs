using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
	private Transform mytransform;

	public int damage;

	public bool impactDestroy;

	public float destroytime;

	public float colliderofftime;

	private float currenttime;

	private Collider mycollider;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		currenttime = 0f;
	}

	private void OnEnable()
	{
		currenttime = 0f;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 15)
		{
			Vector3 forward = mytransform.forward;
			forward[1] = 0f;
			other.transform.root.SendMessage("Damaged", mytransform.forward + Vector3.up * damage);
			if (impactDestroy)
			{
				base.gameObject.active = false;
				mytransform.position = Vector3.one * 5f;
			}
		}
	}

	public void PressDamage(int a)
	{
		damage = a;
	}

	private void Update()
	{
		if (destroytime > 0f)
		{
			currenttime += Time.deltaTime;
			if (colliderofftime > 0f && currenttime >= colliderofftime)
			{
				mycollider.enabled = false;
			}
			if (currenttime >= destroytime)
			{
				currenttime = 0f;
				base.gameObject.active = false;
				mytransform.position = Vector3.one * 5f;
				mycollider.enabled = true;
			}
		}
	}
}
