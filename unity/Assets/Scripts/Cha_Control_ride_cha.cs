using UnityEngine;

public class Cha_Control_ride_cha : MonoBehaviour
{
	public Transform horseSpine;

	public Transform horse;

	public Transform dummy_weapon;

	public Transform ef_souleat;

	public Transform ef_damage;

	public Transform ef_twirl;

	public Transform screen_effect;

	public Transform spear;

	public Transform coin;

	private Transform mytransform;

	private Transform ef_swing;

	private Transform gauge_navi;

	private bool isfinish;

	private TextMesh stage_num;

	private Ef_swing1_ride script_swing;

	private Cha_Control_ride_horse script_horse;

	private Ef_twirl script_twirl;

	private Transform pack;

	private MakeUI script_pack;

	private Cam_Move_ride script_cam;

	private Ef_Coin script_coin;

	private Animation myanimation;

	private float attackdelay;

	private float particledelay;

	private float falldowndelay;

	public int amount_soulstone;

	public int count_coin;

	public int count_monster;

	public int count_behit;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		mytransform = base.transform;
		myanimation = base.GetComponent<Animation>();
	}

	private void Start()
	{
		script_cam = Camera.main.GetComponent<Cam_Move_ride>();
		ef_swing = mytransform.Find("ef_swing1");
		pack = GameObject.FindWithTag("p_plan").transform;
		script_coin = coin.GetComponent<Ef_Coin>();
		script_swing = ef_swing.GetComponent<Ef_swing1_ride>();
		script_horse = horse.GetComponent<Cha_Control_ride_horse>();
		script_twirl = ef_twirl.GetComponent<Ef_twirl>();
		script_pack = pack.GetComponent<MakeUI>();
		myanimation["ride1"].speed = 0.7f;
		myanimation.Play("ride1");
		myanimation["attack_ride1"].speed = 0.3f;
		myanimation["attack_ride2"].speed = 0.3f;
		myanimation["attack_ride1"].layer = 1;
		myanimation["attack_ride2"].layer = 1;
		gauge_navi = script_pack.CreatCustomPlane(Vector2.one * 0.12f, 0f, new Vector3(-1.36f, 2.9f, 1.5f), new Vector2(0.8125f, 0.9375f), new Vector2(0.875f, 1f), "navi_gauge", null, 0f, 0);
		script_pack.CreatCustomPlane(new Vector2(1.6f, 0.1f), 0f, new Vector3(-0.64f, 2.9f, 1.8f), new Vector2(0f, 0.375f), new Vector2(1f, 0.4375f), "navi", null, 0f, 0);
		script_pack.CreatCustomPlane(Vector2.one * 0.15f, 0f, new Vector3(0.53f, 2.9f, 2.1f), new Vector2(0.75f, 0.9375f), new Vector2(0.8125f, 1f), "icon_soul", null, 0f, 0);
		GameObject gameObject = Resources.Load("stagenum_cave") as GameObject;
		Transform transform = (Transform)Object.Instantiate(gameObject.transform, gameObject.transform.localPosition, Quaternion.identity);
		transform.position += Vector3.right * 0.34f;
		stage_num = transform.GetComponent<TextMesh>();
		stage_num.text = "0";
	}

	public void Damaged()
	{
		if (!isfinish)
		{
			GetSoulStone(-3);
		}
	}

	public void Attack(bool _isright)
	{
		if (attackdelay <= 0f && mytransform.position.y < 0.01f)
		{
			if (_isright)
			{
				myanimation.Play("attack_ride1");
				ef_swing.localRotation = Quaternion.Euler(20f, 60f, 170f);
				script_swing.SwingOn(0.16f, 2, 2, 18, 1, 0);
			}
			else
			{
				myanimation.Play("attack_ride2");
				ef_swing.localRotation = Quaternion.Euler(20f, -60f, 10f);
				script_swing.SwingOn(0.16f, 2, 2, 18, 1, 0);
			}
			attackdelay = 1f;
		}
	}

	public void GetSoulStone(int a)
	{
		amount_soulstone = Mathf.Max(0, amount_soulstone + a);
		if (a == 1)
		{
			{
				var emission = ef_souleat.GetComponent<ParticleSystem>().emission;
				emission.enabled = true;
			}
			script_twirl.TwirlOn(4, 4, 20, loop: false);
			base.GetComponent<AudioSource>().Play();
			script_coin.GetCoin(mytransform.position);
			count_coin++;
		}
		else if (a < 0)
		{
			screen_effect.gameObject.active = true;
			script_cam.Hitcam();
			{
				var emission = ef_damage.GetComponent<ParticleSystem>().emission;
				emission.enabled = true;
			}
			count_behit++;
		}
		else
		{
			count_monster++;
		}
		stage_num.text = amount_soulstone.ToString();
		particledelay = 0.5f;
	}

	public void FallDown()
	{
		if (!isfinish && falldowndelay <= 0f)
		{
			script_horse.FallDown();
			GetSoulStone(-3);
			falldowndelay = 1f;
		}
	}

	public void Finish()
	{
		isfinish = true;
	}

	public void RisePocket()
	{
		GameObject.FindWithTag("dummy").GetComponent<Obj_Pocket>().PickPocket();
	}

	public void CrynStop()
	{
		script_twirl.TwirlOff();
		Object.Destroy(spear.gameObject);
		myanimation.Stop();
		myanimation["ride2"].speed = 0.16f;
		myanimation["ride2"].wrapMode = WrapMode.Once;
		myanimation.Play("ride2");
		AnimationState animationState = myanimation.PlayQueued("getoff_horse");
		animationState.speed = 0.15f;
		animationState = myanimation.PlayQueued("risecoin");
		animationState.speed = 0.2f;
		animationState = myanimation.PlayQueued("idle_ridestage");
		animationState.speed = 0.1f;
	}

	private void Update()
	{
		if (!isfinish)
		{
			gauge_navi.position += Vector3.right * Time.deltaTime * 0.033f;
		}
		mytransform.position = horseSpine.position + Vector3.up * -0.085f;
		mytransform.rotation = horse.rotation;
		if (falldowndelay > 0f)
		{
			falldowndelay -= Time.deltaTime;
		}
		else if (attackdelay > 0f)
		{
			attackdelay -= Time.deltaTime;
		}
		if (particledelay > 0f)
		{
			particledelay -= Time.deltaTime;
			if (particledelay < 0f)
			{
				{
					var emission = ef_souleat.GetComponent<ParticleSystem>().emission;
					emission.enabled = false;
				}
				{
					var emission = ef_damage.GetComponent<ParticleSystem>().emission;
					emission.enabled = false;
				}
				particledelay = 0f;
			}
		}
	}
}
