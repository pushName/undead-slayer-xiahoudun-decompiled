using UnityEngine;

public class Monster_efs : MonoBehaviour
{
	private const int MONNUM = 15;

	private const int SHORTMONNUM = 10;

	public Transform shadow;

	public Transform ef_hit;

	public Transform ef_blood;

	public Transform ef_block;

	public Transform arrowmark;

	public Transform num_damage;

	public Transform pt_itemdrop;

	public Texture[] attribute_tex = new Texture[4];

	public Material[] blood_mat = new Material[5];

	public Material split_material;

	public Material item_material;

	public Material main_material;

	public Material hp_bar;

	public AnimationClip[] begrab = new AnimationClip[6];

	public AnimationClip[] bethrust = new AnimationClip[6];

	public AnimationClip[] bekicked = new AnimationClip[6];

	public AnimationClip[] getup = new AnimationClip[6];

	public AnimationClip pierce;

	public AnimationClip[] summon = new AnimationClip[2];

	public float[] speed_begrab = new float[7];

	public float[] speed_bethrust = new float[7];

	public float[] speed_bekicked = new float[7];

	public float[] speed_getup = new float[7];

	public int[] force_grab = new int[7];

	public int[] force_kick = new int[7];

	public AudioClip[] snd_move = new AudioClip[3];

	public AudioClip[] snd_scream = new AudioClip[4];

	private Transform mytransform;

	private Transform[] c_shadow = new Transform[15];

	private int selectshadow;

	private Transform grab_blood;

	private Transform[] c_damagenum = new Transform[10];

	private Transform[] c_ef_hit = new Transform[10];

	private Transform[] c_ef_blood = new Transform[10];

	private Transform[] c_ef_split = new Transform[10];

	private Transform[] c_hpbar = new Transform[15];

	private Transform[] c_item = new Transform[15];

	private Mesh[] m_item = new Mesh[15];

	private Transform selecthpbar;

	private Vector2 hpsize = new Vector2(0.05f, 0.01f);

	private int count_item;

	private int count_damagenum;

	private int ef_attribute;

	private Vector3 originpos = Vector3.one * 3f;

	public bool story;

	private bool infinitymode;

	private int index_ef;

	private int index_destroy;

	private Spawn script_spawn;

	private Spawn_story script_spawn_story;

	private float limit_x = 2.65f;

	private float limit_y_b = -2.6f;

	private float limit_y_f = 2.5f;

	private void Awake()
	{
		mytransform = base.transform;
		for (int i = 0; i < 15; i++)
		{
			c_hpbar[i] = CreatHpbar(hpsize, _turretmode: false, _ally: false);
			c_hpbar[i].position = Vector3.one * 4f;
			c_shadow[i] = (Transform)Object.Instantiate(shadow, originpos, Quaternion.identity);
			c_shadow[i].gameObject.active = false;
		}
		for (int j = 0; j < 10; j++)
		{
			c_damagenum[j] = (Transform)Object.Instantiate(num_damage, originpos, num_damage.rotation);
			c_ef_hit[j] = (Transform)Object.Instantiate(ef_hit, originpos, Quaternion.identity);
			c_ef_blood[j] = (Transform)Object.Instantiate(ef_blood, originpos, Quaternion.identity);
			c_ef_split[j] = SplitOn((j + 1) % 4);
			c_ef_split[j].position = originpos;
			c_ef_hit[j].parent = mytransform;
			c_ef_blood[j].parent = mytransform;
			c_ef_split[j].parent = mytransform;
			c_item[j] = CreatItemBox();
			c_item[j].position = Vector3.one * 5f;
		}
	}

