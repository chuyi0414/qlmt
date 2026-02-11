using System.Collections;
using UnityEngine;

public static class GameAssetPath
{
    private const string EntityRoot = "Prefabs/Entity";
    private const string UIRoot = "Prefabs/UI";

    public static string GetEntity(string relative)
    {
        return $"{EntityRoot}/{relative}";
    }

    public static string GetUI(string relative)
    {
        return $"{UIRoot}/{relative}";
    }
}