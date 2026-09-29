using UnityEngine;

public class Inventory_Gem : MonoBehaviour
{
	public Texture2D[] jeweltex = new Texture2D[5];

	private Texture2D getitem_tex;

	public GUIStyle bt_yesno;

	public Texture2D bg_material;

	public Texture2D icon_key;

	public GUISkin basicSkin;

	private int getitem_grade;

	private short getitem;

	private int cur_stage_index;

	private int language;

	private int posX = 176;

	private int posY = 94;

	private UI_result script_ui;

	private void Awake()
	{
		language = PlayerPrefs.GetInt("language");
		script_ui = GameObject.FindWithTag("ui").GetComponent<UI_result>();
		cur_stage_index = Crypto.Load_int_key("cur_stage_index");
	}

	public void GetItem()
	{
		int max_stage_index = script_ui.max_stage_index;
		int num = 0;
		if (cur_stage_index == max_stage_index && max_stage_index == 12)
		{
			num = 12;
		}
		else if (max_stage_index > 12)
		{
			num = Random.Range(0, 13);
		}
		else if (max_stage_index >= 3)
		{
			num = Random.Range(0, 10);
		}
		if (num < 3)
		{
			getitem = 2;
			int[] intArray = PlayerPrefsX.GetIntArray("treasure");
			int num2 = Random.Range(1, 25);
			getitem_tex = Resources.Load("treasure" + num2) as Texture2D;
			intArray[num2 - 1]++;
			PlayerPrefsX.SetIntArray("treasure", intArray);
		}
		else if (num < 10)
		{
			getitem = 1;
			int[] intArray2 = PlayerPrefsX.GetIntArray("n39");
			int b = Random.Range(0, 625) - cur_stage_index * 5;
			b = Mathf.Max(0, b);
			float f = Mathf.Sqrt(b);
			getitem_grade = 4 - (int)Mathf.Sqrt(f);
			getitem_tex = jeweltex[getitem_grade];
			intArray2[getitem_grade]++;
			PlayerPrefsX.SetIntArray("n39", intArray2);
			script_ui.GetGem();
		}
		else
		{
			getitem = 3;
			getitem_tex = icon_key;
			int num3 = Crypto.Load_int_key("n10");
			num3++;
			Crypto.Save_int_key("n10", num3);
		}
		script_ui.OpenShop();
	}

	private void OnGUI()
	{
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 960f, (float)Screen.height / 640f, 1f));
		GUI.skin = basicSkin;
		GUI.depth = -5;
		if (getitem > 0)
		{
			GUI.DrawTexture(Crypto.Rect2(posX, posY, 128f, 128f), bg_material);
			GUI.DrawTexture(Crypto.Rect2(posX + 48, posY + 46, 32f, 32f), getitem_tex);
			if (GUI.Button(Crypto.Rect2(posX + 32, posY + 92, 64f, 32f), Language.intxt[language, 45], bt_yesno))
			{
				script_ui.GoNext();
				getitem = 0;
			}
			if (getitem == 1)
			{
				GUI.Label(Crypto.Rect2(posX, posY + 10, 128f, 14f), Language.intxt[language, 253] + "  LV " + (getitem_grade + 1), "txt12_w");
			}
			else if (getitem == 2)
			{
				GUI.Label(Crypto.Rect2(posX, posY + 10, 128f, 14f), Language.intxt[language, 254], "txt12_w");
			}
			else if (getitem == 3)
			{
				GUI.Label(Crypto.Rect2(posX, posY + 10, 128f, 14f), Language.intxt[language, 255], "txt12_w");
			}
		}
	}
}