	private void Start()
	{
		if (!story)
		{
			script_spawn = GameObject.FindWithTag("Respawn").GetComponent<Spawn>();
			infinitymode = script_spawn.infinitymode;
		}
		else
		{
			script_spawn_story = GameObject.FindWithTag("Respawn").GetComponent<Spawn_story>();
		}
		grab_blood = (Transform)Object.Instantiate(ef_blood, originpos, Quaternion.identity);
		grab_blood.parent = mytransform;
		grab_blood.localScale = new Vector3(0.5f, 1.2f, 1.2f);
		count_item = 0;
		speed_begrab[0] = 0.25f;
		speed_bethrust[0] = 0.5f;
		speed_bekicked[0] = 0.2f;
		speed_getup[0] = 0.25f;
		speed_begrab[1] = 0.28f;
		speed_bethrust[1] = 0.28f;
		speed_bekicked[1] = 0.25f;
		speed_getup[1] = 0.25f;
		speed_begrab[2] = 0.24f;
		speed_bethrust[2] = 0.28f;
		speed_bekicked[2] = 0.25f;
		speed_getup[2] = 0.25f;
		speed_begrab[3] = 0.35f;
		speed_bethrust[3] = 0.25f;
		speed_bekicked[3] = 0.3f;
		speed_getup[3] = 0.25f;
		speed_begrab[4] = 0.4f;
		speed_bethrust[4] = 0.25f;
		speed_bekicked[4] = 0.25f;
		speed_getup[4] = 0.25f;
		speed_begrab[5] = 0.22f;
		speed_bethrust[5] = 0.25f;
		speed_bekicked[5] = 0.28f;
		speed_getup[5] = 0.25f;
		speed_begrab[6] = 0.35f;
		speed_bethrust[6] = 0.35f;
		speed_bekicked[6] = 0.25f;
		speed_getup[6] = 0.25f;
		force_grab[0] = 40;
		force_grab[1] = 40;
		force_grab[2] = 30;
		force_grab[3] = 80;
		force_grab[4] = 60;
		force_grab[5] = 80;
		force_grab[6] = 60;
		force_kick[0] = -200;
		force_kick[1] = -10;
		force_kick[2] = -10;
		force_kick[3] = 240;
		force_kick[4] = 20;
		force_kick[5] = -160;
		force_kick[6] = -100;
	}

	public void FinishEfs()
	{
		for (int i = 0; i < 10; i++)
		{
			c_ef_split[i].GetComponent<Ef_split1>().FinishNow();
			c_item[i].GetComponent<Itemdrop>().Disappear();
		}
	}

	public void RestrictArea(float _limit_x, float _limit_y_b, float _limit_y_f)
	{
		limit_x = _limit_x;
		limit_y_b = _limit_y_b;
		limit_y_f = _limit_y_f;
	}

	public AudioClip ScreamSFX()
	{
		return snd_scream[Random.Range(0, 4)];
	}

	public void BloodAttribute(int _index)
	{
		if (ef_attribute != _index)
		{
			for (int i = 0; i < 10; i++)
			{
				c_ef_blood[i].GetComponent<Renderer>().material = blood_mat[_index];
			}
		}
		ef_attribute = _index;
	}

	public void EnemyDead(int _destroy, Vector3 _pos, Texture _tex, Vector3 _scale, Vector3 _forcedir)
	{
		if (!story)
		{
			script_spawn.EnemyDead(_destroy, _pos, _tex, _scale, _forcedir);
		}
		else
		{
			script_spawn_story.EnemyDead(_destroy, _pos, _tex, _scale, _forcedir);
		}
		_pos[1] = 0f;
		c_ef_split[index_destroy].gameObject.active = true;
		c_ef_split[index_destroy].position = _pos + _forcedir * 0.2f;
		c_ef_split[index_destroy].rotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
		index_destroy = (index_destroy + 1) % 10;
	}

	public int CreatShadow(Transform _mon, float _size)
	{
		for (int i = 0; i < 15; i++)
		{
			if (c_shadow[i].position.y > 2f)
			{
				selectshadow = i;
				break;
			}
		}
		c_shadow[selectshadow].position = Vector3.one * -1f;
		c_shadow[selectshadow].GetComponent<Shadow>().Pickparent(_mon, _size);
		c_shadow[selectshadow].gameObject.active = true;
		return selectshadow;
	}

	public void DestroyShadow(int _index)
	{
		c_shadow[_index].GetComponent<Shadow>().Finish();
	}

