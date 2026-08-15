using ColossalFramework.Globalization;

namespace TransportLinesManager.Data.Base.Enums
{
    public enum SpawnDelayScope : byte
    {
        Global = 0,
        Depot = 1,
        Line = 2,
    }

    public static class SpawnDelayScopeExtensions
    {
        public static string GetName(this SpawnDelayScope spawnDelayScope) => Locale.Get("TLM_SPAWN_DELAY_" + spawnDelayScope.ToString().ToUpper());
    }
}
