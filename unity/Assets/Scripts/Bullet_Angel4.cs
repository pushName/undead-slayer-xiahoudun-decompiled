using UnityEngine;

public class Bullet_Angel4 : MonoBehaviour
{
	private const int MAXBULLET = 6;

	private Transform[] c_spear = new Transform[6];

	private Transform mytransform;

	private ParticleSystem ef_splash;

	private AI_Asist script_angel;

	private int dx = 1;

	private int curFire;

	private void Awake()
	{
		mytransform = base.transform;
		script_angel = GameObject.Find("asist").GetComponent<AI_Asist>();
		for (int i = 0; i < 6; i++)
		{
			c_spear[i] = mytransform.GetChild(i);
			c_spear[i].GetComponent<Bullet_Angel4_m>().Init(dx);
			dx *= -1;
		}
		ef_splash = mytransform.GetChild(6).GetComponent<ParticleSystem>();
		base.gameObject.active = false;
	}

	private void OnEnable()
	{
		curFire = 0;
		InvokeRepeating("FireOn", 0.1f, 0.2f);
	}

	public void SplashOn(Vector3 _pos)
	{
		_pos.y = 0f;
		ef_splash.transform.position = _pos;
		// 旧版 ParticleEmitter.Emit() 只发一个粒子，按字面等价迁移；
		// 若 splash 实际要成簇，需在重建粒子资源后按视觉调整这个数量。
		ef_splash.Emit(1);
	}

	private void FireOn()
	{
		c_spear[curFire].position = mytransform.position;
		c_spear[curFire].gameObject.active = true;
		curFire++;
		if (curFire >= 6)
		{
			CancelInvoke();
			script_angel.AttackFinish();
			base.gameObject.active = false;
		}
	}

	private void Update()
	{
	}
}