	public void SetDamageNum(Vector3 _pos, short _damage, Vector3 _dir)
	{
		c_damagenum[count_damagenum].GetComponent<DamageNum>().TextOn(_pos, _damage, _dir);
		count_damagenum = (count_damagenum + 1) % 10;
	}

	public void CreatBlood(Vector3 _pos, Vector3 _dir)
	{
		c_ef_blood[index_ef].gameObject.active = true;
		c_ef_blood[index_ef].position = _pos + Vector3.up * 0.05f;
		if (_dir != Vector3.zero)
		{
			c_ef_blood[index_ef].rotation = Quaternion.LookRotation(-_dir);
		}
		c_ef_hit[index_ef].gameObject.active = true;
		c_ef_hit[index_ef].position = _pos + _dir * 0.1f + Vector3.up * 0.05f;
		c_ef_hit[index_ef].rotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
		index_ef = (index_ef + 1) % 10;
	}

	public void CreatBlood_Only(Vector3 _pos, Vector3 _dir)
	{
		c_ef_hit[index_ef].gameObject.active = true;
		c_ef_hit[index_ef].position = _pos + _dir * -0.1f + Vector3.up * 0.05f;
		c_ef_hit[index_ef].rotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
		index_ef = (index_ef + 1) % 10;
	}

	public void CreatGrabBlood(Vector3 _pos, Quaternion _dir)
	{
		grab_blood.gameObject.active = true;
		grab_blood.position = _pos;
		grab_blood.rotation = _dir;
	}

	public Transform SplitOn(int _rnduv)
	{
		Vector2 vector = Vector2.zero;
		switch (_rnduv)
		{
		case 0:
			vector = Vector2.up * 0.5f;
			break;
		case 1:
			vector = Vector2.one * 0.5f;
			break;
		case 2:
			vector = Vector2.zero * 0.5f;
			break;
		case 3:
			vector = Vector2.right * 0.5f;
			break;
		}
		GameObject gameObject = new GameObject("split");
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		gameObject.AddComponent<MeshRenderer>();
		float num = Random.Range(0.15f, 0.2f);
		mesh.vertices = new Vector3[4]
		{
			new Vector3(0f - num, 0.005f, num),
			new Vector3(num, 0.005f, num),
			new Vector3(0f - num, 0.005f, 0f - num),
			new Vector3(num, 0.005f, 0f - num)
		};
		mesh.name = "splitmesh";
		mesh.uv = new Vector2[4]
		{
			Vector2.up * 0.5f + vector,
			Vector2.one * 0.5f + vector,
			Vector2.zero + vector,
			Vector2.right * 0.5f + vector
		};
		mesh.triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		mesh.RecalculateNormals();
		Renderer renderer = gameObject.GetComponent<Renderer>();
		renderer.receiveShadows = false;
		renderer.castShadows = false;
		renderer.sharedMaterial = split_material;
		renderer.sharedMaterial.renderQueue = 2002;
		meshFilter.mesh = mesh;
		gameObject.AddComponent(typeof(Ef_split1));
		return gameObject.transform;
	}

	public void DirectionArrow()
	{
	}

	public Transform SetHpbar()
	{
		for (int i = 0; i < 15; i++)
		{
			if (c_hpbar[i].position.y > 2f)
			{
				selecthpbar = c_hpbar[i];
				break;
			}
		}
		return selecthpbar;
	}

