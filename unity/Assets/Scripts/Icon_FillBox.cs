using UnityEngine;

public class Icon_FillBox : MonoBehaviour
{
	private int slotindex;

	private float max_cooltime = 100f;

	private float cur_cooltime;

	private float scaleratio;

	private float cur_moveUV;

	private float prev_moveUV;

	private bool isthisready;

	private Vector3 originPos;

	private Icon_Skill script_skillcontrol;

	private Mesh thismesh;

	private Vector2[] originUV = new Vector2[4];

	private Vector2[] resetUV = new Vector2[4];

	private void Awake()
	{
		script_skillcontrol = GameObject.FindWithTag("icon_skill").GetComponent<Icon_Skill>();
		thismesh = GetComponent<MeshFilter>().mesh;
		originUV = thismesh.uv;
		resetUV = new Vector2[4]
		{
			new Vector2(0.6875f, 0.375f),
			new Vector2(0.75f, 0.375f),
			new Vector2(0.6875f, 0.3125f),
			new Vector2(0.75f, 0.3125f)
		};
	}

	public void SkillKind(int _slotindex, float _cooltime)
	{
		slotindex = _slotindex;
		max_cooltime = _cooltime;
		scaleratio = 0.0625f / max_cooltime;
		cur_cooltime = max_cooltime;
	}

	public void SoulLack()
	{
		thismesh.uv = new Vector2[4]
		{
			resetUV[0] - Vector2.right * 0.0625f,
			resetUV[1] - Vector2.right * 0.0625f,
			resetUV[2] - Vector2.right * 0.0625f,
			resetUV[3] - Vector2.right * 0.0625f
		};
	}

	public void SoulFull()
	{
		thismesh.uv = originUV;
	}

	public void ResetScale()
	{
		cur_cooltime = 0f;
		thismesh.uv = resetUV;
	}

	private void Update()
	{
		if (cur_cooltime < max_cooltime)
		{
			cur_moveUV += scaleratio * Time.deltaTime;
			if (cur_moveUV - prev_moveUV > 0.002f)
			{
				thismesh.uv = new Vector2[4]
				{
					resetUV[0] - new Vector2(0f, cur_moveUV),
					resetUV[1] - new Vector2(0f, cur_moveUV),
					resetUV[2] - new Vector2(0f, cur_moveUV),
					resetUV[3] - new Vector2(0f, cur_moveUV)
				};
				prev_moveUV = cur_moveUV;
			}
			cur_cooltime += Time.deltaTime;
			isthisready = false;
		}
		else if (!isthisready)
		{
			cur_moveUV = 0f;
			prev_moveUV = 0f;
			thismesh.uv = originUV;
			cur_cooltime = max_cooltime;
			isthisready = true;
			script_skillcontrol.Skill_Possible(slotindex);
		}
	}
}
