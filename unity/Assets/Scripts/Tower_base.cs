using UnityEngine;

public class Tower_base : MonoBehaviour
{
	private Hp_bar_fixed script_hpbar;

	private Cha_Control script_cha;

	private Cam_Move script_cam;

	private Transform hpbar;

	private Transform mytransform;

	private Vector3 attackdir;

	private float damage;

	private bool life = true;

	private short maxhp;

	private short hp;

	private short cur_stage_index;

	public bool iscastle;

	public Transform mydestroy;

	private bool istower;

	private void Awake()
	{
		mytransform = base.transform;
		if (mytransform.childCount != 0)
		{
			istower = true;
		}
	}

	private void Start()
	{
		cur_stage_index = (short)Crypto.Load_int_key("cur_stage_index");
		if (iscastle)
		{
			maxhp += (short)((0.1445f * (float)cur_stage_index * (float)cur_stage_index + 6.3873f * (float)cur_stage_index - 5f) * 20f);
		}
		else
		{
			maxhp += (short)((0.1445f * (float)cur_stage_index * (float)cur_stage_index + 6.3873f * (float)cur_stage_index - 5f) * 3f);
		}
		hp = maxhp;
		hpbar = GameObject.FindWithTag("efs_mon").GetComponent<Monster_efs>().CreatHpbar(new Vector2(0.1f, 0.02f), _turretmode: true, _ally: false);
		hpbar.parent = mytransform;
		script_hpbar = hpbar.GetComponent<Hp_bar_fixed>();
		script_cha = GameObject.FindWithTag("Player").transform.gameObject.GetComponent<Cha_Control>();
		script_cam = Camera.main.GetComponent<Cam_Move>();
		hpbar.position = mytransform.position + Vector3.forward * -0.08f + Vector3.up * 0.2f;
		script_hpbar.Damaged(maxhp, hp, mytransform, 0.2f, -1);
	}

	public void Grabed()
	{
	}

	public void CastleBreak()
	{
		life = false;
		base.GetComponent<Collider>().enabled = false;
		Object.Destroy(hpbar.gameObject);
		Object.Destroy(mytransform.GetChild(0).gameObject);
		if (iscastle)
		{
			GameObject.FindWithTag("ui").GetComponent<UI_Ingame>().WaveSet(100);
		}
	}

	public bool TankDamage()
	{
		hp = (short)((float)hp - (float)maxhp * 0.1f);
		script_hpbar.Damaged(maxhp, hp, mytransform, 0.2f, -1);
		base.GetComponent<AudioSource>().Play();
		if (hp <= 0 && life)
		{
			CastleBreak();
		}
		return life;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer < 20)
		{
			return;
		}
		switch (other.gameObject.layer)
		{
		case 20:
			attackdir = mytransform.position - other.transform.position;
			attackdir[1] = 0f;
			attackdir = Vector3.Normalize(attackdir);
			other.transform.root.GetComponent<Rigidbody>().AddForce(attackdir * -90f);
			damage = script_cha.atk;
			script_cam.Hitcam();
			break;
		case 21:
			damage = script_cha.atk;
			script_cam.Hitcam2(1f);
			break;
		case 22:
			damage = other.GetComponent<Rigidbody>().mass;
			script_cam.Hitcam();
			break;
		case 23:
			damage = other.GetComponent<Rigidbody>().mass * 2f;
			script_cam.Hitcam();
			break;
		case 24:
			damage = other.GetComponent<Rigidbody>().mass;
			script_cam.Hitcam();
			break;
		case 25:
			damage = script_cha.atk;
			break;
		case 26:
			damage = other.GetComponent<Rigidbody>().mass;
			hp -= (short)damage;
			break;
		case 27:
			damage = 2f;
			break;
		case 28:
			damage = script_cha.atk;
			break;
		case 29:
			damage = (float)script_cha.atk * 0.4f;
			break;
		case 30:
			damage = script_cha.atk;
			break;
		case 31:
			damage = script_cha.atk;
			break;
		case 18:
			damage = script_cha.atk;
			break;
		case 19:
			damage = script_cha.atk;
			break;
		}
		hp -= (short)damage;
		script_hpbar.Damaged(maxhp, hp, mytransform, 0.2f, -1);
		base.GetComponent<AudioSource>().Play();
		if (hp <= 0 && life)
		{
			life = false;
			base.GetComponent<Collider>().enabled = false;
			if (iscastle)
			{
				CastleBreak();
				return;
			}
			GameObject.FindWithTag("Respawn").GetComponent<Spawn>().TowerBreak(istower);
			Transform transform = (Transform)Object.Instantiate(mydestroy, mytransform.position, mytransform.rotation);
			transform.GetComponent<Animation>()["destroy_tower"].speed = 0.3f;
			Object.Destroy(transform.gameObject, 3f);
			Object.Destroy(hpbar.gameObject);
			Object.Destroy(base.gameObject);
		}
	}
}
