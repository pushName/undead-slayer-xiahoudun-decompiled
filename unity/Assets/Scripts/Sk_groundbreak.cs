using UnityEngine;

public class Sk_groundbreak : MonoBehaviour
{
	private void OnEnable()
	{
		base.GetComponent<Animation>().Play();
		base.GetComponent<Collider>().enabled = true;
		Invoke("ColliderDisable", 0.4f);
		Invoke("GameobjectDisable", 2f);
	}

	private void ColliderDisable()
	{
		base.GetComponent<Collider>().enabled = false;
	}

	private void GameobjectDisable()
	{
		base.transform.position = Vector3.one * 30f;
		base.gameObject.active = false;
		base.GetComponent<Animation>().Stop();
	}
}
