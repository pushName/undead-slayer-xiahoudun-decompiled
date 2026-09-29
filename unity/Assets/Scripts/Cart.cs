using UnityEngine;

public class Cart : MonoBehaviour
{
	private UI_Ingame script_ui;

	private MakeUI script_pack;

	private Gauge_UV gauge_hp;

	public Transform horse;

	public AudioClip snd_break;

	public Transform pt_hit;

	public Transform cart_destroy;

	private Transform mytransform;

	private Transform navi_gauge;

	private Animation myanimation;

	private float delay;

	private int damage;

	private float hp_length;

	private int maxhp;

	private int hp;

	private bool life = true;

	private bool super;

	private short cur_stage_index;

	private void Awake()
	{
		cur_stage_index = (short)Crypto.Load_int_key("cur_stage_index");
		maxhp = 2000 + 150 * cur_stage_index;
		hp = maxhp;
		mytransform = base.transform;
		myanimation = base.GetComponent<Animation>();
		mytransform.position = Vector3.forward * 0.5f;
		script_ui = GameObject.FindWithTag("ui").GetComponent<UI_Ingame>();
		script_pack = GameObject.FindWithTag("p_plan").GetComponent<MakeUI>();
	}

	private void Start()
	{
		Transform transform = script_pack.CreatCustomPlane(new Vector2(0.2f, 0.06f), 0f, new Vector3(-1.4f, 2.36f, 1.8f), new Vector2(0.75f, 0.625f), new Vector2(0.875f, 0.6875f), "general_hp", "Gauge_UV", 0.1f, 0);
		gauge_hp = transform.GetComponent<Gauge_UV>();
		hp = maxhp;
		HPgaugeSet();
		navi_gauge = script_pack.CreatCustomPlane(Vector2.one * 0.12f, 0f, new Vector3(-0.72f, 1.06f, 1.5f), new Vector2(0.8125f, 0.9375f), new Vector2(0.875f, 1f), "navi_gauge", null, 0f, 0);
		script_pack.CreatCustomPlane(new Vector2(1.6f, 0.1f), 0f, new Vector3(0f, 1.06f, 1.8f), new Vector2(0f, 0.375f), new Vector2(1f, 0.4375f), "navi", null, 0f, 0);
		myanimation["cart_run"].speed = 0.3f;
		myanimation["cart_damage"].speed = 0.3f;
		horse.GetComponent<Animation>()["horse_walk"].speed = 0.4f;
		base.GetComponent<AudioSource>().Play();
		InvokeRepeating("ShowDistance", 0.1f, 1f);
	}

	public void ShowDistance()
	{
		if (mytransform.position.z > 16.8f)
		{
			script_ui.WaveSet(100);
			super = true;
			CancelInvoke("ShowDistance");
		}
	}

	public void Damaged(Vector3 _damageVector)
	{
		if (life && !super)
		{
			damage = (int)_damageVector.y;
			_damageVector[1] = 0f;
			hp = Mathf.Max(0, hp - damage);
			HPgaugeSet();
			myanimation.Stop();
			myanimation.Play("cart_damage");
			horse.GetComponent<Animation>().Stop();
			delay = 2f;
			base.GetComponent<AudioSource>().Stop();
			base.GetComponent<AudioSource>().PlayOneShot(snd_break);
			{
				var emission = pt_hit.GetComponent<ParticleSystem>().emission;
				emission.enabled = true;
			}
			if (hp <= 0)
			{
				Transform transform = (Transform)Object.Instantiate(cart_destroy, mytransform.position, mytransform.rotation);
				transform.GetComponent<Animation>()["cart_destroy"].speed = 0.2f;
				Invoke("FailStage", 1.5f);
				horse.parent = null;
				horse.GetComponent<Animation>()["horse_run"].speed = 0.6f;
				horse.GetComponent<Animation>().Play("horse_run");
				mytransform.position = Vector3.up * 16f;
				life = false;
				Camera.main.GetComponent<Cam_Move>().LookTarget(transform, 30, 0f);
			}
		}
	}

	private void FailStage()
	{
		script_ui.WaveSet(-1);
	}

	public void HPgaugeSet()
	{
		hp_length = (1f - (float)hp / (float)maxhp) * 0.125f;
		gauge_hp.UvMove(Vector2.right * hp_length);
	}

	private void Update()
	{
		if (!life)
		{
			horse.position += Vector3.forward * Time.deltaTime * 1f;
		}
		else if (delay > 0f)
		{
			delay -= Time.deltaTime;
			if (delay <= 0f)
			{
				myanimation.CrossFade("cart_run");
				horse.GetComponent<Animation>().CrossFade("horse_walk");
				base.GetComponent<AudioSource>().Play();
			}
			else if (delay < 1.5f)
			{
				{
					var emission = pt_hit.GetComponent<ParticleSystem>().emission;
					emission.enabled = false;
				}
			}
		}
		else if (life)
		{
			mytransform.position += Vector3.forward * Time.deltaTime * 0.1f;
			navi_gauge.position += Vector3.right * Time.deltaTime * 0.009f;
		}
	}
}
