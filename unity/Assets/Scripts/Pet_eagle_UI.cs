using UnityEngine;

public class Pet_eagle_UI : MonoBehaviour
{
	private void Start()
	{
		base.GetComponent<Animation>()["fly_glide"].layer = 1;
		base.GetComponent<Animation>()["fly_flap"].layer = 1;
		base.GetComponent<Animation>()["fly_glide"].speed = 0.4f;
		base.GetComponent<Animation>()["fly_flap"].speed = 0.4f;
		base.GetComponent<Animation>().Play("fly_flap");
	}

	private void Update()
	{
	}
}
