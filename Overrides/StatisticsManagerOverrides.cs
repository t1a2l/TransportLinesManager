using HarmonyLib;
using TransportLinesManager.Data.Managers;

namespace TransportLinesManager.Overrides
{
    [HarmonyPatch(typeof(StatisticsManager))]
    public static class StatisticsManagerOverrides
    {
        [HarmonyPatch(typeof(StatisticsManager), "SimulationStepImpl")]
        [HarmonyPostfix]
        public static void SimulationStepImpl(int subStep)
        {
            TLMTransportLineStatusesManager.SimulationStepImpl(subStep);
        }

        [HarmonyPatch(typeof(StatisticsManager), "UpdateData")]
        [HarmonyPostfix]
        public static void UpdateData(SimulationManager.UpdateMode mode)
        {
            TLMTransportLineStatusesManager.UpdateData(mode);
        }

    }
}