	public Transform CreatHpbar(Vector2 _size, bool _turretmode, bool _ally)
	{
		GameObject gameObject = new GameObject("hp_bar");
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		gameObject.AddComponent<MeshRenderer>();
		mesh.vertices = new Vector3[4]
		{
			new Vector3(0f - _size.x, _size.y, 0f) * 0.5f,
			new Vector3(_size.x, _size.y, 0f) * 0.5f,
			new Vector3(0f - _size.x, 0f - _size.y, -0.02f) * 0.5f,
			new Vector3(_size.x, 0f - _size.y, -0.02f) * 0.5f
		};
		float num = 0f;
		if (_ally)
		{
			num = 0.75f;
		}
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0.25f + num),
			new Vector2(0.5f, 0.25f + num),
			new Vector2(0f, num),
			new Vector2(0.5f, num)
		};
		Renderer renderer = gameObject.GetComponent<Renderer>();
		renderer.receiveShadows = false;
		renderer.castShadows = false;
		renderer.sharedMaterial = hp_bar;
		mesh.triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		mesh.RecalculateNormals();
		meshFilter.mesh = mesh;
		gameObject.transform.parent = mytransform;
		if (!_turretmode)
		{
			gameObject.AddComponent(typeof(Hp_bar));
		}
		else
		{
			gameObject.AddComponent(typeof(Hp_bar_fixed));
		}
		return gameObject.transform;
	}

	public void SetItemBox(int _level, Vector3 _pos)
	{
		int num = Random.Range(0, 1000);
		int num2 = -1;
		if (infinitymode)
		{
			if (num < 20)
			{
				num2 = Random.Range(100, 120);
				c_item[count_item].GetComponent<Renderer>().material = main_material;
				pt_itemdrop.position = _pos;
				pt_itemdrop.GetComponent<ParticleSystem>().Emit(1);
			}
			else if (num < 70)
			{
				c_item[count_item].GetComponent<Renderer>().material = item_material;
				num2 = ((num >= 35) ? 1 : 2);
			}
			else
			{
				num2 = -1;
			}
		}
		else if (!story)
		{
			num2 = ((num < 2) ? 4 : ((num < 30) ? 3 : ((num < 70) ? 2 : ((num < 120) ? 1 : ((num >= 200) ? (-1) : 0)))));
		}
		if (num2 >= 0)
		{
			Vector2 zero = Vector2.zero;
			float num3 = 0f;
			if (num2 >= 100)
			{
				int num4 = num2 - 100;
				zero = new Vector2(0.125f * (float)(num4 % 8), 0.125f * (float)(num4 / 8));
				num3 = 0.125f;
			}
			else
			{
				int num5 = num2 % 4;
				int num6 = num2 / 4;
				zero = new Vector2((float)num5 * 0.25f, 0.75f - (float)num6 * 0.25f);
				num3 = 0.25f;
			}
			m_item[count_item].uv = new Vector2[4]
			{
				zero + Vector2.up * num3,
				zero + Vector2.one * num3,
				zero,
				zero + Vector2.right * num3
			};
			_pos[0] = Mathf.Clamp(_pos[0], 0f - limit_x, limit_x);
			_pos[2] = Mathf.Clamp(_pos[2], limit_y_b, limit_y_f);
			c_item[count_item].position = _pos + Vector3.up * 0.1f;
			c_item[count_item].GetComponent<Itemdrop>().Whatsindex(num2, _level);
			c_item[count_item].gameObject.active = true;
			count_item = (count_item + 1) % 10;
		}
	}

	public Transform CreatItemBox()
	{
		GameObject gameObject = new GameObject("itemBox");
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		gameObject.AddComponent<MeshRenderer>();
		mesh.vertices = new Vector3[4]
		{
			new Vector3(-0.035f, 0.05f, 0.05f),
			new Vector3(0.035f, 0.05f, 0.05f),
			Vector3.right * -0.035f,
			Vector3.right * 0.035f
		};
		mesh.uv = new Vector2[4]
		{
			Vector2.up,
			Vector2.one,
			Vector2.zero,
			Vector2.right
		};
		Renderer renderer = gameObject.GetComponent<Renderer>();
		renderer.receiveShadows = false;
		renderer.castShadows = false;
		renderer.sharedMaterial = item_material;
		SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
		sphereCollider.radius = 0.08f;
		sphereCollider.enabled = false;
		sphereCollider.isTrigger = true;
		mesh.triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		mesh.RecalculateNormals();
		meshFilter.mesh = mesh;
		m_item[count_item] = mesh;
		count_item++;
		gameObject.transform.parent = mytransform;
		gameObject.layer = 13;
		gameObject.AddComponent(typeof(Itemdrop));
		return gameObject.transform;
	}
}
