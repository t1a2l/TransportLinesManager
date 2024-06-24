using UnityEngine;

namespace TransportLinesManager.ModShared
{
    internal abstract class IBridgeWE : MonoBehaviour
    {
        public abstract bool WeAvailable { get; }
    }
}