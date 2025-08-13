using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public static class ServerDataUtils
{
    #region promptLocs
    private static Dictionary<(int, int), List<string>> chunkStorageDic = new Dictionary<(int, int), List<string>>();
    public static Dictionary<(int, int), List<string>> ChunkStorageDic => chunkStorageDic;

    private static Dictionary<(int, int), bool> chunksReadyDic = new Dictionary<(int, int), bool>();
    public static Dictionary<(int, int), bool> ChunksReadyDic => chunksReadyDic;
    #endregion promptLocs
}
