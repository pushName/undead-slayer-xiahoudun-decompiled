using UnityEngine;

public class Icon_Skill : MonoBehaviour
{
	private const int MAXSKILL = 20;

	private const int MAXSLOT = 5;

	private const int MAXPET = 2;

	public Transform ef_activeskill;

	public Transform pack;

	public Transform icon_pet_folder;

	private Transform mytransform;

	private Transform icon_pause;

	private Transform icon_rotate;

	private MakeUI script_pack;

	private int soulamount;

	private float[] _duration = new float[2];

	private int[] cur_skill_grade = new int[5];

	private int[] pet_activeskill = new int[2];

	private int[] pet_passiveskill = new int[2];

	private int[] pet_hunger = new int[2];

	private int[] item_slot = new int[5];

	private int[] skill_slot = new int[20];

	private int[] skillcount = new int[20];

	private float[] skill_colltime = new float[20];

	private short[] skill_soulcost = new short[21];

	public int[] isskillready = new int[21]
	{
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1
	};

	public bool[] ispetready = new bool[2];

	private int[] skill_use = new int[20];

	private int[] pet_skill_use = new int[2];

	private Transform[] clone_skillicon = new Transform[20];

	private Transform[] clone_peticon = new Transform[2];

	private Transform[] clone_itemicon = new Transform[3];

	private Transform[] clone_fillbox = new Transform[21];

	private Transform icon_shortsp;

	private Vector3 skill_iconpos = new Vector3(1.4f, 2.6f, 2f);

	private Vector3 item_iconpos = new Vector3(-0.9f, 2.74f, 2f);

	private Vector3 movepos = Vector3.right * 0.3f;

	private Vector3 pet_iconpos = new Vector3(-1.4f, 2f, 2f);

	private float duration_delay;

	private bool timereduce;

	private bool generalskill_exist;

	private bool infinitymode;

	private int currentpetskill;

	private int page_skillset;

	private int max_skillset;

	private int lastlist_skill;

	private int startlist_skill;

	public bool move_icon_skill;

	public bool move_icon_pet;

	private int current_costume;

	private Cha_Control script_cha;

	public Transform num_skill;

	private int generalSkillcount = 1;

	private void Awake()
	{
		mytransform = base.transform;
		cur_skill_grade = PlayerPrefsX.GetIntArray("n22");
		pet_activeskill = PlayerPrefsX.GetIntArray("n23");
		pet_passiveskill = PlayerPrefsX.GetIntArray("n27");
		pet_hunger = PlayerPrefsX.GetIntArray("n25");
		script_pack = pack.GetComponent<MakeUI>();
		script_cha = GameObject.FindWithTag("Player").GetComponent<Cha_Control>();
		current_costume = Crypto.Load_int_key("n05");
	}

