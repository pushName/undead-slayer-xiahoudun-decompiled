using UnityEngine;

public class Ef_split1 : MonoBehaviour
{
	private float destroydelay;

	private float alphadelay;

	private Mesh mymesh;

	private Color p_color = Color.white;

	private Transform mytransform;

	private Color[] temp = new Color[4];

	private void Awake()
	{
		mytransform = base.transform;
		mymesh = GetComponent<MeshFilter>().mesh;
	}

	private void Start()
	{
		base.gameObject.active = false;
	}

	public void FinishNow()
	{
		destroydelay = 5f;
	}

	private void Update()
	{
		destroydelay += Time.deltaTime;
		if (destroydelay > 4f)
		{
			mytransform.position = Vector3.one * 3f;
			p_color = Color.white;
			destroydelay = 0f;
			for (int i = 0; i < 4; i++)
			{
				ref Color reference = ref temp[i];
				reference = Color.white;
			}
			mymesh.colors = temp;
			base.gameObject.active = false;
		}
		else
		{
			if (!(destroydelay > 3f))
			{
				return;
			}
			p_color = Color.Lerp(p_color, Color.clear, Time.deltaTime * 5f);
			if (alphadelay > 0.1f)
			{
				for (int j = 0; j < 4; j++)
				{
					ref Color reference2 = ref temp[j];
					reference2 = p_color;
				}
				alphadelay = 0f;
				mymesh.colors = temp;
			}
			else
			{
				alphadelay += Time.deltaTime;
			}
		}
	}
}
