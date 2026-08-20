using System.Text;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using TransportLinesManager.Utils;
using UnityEngine;

namespace TransportLinesManager.UI
{
    public sealed class TimerLabel : UILabel
    {
        // Private string references.
        private readonly string blockedString = Locale.Get("TLM_TIME_BLK");
        private readonly string hourString = Locale.Get("TLM_TIME_HOUR");
        private readonly string hoursString = Locale.Get("TLM_TIME_HOURS");
        private readonly string minuteString = Locale.Get("TLM_TIME_MINUTE");
        private readonly string minutesString = Locale.Get("TLM_TIME_MINUTES");

        // Target field.
        private ushort _buildingID;
        private TransferManager.TransferReason _reason;

        /// <summary>
        /// Sets the target building ID.
        /// </summary>
        public ushort BuildingID { set => _buildingID = value; }

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
                uint framesRemaining = SpawnDelayUtils.GetSpawnTime(_buildingID, _reason);

                // If time delta is less than zero, then clear display text.
                if (framesRemaining == 0)
                {
                    text = string.Empty;
                }
                else
                {
                    // Set timer display text.
                    text = SetTimerLabel((int)framesRemaining);

                    // Set label positon based on current label dimensions.
                    relativePosition = new Vector2(parent.width - width, (parent.height - height) / 2f);
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
