using UnityEngine;

public class Ef_rotfog : MonoBehaviour
{
	private float fogheight;

	private float fogspeed;

	private int fogrotation;

	private int fogalpha;

	private float xyratio;

	private Color transColor;

	private Color targetColor;

	private Color currentColor;

	private Vector3 plusV;

	private Transform mytransform;

	private Material mymaterial;

	private void Awake()
	{
		mytransform = base.transform;
		mymaterial = base.GetComponent<Renderer>().material;
	}

	private void Start()
	{
		mytransform.localScale = Vector3.zero;
		targetColor = new Color(0.5f, 0.5f, 0.5f, 0f);
	}

	public void RotfogOn(float limit_height, float _scale_speed, int _rot_speed, int _fogalpha, float _xyratio)
	{
		base.gameObject.active = true;
		mytransform.localScale = Vector3.one * 0.3f;
		fogheight = limit_height;
		fogspeed = _scale_speed;
		fogrotation = _rot_speed;
		fogalpha = _fogalpha;
		xyratio = _xyratio;
		mymaterial.SetColor("_TintColor", Color.gray);
		plusV = new Vector3(fogspeed, fogspeed * xyratio, fogspeed);
	}

	private void Update()
	{
		currentColor = mymaterial.GetColor("_TintColor");
		if (mytransform.localScale.y > fogheight)
		{
			base.gameObject.active = false;
		}
		else if (mytransform.localScale.y > fogheight * 0.5f)
		{
			mytransform.localScale += plusV * Time.deltaTime;
			transColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * (float)fogalpha);
			mymaterial.SetColor("_TintColor", transColor);
		}
		else
		{
			mytransform.localScale += plusV * Time.deltaTime;
			mytransform.eulerAngles += Vector3.up * fogrotation * Time.deltaTime;
		}
	}
}
