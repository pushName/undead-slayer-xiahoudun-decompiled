using UnityEngine;

public class ConvertSaveData : MonoBehaviour
{
	private const int MAXPET = 2;

	private const int MAXSKILL = 20;

	private const int MAXWORK = 2;

	private const int MAXGENERAL = 12;

	private const int MAXGENERALPOOL = 30;

	private const int MAXTREASURE = 24;

	private const int MAXSTAGE = 90;

	private int camfov;

	private int cashshopkind;

	private int n47;

	private int n11;

	private int n17;

	private int n24;

	private int n09;

	private int n03;

	private int n42;

	private int n06;

	private int n10;

	private int n12;

	private int n16;

	private int n14;

	private int n08;

	private int n21;

	private int n28;

	private int n26;

	private int n30;

	private int n32;

	private int n05;

	private int n44;

	private int n43;

	private int cur_general;

	private int n34;

	private int n31;

	private int n04;

	private int n35;

	private int n38;

	private int cur_stage_index;

	private int play_kind;

	private int n37;

	private int n46;

	private int generalsearch;

	private int caveplay;

	private int perfectplay;

	private int enemykill;

	private int grappling;

	private int exattack;

	private int death;

	private int resurrection;

	private int changelevel;

	private int storycutin;

	private int tutorial;

	private int cashing;

	private int[] n15 = new int[90];

	private int[] n18 = new int[20];

	private int[] n40 = new int[20];

	private int[] n20 = new int[6];

	private int[] n22 = new int[20];

	private int[] n23 = new int[2];

	private int[] n27 = new int[2];

	private int[] n25 = new int[2];

	private int[] n13 = new int[12];

	private int[] n33 = new int[12];

	private int[] n29 = new int[12];

	private int[] n36 = new int[6];

	private int[] skill_slot = new int[5];

	private int[] n41 = new int[26];

	private int[] n45 = new int[26];

	private int[] n19 = new int[26];

	private int[] n39 = new int[5];

	private int[] treasure = new int[24];

	private int[] staff = new int[30];

	private int[] archive = new int[76];

	private int[] bosskill = new int[12];

	private int[] skill_use = new int[20];

	private int[] pet_skill_use = new int[2];

	private void Start()
	{
	}

	public void LoadData()
	{
		camfov = Crypto.Load_int_key("camfov");
		cashshopkind = Crypto.Load_int_key("cashshopkind");
		n47 = Crypto.Load_int_key("n47");
		n11 = Crypto.Load_int_key("n11");
		n17 = Crypto.Load_int_key("n17");
		n24 = Crypto.Load_int_key("n24");
		n09 = Crypto.Load_int_key("n09");
		n03 = Crypto.Load_int_key("n03");
		n42 = Crypto.Load_int_key("n42");
		n06 = Crypto.Load_int_key("n06");
		n10 = Crypto.Load_int_key("n10");
		n12 = Crypto.Load_int_key("n12");
		n16 = Crypto.Load_int_key("n16");
		n14 = Crypto.Load_int_key("n14");
		n15 = PlayerPrefsX.GetIntArray("n15");
		n18 = PlayerPrefsX.GetIntArray("n18");
		n40 = PlayerPrefsX.GetIntArray("n40");
		n20 = PlayerPrefsX.GetIntArray("n20");
		n22 = PlayerPrefsX.GetIntArray("n22");
		n23 = PlayerPrefsX.GetIntArray("n23");
		n27 = PlayerPrefsX.GetIntArray("n27");
		n25 = PlayerPrefsX.GetIntArray("n25");
		n08 = Crypto.Load_int_key("n08");
		n21 = Crypto.Load_int_key("n21");
		n28 = Crypto.Load_int_key("n28");
		n26 = Crypto.Load_int_key("n26");
		n30 = Crypto.Load_int_key("n30");
		n32 = Crypto.Load_int_key("n32");
		n05 = Crypto.Load_int_key("n05");
		n44 = Crypto.Load_int_key("n44");
		n43 = Crypto.Load_int_key("n43");
		n13 = PlayerPrefsX.GetIntArray("n13");
		n33 = PlayerPrefsX.GetIntArray("n33");
		n29 = PlayerPrefsX.GetIntArray("n29");
		n36 = PlayerPrefsX.GetIntArray("n36");
		cur_general = Crypto.Load_int_key("cur_general");
		n34 = Crypto.Load_int_key("n34");
		n31 = Crypto.Load_int_key("n31");
		n04 = Crypto.Load_int_key("n04");
		n35 = Crypto.Load_int_key("n35");
		n38 = Crypto.Load_int_key("n38");
		cur_stage_index = Crypto.Load_int_key("cur_stage_index");
		play_kind = Crypto.Load_int_key("play_kind");
		n37 = Crypto.Load_int_key("n37");
		n46 = Crypto.Load_int_key("n46");
		generalsearch = Crypto.Load_int_key("generalsearch");
		caveplay = Crypto.Load_int_key("caveplay");
		perfectplay = Crypto.Load_int_key("perfectplay");
		skill_slot = PlayerPrefsX.GetIntArray("skill_slot");
		n41 = PlayerPrefsX.GetIntArray("n41");
		n45 = PlayerPrefsX.GetIntArray("n45");
		n19 = PlayerPrefsX.GetIntArray("n19");
		n39 = PlayerPrefsX.GetIntArray("n39");
		treasure = PlayerPrefsX.GetIntArray("treasure");
		staff = PlayerPrefsX.GetIntArray("staff");
		archive = PlayerPrefsX.GetIntArray("archive");
		bosskill = PlayerPrefsX.GetIntArray("bosskill");
		skill_use = PlayerPrefsX.GetIntArray("skill_use");
		pet_skill_use = PlayerPrefsX.GetIntArray("pet_skill_use");
		enemykill = Crypto.Load_int_key("enemykill");
		grappling = Crypto.Load_int_key("grappling");
		exattack = Crypto.Load_int_key("exattack");
		death = Crypto.Load_int_key("death");
		resurrection = Crypto.Load_int_key("resurrection");
		changelevel = Crypto.Load_int_key("changelevel");
		storycutin = Crypto.Load_int_key("storycutin");
		tutorial = Crypto.Load_int_key("tutorial");
		cashing = Crypto.Load_int_key("cashing");
	}

