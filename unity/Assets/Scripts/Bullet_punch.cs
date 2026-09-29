using UnityEngine;

public class Bullet_punch : MonoBehaviour
{
	public Transform pt_thrust;

	private Transform mytransform;

	private Vector3 originScale = new Vector3(0.5f, 2f, 0.5f);

	private Vector3 growVector = new Vector3(1f, 0f, 1f);

	private Color currentColor;

	private Color transColor;

	private Color targetColor = new Color(0.5f, 0.5f, 0.5f, 0f);

	private Renderer myrenderer;

	private Material mymaterial;

	private SphereCollider mycollider;

	private Cha_Skill script_cha;

	private short efon;

	private float delay;

	private int addforce;

	private Transform cha1;

	private Transform enemy;

	private Vector3 directionVector;

	private float stepfactor = 1f;

	private float upangle;

	private bool boom;

	private void Awake()
	{
		mytransform = base.transform;
		myrenderer = base.GetComponent<Renderer>();
		mymaterial = myrenderer.material;
		mycollider = base.GetComponent<Collider>() as SphereCollider;
		cha1 = GameObject.FindWithTag("Player").transform;
		script_cha = cha1.GetComponent<Cha_Skill>();
		base.gameObject.active = false;
	}

	public void PunchOff()
	{
		delay = 4f;
		efon = 0;
		mycollider.enabled = false;
		myrenderer.enabled = false;
		base.gameObject.active = false;
		mytransform.localScale = originScale;
		{
			var emission = pt_thrust.GetComponent<ParticleSystem>().emission;
			emission.enabled = false;
		}
	}

	public void PunchShoot(float _delay, int _addforce, Transform _mon, float _step, float _upangle, float _collidersize, bool _boom)
	{
		base.gameObject.active = true;
		delay = _delay;
		efon = 1;
		enemy = _mon;
		addforce = _addforce;
		mycollider.enabled = false;
		stepfactor = _step;
		upangle = _upangle;
		mycollider.radius = _collidersize;
		boom = _boom;
	}

	private void Update()
	{
		if (efon <= 0)
		{
			return;
		}
		if (efon == 1)
		{
			if (enemy != null)
			{
				directionVector = enemy.position - cha1.position;
			}
			else
			{
				directionVector = cha1.forward * 0.1f;
			}
			if (directionVector != Vector3.zero)
			{
				cha1.rotation = Quaternion.LookRotation(directionVector);
			}
			cha1.position += (directionVector * 10f + cha1.right * stepfactor) * Time.deltaTime * 0.5f;
			if (delay > 0f)
			{
				delay -= Time.deltaTime;
			}
			else
			{
				if (addforce > 0)
				{
					cha1.GetComponent<Rigidbody>().AddForce(cha1.transform.forward * addforce);
				}
				efon = 2;
				mytransform.position = cha1.position + Vector3.up * (0.1f + upangle * 0.1f) + cha1.forward * 0.15f;
				mytransform.up = -cha1.forward - Vector3.up * upangle;
				mytransform.localScale = originScale;
				myrenderer.enabled = true;
				mycollider.enabled = true;
				mymaterial.SetColor("_TintColor", Color.gray);
				{
					var emission = pt_thrust.GetComponent<ParticleSystem>().emission;
					emission.enabled = true;
				}
				if (boom)
				{
					script_cha.BoomOn(1, _collider: false);
				}
			}
		}
		else if (efon == 2)
		{
			if (mytransform.localScale.x > 3f)
			{
				efon = 0;
				myrenderer.enabled = false;
				base.gameObject.active = false;
				mytransform.position = Vector3.one * 4f;
			}
			else if (mytransform.localScale.x > 1f)
			{
				{
					var emission = pt_thrust.GetComponent<ParticleSystem>().emission;
					emission.enabled = false;
				}
				mycollider.enabled = false;
			}
		}
		mytransform.localScale += growVector * Time.deltaTime * 6f;
		currentColor = mymaterial.GetColor("_TintColor");
		transColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * 5f);
		mymaterial.SetColor("_TintColor", transColor);
	}
}
