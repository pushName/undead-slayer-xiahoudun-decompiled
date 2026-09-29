using UnityEngine;

public class Icon_Stage : MonoBehaviour
{
	public int _index;

	public int _kind;

	public int _play;

	private string _name;

	private bool _isclear;

	public void IconDown()
	{
		GameObject.FindWithTag("ui").GetComponent<UI_map>().SetStage(_index, _kind, base.transform.position, _play);
	}
}
