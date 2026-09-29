using UnityEngine;

public class Splash_angel_bomb : MonoBehaviour
{
	private Transform mytransform;

	private Vector3 targetScale;

	private float delay = 2.5f;

	private ParticleSystem mychild;

	private void Awake()
	{
		mytransform = base.transform;
		base.gameObject.active = false;
		targetScale = mytransform.localScale;
		mychild = mytransform.GetChild(0).GetComponent<ParticleSystem>();
	}

	private void OnEnable()
	{
		delay = 1f;
		mytransform.localScale = Vector3.zero;
		// 旧版 Emit() 只发一个粒子，按字面等价迁移；成簇与否等粒子资源重建后按视觉定。
		mychild.Emit(1);
	}

	private void Update()
	{
		if (delay < 0f)
		{
			mytransform.localScale -= Vector3.one * Time.deltaTime * 3f;
			if (mytransform.localScale.y < -0.2f)
			{
				mytransform.localScale = Vector3.zero;
				base.gameObject.active = false;
			}
		}
		else
		{
			mytransform.localScale = Vector3.Lerp(mytransform.localScale, targetScale, Time.deltaTime * 5f);
			delay -= Time.deltaTime;
		}
	}
}
