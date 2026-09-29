using UnityEngine;

public class Chosun : MonoBehaviour
{
	private Transform mytransform;

	private Collider mycollider;

	private Vector3 targetpos;

	private float attack_delay;

	private int attack_count;

	private Vector3 edgepos;

	private Vector3 rndPos;

	private Vector3 directionVector;

	private Transform cha1;

	private bool attack;

	private bool flystart;

	public Transform pt_leaf;

	private void Awake()
	{
		mytransform = base.transform;
		base.GetComponent<Animation>()["appear"].speed = 2.2f;
		base.GetComponent<Animation>()["fly_chosun"].speed = 0.2f;
		mycollider = base.GetComponent<Collider>();
		mycollider.enabled = false;
	}

	private void Start()
	{
		cha1 = GameObject.FindWithTag("Player").transform;
	}

	private void OnEnable()
	{
		base.GetComponent<Animation>()["appear"].speed = 0f;
		base.GetComponent<Animation>().Play("appear");
		AnimationState animationState = base.GetComponent<Animation>().PlayQueued("fly_chosun");
		animationState.speed = 0.25f;
		attack = false;
		mycollider.enabled = false;
		{
			var emission = pt_leaf.GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
		flystart = false;
		attack_count = 0;
		attack_delay = 0f;
	}

	private void Update()
	{
		if (!flystart)
		{
			if (attack_delay > 0.1f)
			{
				base.GetComponent<Animation>()["appear"].speed = 2.2f;
				Time.timeScale = 0.1f;
				flystart = true;
			}
			else
			{
				attack_delay += Time.deltaTime;
			}
			return;
		}
		if (!attack)
		{
			if (base.GetComponent<Animation>().IsPlaying("fly_chosun"))
			{
				attack = true;
				attack_delay = 1.2f;
				base.GetComponent<Animation>()["appear"].speed = 0.25f;
				Time.timeScale = 1f;
				base.GetComponent<AudioSource>().Play();
			}
			return;
		}
		if (attack_delay > 0.5f)
		{
			mytransform.position += mytransform.forward * Time.deltaTime * 2.5f;
			attack_delay -= Time.deltaTime;
			mycollider.enabled = true;
			return;
		}
		if (attack_delay > 0f)
		{
			attack_delay -= Time.deltaTime;
			mytransform.position += mytransform.forward * Time.deltaTime * 4f;
			mycollider.enabled = false;
			return;
		}
		attack_delay = 1.2f;
		edgepos = Random.onUnitSphere * 1f;
		edgepos[1] = 0f;
		attack_count++;
		if (attack_count >= 6)
		{
			mytransform.position = Vector3.one * 15f;
			base.gameObject.active = false;
			mycollider.enabled = false;
			{
				var emission = pt_leaf.GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
		}
		else if (attack)
		{
			rndPos = cha1.position + edgepos;
			directionVector = cha1.position - rndPos;
			mytransform.position = rndPos;
			base.GetComponent<AudioSource>().Play();
			if (directionVector != Vector3.zero)
			{
				mytransform.rotation = Quaternion.LookRotation(directionVector);
			}
		}
	}
}
