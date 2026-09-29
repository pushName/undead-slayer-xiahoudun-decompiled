using UnityEngine;

public class Ef_swing1 : MonoBehaviour
{
	private int uvAnimationTileX;

	private int uvAnimationTileY;

	private int framesPerSecond = 20;

	private int index;

	private int oldindex = -1;

	private float starttime;

	private int lastframe;

	private bool efon;

	private float delay;

	private int impactframe = 1;

	private Rigidbody cha_rigidbody;

	public Transform pt_hit;

	private Cha_Control script_cha;

	private Vector2 size;

	private Vector2 offset;

	private float uIndex;

	private int vIndex;

	private int originlayer = 20;

	private Pt_hit script_pthit;

	private short layerindex;

	private bool layerchange;

	private bool pton;

	private short rndefamount;

	private int addforce;

	private Renderer myrenderer;

	private Collider mycollider;

	private void Awake()
	{
		myrenderer = base.GetComponent<Renderer>();
		mycollider = base.GetComponent<Collider>();
		originlayer = base.gameObject.layer;
	}

	private void Start()
	{
		GameObject gameObject = base.transform.root.gameObject;
		cha_rigidbody = gameObject.GetComponent<Rigidbody>();
		script_pthit = pt_hit.GetComponent<Pt_hit>();
		script_cha = gameObject.GetComponent<Cha_Control>();
		Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), mycollider);
		starttime = 0f;
		myrenderer.enabled = false;
		mycollider.isTrigger = true;
		mycollider.enabled = false;
		delay = 0f;
		base.gameObject.active = false;
	}

	public void SwingOn(float _delay, int cnt_x, int cnt_y, int uvspeed, int impact, int _addforce)
	{
		base.gameObject.layer = originlayer;
		base.gameObject.active = true;
		delay = _delay;
		efon = true;
		uvAnimationTileX = cnt_x;
		uvAnimationTileY = cnt_y;
		lastframe = uvAnimationTileX * uvAnimationTileY;
		framesPerSecond = uvspeed;
		impactframe = impact;
		addforce = _addforce;
		size = new Vector2(1f / (float)uvAnimationTileX, 1f / (float)uvAnimationTileY);
		if (layerchange)
		{
			int num = Random.Range(0, 100);
			if (num < rndefamount)
			{
				base.gameObject.layer = layerindex;
			}
		}
	}

	public void SwingOff()
	{
		efon = false;
		base.gameObject.active = false;
		myrenderer.enabled = false;
		mycollider.enabled = false;
		pton = false;
		oldindex = -1;
		index = 0;
		starttime = 0f;
	}

	public void RndEfOn(int _index, int _amount)
	{
		rndefamount = (short)_amount;
		switch (_amount)
		{
		case 2:
			rndefamount = 5;
			break;
		case 3:
			rndefamount = 10;
			break;
		case 4:
			rndefamount = 20;
			break;
		}
		layerchange = true;
		switch (_index)
		{
		case 0:
			layerindex = 20;
			layerchange = false;
			break;
		case 1:
			layerindex = 30;
			break;
		case 2:
			layerindex = 31;
			break;
		case 3:
			layerindex = 18;
			break;
		case 4:
			layerindex = 19;
			break;
		}
	}

	private void Update()
	{
		if (efon)
		{
			if (delay > 0f)
			{
				delay -= Time.deltaTime;
			}
			else
			{
				if (addforce > 0)
				{
					cha_rigidbody.AddForce(base.transform.root.forward * addforce);
				}
				myrenderer.enabled = true;
				starttime = 0f;
				efon = false;
				script_cha.SwingStart();
			}
		}
		if (!myrenderer.enabled)
		{
			return;
		}
		starttime += Time.deltaTime;
		index = (int)(starttime * (float)framesPerSecond);
		uIndex = index % uvAnimationTileX;
		vIndex = index / uvAnimationTileX;
		if (index == oldindex)
		{
			return;
		}
		if (index >= lastframe)
		{
			myrenderer.enabled = false;
			base.gameObject.active = false;
			mycollider.enabled = false;
			pton = false;
			oldindex = -1;
		}
		else if (index == impactframe || index == impactframe + 1)
		{
			mycollider.enabled = true;
			if (!pton)
			{
				script_pthit.ParticleOn();
				pton = true;
			}
		}
		else
		{
			mycollider.enabled = false;
		}
		offset = Vector2.right * uIndex * size.x + Vector2.up * (1f - size.y - (float)vIndex * size.y);
		myrenderer.material.mainTextureOffset = offset;
		myrenderer.material.mainTextureScale = size;
		oldindex = index;
	}
}
