using UnityEngine;

public class Ef_energy_gather : MonoBehaviour
{
	public Texture[] eftex = new Texture[2];

	private int fogalpha = 2;

	private float dt;

	private float finish_delay = 1.2f;

	private float show_delay = 0.1f;

	private float plustimescale;

	private Material mymaterial;

	private Transform mytransform;

	private bool anistart;

	private Vector3 growVector;

	private Vector3 smoothgrowVector;

	private Vector2 offset;

	private Color currentColor;

	private Color targetColor;

	private Color transColor;

	private Color whiteclear = new Color(0.5f, 0.5f, 0.5f, 0f);

	private void Awake()
	{
		mytransform = base.transform;
		mymaterial = base.GetComponent<Renderer>().sharedMaterial;
	}

	private void Start()
	{
		targetColor = Color.gray;
		base.gameObject.active = false;
		offset = Vector2.zero;
		mymaterial.SetColor("_TintColor", whiteclear);
	}

	public void SetTex(int _index, float _finishdelay)
	{
		mymaterial.mainTexture = eftex[_index];
		finish_delay = _finishdelay;
	}

	private void Update()
	{
		if (dt > finish_delay)
		{
			mytransform.position = Vector3.one * 4f;
			base.gameObject.active = false;
			dt = 0f;
			mymaterial.SetColor("_TintColor", whiteclear);
			offset = Vector2.zero;
			targetColor = Color.gray;
			anistart = false;
		}
		else if (dt > finish_delay - 0.4f)
		{
			targetColor = Color.clear;
		}
		else if (dt > show_delay)
		{
			anistart = true;
		}
		dt += Time.deltaTime;
		if (anistart)
		{
			plustimescale = (float)fogalpha / Time.timeScale;
			currentColor = mymaterial.GetColor("_TintColor");
			transColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * plustimescale);
			mymaterial.SetColor("_TintColor", transColor);
			offset -= Vector2.up * Time.deltaTime * plustimescale * 0.5f;
			mymaterial.SetTextureOffset("_MainTex", offset);
		}
	}
}
