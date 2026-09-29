using UnityEngine;

public class Login_again : MonoBehaviour
{
	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
	}

	private void OnApplicationPause(bool pocus)
	{
		if (pocus && !(Application.loadedLevelName != "Ready"))
		{
		}
	}
}
