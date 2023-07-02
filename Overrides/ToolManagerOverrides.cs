using Klyte.TransportLinesManager.Utils;
using HarmonyLib;

namespace Klyte.TransportLinesManager.Overrides
{
    [HarmonyPatch(typeof(ToolManager))]
    public static class ToolManagerOverrides
    {
        [HarmonyPatch(typeof(ToolManager), "EndOverlayImpl")]
        [HarmonyPostfix]
        public static void AfterEndOverlayImpl(RenderManager.CameraInfo cameraInfo)
        {
            if (WorldInfoPanel.AnyWorldInfoPanelOpen() && (WorldInfoPanel.GetCurrentInstanceID().Building > 0 || WorldInfoPanel.GetCurrentInstanceID().Type == (InstanceType)TLMInstanceType.BuildingLines))
            {
                var buildingId = WorldInfoPanel.GetCurrentInstanceID().Type == (InstanceType)TLMInstanceType.BuildingLines ? WorldInfoPanel.GetCurrentInstanceID().Index >> 8 : WorldInfoPanel.GetCurrentInstanceID().Building;
                ref Building b = ref BuildingManager.instance.m_buildings.m_buffer[buildingId];
                var info = b.Info;
                if (info.m_buildingAI is TransportStationAI tsai)
                {
                    TransportLinesManagerMod.Controller.BuildingLines.RenderBuildingLines(cameraInfo, (ushort)buildingId);
                    TransportLinesManagerMod.Controller.BuildingLines.RenderPlatformStops(cameraInfo, (ushort)buildingId);
                }

            }
        }
    }
}