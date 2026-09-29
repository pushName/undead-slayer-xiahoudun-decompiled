using UnityEngine;

public class Bullet_jumpsplash : MonoBehaviour
{
	private Transform mytransform;

	public float postune;

	private Vector3 originScale = new Vector3(1f, 8f, 1f);

	private Vector3 growVector = new Vector3(0.8f, -2f, 0.8f);

	private Color currentColor;

	private Color transColor;

	private Color targetColor = new Color(0.5f, 0.5f, 0.5f, 0f);

	private Renderer myrenderer;

	private Collider mycollider;

	private void Awake()
	{
		mytransform = base.transform;
		myrenderer = base.GetComponent<Renderer>();
		mycollider = base.GetComponent<Collider>();
	}

	private void OnEnable()
	{
		mytransform.localScale = originScale;
		mytransform.position += mytransform.forward * postune;
		myrenderer.material.SetColor("_TintColor", Color.gray);
		mycollider.enabled = true;
	}

	private void Update()
	{
		mytransform.localScale += growVector * Time.deltaTime * 7f;
		if (mytransform.localScale.y < 1f)
		{
			base.gameObject.active = false;
		}
		else if (mytransform.localScale.y < 4f)
		{
			mycollider.enabled = false;
		}
		currentColor = myrenderer.material.GetColor("_TintColor");
		transColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * 5f);
		myrenderer.material.SetColor("_TintColor", transColor);
	}
}