	private void Start()
	{
		infinitymode = GameObject.FindWithTag("Respawn").GetComponent<Spawn>().infinitymode;
		if (!infinitymode)
		{
			skill_slot = PlayerPrefsX.GetIntArray("skill_slot");
		}
		else
		{
			skill_slot = new int[20];
		}
		item_slot = PlayerPrefsX.GetIntArray("n50");
		DB_PetSkill component = GameObject.FindWithTag("ui").GetComponent<DB_PetSkill>();
		DB_Skill component2 = GameObject.FindWithTag("ui").GetComponent<DB_Skill>();
		if (!infinitymode)
		{
			for (int i = 0; i < 5; i++)
			{
				if (skill_slot[i] >= 0)
				{
					skill_colltime[i] = component2.ss[skill_slot[i], cur_skill_grade[skill_slot[i]]]._cooltime;
					if (current_costume != 15)
					{
						skill_soulcost[i] = component2.ss[skill_slot[i], cur_skill_grade[skill_slot[i]]]._soulprice;
					}
					Vector2 vector = new Vector2(0.125f * (float)(skill_slot[i] % 8), 0.125f * (float)(skill_slot[i] / 8));
					clone_skillicon[i] = script_pack.CreatCustomPlane(Vector2.one * 0.16f, 0.15f, skill_iconpos, vector, vector + Vector2.one * 0.125f, "skill_icon", "Icon_Skill_p", 0f, 1);
					clone_fillbox[i] = script_pack.CreatCustomPlane(Vector2.one * 0.16f, 0f, skill_iconpos - Vector3.forward * 0.5f, new Vector2(0.625f, 0.25f), new Vector2(0.6875f, 0.3125f), "fillbox_skill", "Icon_FillBox", 0f, 1);
					clone_skillicon[i].GetComponent<Icon_Skill_p>().SkillKind(i, skill_slot[i], 0, skill_soulcost[i]);
					clone_fillbox[i].GetComponent<Icon_FillBox>().SkillKind(i, skill_colltime[i]);
					skill_iconpos -= Vector3.up * 0.32f;
					Transform transform = (Transform)Object.Instantiate(num_skill, clone_skillicon[i].position + new Vector3(-0.01f, 0.09f, -0.1f), Quaternion.identity);
					transform.parent = clone_skillicon[i];
					TextMesh component3 = transform.GetComponent<TextMesh>();
					component3.text = skill_soulcost[i].ToString();
					component3.characterSize = 0.02f;
					Transform transform2 = script_pack.CreatCustomPlane(new Vector2(0.2f, 0.1f), 0f, clone_skillicon[i].position + new Vector3(0.01f, 0.054f, -0.05f), new Vector2(0.25f, 0.75f), new Vector2(0.375f, 0.8125f), "bg_point", null, 0f, 0);
					transform2.parent = clone_skillicon[i];
				}
			}
			for (int j = 0; j < 2; j++)
			{
				_duration[j] = component.ps[j, pet_activeskill[j]]._duration;
				Vector2 vector2 = new Vector2(0.75f + 0.125f * (float)j, 0.25f);
				clone_peticon[j] = script_pack.CreatCustomPlane(Vector2.one * 0.16f, 0.15f, Vector3.one * 9f, vector2, vector2 + Vector2.one * 0.125f, "pet_icon", "Icon_Skill_p", 0f, 2);
				clone_peticon[j].GetComponent<Icon_Skill_p>().SkillKind(j, j, 1, 0);
			}
		}
		else
		{
			for (int k = 0; k < 20; k++)
			{
				if (cur_skill_grade[k] < 0)
				{
					cur_skill_grade[k] = 0;
				}
				skill_colltime[k] = component2.ss[skill_slot[k], cur_skill_grade[k]]._cooltime;
				Vector2 vector3 = new Vector2(0.125f * (float)(k % 8), 0.125f * (float)(k / 8));
				clone_skillicon[k] = script_pack.CreatCustomPlane(Vector2.one * 0.16f, 0.15f, Vector3.up * 200f, vector3, vector3 + Vector2.one * 0.125f, "skill_icon", "Icon_Skill_p", 0f, 1);
				clone_fillbox[k] = script_pack.CreatCustomPlane(Vector2.one * 0.16f, 0f, Vector3.up * 200f, new Vector2(0.625f, 0.25f), new Vector2(0.6875f, 0.3125f), "fillbox_skill", "Icon_FillBox", 0f, 1);
				clone_skillicon[k].GetComponent<Icon_Skill_p>().SkillKind(k, k, 0, 0);
				clone_fillbox[k].GetComponent<Icon_FillBox>().SkillKind(k, skill_colltime[k]);
				Transform transform3 = (Transform)Object.Instantiate(num_skill, clone_skillicon[k].position - Vector3.right * 0.1f, Quaternion.identity);
				transform3.parent = clone_skillicon[k];
			}
			for (int l = 0; l < 3; l++)
			{
				if (item_slot[l] > 0)
				{
					Vector2 vector4 = new Vector2(0.125f * (float)(item_slot[l] - 1), 0.875f);
					clone_itemicon[l] = script_pack.CreatCustomPlane(Vector2.one * 0.16f, 0f, item_iconpos, vector4, vector4 + Vector2.one * 0.125f, "item_icon", null, 0f, 0);
					item_iconpos += Vector3.right * 0.2f;
					script_cha.ItemBuff(item_slot[l]);
				}
			}
			icon_rotate = script_pack.CreatCustomPlane(Vector2.one * 0.24f, 0.23f, Vector3.one * 6f, new Vector2(0.75f, 0.75f), new Vector2(0.875f, 0.875f), "icon_rotate", "Icon_Skill_p", 0f, 1);
			icon_rotate.GetComponent<Icon_Skill_p>().ThisisRoll();
		}
		icon_pause = script_pack.CreatCustomPlane(Vector2.one * 0.16f, 0.15f, new Vector3(1.38f, 2.9f, 2f), new Vector2(0.625f, 0.75f), new Vector2(0.75f, 0.875f), "icon_pause", "Icon_Skill_p", 0f, 0);
		icon_pause.GetComponent<Icon_Skill_p>().ThisisPause();
		icon_shortsp = script_pack.CreatCustomPlane(Vector2.one * 0.25f, 0f, new Vector3(-1.37f, 2.5f, 1.8f), new Vector2(0.625f, 0.625f), new Vector2(0.75f, 0.75f), "icon_shortsp", null, 0f, 0);
		icon_shortsp.gameObject.active = false;
		script_cha.SetPetSkillLV(component.ps[1, pet_passiveskill[1]]._passive, component.ps[0, pet_activeskill[0]]._attackpoint, component.ps[1, pet_activeskill[1]]._attackpoint);
		skill_use = PlayerPrefsX.GetIntArray("skill_use");
		pet_skill_use = PlayerPrefsX.GetIntArray("pet_skill_use");
		if (!infinitymode)
		{
			PetSkill_Generation();
			return;
		}
		int[] intArray = PlayerPrefsX.GetIntArray("skill_slot");
		for (int m = 0; m < 5; m++)
		{
			if (intArray[m] >= 0)
			{
				SkillPlus(intArray[m], _isgeneral: false);
			}
		}
	}

