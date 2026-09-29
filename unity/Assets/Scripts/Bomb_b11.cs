using UnityEngine;

public class Bomb_b11 : MonoBehaviour
{
	public float uvspeed = 1f;

	public float hitrate = 0.2f;

	public float startdelay = 0.1f;

	public float finishdelay = 3f;

	private float delay;

	private Material mymaterial;

	private Collider mycollider;

	private Vector2 offset;

	private Transform mytransform;

	private float scalefactor = 12f;

	private bool particleoff;

	private void Awake()
	{
		mytransform = base.transform;
		mymaterial = base.GetComponent<Renderer>().sharedMaterial;
		mycollider = base.GetComponent<Collider>();
	}

	private void OnEnable()
	{
		offset = Vector2.zero;
		mymaterial.mainTextureOffset = Vector2.zero;
		InvokeRepeating("DanceHit", startdelay, hitrate);
		mytransform.localScale = Vector3.one * 2f;
		delay = 0f;
		scalefactor = 12f;
		{
			var emission = mytransform.GetChild(0).GetComponent<ParticleSystem>().emission;
			emission.enabled = true;
		}
	}

	private void DanceHit()
	{
		mycollider.enabled = false;
		mycollider.enabled = true;
	}

	private void Update()
	{
		offset += Vector2.up * Time.deltaTime * uvspeed;
		if (delay > finishdelay)
		{
			CancelInvoke();
			mycollider.enabled = false;
			mytransform.position = Vector3.one * 38f;
			base.gameObject.active = false;
			delay = 0f;
			return;
		}
		scalefactor -= Time.deltaTime * 10f;
		delay += Time.deltaTime;
		mymaterial.mainTextureOffset = offset;
		mytransform.localScale += Vector3.one * Time.deltaTime * scalefactor;
		if (!particleoff && delay > 0.5f)
		{
			{
				var emission = mytransform.GetChild(0).GetComponent<ParticleSystem>().emission;
				emission.enabled = false;
			}
		}
	}
}
