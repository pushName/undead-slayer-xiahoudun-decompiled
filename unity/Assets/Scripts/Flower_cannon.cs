using UnityEngine;

public class Flower_cannon : MonoBehaviour
{
	private Animation myanimation;

	public Transform cannonball;

	public Transform trap;

	private Vector3 ballPos;

	private Vector3 targetPos;

	private Vector3 ballPosStartPos;

	private Transform target;

	private float dy;

	private float posY;

	private bool cannonshot;

	private Trap_Extreme script_trap;

	private void Start()
	{
		myanimation = base.GetComponent<Animation>();
		base.GetComponent<Animation>()["flower"].speed = 0.1f;
		base.GetComponent<Animation>()["flower_idle"].speed = 0.001f;
		target = GameObject.FindWithTag("Player").transform;
		script_trap = trap.GetComponent<Trap_Extreme>();
		ballPosStartPos = cannonball.position;
	}

	public void StartShoot()
	{
		myanimation.Play("flower");
	}

	public void FisnishShoot()
	{
		myanimation.Stop();
		myanimation.Play("flower_idle");
	}

	private void Update()
	{
		if (cannonshot)
		{
			ballPos = Vector3.MoveTowards(ballPos, targetPos, Time.deltaTime);
			posY += Time.deltaTime * dy;
			dy -= Time.deltaTime * 2f;
			ballPos.y = posY;
			if (ballPos.y < 0f)
			{
				cannonshot = false;
				script_trap.DirectFire(ballPos);
				trap.GetChild(0).GetComponent<ParticleSystem>().Emit(1);
				trap.GetChild(1).GetComponent<ParticleSystem>().Emit(1);
				cannonball.position = Vector3.up * 34f;
			}
			else
			{
				cannonball.position = ballPos;
			}
		}
	}

	public void CannonShot()
	{
		ballPos = ballPosStartPos;
		targetPos = target.position;
		dy = 1.5f;
		posY = ballPosStartPos.y;
		cannonshot = true;
	}
}
