using UnityEngine;

public class Attribute_swing : MonoBehaviour
{
	public Material[] attribute_mat = new Material[5];

	public void ChangeAttribute(int _index, int _amount)
	{
		base.GetComponent<Renderer>().material = attribute_mat[_index];
		GetComponent<Ef_swing1>().RndEfOn(_index, _amount);
	}
}
