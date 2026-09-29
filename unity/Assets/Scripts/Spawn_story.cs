using UnityEngine;

public class Spawn_story : MonoBehaviour
{
	public Transform[] enemy_source = new Transform[3];

	public Transform[] mon_destroy = new Transform[3];

	private Transform spawnenemy;

	private Transform clone_enemy;

	private Transform[] enemyset = new Transform[3];

	private Transform[] clone_destroy = new Transform[6];

	private Transform stage;

	private int monnum = 3;

	private int destroy_human_kind;

	private int destroy_beast_kind = 3;

	private int totalEnemyNum = 10;

	private int regen = -1;

	private float spawndelay;

	private bool countdown;

	private bool guide;

	private int cur_difficulty;

	private int rndpoint;

	private int rndoldpoint = -1;

	private Vector3 rndpos;

	private GameObject last_mon;

	private Vector3[] spawpoint = new Vector3[8];

	private int storymonnum = 3;

	private int max_stage_index = -1;

	private void Awake()
	{
		max_stage_index = Crypto.Load_int_key("n06");
		if (max_stage_index != -1)
		{
			totalEnemyNum = 0;
		}
	}

	private void Start()
	{
		for (int i = 0; i < 3; i++)
		{
			clone_destroy[i] = (Transform)Object.Instantiate(mon_destroy[i], Vector3.one * 6f, Quaternion.identity);
			clone_destroy[i + 3] = (Transform)Object.Instantiate(mon_destroy[3], Vector3.one * 6f, Quaternion.identity);
		}
		stage = GameObject.Find("stage").transform;
		for (int j = 0; j < 8; j++)
		{
			ref Vector3 reference = ref spawpoint[j];
			reference = stage.GetChild(0).GetChild(j).position;
		}
		cur_difficulty = 0;
	}

	public void StoryEnemyDead()
	{
		storymonnum--;
	}

	public void EnemyDead(int _destroy, Vector3 _pos, Texture _tex, Vector3 _scale, Vector3 _forcedir)
	{
		if (!guide)
		{
			if (max_stage_index == -1)
			{
				GameObject.Find("UI_Ingame").GetComponent<UI_Ingame_story>().GuideOn(0);
			}
			guide = true;
		}
		monnum--;
		if (monnum <= 0 && regen == -2)
		{
			GameObject.Find("scene_trans").GetComponent<Story_trans>().ScreenOn(2f);
		}
		if (_destroy == 2)
		{
			clone_destroy[destroy_human_kind].position = _pos + _forcedir * 0.15f;
			clone_destroy[destroy_human_kind].rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
			clone_destroy[destroy_human_kind].GetComponent<Mon_Destroy>().TextureChange(_tex, _scale, _destroy);
			clone_destroy[destroy_human_kind].gameObject.active = true;
			destroy_human_kind = (destroy_human_kind + 1) % 3;
		}
		if (_destroy == 1)
		{
			clone_destroy[destroy_beast_kind].position = _pos + _forcedir * 0.15f;
			clone_destroy[destroy_beast_kind].rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
			clone_destroy[destroy_beast_kind].GetComponent<Mon_Destroy>().TextureChange(_tex, _scale, _destroy);
			clone_destroy[destroy_beast_kind].gameObject.active = true;
			destroy_beast_kind = (destroy_beast_kind + 1) % 3 + 3;
		}
	}

	public void RegenStart()
	{
		regen = 0;
		for (int i = 0; i < 3; i++)
		{
			int num = i + cur_difficulty;
			enemyset[i] = enemy_source[num];
		}
	}

	public int SetRndPoint()
	{
		rndpoint = Random.Range(0, 8);
		if (rndpoint == rndoldpoint)
		{
			rndpoint = (rndpoint + 1) % 8;
		}
		rndoldpoint = rndpoint;
		return rndpoint;
	}

	private void Update()
	{
		if (regen >= 0)
		{
			if (spawndelay > 0f)
			{
				spawndelay -= Time.deltaTime;
			}
			else if (totalEnemyNum > 0 && spawndelay <= 0f)
			{
				rndpos = spawpoint[SetRndPoint()];
				int num = Random.Range(0, 6);
				num = ((num > 2) ? ((num <= 4) ? 1 : 2) : 0);
				spawnenemy = enemyset[num];
				clone_enemy = (Transform)Object.Instantiate(spawnenemy, rndpos, Quaternion.identity);
				clone_enemy.name = "enemy";
				monnum++;
				totalEnemyNum--;
				spawndelay = 0.2f;
			}
			else if (totalEnemyNum <= 0)
			{
				regen = -2;
			}
		}
		else if (monnum <= 5 + storymonnum && !countdown && regen == -2)
		{
			for (int i = 0; i < monnum - storymonnum; i++)
			{
				last_mon = GameObject.Find("enemy");
				last_mon.GetComponent<AI_Enemy01>().CountDown();
				last_mon.name = "enemy_confirm";
			}
			countdown = true;
		}
	}
}
