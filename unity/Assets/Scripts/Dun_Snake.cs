using UnityEngine;

public class Dun_Snake : MonoBehaviour
{
	private Transform mytransform;

	private Animation myanimation;

	private Vector3 startpos;

	private Vector3 targetpos;

	private float delay = 3f;

	private float speed;

	public Transform holes;

	public Transform trap1;

	public Transform trap2;

	private Trap_Extreme script_trap1;

	private Trap_Extreme script_trap2;

	private void Awake()
	{
		mytransform = base.transform;
		myanimation = base.GetComponent<Animation>();
		base.GetComponent<Animation>()["snake_move"].speed = 0.3f;
		script_trap1 = trap1.GetComponent<Trap_Extreme>();
		script_trap2 = trap2.GetComponent<Trap_Extreme>();
	}

	private void OnEnable()
	{
	}

	private void Start()
	{
		mytransform.position = Vector3.up * 25f;
		delay = 2f;
	}

	public void FisnishShoot()
	{
		myanimation.Stop();
	}

	public void SetDamage(int _damage)
	{
		script_trap1.SetDamage(_damage);
		script_trap2.SetDamage(_damage);
	}

	public void StartShoot()
	{
		delay = Random.Range(2f, 3.6f);
		int num = Random.Range(0, 7);
		startpos = holes.GetChild(num).position;
		int num2 = 0;
		if (num == 0)
		{
			num2 = Random.Range(1, 7);
		}
		else
		{
			int num3 = Random.Range(0, 5);
			num2 = ((num3 != 0) ? ((num3 % 2 != 0) ? ((num - 1) % 7) : ((num + 1) % 7)) : 0);
		}
		targetpos = holes.GetChild(num2).position;
		Vector3 forward = startpos - targetpos;
		mytransform.position = startpos;
		mytransform.forward = forward;
		myanimation.Play("snake_move");
		script_trap1.DirectFire(startpos);
		{
			var emission = trap1.GetChild(0).GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		Invoke("PtOn2", 0.6f);
	}

	private void PtOn2()
	{
		script_trap2.DirectFire(targetpos);
		{
			var emission = trap2.GetChild(0).GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
	}

	private void Update()
	{
	}
}
