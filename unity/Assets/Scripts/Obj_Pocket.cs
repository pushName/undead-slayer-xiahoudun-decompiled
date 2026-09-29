using UnityEngine;

public class Obj_Pocket : MonoBehaviour
{
	private Transform mytransform;

	private Transform hand;

	private bool pick;

	private void Awake()
	{
		mytransform = base.transform;
		mytransform.localScale = Vector3.zero;
	}

	public void PickPocket()
	{
		hand = GameObject.Find("Bip01 L Finger0").transform;
		pick = true;
		mytransform.position = hand.position;
		mytransform.parent = hand;
	}

	private void Update()
	{
		if (pick)
		{
			mytransform.localScale = Vector3.Lerp(mytransform.localScale, Vector3.one * 0.6f, Time.deltaTime * 7f);
			mytransform.up = Vector3.up;
		}
	}
}
