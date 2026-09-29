using UnityEngine;

public class Bullet_Angel1 : MonoBehaviour
{
	public float bullet_speed = 2f;

	private Transform mytransform;

	private ParticleSystem mychild;

	private AI_Asist script_angel;

	private void Awake()
	{
		mytransform = base.transform;
		script_angel = GameObject.Find("asist").GetComponent<AI_Asist>();
		base.gameObject.active = false;
		if (mytransform.childCount > 0)
		{
			mychild = mytransform.GetChild(0).GetComponent<ParticleSystem>();
		}
	}

	private void OnEnable()
	{
		if (mychild != null)
		{
			mychild.Play();
		}
	}

	private void Update()
	{
		mytransform.position += mytransform.forward * Time.deltaTime * bullet_speed;
		if (mytransform.position.y < 0f)
		{
			script_angel.AttackFinish();
			base.gameObject.active = false;
		}
	}
}
