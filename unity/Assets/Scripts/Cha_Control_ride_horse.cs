using UnityEngine;

public class Cha_Control_ride_horse : MonoBehaviour
{
	public GameObject map;

	private Transform mytransform;

	private Vector3 pickPoint;

	private Vector3 prevPoint;

	private float startdelay;

	private bool isintro = true;

	private float dubbleclick;

	private float movespeed;

	private float action_delay;

	private bool finish;

	private short changeScene;

	public AudioClip run;

	public AudioClip behit;

	public AudioClip jump;

	public Transform pt_stepfog;

	private short monmovestat;

	private Vector2 dragStartpos = Vector2.zero;

	private float dragSumposY;

	private float dragdistanceY;

	private bool keydown;

	private RaycastHit casthit;

	private Vector3 rightlean = new Vector3(1f, 0f, 2f);

	private Vector3 leftlean = new Vector3(-1f, 0f, 2f);

	public Texture[] tex_stepfog = new Texture[5];

	public Transform cha1;

	private Cha_Control_ride_cha script_cha;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		mytransform = base.transform;
	}

	private void Start()
	{
		script_cha = cha1.GetComponent<Cha_Control_ride_cha>();
		mytransform.position = new Vector3(0f, 0f, -0.5f);
		base.GetComponent<Animation>()["horse_run"].speed = 0.7f;
		base.GetComponent<Animation>()["horse_jump"].speed = 0.34f;
		base.GetComponent<Animation>()["horse_cry"].speed = 0.5f;
		base.GetComponent<Animation>()["horse_jump"].layer = 1;
		base.GetComponent<Animation>()["horse_cry"].layer = 2;
		base.GetComponent<Animation>().Play("horse_run");
		int num = Crypto.Load_int_key("cur_stage_kind");
		pt_stepfog.GetComponent<Renderer>().sharedMaterial.mainTexture = tex_stepfog[num - 1];
	}

	public void RidingFinish()
	{
		base.GetComponent<AudioSource>().clip = run;
		base.GetComponent<AudioSource>().Play();
		isintro = true;
		startdelay = 0f;
		finish = true;
		script_cha.Finish();
	}

	public void FallDown()
	{
		if (!finish)
		{
			base.GetComponent<Rigidbody>().AddForce(Vector3.forward * -130f);
			base.GetComponent<Animation>().Play("horse_cry");
		}
	}

	public void HorseJump()
	{
		base.GetComponent<AudioSource>().clip = jump;
		base.GetComponent<AudioSource>().Play();
		base.GetComponent<Animation>().Play("horse_jump");
	}

	private void Update()
	{
		if (isintro)
		{
			if (finish)
			{
				if (changeScene != 2)
				{
					if (changeScene == 1)
					{
						base.transform.position = Vector3.zero;
						base.GetComponent<Animation>().Play("horse_stand");
						base.GetComponent<AudioSource>().Stop();
						base.GetComponent<Animation>().Stop();
						base.GetComponent<Animation>()["horse_cry"].speed = 0.25f;
						base.GetComponent<Animation>().Play("horse_cry");
						base.GetComponent<AudioSource>().PlayOneShot(behit);
						AnimationState animationState = base.GetComponent<Animation>().PlayQueued("horse_stand");
						animationState.layer = 2;
						animationState.speed = 0.1f;
						changeScene = 2;
					}
					else if (changeScene == 0)
					{
						if (mytransform.position.z > 2f)
						{
							script_cha.CrynStop();
							Object.Destroy(pt_stepfog.gameObject);
							Application.LoadLevel("Result_ride");
							changeScene = 1;
						}
						else
						{
							mytransform.position += Vector3.forward * Time.deltaTime;
							mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.LookRotation(Vector3.forward), Time.deltaTime * 5f);
						}
					}
				}
			}
			else if (startdelay > 1f)
			{
				isintro = false;
			}
			else if (mytransform.position.z < 0f)
			{
				mytransform.position += Vector3.forward * 0.4f * Time.deltaTime;
			}
			startdelay += Time.deltaTime;
			return;
		}
		if (Mathf.Abs(mytransform.position.x) > 0.7f || mytransform.position.z > 1f || mytransform.position.z < -0.4f)
		{
			mytransform.position = Vector3.Lerp(mytransform.position, Vector3.zero, Time.deltaTime * 1f);
			pickPoint = Vector3.zero;
		}
		else if (Input.anyKeyDown)
		{
			dragStartpos = Input.mousePosition;
		}
		else if (Input.anyKey)
		{
			keydown = true;
			dragdistanceY = (Input.mousePosition.y - dragStartpos.y) * Time.deltaTime;
			if ((double)dragdistanceY > 0.001 * (double)Screen.height)
			{
				dragSumposY += dragdistanceY;
			}
			else
			{
				dragSumposY = 0f;
			}
			dragStartpos = Input.mousePosition;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (dragSumposY > 0.003f * (float)Screen.height && monmovestat == 1)
			{
				HorseJump();
				dragSumposY = 0f;
			}
			else if (dubbleclick < 0.25f && monmovestat == 1)
			{
				float num = Vector3.Distance(Input.mousePosition, prevPoint);
				if (num < 50f)
				{
					HorseJump();
				}
			}
			if (map.GetComponent<Collider>().Raycast(ray, out casthit, 4f))
			{
				pickPoint = casthit.point;
				base.GetComponent<Animation>().CrossFade("horse_run");
			}
			movespeed = 0.5f;
			if (pickPoint.x - mytransform.position.x > 0.01f)
			{
				mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.LookRotation(rightlean), Time.deltaTime * 3f);
			}
			else if (pickPoint.x - mytransform.position.x < -0.01f)
			{
				mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.LookRotation(leftlean), Time.deltaTime * 3f);
			}
			else
			{
				mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.LookRotation(Vector3.forward), Time.deltaTime * 3f);
			}
		}
		else if (keydown)
		{
			dubbleclick = 0f;
			prevPoint = Input.mousePosition;
			keydown = false;
		}
		else
		{
			dubbleclick += 2f * Time.deltaTime;
			dragSumposY = 0f;
			if (movespeed > 0f)
			{
				movespeed -= Time.deltaTime;
			}
			else
			{
				movespeed = 0f;
			}
			mytransform.rotation = Quaternion.Lerp(mytransform.rotation, Quaternion.LookRotation(Vector3.forward), Time.deltaTime * 3f);
		}
		mytransform.position = Vector3.MoveTowards(mytransform.position, pickPoint, Time.deltaTime * movespeed);
		if (base.GetComponent<Animation>().IsPlaying("horse_cry"))
		{
			if (monmovestat != 3)
			{
				base.GetComponent<AudioSource>().PlayOneShot(behit);
				monmovestat = 3;
			}
		}
		else if (action_delay > 0f)
		{
			action_delay -= Time.deltaTime;
			monmovestat = 4;
		}
		else if (base.GetComponent<Animation>().IsPlaying("horse_jump"))
		{
			monmovestat = 2;
		}
		else if (base.GetComponent<Animation>().IsPlaying("horse_run") && monmovestat != 1)
		{
			base.GetComponent<AudioSource>().clip = run;
			base.GetComponent<AudioSource>().Play();
			monmovestat = 1;
		}
	}
}
