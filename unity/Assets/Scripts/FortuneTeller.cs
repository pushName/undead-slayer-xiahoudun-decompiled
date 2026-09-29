using UnityEngine;

public class FortuneTeller : MonoBehaviour
{
	private Transform mytransform;

	public Material main_material;

	public Transform ef_shuffle;

	private GameObject c_plane;

	private Renderer creatrender;

	private float creat_delay;

	private bool efon;

	private Vector3 targetVector;

	private void Awake()
	{
		mytransform = base.transform;
	}

	private void OnEnable()
	{
		ef_shuffle.gameObject.active = true;
		targetVector = new Vector3(0f, 0.2f, 0.5f);
		mytransform.position = targetVector - Vector3.up * 2f;
		if (c_plane != null)
		{
			Object.Destroy(c_plane);
		}
		creat_delay = 0f;
		efon = false;
	}

	public void FortuneMesh(int _atk, int _def, int _hp, int _rate)
	{
		c_plane = new GameObject("fortunemesh");
		MeshFilter meshFilter = c_plane.AddComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		c_plane.AddComponent<MeshRenderer>();
		mesh.vertices = new Vector3[4]
		{
			Vector3.up * _atk * 0.1f,
			Vector3.right * _def * 0.1f,
			-Vector3.right * _rate * 0.1f,
			-Vector3.up * _hp * 0.1f
		};
		mesh.uv = new Vector2[4]
		{
			Vector2.up,
			Vector2.one,
			Vector2.zero,
			Vector2.right
		};
		creatrender = c_plane.GetComponent<Renderer>();
		creatrender.enabled = false;
		creatrender.sharedMaterial = main_material;
		mesh.triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		mesh.RecalculateNormals();
		meshFilter.mesh = mesh;
		c_plane.transform.position = mytransform.position + Vector3.forward * 0.1f + Vector3.up * -0.32f;
		c_plane.transform.parent = mytransform;
		c_plane.layer = mytransform.gameObject.layer;
	}

	private void Update()
	{
		if (creat_delay > 3f)
		{
			if (creatrender != null)
			{
				creatrender.enabled = true;
			}
			ef_shuffle.gameObject.active = false;
			efon = true;
		}
		else if (!efon)
		{
			creat_delay += Time.deltaTime;
		}
		mytransform.position = Vector3.MoveTowards(mytransform.position, targetVector, Time.deltaTime * 5f);
	}
}
