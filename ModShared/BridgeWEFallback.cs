namespace TransportLinesManager.ModShared
{
    internal class BridgeWEFallback : IBridgeWE
    {
        public override bool WeAvailable { get; } = false;
    }
}