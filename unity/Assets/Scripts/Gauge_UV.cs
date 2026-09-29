using UnityEngine;

public class Gauge_UV : MonoBehaviour
{
	private Mesh thismesh;

	private Vector2[] originUV = new Vector2[4];

	private void Awake()
	{
		thismesh = GetComponent<MeshFilter>().mesh;
		originUV = thismesh.uv;
	}

	public void UvMove(Vector2 _amount)
	{
		thismesh.uv = new Vector2[4]
		{
			originUV[0] + _amount,
			originUV[1] + _amount,
			originUV[2] + _amount,
			originUV[3] + _amount
		};
	}
}
