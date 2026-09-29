using UnityEngine;

public class Map_extreme : MonoBehaviour
{
	public Transform trap;

	public void SetWave(int _wave)
	{
		if (trap != null)
		{
			trap.SendMessage("SetDamage", _wave * 5);
		}
	}

	public void TrapStop()
	{
		if (trap != null)
		{
			trap.SendMessage("StopActive");
		}
	}
}
