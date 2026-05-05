using HarmonyLib;

namespace TransportLinesManager.Overrides
{
	[HarmonyPatch(typeof(PublicTransportLineInfo))]
	public static class PublicTransportLineInfoOverrides 
	{
		[HarmonyPatch(typeof(PublicTransportLineInfo), "RefreshData")]
        [HarmonyPrefix]
#pragma warning disable IDE0060 // Remove unused parameter
        public static bool RefreshData(bool colors, bool visible)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return false;
		}
	}
}
