using UnityEngine;

public class Cam_Move : MonoBehaviour
{
	private int dx = 1;

	private bool zoom;

	private bool iconmove;

	private int z_speed;

	private float z_time;

	private float zoomdelay;

	private float fov = 30f;

	private float originfov = 30f;

	private Transform cha1;

	private Transform target;

	private Transform mytransform;

	private Vector3 distancetarget;

	private Vector3 chaposition;

	private float boundfactor = 1f;

	private Vector3 hit_shake1 = new Vector3(0.06f, 0f, 0.03f);

	private Vector3 hit_shake2 = new Vector3(0f, 0.03f, 0.02f);

	private float resetcam_delay;

	private bool resetstart;

	private bool fovchange;

	private Camera mycamera;

	public Vector3 limitpos;

	private Icon_Skill script_skillicon;

	private float fovbk_delay;

	private float limit_x = 2.1f;

	private float limit_y_b = -3.3f;

	private float limit_y_f = 0.85f;

	private bool hidestop;

	private short topviewon;

	private float topviewdelay = 3f;

	private short movespeed = 10;

	private UI_Ingame script_ui;

	private bool story_scene;

	private void Awake()
	{
		mytransform = base.transform;
		mycamera = base.GetComponent<Camera>();
		cha1 = GameObject.FindWithTag("Player").transform;
		script_skillicon = GameObject.FindWithTag("icon_skill").GetComponent<Icon_Skill>();
		ResetCam();
		script_ui = GameObject.FindWithTag("ui").GetComponent<UI_Ingame>();
	}

	private void Start()
	{
		int num = Crypto.Load_int_key("play_kind");
		if (num > 5)
		{
			limit_x = 0.5f;
			limit_y_b = -1f;
			limit_y_f = 15.5f;
		}
		distancetarget = new Vector3(0f, 1.3f, -1.04f);
	}

	public void Topview()
	{
		distancetarget = new Vector3(0f, 1.3f, -0.1f);
		topviewon = 1;
		topviewdelay = 3f;
		movespeed = 5;
	}

	public void LookTarget(Transform _target, int _fov, float _delay)
	{
		target = _target;
		if (_delay > 0f)
		{
			resetstart = true;
			resetcam_delay = _delay;
		}
		if (_fov != 0)
		{
			ZoomIn(-1, _fov, _delay);
		}
	}

	public void ThisIsStory()
	{
		story_scene = true;
	}

	public void IconHideStop(bool _hidestop)
	{
		hidestop = _hidestop;
	}

	public void ResetCam()
	{
		target = cha1;
		fov = originfov;
		fovchange = true;
		resetstart = false;
		zoom = false;
		zoomdelay = 0f;
	}

	public void FovTest(float _tempfov)
	{
		originfov = _tempfov;
		fov = originfov;
		mycamera.fieldOfView = originfov;
	}

	private void Update()
	{
		if (resetstart)
		{
			if (resetcam_delay > 0f)
			{
				resetcam_delay -= Time.deltaTime;
			}
			else
			{
				resetcam_delay = 0f;
				resetstart = false;
				ResetCam();
				cha1.GetComponent<Cha_Control>().StartControl();
			}
		}
		chaposition = target.position + distancetarget;
		mytransform.position = Vector3.Lerp(mytransform.position, chaposition, Time.deltaTime * (float)movespeed);
		limitpos = mytransform.position;
		if (topviewon != 1)
		{
			limitpos[2] = Mathf.Clamp(limitpos[2], limit_y_b * boundfactor, limit_y_f * boundfactor);
		}
		limitpos[0] = Mathf.Clamp(limitpos[0], (0f - limit_x) * boundfactor, limit_x * boundfactor);
		if (topviewon == 2)
		{
			mytransform.position = Vector3.MoveTowards(mytransform.position, limitpos, Time.deltaTime * 3f);
		}
		else
		{
			mytransform.position = limitpos;
		}
		if (!hidestop)
		{
			if (chaposition[0] > 2.4f)
			{
				if (!iconmove)
				{
					iconmove = true;
					script_skillicon.SkillIcon_Move(_move: true);
				}
			}
			else if (chaposition[0] < -2.4f)
			{
				if (!iconmove)
				{
					iconmove = true;
					script_skillicon.PetIcon_Move(_move: true);
				}
			}
			else if (iconmove)
			{
				iconmove = false;
				script_skillicon.SkillIcon_Move(_move: false);
				script_skillicon.PetIcon_Move(_move: false);
			}
		}
		if (fovchange)
		{
			boundfactor = 0.8f + 0.2f * originfov / mycamera.fieldOfView;
			if (zoom)
			{
				if (z_speed == -1)
				{
					mycamera.fieldOfView = fov;
				}
				else
				{
					mycamera.fieldOfView = Mathf.Lerp(mycamera.fieldOfView, fov, Time.deltaTime * (float)z_speed);
				}
				if (zoomdelay < z_time)
				{
					zoomdelay += Time.deltaTime;
				}
				else
				{
					zoom = false;
					zoomdelay = 0f;
					fovbk_delay = 0f;
				}
			}
			else if (fovbk_delay < 2f)
			{
				fovbk_delay += Time.deltaTime;
				mycamera.fieldOfView = Mathf.Lerp(mycamera.fieldOfView, originfov, Time.deltaTime * 3f);
			}
			else
			{
				fovbk_delay = 0f;
				fovchange = false;
				mycamera.fieldOfView = originfov;
			}
		}
		if (topviewon <= 0)
		{
			return;
		}
		topviewdelay -= Time.deltaTime;
		if (topviewon == 1)
		{
			mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.Euler(80f, 0f, 0f), Time.deltaTime * 5f);
			mycamera.fieldOfView = Mathf.Lerp(mycamera.fieldOfView, 40f, Time.deltaTime * 3f);
			if (topviewdelay < 1.5f)
			{
				distancetarget = new Vector3(0f, 1.3f, -1.04f);
				topviewon = 2;
				movespeed = 5;
			}
		}
		else if (topviewon == 2)
		{
			mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.Euler(50f, 0f, 0f), Time.deltaTime * 5f);
			mycamera.fieldOfView = Mathf.Lerp(mycamera.fieldOfView, originfov, Time.deltaTime * 3f);
			if (topviewdelay < 0f)
			{
				topviewon = 0;
				mytransform.rotation = Quaternion.Euler(50f, 0f, 0f);
				movespeed = 10;
			}
		}
	}

	public void ZoomIn(int _zoomspeed, int _fov, float _delay)
	{
		zoom = true;
		z_speed = _zoomspeed;
		fov = _fov;
		z_time = _delay;
		fovchange = true;
	}

	public void Hitcam()
	{
		if (!story_scene)
		{
			script_ui.ComboPlus(0.01f);
		}
		dx = -dx;
		mytransform.position += hit_shake1 * dx;
	}

	public void Hitcam2(float _factor)
	{
		mytransform.position += hit_shake2 * _factor;
	}
}
