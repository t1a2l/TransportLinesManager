using Commons.Utils;
using TransportLinesManager.Data.Tsd;
using System.Collections.Generic;

namespace TransportLinesManager.Utils
{
    public static class TLMPrefabUtils
    {

        internal static List<TransportAsset> LoadBasicAssets(TransportSystemDefinition definition)
        {
            var basicAssetsList = new List<TransportAsset>();
            LogUtils.DoLog("LoadBasicAssets: pre prefab read");
            for (uint i = 0; i < PrefabCollection<VehicleInfo>.PrefabCount(); i++)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(i);
                if (prefab != null && definition.IsFromSystem(prefab))
                {
                    var item = new TransportAsset
                    {
                        name = prefab.name,
                        spawn_percent = new Dictionary<int, int>(),
                        count = new Dictionary<int, Count>()
                    };
                    basicAssetsList.Add(item);
                }
            }
            return basicAssetsList;
        }
        internal static List<TransportAsset> LoadBasicAssetsIntercity(TransportSystemDefinition definition)
        {
            var basicAssetsList = new List<TransportAsset>();
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
                    var item = new TransportAsset
                    {
                        name = prefab.name,
                        spawn_percent = new Dictionary<int, int>(),
                        count = new Dictionary<int, Count>()
                    };
                    basicAssetsList.Add(item);
                }
            }
            return basicAssetsList;
        }
    }


}

