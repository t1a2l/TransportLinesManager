using ColossalFramework;
using UnityEngine;

namespace TransportLinesManager.Utils
{
    public class SpawnDelayTicker : MonoBehaviour
    {
        private uint m_lastFrame;

        public void Start()
        {
            m_lastFrame = Singleton<SimulationManager>.instance.m_currentFrameIndex;
        }

        public void Update()
        {
            if (!SimulationManager.exists)
            {
                return;
            }

            uint currentFrame = Singleton<SimulationManager>.instance.m_currentFrameIndex;

            if (currentFrame == m_lastFrame)
            {
                return;
            }

            m_lastFrame = currentFrame;

            // Run once per simulation tick
            SpawnDelayUtils.ProcessPendingQueues(currentFrame);
        }
    }
}
