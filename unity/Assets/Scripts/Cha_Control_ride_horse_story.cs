using UnityEngine;

public class Cha_Control_ride_horse_story : MonoBehaviour
{
	private short changeScene = 1;

	public AudioClip cry;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.transform.gameObject);
	}

	private void Start()
	{
		base.transform.position = Vector3.forward;
		base.GetComponent<Animation>()["horse_run"].speed = 0.7f;
		base.GetComponent<Animation>()["horse_cry"].speed = 0.25f;
		base.GetComponent<Animation>().Play("horse_run");
	}

	public void Scene_third()
	{
		changeScene = 3;
	}

	private void Update()
	{
		if (changeScene <= 2)
		{
			base.transform.position += Vector3.forward * Time.deltaTime;
		}
		else if (changeScene == 3)
		{
			base.transform.position = Vector3.zero;
			base.GetComponent<Animation>().Play("horse_stand");
			if (Application.loadedLevelName == "Story_3")
			{
				base.GetComponent<AudioSource>().Stop();
				base.GetComponent<Animation>().Stop();
				base.GetComponent<Animation>().Play("horse_cry");
				base.GetComponent<AudioSource>().PlayOneShot(cry);
				AnimationState animationState = base.GetComponent<Animation>().PlayQueued("horse_stand");
				animationState.speed = 0.2f;
				changeScene = 4;
			}
		}
		if (base.transform.position.z > 5.5f && changeScene == 2)
		{
			changeScene = 3;
			Object.Destroy(base.transform.Find("ride_stepfog").gameObject);
		}
		else if (base.transform.position.z > 4.55f && changeScene == 1)
		{
			GameObject.Find("scene_trans").GetComponent<Story_trans>().ScreenOn(2f);
			GameObject.Find("Cam_loading").GetComponent<Cam_Loading_Story>().DisappearCam();
			// 旧版 minEnergy/maxEnergy 是粒子生命期区间，Shuriken 里对应 main.startLifetime。
			// main 返回的是结构体，必须先绑定到变量才能赋值。原值 min=max=0.4，用"两常量间随机"原样保留。
			ParticleSystem.MainModule stepfogMain = base.transform.Find("ride_stepfog").GetComponent<ParticleSystem>().main;
			stepfogMain.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.4f);
			changeScene = 2;
		}
	}
}
