using UnityEngine;

public class TrailParent : MonoBehaviour
{
	private Transform parent_cha;

	private Transform mytransform;

	private void Awake()
	{
		mytransform = base.transform;
		parent_cha = GameObject.FindWithTag("Player").transform;
	}

	private void Update()
	{
		mytransform.position = parent_cha.position;
	}
}
