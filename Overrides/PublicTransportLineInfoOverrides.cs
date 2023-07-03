using HarmonyLib;

namespace TransportLinesManager.Overrides
{
	[HarmonyPatch(typeof(PublicTransportLineInfo))]
	public static class PublicTransportLineInfoOverrides 
	{
		[HarmonyPatch(typeof(PublicTransportLineInfo), "RefreshData")]
        [HarmonyPrefix]
        public static bool RefreshData(bool colors, bool visible)
		{
            return false;
		}
	}
}
