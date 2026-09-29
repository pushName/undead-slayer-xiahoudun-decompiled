using UnityEngine;

public class Map_Compose : MonoBehaviour
{
	private const int MAXCOIN = 14;

	private const int MAXOBJ = 7;

	private const int MAXENEMY = 3;

	public Transform[] map = new Transform[5];

	public Transform soulstone;

	public Transform barricate;

	public Transform enemy;

	public Transform dummy_gui;

	private float[] mapposZ = new float[2];

	private Transform[] clone_pmap = new Transform[2];

	private Transform[] c_soulstone = new Transform[14];

	private Transform[] c_barricate = new Transform[7];

	private Transform[] c_enemy = new Transform[3];

	private int count_soulstone;

	private int count_barricate;

	private short maplength;

	private float ss_creat_delay;

	private float ss_creat_time = 100f;

	public int finalmap = 1;

	private Transform clone_obj;

	private float ss_posX;

	private float barricate_posX;

	private int negative_factor;

	private int tutorial = -1;

	private void Awake()
	{
		GameObject gameObject = GameObject.Find("Loading_f");
		if (gameObject == null)
		{
			LoadingFinish();
			Camera.main.transform.GetChild(0).gameObject.active = true;
		}
	}

	public void LoadingFinish()
	{
		ss_creat_time = Random.Range(1.5f, 2f);
		ss_creat_delay = 0f;
		tutorial = Crypto.Load_int_key("tutorial");
		if (tutorial == 5)
		{
			dummy_gui.gameObject.active = true;
			Time.timeScale = 1E-06f;
			Crypto.Save_int_key("tutorial", -1);
			GameObject.Find("pet_horse_riding").GetComponent<AudioSource>().Stop();
		}
	}

	private void Start()
	{
		base.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("vol_bgm");
		AudioListener.volume = PlayerPrefs.GetFloat("vol_master");
		float num = 0f;
		int num2 = Crypto.Load_int_key("cur_stage_kind");
		for (int i = 0; i < 2; i++)
		{
			mapposZ[i] += num;
			num += 40f;
			clone_pmap[i] = (Transform)Object.Instantiate(map[num2 - 1], Vector3.forward * mapposZ[i], Quaternion.identity);
		}
		for (int j = 0; j < 3; j++)
		{
			c_enemy[j] = (Transform)Object.Instantiate(enemy, (3 + j) * Vector3.one, Quaternion.identity);
		}
		for (int k = 0; k < 7; k++)
		{
			c_barricate[k] = (Transform)Object.Instantiate(barricate, -2f * Vector3.forward, Quaternion.identity);
		}
		for (int l = 0; l < 14; l++)
		{
			c_soulstone[l] = (Transform)Object.Instantiate(soulstone, -2f * Vector3.forward, Quaternion.identity);
		}
	}

	private void Update()
	{
		ss_creat_delay += Time.deltaTime;
		if (ss_creat_delay >= ss_creat_time)
		{
			int num = Random.Range(0, 20);
			if (num < 2)
			{
				num = 2;
			}
			if (num >= 12)
			{
				barricate_posX = Random.Range(-0.06f, 0.06f);
				ss_posX = Random.Range(-0.3f, 0.3f);
				for (int i = 0; i < 8; i++)
				{
					c_soulstone[count_soulstone].position = Vector3.right * (ss_posX + barricate_posX * (float)i) + Vector3.forward * (2f + 0.3f * (float)i);
					count_soulstone = (count_soulstone + 1) % 14;
				}
			}
			else if (num >= 6)
			{
				for (int j = 0; j < 3; j++)
				{
					if (!c_enemy[j].gameObject.active)
					{
						int num2 = Random.Range(0, 2) * 2 - 1;
						c_enemy[j].gameObject.active = true;
						c_enemy[j].position = new Vector3((float)num2 * 1.4f, 0f, Random.Range(-0.2f, 0.7f));
						break;
					}
				}
			}
			else
			{
				ss_posX = Random.Range(-0.3f, 0.3f);
				c_soulstone[count_soulstone].position = Vector3.right * ss_posX + Vector3.forward * 3f;
				count_soulstone = (count_soulstone + 1) % 14;
				barricate_posX = ss_posX - 0.18f * (float)num * 0.5f;
				negative_factor = 0;
				for (int k = 0; k < num + 2; k++)
				{
					c_barricate[count_barricate].position = Vector3.right * barricate_posX + Vector3.forward * (2.2f + Random.Range(-0.1f, 0.1f));
					c_barricate[count_barricate].rotation = Quaternion.Euler(0f, negative_factor + Random.Range(-24, 24), 0f);
					barricate_posX += 0.18f;
					negative_factor += 180;
					count_barricate = (count_barricate + 1) % 7;
				}
			}
			ss_creat_time = Random.Range(1.6f, 2f);
			ss_creat_time = 1.6f;
			ss_creat_delay = 0f;
		}
		for (int l = 0; l < 2; l++)
		{
			mapposZ[l] += -1.8f * Time.deltaTime;
			if (mapposZ[l] < -40f)
			{
				mapposZ[l] += 80f;
				clone_pmap[l].position = Vector3.forward * mapposZ[l];
				maplength++;
				if (maplength >= finalmap)
				{
					GameObject.Find("pet_horse_riding").GetComponent<Cha_Control_ride_horse>().RidingFinish();
				}
			}
			clone_pmap[l].position = Vector3.forward * mapposZ[l];
		}
	}
}
