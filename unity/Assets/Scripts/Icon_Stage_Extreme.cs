using UnityEngine;

public class Icon_Stage_Extreme : MonoBehaviour
{
	public int _index;

	private string _name;

	private bool _isclear;

	public void IconDown()
	{
		GameObject.FindWithTag("ui").GetComponent<UI_Extreme>().SetStage(_index);
	}
}