	public void Set_General(short _soulcost, float _cooltime, short _generalkind, bool _infinitymode)
	{
		clone_fillbox[20] = script_pack.CreatCustomPlane(Vector2.one * 0.16f, 0f, Vector3.one * 5f, new Vector2(0.625f, 0.25f), new Vector2(0.6875f, 0.3125f), "fillbox_skill", "Icon_FillBox", 0f, 0);
		generalskill_exist = true;
		if (!_infinitymode)
		{
			skill_soulcost[20] = _soulcost;
			Transform transform = (Transform)Object.Instantiate(num_skill, Vector3.one * 5f + new Vector3(-0.01f, 0.09f, 0.1f), Quaternion.identity);
			transform.parent = clone_fillbox[20];
			TextMesh component = transform.GetComponent<TextMesh>();
			component.text = _soulcost.ToString();
			component.characterSize = 0.02f;
			Transform transform2 = script_pack.CreatCustomPlane(new Vector2(0.2f, 0.1f), 0f, Vector3.one * 5f + new Vector3(0.01f, 0.054f, 0.2f), new Vector2(0.25f, 0.75f), new Vector2(0.375f, 0.8125f), "bg_point", null, 0f, 0);
			transform2.parent = clone_fillbox[20];
		}
		else
		{
			skill_soulcost[20] = 0;
			Transform transform3 = (Transform)Object.Instantiate(num_skill, clone_fillbox[20].position - Vector3.right * 0.1f, Quaternion.identity);
			transform3.parent = clone_fillbox[20];
			_cooltime = 0.1f;
		}
		clone_fillbox[20].GetComponent<Icon_FillBox>().SkillKind(20, _cooltime);
	}

	public bool ResetCooltime_general()
	{
		bool result = true;
		if (!infinitymode)
		{
			isskillready[20] = 0;
			clone_fillbox[20].GetComponent<Icon_FillBox>().ResetScale();
		}
		else if (generalSkillcount <= 0)
		{
			result = false;
		}
		else
		{
			generalSkillcount--;
			clone_fillbox[20].GetChild(0).GetComponent<TextMesh>().text = generalSkillcount.ToString();
			clone_fillbox[20].GetComponent<Icon_FillBox>().ResetScale();
		}
		return result;
	}

	public void RegenGeneralSkill()
	{
		if (generalskill_exist)
		{
			generalSkillcount = 1;
			clone_fillbox[20].GetChild(0).GetComponent<TextMesh>().text = generalSkillcount.ToString();
		}
	}

	public void ShortSp(float _sp)
	{
		if (_sp >= 10f)
		{
			icon_shortsp.gameObject.active = false;
		}
		else
		{
			icon_shortsp.gameObject.active = true;
		}
	}

	public void ItemUse(int _index)
	{
		Object.Destroy(clone_itemicon[_index].gameObject);
		script_cha.Heal(50);
	}

	public void ResetCooltime_skill(int _currentslot)
	{
		if (!infinitymode)
		{
			skill_use[skill_slot[_currentslot]]++;
			isskillready[_currentslot] = 0;
			clone_fillbox[_currentslot].GetComponent<Icon_FillBox>().ResetScale();
			return;
		}
		skillcount[_currentslot]--;
		clone_skillicon[_currentslot].GetChild(0).GetComponent<TextMesh>().text = skillcount[_currentslot].ToString();
		if (skillcount[_currentslot] <= 0)
		{
			clone_skillicon[_currentslot].position = Vector3.one * 200f;
			clone_fillbox[_currentslot].position = Vector3.one * 200f;
			startlist_skill = 0;
			page_skillset = 0;
			Invoke("ArrayIcon", 0.2f);
		}
		else
		{
			isskillready[_currentslot] = 0;
			clone_fillbox[_currentslot].GetComponent<Icon_FillBox>().ResetScale();
		}
	}

	public void Skill_Possible(int _index)
	{
		if (skill_soulcost[_index] <= soulamount)
		{
			isskillready[_index] = 2;
			return;
		}
		isskillready[_index] = 1;
		clone_fillbox[_index].GetComponent<Icon_FillBox>().SoulLack();
	}

	public void Skillset_roll()
	{
		for (int i = 0; i < 20; i++)
		{
			clone_skillicon[i].position = Vector3.one * 5f;
			clone_fillbox[i].position = Vector3.one * 5f;
		}
		page_skillset = (page_skillset + 1) % max_skillset;
		if (page_skillset == 0)
		{
			startlist_skill = 0;
		}
		else
		{
			startlist_skill = lastlist_skill + 1;
		}
		ArrayIcon();
	}

