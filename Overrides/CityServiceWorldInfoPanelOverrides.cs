using ColossalFramework;
using Klyte.TransportLinesManager.CommonsWindow;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Data.Base;
using Klyte.TransportLinesManager.Data.DataContainers;
using Klyte.Commons.Extensions;

namespace Klyte.TransportLinesManager.Overrides
{
    [HarmonyPatch(typeof(CityServiceWorldInfoPanel))]
    public static class CityServiceWorldInfoPanelOverrides
    {
        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnLinesOverviewClicked")]
        [HarmonyPrefix]
        public static bool OnLinesOverviewClicked(CityServiceWorldInfoPanel __instance, ref InstanceID ___m_InstanceID)
        {
            if (___m_InstanceID.Type != InstanceType.Building || ___m_InstanceID.Building == 0)
            {
                return false;
            }
            ushort building = ___m_InstanceID.Building;
            BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].Info;
            if (info != null)
            {
                if(info.m_buildingAI is TransportStationAI stationAI)
				{
                    TLMPanel.Instance.OpenAt(TransportSystemDefinition.From(stationAI));
				}
                else if(info.m_buildingAI is DepotAI depotAI)
				{
                    TLMPanel.Instance.OpenAt(TransportSystemDefinition.From(depotAI));
				}
            }
            return false;
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "UpdateBindings")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TranspileUpdateBindingsCSWIP(IEnumerable<CodeInstruction> instructions)
        {
            var inst = new List<CodeInstruction>(instructions);
            MethodInfo CanAllowRegionalLines = typeof(CityServiceWorldInfoPanelOverrides).GetMethod("CanAllowVanillaRegionalLines", Patcher.allFlags);

            for (int i = 0; i < inst.Count - 1; i++)
            {
                if (inst[i + 1].opcode == OpCodes.Ldnull
                    && inst[i].opcode == OpCodes.Ldloc_S
                    && inst[i].operand is LocalBuilder lb
                    && lb.LocalIndex == 5
                    )
                {
                    inst.RemoveAt(i + 1);
                    inst.RemoveAt(i + 1);
                    inst.InsertRange(i + 1, new List<CodeInstruction> {
                        new CodeInstruction(OpCodes.Ldloc_0),
                        new CodeInstruction(OpCodes.Call, CanAllowRegionalLines),
                    });
                    break;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        private static bool CanAllowVanillaRegionalLines(TransportStationAI stationAI, ushort buildingId) => !(stationAI is null) && !TLMBuildingDataContainer.Instance.SafeGet(buildingId).TlmManagedRegionalLines;
    }
}
