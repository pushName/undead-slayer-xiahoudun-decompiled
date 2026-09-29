using UnityEngine;

public class Ef_stepfog : MonoBehaviour
{
	public float fogheight;

	public float fogspeed;

	public int fogalpha;

	public float smoothfactor;

	public float xyratio;

	private float dt;

	private Renderer myrenderer;

	private Transform mytransform;

	private Vector3 growVector;

	private Vector3 smoothgrowVector;

	private Color currentColor;

	private Color targetColor;

	private Color transColor;

	private Vector3 originScale;

	private void Awake()
	{
		mytransform = base.transform;
		myrenderer = base.GetComponent<Renderer>();
	}

	private void Start()
	{
		originScale = mytransform.localScale;
		targetColor = new Color(0.5f, 0.5f, 0.5f, 0f);
		myrenderer.enabled = false;
		myrenderer.material.SetColor("_TintColor", Color.gray);
		growVector = new Vector3(fogspeed, fogspeed * xyratio, fogspeed);
		smoothgrowVector = new Vector3(fogspeed * smoothfactor, fogspeed * smoothfactor * 0.5f, fogspeed * smoothfactor);
		base.gameObject.active = false;
	}

	private void Update()
	{
		if (dt < 0.1f)
		{
			dt += Time.deltaTime;
		}
		else
		{
			myrenderer.enabled = true;
		}
		if (myrenderer.enabled)
		{
			currentColor = myrenderer.material.GetColor("_TintColor");
			transColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * (float)fogalpha);
			myrenderer.material.SetColor("_TintColor", transColor);
			if (mytransform.localScale.y > fogheight)
			{
				mytransform.position = Vector3.one * 4f;
				myrenderer.enabled = false;
				base.gameObject.active = false;
				dt = 0f;
				myrenderer.material.SetColor("_TintColor", Color.gray);
				mytransform.localScale = originScale;
			}
			else if (mytransform.localScale.y > fogheight * 0.8f)
			{
				mytransform.localScale += smoothgrowVector * Time.deltaTime;
			}
			else
			{
				mytransform.localScale += growVector * Time.deltaTime;
			}
		}
	}
}
