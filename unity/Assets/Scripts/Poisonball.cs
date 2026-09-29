using UnityEngine;

public class Poisonball : MonoBehaviour
{
	private Transform mytransform;

	private float shoot_delay;

	private Vector3 shootdir;

	private bool explode;

	private ParticleSystem myparticle;

	private Transform boom;

	private void Awake()
	{
		mytransform = base.transform;
		myparticle = base.GetComponent<ParticleSystem>();
	}

	private void Start()
	{
		shootdir = mytransform.parent.forward - Vector3.up * 0.5f;
		mytransform.parent = null;
		boom = GameObject.FindWithTag("skill").transform;
	}

	private void Update()
	{
		if (explode)
		{
			return;
		}
		if (shoot_delay > 1f)
		{
			mytransform.position += shootdir * Time.deltaTime * 2.5f;
			if (mytransform.position.y <= 0f)
			{
				base.GetComponent<Collider>().enabled = true;
				Object.Destroy(base.GetComponent<Collider>(), 0.4f);
				Object.Destroy(base.gameObject, 1.2f);
				explode = true;
				// 旧版 ParticleEmitter.rndVelocity（每轴随机初速）在 2022 无同名 API，
				// 这里用 VelocityOverLifetime 的"两常量间随机" ±1 近似；粒子资源重建后需按视觉复核。
				ParticleSystem.VelocityOverLifetimeModule rndvel = myparticle.velocityOverLifetime;
				rndvel.enabled = true;
				rndvel.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
				rndvel.y = new ParticleSystem.MinMaxCurve(-1f, 1f);
				rndvel.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
				boom.GetComponent<Ef_boom>().SetTex(2, mytransform.position, _collider: true);
			}
		}
		else
		{
			shoot_delay += Time.deltaTime;
		}
	}
}
