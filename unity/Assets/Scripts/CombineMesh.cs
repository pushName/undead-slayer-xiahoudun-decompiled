using UnityEngine;

public class CombineMesh : MonoBehaviour
{
	private MeshFilter meshFilter;

	private Transform mytransform;

	public Mesh splitmesh;

	private void Awake()
	{
		mytransform = base.transform;
		meshFilter = GetComponent<MeshFilter>();
	}

	public void CreatSplitMesh(Vector3 _pos, Vector3 _scale)
	{
		GameObject gameObject = new GameObject("split");
		Transform transform = gameObject.transform;
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		int num = Random.Range(0, 2);
		int num2 = Random.Range(0, 2);
		splitmesh.uv = new Vector2[4]
		{
			new Vector2(0f + (float)num * 0.5f, 0.5f + (float)num2 * 0.5f),
			new Vector2(0.5f + (float)num * 0.5f, 0.5f + (float)num2 * 0.5f),
			new Vector2(0f + (float)num * 0.5f, 0f + (float)num2 * 0.5f),
			new Vector2(0.5f + (float)num * 0.5f, 0f + (float)num2 * 0.5f)
		};
		meshFilter.mesh = splitmesh;
		transform.position = _pos;
		transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
		transform.localScale = Random.Range(0.7f, 1f) * _scale;
		transform.parent = mytransform;
		Merge(splitmesh, transform);
	}

	public void Merge(Mesh childmesh, Transform childtransform)
	{
		Transform transform = (Transform)Object.Instantiate(base.transform, base.transform.position, Quaternion.identity);
		CombineInstance[] array = new CombineInstance[2];
		array[0].mesh = transform.GetComponent<MeshFilter>().mesh;
		array[1].mesh = childmesh;
		array[1].transform = childtransform.localToWorldMatrix;
		meshFilter.mesh = new Mesh();
		meshFilter.mesh.CombineMeshes(array);
		base.transform.gameObject.active = true;
	}
}
