using UnityEngine;

public class Ef_splash : MonoBehaviour
{
	public Transform rising_fire;

	private Transform cha1;

	private Transform mytransform;

	private Collider mycollider;

	private Renderer myrenderer;

	private float showtime = 1.5f;

	private float dis;

	private Color originTintcolor;

	private bool collidsionOn;

	private bool showon;

	private Color tintcolor;

	private Color transColor;

	private void Awake()
	{
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		myrenderer = base.GetComponent<Renderer>();
	}

	private void Start()
	{
		cha1 = GameObject.FindWithTag("Player").transform;
		myrenderer.enabled = false;
		mycollider.enabled = false;
		originTintcolor = myrenderer.material.GetColor("_TintColor");
		base.gameObject.active = false;
	}

	public void SplashOn(bool col, float _dis, float _delay)
	{
		collidsionOn = col;
		showtime = 1.5f;
		myrenderer.material.SetColor("_TintColor", originTintcolor);
		dis = _dis;
		Invoke("ShowOn", _delay);
	}

	public void SplashOff()
	{
		if (mycollider.enabled)
		{
			mycollider.enabled = false;
		}
		base.gameObject.active = false;
		CancelInvoke("ShowOn");
	}

	public void ShowOn()
	{
		base.gameObject.active = true;
		rising_fire.gameObject.active = true;
		myrenderer.enabled = true;
		if (collidsionOn)
		{
			mycollider.enabled = true;
		}
		mytransform.rotation = cha1.rotation;
		mytransform.position = cha1.position + Vector3.up * 0.02f + cha1.forward * dis;
		mytransform.localRotation = Quaternion.identity;
		showon = true;
	}

	private void Update()
	{
		if (!showon)
		{
			return;
		}
		showtime -= Time.deltaTime;
		if (showtime > 1.3f)
		{
			return;
		}
		if (showtime > 0.5f)
		{
			if (mycollider.enabled)
			{
				mycollider.enabled = false;
			}
		}
		else if (showtime > 0f)
		{
			if (mycollider.enabled)
			{
				mycollider.enabled = false;
			}
			tintcolor = myrenderer.material.GetColor("_TintColor");
			transColor = Color.Lerp(tintcolor, Color.clear, Time.deltaTime * 14f);
			myrenderer.material.SetColor("_TintColor", transColor);
		}
		else
		{
			base.gameObject.active = false;
			mytransform.position = Vector3.one * 14f;
		}
	}
}
