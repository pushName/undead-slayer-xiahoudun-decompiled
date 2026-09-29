using UnityEngine;

public class Cha_Costume : MonoBehaviour
{
	private GameObject prevCostum;

	private GameObject curCostum;

	private int current_costume;

	private void Awake()
	{
		current_costume = Crypto.Load_int_key("n43");
	}

	private void Start()
	{
		if (Application.loadedLevelName == "Result")
		{
			int isclear = GameObject.Find("ui_result").GetComponent<UI_result>().isclear;
			if (isclear > 0)
			{
				base.GetComponent<Animation>()["costume"].speed = 0.24f;
				base.GetComponent<Animation>().Play("costume");
			}
			else
			{
				base.GetComponent<Animation>()["fail"].speed = 0.24f;
				base.GetComponent<Animation>().Play("fail");
				AnimationState animationState = base.GetComponent<Animation>().PlayQueued("fail_loop");
				animationState.speed = 0.2f;
			}
		}
		Costume(current_costume);
	}

	public void ResetCostume()
	{
		Costume(current_costume);
	}

	public void Costume(int _costume)
	{
		if (_costume + 1 < base.transform.childCount)
		{
			curCostum = base.transform.GetChild(_costume + 1).gameObject;
			if (prevCostum != null)
			{
				prevCostum.active = false;
			}
			curCostum.active = true;
			prevCostum = curCostum;
		}
	}

	public void OpenGiftBox()
	{
		base.GetComponent<Animation>()["open"].speed = 0.25f;
		base.GetComponent<Animation>()["open"].layer = 1;
		base.GetComponent<Animation>().Play("open");
		AnimationState animationState = base.GetComponent<Animation>().CrossFadeQueued("costume", 0.01f, QueueMode.CompleteOthers);
		animationState.speed = 0.24f;
		animationState.layer = 1;
	}

	public void OpenImpact()
	{
		GameObject.Find("giftbox").GetComponent<GiftBox>().OpenBox();
	}

	public void Disappear()
	{
		curCostum.active = false;
	}

	public void Appear()
	{
		curCostum.active = true;
	}
}
