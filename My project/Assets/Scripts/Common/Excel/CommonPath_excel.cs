using System;
using System.Collections.Generic;
using Tools;
using Util;

public class CommonPath_Excel_Data : JsonUtil.JsonDataBase
{
	public long ID;
	public string key;
	public string path;
}

[CustomExcel]
public class CommonPath_Excel
{
	private static CommonPath_Excel _init = null;
	public static CommonPath_Excel init => _init ??= new CommonPath_Excel();

	private Dictionary<long, CommonPath_Excel_Data> _cacheData = new Dictionary<long, CommonPath_Excel_Data>();
	public void Load()
	{
		foreach (var data in ExcelReader.init.LoadExcelDataJson())
			_cacheData.TryAdd(data.Key, data.Value as CommonPath_Excel_Data);
	}

	public CommonPath_Excel_Data GetDataById(long id)
	{
		return _cacheData.GetValueOrDefault(id);
	}
}
