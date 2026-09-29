using UnityEngine;

public class Swordwind_draw : MonoBehaviour
{
	private float current_delay;

	public float startdelay;

	private Renderer myrenderer;

	private bool show;

	private void Awake()
	{
		myrenderer = base.GetComponent<Renderer>();
	}

	private void Start()
	{
		myrenderer.enabled = false;
	}

	private void OnEnable()
	{
		show = false;
		current_delay = 0f;
		myrenderer.enabled = false;
	}

	private void Update()
	{
		current_delay += Time.deltaTime;
		if (!show && current_delay > startdelay)
		{
			myrenderer.enabled = true;
			show = true;
			current_delay = 0f;
		}
	}
}
