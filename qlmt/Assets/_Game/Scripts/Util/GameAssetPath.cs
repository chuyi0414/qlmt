using System.Collections;
using UnityEngine;

public static class GameAssetPath
{
    private const string EntityRoot = "Prefabs/Entity";
    private const string UIRoot = "Prefabs/UI";
    private const string DataTableRoot = "DataTables";

    public static string GetEntity(string relative)
    {
        return $"{EntityRoot}/{relative}";
    }

    public static string GetUI(string relative)
    {
        return $"{UIRoot}/{relative}";
    }

    public static string GetDataTable(string relative)
    {
        return $"{DataTableRoot}/{relative}";
    }
}
