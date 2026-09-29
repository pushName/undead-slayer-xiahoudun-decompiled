using UnityEngine;

public class AI_Asist : MonoBehaviour
{
	private float delay_atack;

	private float firerate = 1f;

	private Transform mytransform;

	private Transform cha1;

	private Collider mycollider;

	private Vector3 directionVector;

	private Quaternion rotate;

	private bool shoot;

	private float speed = 0.3f;

	private float cur_speed = 0.3f;

	private Transform m_arrow;

	public Transform a_mesh;

	private Transform ef_splash;

	public Texture[] tex_pt = new Texture[8];

	public Texture[] tex_grade = new Texture[8];

	private int a_index;

	private Vector3 targetPos;

	private DB_angel script_db;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		cha1 = GameObject.FindWithTag("Player").transform;
		a_index = Crypto.Load_int_key("n61");
		script_db = GetComponent<DB_angel>();
	}

	private void Start()
	{
		if (a_index == 0)
		{
			mytransform.position = Vector3.up * 21f;
			base.gameObject.active = false;
		}
		else
		{
			mytransform.position = Vector3.up * 0.2f;
			base.GetComponent<ParticleSystem>().GetComponent<Renderer>().material.mainTexture = tex_pt[a_index - 1];
			a_mesh.GetComponent<Renderer>().material.mainTexture = tex_grade[a_index - 1];
			int num = 25;
			speed = script_db.ag[a_index - 1]._speed;
			firerate = script_db.ag[a_index - 1]._firerate;
			num = script_db.ag[a_index - 1]._arrowkind;
			GameObject gameObject = Resources.Load("bullet_angel" + a_index) as GameObject;
			m_arrow = (Transform)Object.Instantiate(gameObject.transform, Vector3.up * 4f, Quaternion.identity);
			m_arrow.gameObject.layer = num;
			short splashEF = script_db.ag[a_index - 1]._splashEF;
			if (splashEF > 0)
			{
				GameObject gameObject2 = Resources.Load("angel_splash" + splashEF) as GameObject;
				ef_splash = (Transform)Object.Instantiate(gameObject2.transform, Vector3.up * 4f, Quaternion.identity);
			}
			else
			{
				ef_splash = null;
			}
		}
		InvokeRepeating("LoadDir", 0.1f, 0.1f);
	}

	public void SetIndex(int _index)
	{
		a_index = _index;
		Start();
	}

	public void LoadDir()
	{
		directionVector = cha1.position - mytransform.position;
		directionVector[1] = 0f;
		if (directionVector != Vector3.zero)
		{
			rotate = Quaternion.LookRotation(directionVector);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!shoot)
		{
			int layer = other.gameObject.layer;
			if (other.gameObject.layer == 8)
			{
				AttackOn(other.transform.position);
				mycollider.enabled = false;
				delay_atack = 0f;
				shoot = true;
			}
		}
	}

	private void AttackOn(Vector3 _targetpos)
	{
		targetPos = _targetpos;
		m_arrow.forward = targetPos - mytransform.position;
		m_arrow.position = mytransform.position;
		m_arrow.gameObject.active = true;
	}

	public void AttackFinish()
	{
		shoot = false;
		if (ef_splash != null)
		{
			Vector3 forward = m_arrow.forward;
			forward.y = 0f;
			targetPos.y = 0f;
			ef_splash.forward = forward;
			ef_splash.position = targetPos;
			ef_splash.gameObject.active = true;
		}
	}

	private void Update()
	{
		if (shoot)
		{
			cur_speed = Mathf.Lerp(cur_speed, 0f, Time.deltaTime * 4f);
		}
		else
		{
			cur_speed = Mathf.Lerp(cur_speed, speed, Time.deltaTime * 4f);
			if (delay_atack > firerate)
			{
				mycollider.enabled = true;
				delay_atack = 0f;
			}
			else
			{
				delay_atack += Time.deltaTime;
			}
		}
		mytransform.rotation = Quaternion.Lerp(mytransform.rotation, rotate, Time.deltaTime * cur_speed * 4f);
		mytransform.position += mytransform.forward * Time.deltaTime * cur_speed;
	}
}
