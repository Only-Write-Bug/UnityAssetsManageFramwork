using System;
using System.Collections.Generic;

public class CommonPath_Excel_Data
{
	public long ID;
	public string key;
	public string path;
}

public class CommonPath_excel
{
	private Dictionary<long, CommonPath_Excel_Data> _cacheData = new Dictionary<long, CommonPath_Excel_Data>();
}
