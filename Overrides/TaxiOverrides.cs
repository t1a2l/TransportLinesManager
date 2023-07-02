using HarmonyLib;

namespace Klyte.TransportLinesManager.Overrides
{
    [HarmonyPatch]
    public static class TaxiOverrides
    {

        [HarmonyPatch(typeof(TouristAI), "GetTaxiProbability")]
        [HarmonyPrefix]
        public static bool TouristAIGetTaxiProbability(ref CitizenInstance citizenData, ref int __result)
        {
            if (GameAreaManager.instance.PointOutOfArea(citizenData.GetLastFramePosition()))
            {
                __result = 0;
            }
            else
			{
                __result = 20;
			}
            return false;
        }

        [HarmonyPatch(typeof(ResidentAI), "GetTaxiProbability")]
        [HarmonyPrefix]
        public static bool ResidentAIGetTaxiProbability(ref CitizenInstance citizenData, ref int __result)
        {
            if (GameAreaManager.instance.PointOutOfArea(citizenData.GetLastFramePosition()))
            {
                __result = 0;
                return false;
            }
            return true;
        }

    }
}
