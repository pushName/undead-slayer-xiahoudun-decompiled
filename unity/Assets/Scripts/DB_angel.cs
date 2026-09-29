using UnityEngine;

public class DB_angel : MonoBehaviour
{
	public angelkind[] ag = new angelkind[8];

	private void Awake()
	{
		ag[0]._name = 436;
		ag[0]._info = 446;
		ag[0]._firerate = 3f;
		ag[0]._speed = 0.3f;
		ag[0]._arrowkind = 25;
		ag[0]._splashEF = 0;
		ag[1]._name = 437;
		ag[1]._info = 447;
		ag[1]._firerate = 3f;
		ag[1]._speed = 0.32f;
		ag[1]._arrowkind = 30;
		ag[1]._splashEF = 2;
		ag[2]._name = 438;
		ag[2]._info = 448;
		ag[2]._firerate = 5f;
		ag[2]._speed = 0.34f;
		ag[2]._arrowkind = 23;
		ag[2]._splashEF = 3;
		ag[3]._name = 439;
		ag[3]._info = 449;
		ag[3]._firerate = 4f;
		ag[3]._speed = 0.36f;
		ag[3]._arrowkind = 31;
		ag[3]._splashEF = 4;
		ag[4]._name = 440;
		ag[4]._info = 450;
		ag[4]._firerate = 3.5f;
		ag[4]._speed = 0.38f;
		ag[4]._arrowkind = 31;
		ag[4]._splashEF = 5;
		ag[5]._name = 441;
		ag[5]._info = 451;
		ag[5]._firerate = 3.4f;
		ag[5]._speed = 0.4f;
		ag[5]._arrowkind = 25;
		ag[5]._splashEF = 6;
		ag[6]._name = 442;
		ag[6]._info = 452;
		ag[6]._firerate = 2.2f;
		ag[6]._speed = 0.6f;
		ag[6]._arrowkind = 25;
		ag[6]._splashEF = 0;
		ag[7]._name = 443;
		ag[7]._info = 453;
		ag[7]._firerate = 3.2f;
		ag[7]._speed = 0.5f;
		ag[7]._arrowkind = 25;
		ag[7]._splashEF = 8;
	}
}
