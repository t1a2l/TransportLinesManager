using Commons.Utils;
using TransportLinesManager.Data.Tsd;
using System.Collections.Generic;

namespace TransportLinesManager.Utils
{
    public static class TLMPrefabUtils
    {

        internal static List<string> LoadBasicAssets(TransportSystemDefinition definition)
        {
            var basicAssetsList = new List<string>();
            LogUtils.DoLog("LoadBasicAssets: pre prefab read");
            for (uint i = 0; i < PrefabCollection<VehicleInfo>.PrefabCount(); i++)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(i);
                if (prefab != null && definition.IsFromSystem(prefab))
                {
                    basicAssetsList.Add(prefab.name);
                }
            }
            return basicAssetsList;
        }
        internal static List<string> LoadBasicAssetsIntercity(TransportSystemDefinition definition)
        {
            var basicAssetsList = new List<string>();
            if (definition.LevelIntercity is null)
            {
                return basicAssetsList;
            }
            LogUtils.DoLog("LoadBasicAssetsIntercity: pre prefab read");
            for (uint i = 0; i < PrefabCollection<VehicleInfo>.PrefabCount(); i++)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(i);
                if (prefab != null && definition.IsFromSystemIntercity(prefab))
                {
                    basicAssetsList.Add(prefab.name);
                }
            }
            return basicAssetsList;
        }
    }


}

