using ColossalFramework;
using TransportLinesManager.CommonsWindow;
using HarmonyLib;
using TransportLinesManager.Data.Tsd;

namespace TransportLinesManager.Overrides
{
    [HarmonyPatch(typeof(PublicTransportVehicleWorldInfoPanel))]
    public class PublicTransportVehicleWorldInfoPanelOverrides
    {

        [HarmonyPatch(typeof(PublicTransportVehicleWorldInfoPanel), "OnLinesOverviewClicked")]
        [HarmonyPrefix]
#pragma warning disable IDE0060 // Remove unused parameter
        public static bool OnGoToLines(PublicTransportVehicleWorldInfoPanel __instance, ref InstanceID ___m_InstanceID)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            if (___m_InstanceID.Type != InstanceType.Vehicle || ___m_InstanceID.Vehicle == 0)
            {
                return false;
            }
            ushort vehicle = ___m_InstanceID.Vehicle;
            ushort firstVehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehicle].GetFirstVehicle(vehicle);
            if (firstVehicle != 0)
            {
                VehicleInfo info = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[firstVehicle].Info;
                if (info != null)
                {
                    TLMPanel.Instance.OpenAt(TransportSystemDefinition.From(info));
                }
            }
            return false;
        }

    }
}
