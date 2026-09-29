using UnityEngine;

public class MeshExchange : MonoBehaviour
{
	public Transform getcha;

	private SkinnedMeshRenderer myskin;

	private SkinnedMeshRenderer getskin;

	private Matrix4x4[] mybindPoses = new Matrix4x4[24];

	private void Start()
	{
		getskin = getcha.GetComponent<SkinnedMeshRenderer>();
		myskin = GetComponent<SkinnedMeshRenderer>();
		myskin.sharedMesh = getskin.sharedMesh;
		for (int i = 0; i < 24; i++)
		{
			ref Matrix4x4 reference = ref mybindPoses[i];
			reference = myskin.bones[i].worldToLocalMatrix * getcha.localToWorldMatrix;
		}
		myskin.sharedMesh.bindposes = mybindPoses;
		myskin.sharedMesh.boneWeights = getskin.sharedMesh.boneWeights;
		Debug.Log("renderer.sharedMesh.bindposes.Length = " + myskin.sharedMesh.bindposes.Length);
		Debug.Log("renderer.bones.Length = " + myskin.bones.Length);
	}

	private void Update()
	{
	}
}
