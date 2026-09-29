using UnityEngine;

public class RepeatDamage : MonoBehaviour
{
	private Collider mycollider;

	public float startdelay = 0.5f;

	public float repeatdelay = 0.2f;

	private void Awake()
	{
		mycollider = base.GetComponent<Collider>();
	}

	private void OnEnable()
	{
		InvokeRepeating("ColliderClick", startdelay, repeatdelay);
	}

	public void DamageCancel()
	{
		CancelInvoke();
	}

	public void ColliderClick()
	{
		mycollider.enabled = true;
	}
}
