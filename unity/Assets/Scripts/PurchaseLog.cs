using System;
using System.IO;
using UnityEngine;

public static class PurchaseLog
{
	public static void FileCreat()
	{
		using StreamWriter streamWriter = new StreamWriter(Application.persistentDataPath + "/purchaselog.txt", append: true);
		streamWriter.WriteLine("\r\n");
	}

	public static void LogOn(string _log)
	{
		using StreamWriter streamWriter = new StreamWriter(Application.persistentDataPath + "/purchaselog.txt", append: true);
		streamWriter.WriteLine(DateTime.Now.ToString() + "\r\n\t\t" + _log + "\r\n");
		streamWriter.Close();
	}
}