	public bool SkillPlus(int _index, bool _isgeneral)
	{
		bool result = false;
		if (skillcount[_index] < 9)
		{
			skillcount[_index]++;
			clone_skillicon[_index].GetChild(0).GetComponent<TextMesh>().text = skillcount[_index].ToString();
		}
		else
		{
			result = true;
		}
		ArrayIcon();
		return result;
	}

	public void ArrayIcon()
	{
		int num = 0;
		max_skillset = 0;
		for (int i = 0; i < 20; i++)
		{
			if (skillcount[i] > 0)
			{
				clone_skillicon[i].position = Vector3.one * 200f;
				clone_fillbox[i].position = Vector3.one * 200f;
				if (i >= startlist_skill && num < 5)
				{
					clone_skillicon[i].localPosition = new Vector3(1.4f, 2.6f, 2f) - Vector3.up * 0.28f * num;
					clone_fillbox[i].localPosition = new Vector3(1.4f, 2.6f, 1.5f) - Vector3.up * 0.28f * num;
					num++;
					lastlist_skill = i;
				}
				max_skillset++;
			}
		}
		max_skillset = (max_skillset - 1) / 5 + 1;
		if (max_skillset > 1)
		{
			icon_rotate.localPosition = new Vector3(1.376f, 1.14f, 2f);
		}
		else
		{
			icon_rotate.localPosition = Vector3.one * 12f;
		}
	}

	public void SoulMeasure(int _soul)
	{
		soulamount = _soul;
		for (int i = 0; i < 6; i++)
		{
			if (isskillready[i] > 0)
			{
				if (skill_soulcost[i] <= soulamount)
				{
					isskillready[i] = 2;
					clone_fillbox[i].GetComponent<Icon_FillBox>().SoulFull();
				}
				else
				{
					isskillready[i] = 1;
					clone_fillbox[i].GetComponent<Icon_FillBox>().SoulLack();
				}
			}
		}
	}

	public void PetSkill_Generation()
	{
		int num = 0;
		for (int i = 0; i < 2; i++)
		{
			if (pet_hunger[i] > 0)
			{
				clone_peticon[i].position = pet_iconpos + Vector3.up * -0.32f * num + icon_pet_folder.position;
				ispetready[i] = true;
				num++;
			}
		}
	}

	public void Ef_SkillUse(Vector3 useiconpos)
	{
		ef_activeskill.position = useiconpos - Vector3.forward * 1.2f;
		ef_activeskill.GetComponent<Ef_twirl>().TwirlOn(4, 4, 20, loop: false);
	}

	public void Duration_reduce(int i)
	{
		pet_skill_use[i]++;
		ispetready[i] = false;
		timereduce = true;
		clone_peticon[i].position = Vector3.one * 9f;
		currentpetskill = i;
		pet_hunger[i]--;
		PlayerPrefsX.SetIntArray("n25", pet_hunger);
	}

	public void SaveSkillUse()
	{
		PlayerPrefsX.SetIntArray("skill_use", skill_use);
		PlayerPrefsX.SetIntArray("pet_skill_use", pet_skill_use);
	}

	public void SkillIcon_Move(bool _move)
	{
		move_icon_skill = _move;
	}

	public void PetIcon_Move(bool _move)
	{
		move_icon_pet = _move;
	}

	public void GeneralDead()
	{
		icon_shortsp.gameObject.active = false;
	}

	public void GeneralSkillFillbox(bool _active)
	{
		if (generalskill_exist)
		{
			if (!_active)
			{
				clone_fillbox[20].position = Vector3.one * 5f;
			}
			else
			{
				clone_fillbox[20].position = new Vector3(1.39f, 2.45f, 1.5f);
			}
		}
	}

	private void Update()
	{
		if (timereduce)
		{
			duration_delay += Time.deltaTime;
			if (duration_delay >= _duration[currentpetskill])
			{
				duration_delay = 0f;
				timereduce = false;
				script_cha.PetSkillFinish(currentpetskill);
			}
		}
		if (move_icon_skill)
		{
			mytransform.position = Vector3.Lerp(mytransform.position, movepos, Time.deltaTime * 9f);
		}
		else
		{
			mytransform.position = Vector3.Lerp(mytransform.position, Vector3.zero, Time.deltaTime * 12f);
		}
		if (move_icon_pet)
		{
			icon_pet_folder.position = Vector3.Lerp(icon_pet_folder.position, -movepos, Time.deltaTime * 9f);
		}
		else
		{
			icon_pet_folder.position = Vector3.Lerp(icon_pet_folder.position, Vector3.zero, Time.deltaTime * 12f);
		}
	}
}
