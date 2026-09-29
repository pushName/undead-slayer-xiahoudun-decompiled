using UnityEngine;

public class Hp_bar : MonoBehaviour
{
	private Mesh thismesh;

	private Vector2[] originUV = new Vector2[4];

	private Transform mytransform;

	private Transform parentmon;

	private float _amount;

	private Vector2 amountU;

	private Vector2 amuontV;

	private float posY;

	private int oldstatus;

	private void Awake()
	{
		mytransform = base.transform;
		thismesh = GetComponent<MeshFilter>().mesh;
		originUV = thismesh.uv;
		amuontV = Vector2.up * 0.25f;
	}

	public void Damaged(int _maxhp, int _hp, Transform _parent, float _height, int _status)
	{
		parentmon = _parent;
		if (_maxhp != 0)
		{
			_amount = (1f - (float)_hp / (float)_maxhp) * 0.5f;
			amountU = Vector2.right * _amount;
			if (_status == -1)
			{
				_status = oldstatus;
			}
			thismesh.uv = new Vector2[4]
			{
				originUV[0] + amountU + amuontV * _status,
				originUV[1] + amountU + amuontV * _status,
				originUV[2] + amountU + amuontV * _status,
				originUV[3] + amountU + amuontV * _status
			};
		}
		posY = _height;
		oldstatus = _status;
	}

	public void FreeSelect()
	{
		mytransform.position = Vector3.one * 4f;
		parentmon = null;
	}

	private void Update()
	{
		if (parentmon != null)
		{
			mytransform.position = parentmon.position + new Vector3(0f, posY, -0.02f);
		}
	}
}
