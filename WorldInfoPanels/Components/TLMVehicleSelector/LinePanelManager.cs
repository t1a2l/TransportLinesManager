using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework.UI;
using Commons.Utils;
using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Data.Base.ConfigurationContainers;
using TransportLinesManager.Data.Base.ConfigurationContainers.OutsideConnections;
using TransportLinesManager.Data.Extensions;
using TransportLinesManager.Utils;
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
        /// Gets the Current Regional Connection.
        /// </summary>
        public static OutsideConnectionLineInfo CurrentRegionalConnection { get; private set; }

        public static bool IsRegionalConnectionTarget => CurrentRegionalConnection != null;

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
                    s_gameObject = new GameObject("TLMLinePanel");
                    s_gameObject.transform.parent = UIView.GetAView().transform;

                    // Add panel and set parent transform.
                    s_panel = s_gameObject.AddComponent<LinePanel>();

                    // Show panel.
                    Panel.Show();
                }
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("exception creating TLMLinePanel " + e.Message);
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
        /// <param name="lineId">the id of the line.</param>
        /// <param name="fromBuilding">true for regional connection otherwsie false.</param>
        internal static void SetTarget(ushort lineId, bool fromBuilding)
        {
            // If no existing panel, create it.
            if (Panel == null)
            {
                Create();
            }

            CurrentRegionalConnection = null;

            if (fromBuilding && !SetRegionalConnection(lineId))
            {
                Close();
                return;
            }

            // Set the target.
            Panel.SetTarget(lineId, fromBuilding);
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

            if (!UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) || (lineId == 0 && !fromBuilding))
            {
                Close();
                return;
            }

            SetTarget(lineId, fromBuilding);
        }

        private static bool SetRegionalConnection(ushort lineId)
        {
            ushort stationBuildingId = WorldInfoPanel.GetCurrentInstanceID().Building;
            var stationData = TransportLinesManagerMod.Controller.BuildingLines.SafeGet(stationBuildingId);

            if (stationData == null || !stationData.TryGetRegionalConnection(lineId, out _, out OutsideConnectionLineInfo connection))
            {
                LogUtils.DoErrorLog("Could not resolve regional vehicle target: station={0}; line={1}", stationBuildingId, lineId);
                return false;
            }

            LogUtils.DoLog("Regional vehicle target resolved: station={0}; line={1}; outsideNode={2}; virtualNode={3}",
               stationBuildingId,
               lineId,
               connection.m_nodeOutsideConnection,
               connection.m_nodeVirtual);

            CurrentRegionalConnection = connection;
            return true;
        }
    }
}
