using UnityEngine;

namespace TransportLinesManager.ModShared
{
    internal abstract class IBridgeWTS : MonoBehaviour
    {
        public abstract bool WtsAvailable { get; }
    }
}