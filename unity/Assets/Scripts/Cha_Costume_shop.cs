using UnityEngine;

public class Cha_Costume_shop : MonoBehaviour
{
	private Transform mytransform;

	private GameObject prevCostum;

	private GameObject curCostum;

	private int current_costume;

	private AnimationState tt;

	private void Awake()
	{
		mytransform = base.transform;
		current_costume = Crypto.Load_int_key("n43");
	}

	private void Start()
	{
		tt = base.GetComponent<Animation>()["costume"];
		int childCount = base.transform.childCount;
		for (int i = 1; i < childCount; i++)
		{
			mytransform.GetChild(i).gameObject.active = false;
		}
		Costume(current_costume);
	}

	private void Update()
	{
		tt.time = Time.realtimeSinceStartup * 0.24f;
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
}
