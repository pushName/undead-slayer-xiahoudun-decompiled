using UnityEngine;

public class Bullet_arrow_general : MonoBehaviour
{
	public float accel_factor = 3f;

	public float bullet_speed;

	public int growspeed = 2;

	private Transform mytransform;

	public float delay_finish = 3f;

	private float delay;

	private float accel_speed;

	private Vector3 originscale;

	private Vector3 startscale;

	private void Awake()
	{
		mytransform = base.transform;
		originscale = mytransform.localScale;
		startscale = originscale;
		startscale[2] = 0f;
	}

	private void OnEnable()
	{
		{
			var emission = mytransform.GetChild(0).GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		mytransform.localScale = startscale;
		accel_speed = -1f;
		delay = 0f;
	}

	private void Update()
	{
		mytransform.localScale = Vector3.MoveTowards(mytransform.localScale, originscale, Time.deltaTime * (float)growspeed);
		accel_speed = Mathf.MoveTowards(accel_speed, bullet_speed, Time.deltaTime * accel_factor);
		delay += Time.deltaTime;
		if (delay > delay_finish)
		{
			base.gameObject.active = false;
			{
				var emission = mytransform.GetChild(0).GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
			delay = 0f;
		}
		if (accel_speed > 0f)
		{
			mytransform.position += mytransform.forward * Time.deltaTime * accel_speed;
		}
	}
}
