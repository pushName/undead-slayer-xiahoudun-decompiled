using UnityEngine;

public class MakeUI : MonoBehaviour
{
	public Material main_material;

	public Material mat_txtresult;

	private Transform skillicon_folder;

	public Transform peticon_folder;

	private void Start()
	{
		skillicon_folder = GameObject.FindWithTag("icon_skill").transform;
	}

	public Transform CreatCustomPlane(Vector2 _size, float _collidersize, Vector3 _pos, Vector2 _leftbottomUV, Vector2 _rightupUV, string _name, string _scriptname, float _skew, short skillicon)
	{
		GameObject gameObject = new GameObject(_name);
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		if (_collidersize > 0f)
		{
			SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
			sphereCollider.radius = _collidersize;
			sphereCollider.isTrigger = true;
		}
		gameObject.AddComponent<MeshRenderer>();
		mesh.vertices = new Vector3[4]
		{
			new Vector3(0f - _size.x + _skew, _size.y, 0f) * 0.5f,
			new Vector3(_size.x + _skew, _size.y, 0f) * 0.5f,
			new Vector3(0f - _size.x, 0f - _size.y, 0f) * 0.5f,
			new Vector3(_size.x, 0f - _size.y, 0f) * 0.5f
		};
		mesh.uv = new Vector2[4]
		{
			new Vector2(_leftbottomUV.x, _rightupUV.y),
			new Vector2(_rightupUV.x, _rightupUV.y),
			new Vector2(_leftbottomUV.x, _leftbottomUV.y),
			new Vector2(_rightupUV.x, _leftbottomUV.y)
		};
		Renderer renderer = gameObject.GetComponent<Renderer>();
		renderer.receiveShadows = false;
		renderer.castShadows = false;
		renderer.sharedMaterial = main_material;
		mesh.triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		mesh.RecalculateNormals();
		meshFilter.mesh = mesh;
		gameObject.transform.position += _pos;
		switch (skillicon)
		{
		case 0:
			gameObject.transform.parent = base.transform;
			break;
		case 1:
			gameObject.transform.parent = skillicon_folder;
			break;
		case 2:
			gameObject.transform.parent = peticon_folder;
			break;
		}
		gameObject.layer = base.gameObject.layer;
		if (_scriptname != null)
		{
			gameObject.AddComponent(System.Type.GetType(_scriptname));
		}
		return gameObject.transform;
	}

	public Transform Creat_result(Vector2 _size, Vector3 _pos, string _name)
	{
		GameObject gameObject = new GameObject(_name);
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		gameObject.AddComponent<MeshRenderer>();
		mesh.vertices = new Vector3[4]
		{
			new Vector3(0f - _size.x, _size.y, 0f) * 0.5f,
			new Vector3(_size.x, _size.y, 0f) * 0.5f,
			new Vector3(0f - _size.x, 0f - _size.y, 0f) * 0.5f,
			new Vector3(_size.x, 0f - _size.y, 0f) * 0.5f
		};
		mesh.uv = new Vector2[4]
		{
			Vector2.zero,
			Vector2.zero,
			Vector2.zero,
			Vector2.zero
		};
		Renderer renderer = gameObject.GetComponent<Renderer>();
		renderer.receiveShadows = false;
		renderer.castShadows = false;
		renderer.sharedMaterial = mat_txtresult;
		mesh.triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		mesh.RecalculateNormals();
		meshFilter.mesh = mesh;
		gameObject.transform.position += _pos;
		gameObject.transform.parent = base.transform;
		gameObject.layer = base.gameObject.layer;
		gameObject.AddComponent(typeof(Txt_result));
		return gameObject.transform;
	}

	public Transform Creat_star(Vector2 _size)
	{
		GameObject gameObject = new GameObject("star");
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		gameObject.AddComponent<MeshRenderer>();
		mesh.vertices = new Vector3[4]
		{
			new Vector3(0f - _size.x, _size.y, 0f) * 0.5f,
			new Vector3(_size.x, _size.y, 0f) * 0.5f,
			new Vector3(0f - _size.x, 0f - _size.y, 0f) * 0.5f,
			new Vector3(_size.x, 0f - _size.y, 0f) * 0.5f
		};
		mesh.uv = new Vector2[4]
		{
			Vector2.up * 0.25f,
			new Vector2(1f, 0.25f),
			Vector2.zero,
			Vector2.right
		};
		Renderer renderer = gameObject.GetComponent<Renderer>();
		renderer.receiveShadows = false;
		renderer.castShadows = false;
		renderer.sharedMaterial = mat_txtresult;
		mesh.triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
		mesh.RecalculateNormals();
		meshFilter.mesh = mesh;
		gameObject.transform.position += new Vector3(0f, 2.5f, 2f);
		gameObject.transform.parent = base.transform;
		gameObject.layer = base.gameObject.layer;
		gameObject.AddComponent(typeof(Txt_star));
		return gameObject.transform;
	}
}
