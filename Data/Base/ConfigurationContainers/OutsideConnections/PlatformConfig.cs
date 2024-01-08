﻿using Commons.Interfaces;
using Commons.Utils;
using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Utils;
using System;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using ColossalFramework;

namespace TransportLinesManager.Data.Base.ConfigurationContainers.OutsideConnections
{
    public class PlatformConfig : IIdentifiable
    {
        [XmlAttribute("passengerLaneId")]
        public long? Id { get; set; }

        [XmlIgnore]
        public uint PlatformLaneId => (uint)Id;
        [XmlAttribute("vehicleLaneId")]
        public uint VehicleLaneId { get; set; }

        [XmlElement("targetOutsideConnectionBuildings")]
        public NonSequentialList<OutsideConnectionLineInfo> TargetOutsideConnections { get; set; } = new NonSequentialList<OutsideConnectionLineInfo>();

        public void ReleaseNodes(ushort sourceBuilding)
        {
            if (SimulationManager.exists)
            {
                foreach (var val in TargetOutsideConnections.ToArray())
                {
                    ReleaseNodes(sourceBuilding, val.Value);
                    TargetOutsideConnections.Remove(val.Key);
                }
            }
        }
        public void ReleaseNodes(ushort sourceBuilding, OutsideConnectionLineInfo outsideConnection)
        {
            if (SimulationManager.exists)
            {
                var bm = BuildingManager.instance;
                var instance = NetManager.instance;
                ref Building data = ref bm.m_buildings.m_buffer[sourceBuilding];
                ushort num = 0;
                ushort num2 = data.m_netNode;
                int num3 = 0;
                while (num2 != 0)
                {
                    ushort nextBuildingNode = instance.m_nodes.m_buffer[num2].m_nextBuildingNode;
                    if (num2 == outsideConnection.m_nodeStation || num2 == outsideConnection.m_nodeOutsideConnection)
                    {
                        if (num != 0)
                        {
                            instance.m_nodes.m_buffer[num].m_nextBuildingNode = nextBuildingNode;
                        }
                        else
                        {
                            data.m_netNode = nextBuildingNode;
                        }
                        ReleaseLines(num2);
                        instance.ReleaseNode(num2);
                        num2 = num;
                    }
                    num = num2;
                    num2 = nextBuildingNode;
                    if (++num3 > 32768)
                    {
                        LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }
        }
        private void ReleaseLines(ushort node)
        {
            NetManager instance = NetManager.instance;
            for (int i = 0; i < 8; i++)
            {
                ushort segment = instance.m_nodes.m_buffer[node].GetSegment(i);
                if (segment != 0)
                {
                    instance.ReleaseSegment(segment, true);
                }
            }
        }
        public void UpdateStationNodes(ushort stationId)
        {
            var keys = TargetOutsideConnections.Keys.ToArray();
            foreach (var key in keys)
            {
                if (TargetOutsideConnections[key] is null && CreateConnectionLines(stationId, (ushort)key) is OutsideConnectionLineInfo conn)
                {
                    TargetOutsideConnections[key] = conn;
                }
                else
                {
                    TargetOutsideConnections.Remove(key);
                }
            }
        }

        public void AddDestination(ushort stationId, ushort outsideConnectionId, string name, Color clr)
        {
            if (!TargetOutsideConnections.ContainsKey(outsideConnectionId))
            {
                if (CreateConnectionLines(stationId, outsideConnectionId) is OutsideConnectionLineInfo conn)
                {
                    conn.Identifier = name;
                    conn.LineColor = clr;
                    TargetOutsideConnections[outsideConnectionId] = conn;
                }
            }
        }
        public void RemoveDestination(ushort stationId, ushort outsideConnectionId)
        {
            if (TargetOutsideConnections.ContainsKey(outsideConnectionId))
            {
                ReleaseNodes(stationId, TargetOutsideConnections[outsideConnectionId]);

                TargetOutsideConnections.Remove(outsideConnectionId);
            }
        }

        private Vector3 StationPlatformPosition => NetManager.instance.m_lanes.m_buffer[VehicleLaneId].m_bezier.Position(.5f);

        private OutsideConnectionLineInfo CreateConnectionLines(ushort stationId, ushort outsideConnectionId)
        {
            ref Building stationBuilding = ref BuildingManager.instance.m_buildings.m_buffer[stationId];
            ref Building outsideConnectionBuilding = ref BuildingManager.instance.m_buildings.m_buffer[outsideConnectionId];
            var outsideConnectionTSD = TransportSystemDefinition.FromOutsideConnection(outsideConnectionBuilding.Info.GetService(), outsideConnectionBuilding.Info.GetSubService(), outsideConnectionBuilding.Info.GetClassLevel(), VehicleInfo.VehicleType.None);
            if ((stationBuilding.Info.m_buildingAI is TransportStationAI) && (outsideConnectionBuilding.m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.None && outsideConnectionTSD != null)
            {
                var stationPlatformPosition = StationPlatformPosition;
                var result = new OutsideConnectionLineInfo();
                NetManager instance = NetManager.instance;
                if (outsideConnectionTSD.CreateConnectionNode(out result.m_nodeStation, stationPlatformPosition))
                {
                    if ((stationBuilding.m_flags & Building.Flags.Active) == Building.Flags.None)
                    {
                        instance.m_nodes.m_buffer[result.m_nodeStation].m_flags |= NetNode.Flags.Disabled;
                    }
                    instance.m_nodes.m_buffer[result.m_nodeStation].m_flags |= NetNode.Flags.Fixed;
                    instance.m_nodes.m_buffer[result.m_nodeStation].m_lane = PlatformLaneId;
                    instance.UpdateNode(result.m_nodeStation);
                    instance.m_nodes.m_buffer[result.m_nodeStation].m_nextBuildingNode = stationBuilding.m_netNode;
                    stationBuilding.m_netNode = result.m_nodeStation;
                }
                Building.Flags incomingOutgoing = ((outsideConnectionBuilding.m_flags & Building.Flags.IncomingOutgoing) != Building.Flags.Incoming) ? Building.Flags.Incoming : Building.Flags.Outgoing;
                Vector3 outsideConnectionPlatformPosition = TransportStationAIExtension.FindStopPosition(outsideConnectionId, ref outsideConnectionBuilding, incomingOutgoing);
                if (outsideConnectionTSD.CreateConnectionNode(out result.m_nodeOutsideConnection, outsideConnectionPlatformPosition))
                {
                    if ((stationBuilding.m_flags & Building.Flags.Active) == Building.Flags.None)
                    {
                        instance.m_nodes.m_buffer[result.m_nodeOutsideConnection].m_flags |= NetNode.Flags.Disabled;
                    }
                    instance.UpdateNode(result.m_nodeOutsideConnection);
                    instance.m_nodes.m_buffer[result.m_nodeOutsideConnection].m_nextBuildingNode = stationBuilding.m_netNode;
                    stationBuilding.m_netNode = result.m_nodeOutsideConnection;
                }
                result.m_nodeVirtual = 0;
                if (stationBuilding.Info != null && stationBuilding.Info.m_class.m_subService == ItemClass.SubService.PublicTransportBus && stationBuilding.Info.m_class.m_level == ItemClass.Level.Level3)
                {
                    ushort num = 0;
                    Vector3 position2 = outsideConnectionPlatformPosition;
                    if ((outsideConnectionBuilding.m_flags & Building.Flags.IncomingOutgoing) == Building.Flags.IncomingOutgoing)
                    {
                        position2 = TransportStationAIExtension.FindStopPosition(outsideConnectionId, ref outsideConnectionBuilding, Building.Flags.Outgoing);
                    }
                    else
                    {
                        num = TransportStationAIExtension.FindNearestConnection(outsideConnectionId, outsideConnectionBuilding.m_flags & ~Building.Flags.IncomingOutgoing);
                    }
                    if (num != 0)
                    {
                        position2 = TransportStationAIExtension.FindStopPosition(num, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[num], incomingOutgoing);
                    }
                    if (outsideConnectionTSD.CreateConnectionNode(out result.m_nodeVirtual, position2))
                    {
                        if ((stationBuilding.m_flags & Building.Flags.Active) == 0)
                        {
                            instance.m_nodes.m_buffer[result.m_nodeVirtual].m_flags |= NetNode.Flags.Disabled;
                        }
                        instance.UpdateNode(result.m_nodeVirtual);
                        instance.m_nodes.m_buffer[result.m_nodeVirtual].m_nextBuildingNode = stationBuilding.m_netNode;
                        stationBuilding.m_netNode = result.m_nodeVirtual;
                    }
                }
                if (result.m_nodeStation != 0 && result.m_nodeOutsideConnection != 0)
                {
                    if ((outsideConnectionBuilding.m_flags & Building.Flags.Incoming) != Building.Flags.None)
                    {
                        if (outsideConnectionTSD.CreateConnectionSegment(out result.m_segmentFromStationToOutsideConnection, result.m_nodeStation, result.m_nodeOutsideConnection, 0))
                        {
                            instance.m_segments.m_buffer[result.m_segmentFromStationToOutsideConnection].m_flags |= NetSegment.Flags.Untouchable;
                            instance.UpdateSegment(result.m_segmentFromStationToOutsideConnection);
                        }
                        if (result.m_nodeVirtual != 0 && outsideConnectionTSD.CreateConnectionSegment(out result.m_segmentFromVirtualToStation, result.m_nodeVirtual, result.m_nodeStation, 0))
                        {
                            instance.m_segments.m_buffer[result.m_segmentFromVirtualToStation].m_flags |= NetSegment.Flags.Untouchable;
                            instance.UpdateSegment(result.m_segmentFromVirtualToStation);
                        }
                    }
                    if ((outsideConnectionBuilding.m_flags & Building.Flags.Outgoing) != Building.Flags.None)
                    {
                        if (outsideConnectionTSD.CreateConnectionSegment(out result.m_segmentFromOutsideConnectionToStation, result.m_nodeOutsideConnection, result.m_nodeStation, 0))
                        {
                            instance.m_segments.m_buffer[result.m_segmentFromOutsideConnectionToStation].m_flags |= NetSegment.Flags.Untouchable;
                            instance.UpdateSegment(result.m_segmentFromOutsideConnectionToStation);
                        }
                        if (result.m_nodeVirtual != 0 && outsideConnectionTSD.CreateConnectionSegment(out result.m_segmentStationToVirtual, result.m_nodeStation, result.m_nodeVirtual, 0))
                        {
                            instance.m_segments.m_buffer[result.m_segmentStationToVirtual].m_flags |= NetSegment.Flags.Untouchable;
                            instance.UpdateSegment(result.m_segmentStationToVirtual);
                        }
                    }
                    return result;
                }
                else
                {
                    instance.ReleaseNode(result.m_nodeStation);
                    instance.ReleaseNode(result.m_nodeOutsideConnection);
                }
            }
            return null;
        }

    }
}