	public void ConvertData()
	{
		Crypto.Save_int_key("camfov", camfov);
		Crypto.Save_int_key("cashshopkind", cashshopkind);
		Crypto.Save_int_key("n47", n47);
		PlayerPrefs.SetInt("updateRank", 1);
		Crypto.Save_int_key("n11", n11);
		Crypto.Save_int_key("n17", n17);
		Crypto.Save_int_key("n24", n24);
		Crypto.Save_int_key("n09", n09);
		Crypto.Save_int_key("n03", n03);
		Crypto.Save_int_key("n42", n42);
		Crypto.Save_int_key("n06", n06);
		Crypto.Save_int_key("n10", n10);
		Crypto.Save_int_key("n12", n12);
		Crypto.Save_int_key("n16", n16);
		Crypto.Save_int_key("n14", n14);
		PlayerPrefsX.SetIntArray("n15", n15);
		PlayerPrefsX.SetIntArray("n18", n18);
		PlayerPrefsX.SetIntArray("n40", n40);
		PlayerPrefsX.SetIntArray("n20", n20);
		PlayerPrefsX.SetIntArray("n22", n22);
		PlayerPrefsX.SetIntArray("n23", n23);
		PlayerPrefsX.SetIntArray("n27", n27);
		PlayerPrefsX.SetIntArray("n25", n25);
		Crypto.Save_int_key("n08", n08);
		Crypto.Save_int_key("n21", n21);
		Crypto.Save_int_key("n28", n28);
		Crypto.Save_int_key("n26", n26);
		Crypto.Save_int_key("n30", n30);
		Crypto.Save_int_key("n32", n32);
		Crypto.Save_int_key("n05", n05);
		Crypto.Save_int_key("n44", n44);
		Crypto.Save_int_key("n43", n43);
		PlayerPrefsX.SetIntArray("n13", n13);
		PlayerPrefsX.SetIntArray("n33", n33);
		PlayerPrefsX.SetIntArray("n29", n29);
		PlayerPrefsX.SetIntArray("n36", n36);
		Crypto.Save_int_key("cur_general", cur_general);
		Crypto.Save_int_key("n34", n34);
		Crypto.Save_int_key("n31", n31);
		Crypto.Save_int_key("n04", n04);
		Crypto.Save_int_key("n35", n35);
		Crypto.Save_int_key("n38", n38);
		Crypto.Save_int_key("cur_stage_index", cur_stage_index);
		Crypto.Save_int_key("play_kind", play_kind);
		Crypto.Save_int_key("n37", n37);
		Crypto.Save_int_key("generalsearch", generalsearch);
		Crypto.Save_int_key("caveplay", caveplay);
		Crypto.Save_int_key("perfectplay", perfectplay);
		PlayerPrefsX.SetIntArray("skill_slot", skill_slot);
		PlayerPrefsX.SetIntArray("n41", n41);
		PlayerPrefsX.SetIntArray("n45", n45);
		PlayerPrefsX.SetIntArray("n19", n19);
		PlayerPrefsX.SetIntArray("n39", n39);
		PlayerPrefsX.SetIntArray("treasure", treasure);
		PlayerPrefsX.SetIntArray("staff", staff);
		PlayerPrefsX.SetIntArray("archive", archive);
		PlayerPrefsX.SetIntArray("bosskill", bosskill);
		PlayerPrefsX.SetIntArray("skill_use", skill_use);
		PlayerPrefsX.SetIntArray("pet_skill_use", pet_skill_use);
		Crypto.Save_int_key("enemykill", enemykill);
		Crypto.Save_int_key("grappling", grappling);
		Crypto.Save_int_key("exattack", exattack);
		Crypto.Save_int_key("death", death);
		Crypto.Save_int_key("resurrection", resurrection);
		Crypto.Save_int_key("changelevel", changelevel);
		Crypto.Save_int_key("storycutin", storycutin);
		Crypto.Save_int_key("tutorial", tutorial);
		Crypto.Save_int_key("cashing", cashing);
	}
}
