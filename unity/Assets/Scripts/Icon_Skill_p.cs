using UnityEngine;

public class Icon_Skill_p : MonoBehaviour
{
	private UI_Ingame script_IngameUI;

	private Cha_Skill script_chaskill;

	private Icon_Skill script_skillcontrol;

	private int currentSkill = -1;

	private int slotindex;

	private int soulcost = 1;

	private int isskillready;

	private bool ispetready;

	private bool pause;

	private bool roll;

	public bool general;

	public bool g_skill;

	private int skill_pet_item;

	private bool clickon;

	private void Start()
	{
		script_chaskill = GameObject.FindWithTag("Player").GetComponent<Cha_Skill>();
		script_skillcontrol = GameObject.FindWithTag("icon_skill").GetComponent<Icon_Skill>();
		script_IngameUI = GameObject.FindWithTag("ui").GetComponent<UI_Ingame>();
	}

	public void SkillKind(int _slotindex, int _kind, int _skill_pet_item, short _soulcost)
	{
		slotindex = _slotindex;
		currentSkill = _kind;
		skill_pet_item = _skill_pet_item;
		soulcost = _soulcost;
	}

	public void ThisisPause()
	{
		pause = true;
	}

	public void ThisisRoll()
	{
		roll = true;
	}

	public bool SkillColliderOn()
	{
		clickon = false;
		if (general)
		{
			clickon = script_IngameUI.CallGeneral(0);
		}
		else if (g_skill)
		{
			isskillready = script_skillcontrol.isskillready[20];
			if (isskillready == 2 && script_skillcontrol.ResetCooltime_general())
			{
				script_chaskill.SkillOn(currentSkill, _general: true);
				script_IngameUI.GainSoul(-soulcost);
				clickon = true;
			}
		}
		else if (pause)
		{
			script_IngameUI.PauseOn();
		}
		else if (roll)
		{
			script_skillcontrol.Skillset_roll();
		}
		else if (skill_pet_item == 0)
		{
			if (script_skillcontrol.move_icon_skill)
			{
				return clickon;
			}
			isskillready = script_skillcontrol.isskillready[slotindex];
			if (isskillready == 2)
			{
				script_skillcontrol.Ef_SkillUse(base.transform.position);
				script_skillcontrol.ResetCooltime_skill(slotindex);
				script_IngameUI.GainSoul(-soulcost);
				script_chaskill.SkillOn(currentSkill, _general: false);
				clickon = true;
			}
		}
		else if (skill_pet_item == 1)
		{
			if (script_skillcontrol.move_icon_pet)
			{
				return clickon;
			}
			ispetready = script_skillcontrol.ispetready[currentSkill];
			if (ispetready)
			{
				script_skillcontrol.Ef_SkillUse(base.transform.position);
				script_skillcontrol.Duration_reduce(currentSkill);
				script_chaskill.PetSkillOn(currentSkill);
				clickon = true;
			}
		}
		else
		{
			script_skillcontrol.Ef_SkillUse(base.transform.position);
			script_skillcontrol.ItemUse(slotindex);
		}
		return clickon;
	}
}
