using UnityEngine;

public class Cha_Control_ride_cha_story1 : MonoBehaviour
{
	public Transform horseSpine;

	public Transform horse;

	private Transform mytransform;

	private Animation myanimation;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		mytransform = base.transform;
		myanimation = base.GetComponent<Animation>();
	}

	private void Start()
	{
		myanimation["ride2"].speed = 0.7f;
		myanimation.Play("ride2");
	}

	public void CrynStop()
	{
		myanimation.Stop();
		myanimation["ride2"].speed = 0.16f;
		myanimation["ride2"].wrapMode = WrapMode.Once;
		myanimation.Play("ride2");
		AnimationState animationState = myanimation.PlayQueued("getoff_horse");
		animationState.wrapMode = WrapMode.ClampForever;
		animationState.speed = 0.15f;
	}

	private void Update()
	{
		mytransform.position = horseSpine.position + Vector3.up * -0.085f;
		mytransform.rotation = horse.rotation;
	}
}
