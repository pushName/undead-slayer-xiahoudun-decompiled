using UnityEngine;

public class ImageNumber : MonoBehaviour
{
	private const int NUMBERSCALE = 3;

	private Vector2[] imagesetUV = new Vector2[11]
	{
		new Vector2(0f, 0.875f),
		new Vector2(0.125f, 0.875f),
		new Vector2(0.25f, 0.875f),
		new Vector2(0.375f, 0.875f),
		new Vector2(0.5f, 0.875f),
		new Vector2(0f, 0.75f),
		new Vector2(0.125f, 0.75f),
		new Vector2(0.25f, 0.75f),
		new Vector2(0.375f, 0.75f),
		new Vector2(0.5f, 0.75f),
		new Vector2(0.625f, 0.875f)
	};

	private Vector2 imagenumber;

	public int[] number = new int[3];

	private int finalnum;

	private short size_a;

	private Mesh thismesh;

	private void Start()
	{
	}

	public void ImageNum(int _num, int _digit, int _imagekind)
	{
		thismesh = GetComponent<MeshFilter>().mesh;
		size_a = 0;
		for (int i = 0; i < 3; i++)
		{
			number[i] = _num % 10;
			_num /= 10;
			if (_num != 0)
			{
				size_a++;
			}
		}
		finalnum = number[_digit];
		imagenumber = imagesetUV[finalnum];
		if ((_num != 0 || _digit != 0) && _digit >= size_a && finalnum == 0)
		{
			imagenumber = imagesetUV[10];
		}
		switch (_imagekind)
		{
		case 0:
			thismesh.uv = new Vector2[4]
			{
				imagenumber + Vector2.up * 0.125f,
				imagenumber + Vector2.one * 0.125f,
				imagenumber,
				imagenumber + Vector2.right * 0.125f
			};
			break;
		case 1:
			thismesh.uv = new Vector2[4]
			{
				imagenumber * 0.5f + Vector2.up * 0.0625f + Vector2.up * 0.25f,
				imagenumber * 0.5f + Vector2.one * 0.0625f + Vector2.up * 0.25f,
				imagenumber * 0.5f + Vector2.up * 0.25f,
				imagenumber * 0.5f + Vector2.right * 0.0625f + Vector2.up * 0.25f
			};
			break;
		}
	}
}
