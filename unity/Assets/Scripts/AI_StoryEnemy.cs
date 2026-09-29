using UnityEngine;

public class AI_StoryEnemy : MonoBehaviour
{
	public int enemykind;

	private Texture originTex;

	public int monmovestat;

	public AudioClip move;

	public AudioClip scream;

	public AudioClip death;

	public AudioClip stab;

	public AudioClip kick;

	private Transform mon_destroy;

	private Transform mytransform;

	private Transform hpbar;

	private Vector3 chapos;

	private Vector3 targetpos;

	private bool life = true;

	private bool impact;

	private bool attackstart;

	private bool dash;

	private int currentattack;

	private float movespeed;

	private float attackrange;

	private bool downhigh;

	private bool downhigh2;

	private int hp;

	private int maxhp;

	private int block;

	private float runspeed;

	private float firerange;

	public Transform target;

	private Transform cha1;

	private Quaternion lookrotation;

	private Vector3 directionVector;

	private float damage;

	private int accuracy;

	private Cha_Control script_cha;

	private SoundEf_slash script_sound;

	private Cam_Move script_cam;

	private Hp_bar script_hpbar;

	private NPC1 script_npc;

	private Monster_efs script_monEf;

	private monster enemy;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void Start()
	{
		GameObject gameObject = GameObject.FindWithTag("Respawn");
		enemy = gameObject.GetComponent<DB_Monster>().enemy[enemykind];
		maxhp = 5;
		block = enemy._block;
		runspeed = enemy._runspeed;
		firerange = 0.22f;
		base.GetComponent<Animation>()["move"].speed = enemy._speed_move;
		base.GetComponent<Animation>()["down"].speed = 0.24f;
		base.GetComponent<Animation>()["down_high"].speed = 0.28f;
		base.GetComponent<Animation>()["stop"].speed = 0.1f;
		base.GetComponent<Animation>()["story_attack1"].speed = 0.3f;
		base.GetComponent<Animation>()["story_attack1_i"].speed = 0.3f;
		base.GetComponent<Animation>()["story_attack2"].speed = 0.3f;
		base.GetComponent<Animation>()["story_attack2_i"].speed = 0.3f;
		base.GetComponent<Animation>()["story_attack3"].speed = 0.3f;
		base.GetComponent<Animation>()["story_attack3_i"].speed = 0.3f;
		base.GetComponent<Animation>()["idle"].speed = enemy._speed_idle;
		base.GetComponent<Animation>()["down"].layer = 2;
		base.GetComponent<Animation>()["down_high"].layer = 2;
		base.GetComponent<Animation>()["move"].layer = 0;
		base.GetComponent<Animation>()["stop"].layer = 0;
		base.GetComponent<Animation>()["story_attack1"].layer = 1;
		base.GetComponent<Animation>()["story_attack1_i"].layer = 1;
		base.GetComponent<Animation>()["story_attack2"].layer = 1;
		base.GetComponent<Animation>()["story_attack2_i"].layer = 1;
		base.GetComponent<Animation>()["story_attack3"].layer = 1;
		base.GetComponent<Animation>()["story_attack3_i"].layer = 1;
		base.GetComponent<Animation>()["idle"].layer = 0;
		monmovestat = 0;
		originTex = mytransform.Find("mon").GetComponent<Renderer>().material.mainTexture;
		cha1 = GameObject.FindWithTag("Player").transform;
		script_cha = cha1.gameObject.GetComponent<Cha_Control>();
		script_sound = GameObject.FindWithTag("sound").GetComponent<SoundEf_slash>();
		script_cam = Camera.main.GetComponent<Cam_Move>();
		script_monEf = GameObject.FindWithTag("efs_mon").GetComponent<Monster_efs>();
		hpbar = script_monEf.CreatHpbar(new Vector2(0.05f, 0.01f), _turretmode: false, _ally: false);
		script_npc = target.GetComponent<NPC1>();
		script_hpbar = hpbar.GetComponent<Hp_bar>();
		hp = maxhp;
		accuracy = script_cha.hitrate;
		script_hpbar.Damaged(maxhp, hp, mytransform, 0.15f, 0);
		script_monEf.CreatShadow(mytransform.Find("Bone_pelvis"), 1f);
		hpbar.GetComponent<Renderer>().enabled = false;
		base.GetComponent<Animation>().Play("move");
	}

