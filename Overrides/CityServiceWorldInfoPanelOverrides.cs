using ColossalFramework;
using TransportLinesManager.CommonsWindow;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using Commons.Utils;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Data.DataContainers;
using Commons.Extensions;
using ColossalFramework.UI;
using TransportLinesManager.UI;
using UnityEngine;
using System.Linq;

namespace TransportLinesManager.Overrides
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
        public static IEnumerable<CodeInstruction> TranspileUpdateBindingsCSWIP(IEnumerable<CodeInstruction> instructions)
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
                    inst.InsertRange(i + 1, [
                        new(OpCodes.Ldloc_0),
                        new(OpCodes.Call, CanAllowRegionalLines),
                    ]);
                    break;
                }
            }
            LogUtils.PrintMethodIL(inst);
            return inst;
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        public static void UpdateBindingsPostfix()
        {
            // Currently selected building.
            ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;

            // Create spawn delay label if it isn't already set up.
            if (s_timerLabel == null)
            {
                // Get info panel.
                CityServiceWorldInfoPanel infoPanel = UIView.library.Get<CityServiceWorldInfoPanel>(typeof(CityServiceWorldInfoPanel).Name);

                // Get ParkButtons UIPanel.
                UIComponent wrapper = infoPanel?.Find("Wrapper");
                UIComponent mainSectionPanel = wrapper?.Find("MainSectionPanel");
                UIComponent mainBottom = mainSectionPanel?.Find("MainBottom");
                UIComponent buttonPanels = mainSectionPanel?.Find("ButtonPanels");
                UIComponent parkButtons = mainSectionPanel?.Find("ParkButtons");

                if (parkButtons != null)
                {
                    // Add timer countdown label.
                    s_timerLabel = parkButtons.AddUIComponent<TimerLabel>();
                    s_timerLabel.textScale = 0.75f;
                    s_timerLabel.textColor = new Color32(185, 221, 254, 255);
                    s_timerLabel.font = Resources.FindObjectsOfTypeAll<UIFont>().FirstOrDefault(f => f.name == "OpenSans-Regular");
                }
                else
                {
                    LogUtils.DoErrorLog("couldn't find CityServiceWorldInfoPanel components");
                    return;
                }
            }

            // Local references.
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            BuildingInfo buildingInfo = buildingBuffer[buildingID].Info;

            // Is this a depot building?
            DepotAI depotAI = buildingInfo.GetAI() as DepotAI;
            if (depotAI == null)
            {
                // Not a depot building - hide the label.
                s_timerLabel.Hide();
            }
            else
            {
                // Depot building - show the label.
                s_timerLabel.BuildingID = buildingID;
                s_timerLabel.Reason = depotAI.m_transportInfo.m_vehicleReason;
                s_timerLabel.Show();
            }
        }

        // Timer label reference.
        private static TimerLabel s_timerLabel;

        private static bool CanAllowVanillaRegionalLines(TransportStationAI stationAI, ushort buildingId) => stationAI is not null && !TLMBuildingDataContainer.Instance.SafeGet(buildingId).TlmManagedRegionalLines;
    }
}
