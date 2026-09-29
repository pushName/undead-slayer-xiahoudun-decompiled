using UnityEngine;

public class SwordDance : MonoBehaviour
{
	public float uvspeed = 1f;

	public float hitrate = 0.2f;

	public float startdelay = 0.1f;

	private Material mymaterial;

	private Collider mycollider;

	private Vector2 offset;

	private void Awake()
	{
		mymaterial = base.GetComponent<Renderer>().sharedMaterial;
		mycollider = base.GetComponent<Collider>();
	}

	private void OnEnable()
	{
		offset = Vector2.zero;
		mymaterial.mainTextureOffset = Vector2.zero;
		InvokeRepeating("DanceHit", startdelay, hitrate);
	}

	private void DanceHit()
	{
		mycollider.enabled = false;
		mycollider.enabled = true;
	}

	private void Update()
	{
		offset += Vector2.right * Time.deltaTime * uvspeed;
		if (offset.x >= 0.9f)
		{
			CancelInvoke();
			mycollider.enabled = false;
			base.transform.position = Vector3.one * 38f;
			base.gameObject.active = false;
		}
		else
		{
			mymaterial.mainTextureOffset = offset;
		}
	}
}
