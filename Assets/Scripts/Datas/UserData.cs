
using System;
using UnityEngine;

public static class UserData
{
    private static ES3File _data;

    public static ES3File data{get{
        _data = _data??new ES3File("userData.es3");
        ES3.CreateBackup("userData.es3");
        return _data;
    }}
    public static bool HasData{get{
        return ES3.FileExists("userData.es3");
    }}

    public static bool HasKey(string key)
    {
        return data.KeyExists(key);
    }

    public static void Save(string info="")
    {
        data.Sync();
        Debug.Log($"[SaveInfo] save at {DateTime.Now},{info}");
    }
}
