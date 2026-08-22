using System;
using ColossalFramework.UI;
using Commons.Utils;
using UnityEngine;

namespace TransportLinesManager.WorldInfoPanels.Components.TLMVehicleSelector
{
    internal static class LinePanelManager
    {
        // Instance references.
        private static GameObject s_gameObject;
        private static LinePanel s_panel;

        /// <summary>
        /// Gets the active panel instance.
        /// </summary>
        internal static LinePanel Panel => s_panel;

        /// <summary>
        /// Creates the panel object in-game and displays it.
        /// </summary>
        internal static void Create()
        {
            try
            {
                // If no instance already set, create one.
                if (s_gameObject == null)
                {
                    // Give it a unique name for easy finding with ModTools.
                    s_gameObject = new GameObject("TCBuildingInfoPanel");
                    s_gameObject.transform.parent = UIView.GetAView().transform;

                    // Add panel and set parent transform.
                    s_panel = s_gameObject.AddComponent<LinePanel>();

                    // Show panel.
                    Panel.Show();
                }
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("exception creating TCBuildingInfoPanel " + e.Message);
            }
        }

        /// <summary>
        /// Closes the panel by destroying the object (removing any ongoing UI overhead).
        /// </summary>
        internal static void Close()
        {
            if (s_panel != null)
            {
                GameObject.Destroy(s_panel);
                GameObject.Destroy(s_gameObject);

                s_panel = null;
                s_gameObject = null;
            }
        }

        /// <summary>
        /// Sets the target to the selected line, creating the panel if necessary.
        /// </summary>
        /// <param name="lineID">New line ID.</param>
        internal static void SetTarget(ushort lineID)
        {
            // If no existing panel, create it.
            if (Panel == null)
            {
                Create();
            }

            // Set the target.
            Panel.SetTarget(lineID);
        }

        /// <summary>
        /// Handles line info world target line changes.
        /// </summary>
        internal static void TargetChanged()
        {
            if (s_panel == null)
            {
                return;
            }

            if (!UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) || fromBuilding || lineId == 0)
            {
                Close();
                return;
            }

            SetTarget(lineId);
        }
    }
}
