using UnityEngine;

public class Bullet_runswing_b06 : MonoBehaviour
{
	public Transform ef_trail;

	private Collider mycollider;

	private float disable_delay;

	private float collideron_delay;

	private float current_time;

	private void Awake()
	{
		mycollider = base.GetComponent<Collider>();
		collideron_delay = 0.2f;
		disable_delay = 0.4f;
		base.gameObject.active = false;
	}

	private void OnEnable()
	{
		ef_trail.gameObject.active = true;
		mycollider.enabled = false;
		current_time = 0f;
	}

	private void Update()
	{
		current_time += Time.deltaTime;
		if (current_time > disable_delay)
		{
			base.gameObject.active = false;
			mycollider.enabled = false;
			current_time = 0f;
		}
		else if (current_time > collideron_delay)
		{
			mycollider.enabled = true;
		}
	}
}
