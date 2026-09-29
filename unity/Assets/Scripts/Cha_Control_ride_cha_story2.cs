using UnityEngine;

public class Cha_Control_ride_cha_story2 : MonoBehaviour
{
	private bool dooropen;

	private void Awake()
	{
	}

	private void Start()
	{
		base.GetComponent<Animation>().Stop();
		base.transform.position = new Vector3(0f, -1.33f, -0.03f);
		base.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
		base.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
		GameObject gameObject = GameObject.Find("cha1_riding");
		gameObject.GetComponent<Animation>()["push_door"].speed = 0.13f;
		gameObject.GetComponent<Animation>().Play("push_door");
		AnimationState animationState = gameObject.GetComponent<Animation>().CrossFadeQueued("walk_free");
		animationState.speed = 0.4f;
	}

	private void Update()
	{
		if (base.GetComponent<Animation>().IsPlaying("walk_free"))
		{
			base.transform.position -= new Vector3(0f, 0.05f, 1f) * Time.deltaTime * 0.3f;
		}
		else if (!dooropen)
		{
			GameObject.Find("door").GetComponent<Animation>()["open_door"].speed = 0.05f;
			dooropen = true;
		}
	}
}
