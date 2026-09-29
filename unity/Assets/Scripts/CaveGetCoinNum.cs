using UnityEngine;

public class CaveGetCoinNum : MonoBehaviour
{
	private Transform mytransform;

	private float imgnum;

	private int oldnum;

	private int targetcoin;

	private TextMesh tt;

	private float finishdelay;

	private bool movefisnish;

	private void Awake()
	{
		mytransform = base.transform;
		tt = GetComponent<TextMesh>();
	}

	private void Start()
	{
		base.gameObject.active = false;
		mytransform.GetChild(0).gameObject.active = false;
	}

	public void GetOn(int _getcoin)
	{
		mytransform.position = new Vector3(0f, 6f, 1.5f);
		base.gameObject.active = true;
		targetcoin = _getcoin;
		movefisnish = false;
		mytransform.GetChild(0).gameObject.active = true;
	}

	private void Update()
	{
		mytransform.GetChild(0).Rotate(Vector3.up * Time.deltaTime * 600f);
		if (movefisnish)
		{
			if (imgnum < (float)targetcoin)
			{
				imgnum += Time.deltaTime * 50f;
			}
			else
			{
				imgnum = targetcoin;
				finishdelay += Time.deltaTime;
				if (finishdelay > 2f)
				{
					finishdelay = 0f;
					base.gameObject.active = false;
					mytransform.GetChild(0).gameObject.active = false;
				}
			}
			if (imgnum > (float)oldnum)
			{
				oldnum = (int)imgnum;
				tt.text = oldnum.ToString();
			}
		}
		else if (mytransform.position.y > 2.5f)
		{
			mytransform.position -= Vector3.up * Time.deltaTime * 3f;
		}
		else
		{
			mytransform.position = new Vector3(0f, 2.5f, 1.5f);
			mytransform.GetChild(1).GetComponent<ParticleSystem>().Emit(1);
			movefisnish = true;
		}
	}
}
