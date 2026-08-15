using System.Text;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using TransportLinesManager.Overrides;
using UnityEngine;

namespace TransportLinesManager.UI
{
    public sealed class TimerLabel : UILabel
    {
        // Private string references.
        private readonly string blockedString = Locale.Get("VSD_TIM_BLK");
        private readonly string hourString = Locale.Get("VSD_TIM_HR");
        private readonly string hoursString = Locale.Get("VSD_TIM_HRS");
        private readonly string minuteString = Locale.Get("VSD_TIM_MN");
        private readonly string minutesString = Locale.Get("VSD_TIM_MNS");

        // Target field.
        private ushort _buildingID;
        private ushort _lineId;
        private TransferManager.TransferReason _reason;

        /// <summary>
        /// Sets the target building ID.
        /// </summary>
        public ushort BuildingID { set => _buildingID = value; }

        /// <summary>
        /// Sets the target line ID.
        /// </summary>
        public ushort TransportLineID { set => _lineId = value; }

        /// <summary>
        /// Sets the countdown transfer reason.
        /// </summary>
        public TransferManager.TransferReason Reason { set => _reason = value; }

        /// <summary>
        /// Updates the label display.
        /// Called by game every update.
        /// </summary>
        public override void Update()
        {
            // Don't do anything if not visible.
            if (m_IsVisible)
            {
                // Calculate time delta.
                int timerValue = (int)(TLMDepotAIOverrides.GetSpawnTime(_buildingID, _lineId, _reason) - Singleton<SimulationManager>.instance.m_currentFrameIndex);

                // If time delta is less than zero, then clear display text.
                if (timerValue < 0)
                {
                    timerValue = 0;
                    this.text = string.Empty;
                }
                else
                {
                    // Set timer display text.
                    this.text = SetTimerLabel(timerValue);

                    // Set label positon based on current label dimensions.
                    this.relativePosition = new Vector2(this.parent.width - this.width, (this.parent.height - this.height) / 2f);
                }
            }

            base.Update();
        }

        /// <summary>
        /// Sets the text for the spawn timer.
        /// </summary>
        /// <param name="value">Fame count until spawning is next permitted.</param>
        private string SetTimerLabel(int value)
        {
            // Comvert frame count to hours per current SimulationManager settings.
            System.TimeSpan timespan = System.TimeSpan.FromHours(value / SimulationManager.DAYTIME_HOUR_TO_FRAME);

            // Format label to display hours and minutes.
            StringBuilder labelString = new(blockedString);
            labelString.Append(" ");
            labelString.Append(timespan.Hours);
            labelString.Append(" ");
            labelString.Append(timespan.Hours == 1 ? hourString : hoursString);
            labelString.Append(" ");
            labelString.Append(timespan.Minutes);
            labelString.Append(" ");
            labelString.Append(timespan.Minutes == 1 ? minuteString : minutesString);
            return labelString.ToString();
        }
    }
}
