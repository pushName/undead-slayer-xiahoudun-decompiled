using UnityEngine;

public class ImageNumber_temp : MonoBehaviour
{
	private const int SIZE = 6;

	public Vector2[] imagenumber = new Vector2[6];

	public int[] number = new int[6];

	public Texture num;

	private int index;

	private int uvAnimationTileX = 5;

	private int uvAnimationTileY = 2;

	private short size_a;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public Vector2[] ImageNum(int a)
	{
		size_a = 0;
		for (int i = 0; i < 4; i++)
		{
			number[i] = a % 10;
			a /= 10;
			if (a != 0)
			{
				size_a++;
			}
		}
		for (int j = 0; j < size_a; j++)
		{
			index = number[j];
			Vector2 vector = new Vector2(1f / (float)uvAnimationTileX, 1f / (float)uvAnimationTileY);
			float num = index % uvAnimationTileX;
			int num2 = index / uvAnimationTileX;
			ref Vector2 reference = ref imagenumber[j];
			reference = new Vector2(num * vector.x, 1f - vector.y - (float)num2 * vector.y);
		}
		return imagenumber;
	}
}
