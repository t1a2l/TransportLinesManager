using System.Collections;
using UnityEngine;
using HarmonyLib;

namespace TransportLinesManager.Overrides
{
    [HarmonyPatch]
    public class InstanceManagerOverrides
    {

        public delegate void OnBuildingNameChanged(ushort buildingID);
        public static event OnBuildingNameChanged EventOnBuildingRenamed;

        [HarmonyPatch(typeof(InstanceManager), "SetName")]
        [HarmonyPostfix]
        public static void OnInstanceRenamed(ref InstanceID id)
        {
            if (id.Building > 0)
            {
                CallBuildRenamedEvent(id.Building);
            }

        }

        public static void CallBuildRenamedEvent(ushort building) => BuildingManager.instance.StartCoroutine(CallBuildRenamedEvent_impl(building));
        
        private static IEnumerator CallBuildRenamedEvent_impl(ushort building)
        {
            yield return new WaitForSeconds(1);
            EventOnBuildingRenamed?.Invoke(building);
        }

    }
}