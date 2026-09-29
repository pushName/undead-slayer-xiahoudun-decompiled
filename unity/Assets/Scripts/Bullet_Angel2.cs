using UnityEngine;

public class Bullet_Angel2 : MonoBehaviour
{
	public float delay_collision = 0.2f;

	public float delay_destroy = 0.6f;

	private float delay;

	private Transform mytransform;

	private AI_Asist script_angel;

	private Collider mycollider;

	private bool impactOn;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		script_angel = GameObject.Find("asist").GetComponent<AI_Asist>();
		base.gameObject.active = false;
	}

	private void OnEnable()
	{
		base.GetComponent<Collider>().enabled = false;
		delay = 0f;
		impactOn = false;
		mytransform.rotation = Quaternion.LookRotation(new Vector3(mytransform.forward.x, 0f, mytransform.forward.z));
		base.GetComponent<ParticleSystem>().Emit(1);
	}

	private void Update()
	{
		if (delay > delay_destroy)
		{
			mycollider.enabled = false;
			base.GetComponent<ParticleSystem>().Clear();
			base.gameObject.active = false;
		}
		else if (!impactOn && delay > delay_collision)
		{
			script_angel.AttackFinish();
			mycollider.enabled = true;
			impactOn = true;
		}
		delay += Time.deltaTime;
	}
}