	public void Grabed()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer < 20)
		{
			return;
		}
		currentattack = 0;
		hpbar.GetComponent<Renderer>().enabled = true;
		accuracy = script_cha.hitrate;
		attackstart = false;
		downhigh2 = script_cha.attack_rising;
		Vector3 vector = Vector3.Normalize(mytransform.position - chapos);
		float num = 0f;
		int num2 = 0;
		switch (other.gameObject.layer)
		{
		case 20:
			num = 50f;
			num2 = Random.Range(0, 100);
			damage = script_cha.atk;
			script_cam.Hitcam();
			downhigh = false;
			break;
		case 21:
			num = 30f;
			num2 = 100;
			damage = script_cha.atk;
			script_cam.Hitcam2(1f);
			downhigh = true;
			break;
		case 28:
			num = 50f;
			damage = script_cha.atk;
			script_cam.Hitcam();
			movespeed = 0f;
			break;
		}
		if (num2 < block - accuracy && monmovestat >= 0)
		{
			script_cha.Blocked(mytransform.position);
			base.GetComponent<Rigidbody>().AddForce(vector * 10f);
		}
		else
		{
			script_sound.SoundOn(1);
			base.GetComponent<Animation>().Stop();
			if (downhigh || downhigh2)
			{
				mytransform.Rotate(0f, Random.Range(0, 360), 0f);
				base.GetComponent<Animation>().Play("down_high");
			}
			else
			{
				base.GetComponent<Animation>().Play("down");
			}
			base.GetComponent<Rigidbody>().AddForce(vector * num);
			script_monEf.CreatBlood(mytransform.position, directionVector);
			hp -= (int)damage;
			script_hpbar.Damaged(maxhp, hp, mytransform, 0.15f, 0);
			base.GetComponent<AudioSource>().PlayOneShot(scream);
		}
		if (hp <= 0 && life)
		{
			monmovestat = -4;
			life = false;
			script_sound.SoundOn(4);
			Object.Destroy(base.gameObject);
			Object.Destroy(hpbar.gameObject);
			Vector3 position = mytransform.position;
			position[1] = 0f;
			script_monEf.EnemyDead(2, position, originTex, mytransform.localScale, vector);
			GameObject.FindWithTag("Respawn").GetComponent<Spawn_story>().StoryEnemyDead();
		}
	}

	public void Attack(int _kind)
	{
		switch (_kind)
		{
		case 0:
		{
			base.GetComponent<Animation>().Play("story_attack1");
			AnimationState animationState3 = base.GetComponent<Animation>().PlayQueued("story_attack1_i", QueueMode.CompleteOthers);
			animationState3.speed = base.GetComponent<Animation>()["story_attack1_i"].speed;
			break;
		}
		case 1:
		{
			base.GetComponent<Animation>().Play("story_attack2");
			AnimationState animationState2 = base.GetComponent<Animation>().PlayQueued("story_attack2_i", QueueMode.CompleteOthers);
			animationState2.speed = base.GetComponent<Animation>()["story_attack2_i"].speed;
			break;
		}
		case 2:
		{
			base.GetComponent<Animation>().Play("story_attack3");
			AnimationState animationState = base.GetComponent<Animation>().PlayQueued("story_attack3_i", QueueMode.CompleteOthers);
			animationState.speed = base.GetComponent<Animation>()["story_attack3_i"].speed;
			break;
		}
		}
	}

	private void Update()
	{
		if (!life)
		{
			return;
		}
		targetpos = target.position;
		chapos = cha1.position;
		attackrange = Vector3.Distance(mytransform.position, targetpos);
		if (base.GetComponent<Animation>().IsPlaying("down_high"))
		{
			directionVector = Vector3.Normalize(targetpos - mytransform.position);
			monmovestat = -1;
		}
		else if (base.GetComponent<Animation>().IsPlaying("down"))
		{
			directionVector = Vector3.Normalize(targetpos - mytransform.position);
			monmovestat = -1;
		}
		else if (base.GetComponent<Animation>().IsPlaying("story_attack1") || base.GetComponent<Animation>().IsPlaying("story_attack2") || base.GetComponent<Animation>().IsPlaying("story_attack3"))
		{
			directionVector = Vector3.Normalize(targetpos - mytransform.position);
			impact = false;
			monmovestat = 11;
			if (directionVector != Vector3.zero)
			{
				lookrotation = Quaternion.LookRotation(directionVector);
				mytransform.rotation = Quaternion.Slerp(mytransform.rotation, lookrotation, Time.deltaTime * 6f);
			}
			if (!dash)
			{
				mytransform.position += directionVector * Time.deltaTime * 0.7f;
			}
			else
			{
				mytransform.position = Vector3.Lerp(mytransform.position, target.position, Time.deltaTime * 5f);
			}
		}
		else if (base.GetComponent<Animation>().IsPlaying("story_attack1_i") || base.GetComponent<Animation>().IsPlaying("story_attack2_i") || base.GetComponent<Animation>().IsPlaying("story_attack3_i"))
		{
			dash = true;
			monmovestat = 12;
			if (!impact)
			{
				script_npc.Behit();
				impact = true;
				base.GetComponent<AudioSource>().PlayOneShot(stab);
			}
			directionVector = Vector3.Normalize(targetpos - mytransform.position);
			if (directionVector != Vector3.zero)
			{
				lookrotation = Quaternion.LookRotation(directionVector);
				mytransform.rotation = Quaternion.Slerp(mytransform.rotation, lookrotation, Time.deltaTime * 6f);
			}
		}
		else if (base.GetComponent<Animation>().IsPlaying("move") || base.GetComponent<Animation>().IsPlaying("stop") || !base.GetComponent<Animation>().isPlaying)
		{
			directionVector = Vector3.Normalize(targetpos - mytransform.position);
			attackstart = false;
			monmovestat = 1;
			mytransform.position += directionVector * Time.deltaTime * movespeed;
			if (directionVector != Vector3.zero)
			{
				lookrotation = Quaternion.LookRotation(directionVector);
			}
			mytransform.rotation = Quaternion.Slerp(mytransform.rotation, lookrotation, Time.deltaTime * 3f);
		}
		else
		{
			monmovestat = 1;
		}
		if (attackrange > 1.1f)
		{
			movespeed = runspeed;
		}
		else if (attackrange < firerange)
		{
			if (!attackstart)
			{
				if (monmovestat >= 0)
				{
					Attack(currentattack);
					currentattack = (currentattack + 1) % 3;
					attackstart = true;
				}
			}
			else
			{
				base.GetComponent<Animation>().CrossFade("stop");
			}
		}
		else
		{
			movespeed = runspeed;
			base.GetComponent<Animation>().CrossFade("move");
		}
	}
}
