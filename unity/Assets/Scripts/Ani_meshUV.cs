using UnityEngine;

public class Ani_meshUV : MonoBehaviour
{
	private Mesh thismesh;

	private Vector2[] tempUV = new Vector2[4];

	private int uvAnimationTileX = 5;

	private int uvAnimationTileY = 5;

	private int framesPerSecond = 20;

	private int index;

	private int oldindex = -1;

	private int impact = 18;

	private float starttime;

	private int length = 14;

	private bool loop = true;

	private bool dead;

	private bool attack;

	private bool damaged;

	private bool visible;

	private bool loopattack;

	private Vector2 size;

	private Vector2 offset;

	private float uIndex;

	private int vIndex;

	private int startframe;

	private Transform mytransform;

	private Renderer myrenderer;

	private Material mymaterial;

	private Collider mycollider;

	private void Awake()
	{
		thismesh = GetComponent<MeshFilter>().mesh;
		starttime = 0f;
		mytransform = base.transform;
		mycollider = base.GetComponent<Collider>();
		myrenderer = base.GetComponent<Renderer>();
		size = new Vector2(1f / (float)uvAnimationTileX, 1f / (float)uvAnimationTileY);
	}

	public void Damaged(Vector3 _hitdir)
	{
		if (!damaged)
		{
			index = 0;
			starttime = 0f;
			startframe = 22;
			length = 3;
			loop = false;
			damaged = true;
			attack = false;
			framesPerSecond = 10;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!damaged && !attack && other.gameObject.layer == 8)
		{
			index = 0;
			starttime = 0f;
			startframe = 14;
			length = 9;
			loop = false;
			attack = true;
			framesPerSecond = 12;
			mycollider.enabled = false;
		}
	}

	public void ColliderOn()
	{
		mycollider.enabled = true;
	}

	private void Update()
	{
		if (dead)
		{
			return;
		}
		visible = myrenderer.isVisible;
		if (!attack && !damaged)
		{
			if (mytransform.position.z < 9.4f)
			{
				mytransform.position += Vector3.forward * Time.deltaTime * 0.2f;
			}
			else if (!loopattack)
			{
				startframe = 14;
				length = 9;
				loop = false;
				attack = true;
				framesPerSecond = 12;
			}
			if (!visible)
			{
				return;
			}
		}
		starttime += Time.deltaTime;
		index = (int)(starttime * (float)framesPerSecond);
		if (loop)
		{
			index = index % length + startframe;
		}
		else
		{
			index += startframe;
		}
		uIndex = index % uvAnimationTileX;
		vIndex = uvAnimationTileY - 1 - index / uvAnimationTileX;
		if (index == oldindex)
		{
			return;
		}
		if (index >= startframe + length)
		{
			if (attack)
			{
				index = 0;
				starttime = 0f;
				startframe = 0;
				oldindex = -1;
				length = 14;
				loop = true;
				attack = false;
				framesPerSecond = 20;
				base.gameObject.layer = 15;
				Invoke("ColliderOn", 0.5f);
			}
			else if (damaged)
			{
				index = startframe + length - 1;
				dead = true;
				Object.Destroy(base.gameObject, 2f);
			}
			mycollider.enabled = false;
			uIndex = index % uvAnimationTileX;
			vIndex = uvAnimationTileY - 1 - index / uvAnimationTileX;
		}
		if (index == impact)
		{
			mycollider.enabled = true;
			base.gameObject.layer = 27;
		}
		if (visible)
		{
			offset = Vector2.right * uIndex * size.x + Vector2.up * vIndex * size.y;
			ref Vector2 reference = ref tempUV[2];
			reference = offset + Vector2.up * size.y;
			ref Vector2 reference2 = ref tempUV[3];
			reference2 = offset + Vector2.up * size.y + Vector2.right * size.x;
			ref Vector2 reference3 = ref tempUV[0];
			reference3 = offset;
			ref Vector2 reference4 = ref tempUV[1];
			reference4 = offset + Vector2.right * size.x;
			thismesh.uv = tempUV;
		}
		oldindex = index;
	}
}
