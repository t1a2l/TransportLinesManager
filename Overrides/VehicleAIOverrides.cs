using ColossalFramework;
using HarmonyLib;
using Commons.Extensions;
using Commons.Utils;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Data.DataContainers;
using TransportLinesManager.Data.Extensions;
using TransportLinesManager.Interfaces;
using TransportLinesManager.Utils;
using System;
using System.Reflection;
using UnityEngine;

namespace TransportLinesManager.Overrides
{
    [HarmonyPatch(typeof(VehicleAI))]
    public static class VehicleAIOverrides
    {

        private static MethodInfo BusAI_StartPathFind = typeof(BusAI).GetMethod("StartPathFind", Patcher.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null);
        private static MethodInfo TramAI_StartPathFind = typeof(TramAI).GetMethod("StartPathFind", Patcher.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null);
        private static MethodInfo TrolleyAI_StartPathFind = typeof(TrolleybusAI).GetMethod("StartPathFind", Patcher.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType() }, null);
        private static MethodInfo BusAI_UnloadPassengers = typeof(BusAI).GetMethod("UnloadPassengers", Patcher.allFlags);
        private static MethodInfo TramAI_UnloadPassengers = typeof(TramAI).GetMethod("UnloadPassengers", Patcher.allFlags);
        private static MethodInfo TrolleybusAI_UnloadPassengers = typeof(TrolleybusAI).GetMethod("UnloadPassengers", Patcher.allFlags);

        [HarmonyPatch(typeof(VehicleAI), "SimulationStep", new Type[] { typeof(ushort), typeof(Vehicle), typeof(ushort), typeof(Vehicle), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        [HarmonyPrefix]
        public static void SimulationStep(ushort vehicleID, ref Vehicle vehicleData)
        {
            if (vehicleData.m_transportLine != 0 && vehicleData.m_path == 0 && (vehicleData.m_flags & Vehicle.Flags.WaitingPath) != 0)
            {
                vehicleData.m_flags &= ~Vehicle.Flags.WaitingPath;
                vehicleData.Info.m_vehicleAI.SetTransportLine(vehicleID, ref vehicleData, 0);
            }
        }

        [HarmonyPatch(typeof(VehicleAI), "ArrivingToDestination")]
        [HarmonyPrefix]
        public static bool ArrivingToDestination(ushort vehicleID, ref Vehicle vehicleData, VehicleAI __instance)
        {
            if (!(
                (__instance is BusAI && TLMBaseConfigXML.CurrentContextConfig.ExpressBusesEnabled)
                || (__instance is TramAI && TLMBaseConfigXML.CurrentContextConfig.ExpressTramsEnabled)
                || (__instance is TrolleybusAI && TLMBaseConfigXML.CurrentContextConfig.ExpressTrolleybusesEnabled)
                ) || vehicleData.m_transportLine == 0 || (vehicleData.m_flags & Vehicle.Flags.GoingBack) != 0 || vehicleData.GetFirstVehicle(vehicleID) != vehicleID)
            {
                CheckDespawn(vehicleID, ref vehicleData);
                return true;
            }
            var currentStop = vehicleData.m_targetBuilding;
            ref TransportLine line = ref TransportManager.instance.m_lines.m_buffer[vehicleData.m_transportLine];
            if (currentStop == 0 || currentStop == line.m_stops || TLMStopDataContainer.Instance.SafeGet(currentStop).IsTerminal)
            {
                CheckDespawn(vehicleID, ref vehicleData);
                return true;
            }
            TLMLineUtils.GetQuantityPassengerWaiting(currentStop, out int residents, out int tourists, out _);
            var unloadPredict = GetQuantityPassengerUnloadOnNextStop(vehicleID, ref vehicleData, out bool full, out bool empty);
            if (unloadPredict > 0 || (!full && residents + tourists > 0))
            {
                return true;
            }
            var nextStop = TransportLine.GetNextStop(currentStop);
            vehicleData.m_targetBuilding = nextStop;
            var pathfindParams = new object[] { vehicleID, vehicleData };
            var unloadParams = new object[] { vehicleID, vehicleData, currentStop, nextStop };
            if (__instance is BusAI busAi)
            {
                if (!(bool)BusAI_StartPathFind.Invoke(busAi, pathfindParams))
                {
                    vehicleData.m_targetBuilding = currentStop;
                    return true;
                }
                vehicleData = (Vehicle)pathfindParams[1];

                BusAI_UnloadPassengers.Invoke(busAi, unloadParams);

            }
            else if (__instance is TrolleybusAI trolleyAI)
            {
                if (!(bool)TrolleyAI_StartPathFind.Invoke(trolleyAI, pathfindParams))
                {
                    vehicleData.m_targetBuilding = currentStop;
                    return true;
                }
                vehicleData = (Vehicle)pathfindParams[1];

                TrolleybusAI_UnloadPassengers.Invoke(trolleyAI, unloadParams);
            }
            else if (__instance is TramAI tramAI)
            {
                if (!(bool)TramAI_StartPathFind.Invoke(tramAI, pathfindParams))
                {
                    vehicleData.m_targetBuilding = currentStop;
                    return true;
                }
                vehicleData = (Vehicle)pathfindParams[1];

                TramAI_UnloadPassengers.Invoke(tramAI, unloadParams);
            }
            else
            {
                return true;
            }
            if (vehicleData.m_path == 0 && (vehicleData.m_flags & Vehicle.Flags.WaitingPath) != 0)
            {
                vehicleData.m_flags &= ~Vehicle.Flags.WaitingPath;
                vehicleData.Info.m_vehicleAI.SetTransportLine(vehicleID, ref vehicleData, 0);
            }
            return false;
        }

        [HarmonyPatch(typeof(VehicleAI), "GetColor", new Type[] { typeof(ushort), typeof(Vehicle), typeof(InfoManager.InfoMode), typeof(InfoManager.SubInfoMode) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal })]
        [HarmonyPrefix]
        public static bool PreGetColor(VehicleAI __instance, ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode, ref Color __result)
        {
            if (data.m_transportLine != 0 && infoMode == InfoManager.InfoMode.None)
            {
                var tsd = TransportSystemDefinition.GetDefinitionForLine(data.m_transportLine, false);
                if (tsd.TransportType == TransportInfo.TransportType.EvacuationBus)
                {
                    return true;
                }

                ITLMTransportTypeExtension ext = tsd.GetTransportExtension();
                uint prefix = TLMPrefixesUtils.GetPrefix(data.m_transportLine);

                if (ext.IsUsingColorForModel(prefix) && ext.GetColor(prefix) != default)
                {
                    __result = ext.GetColor(prefix);
                    return false;
                }
            }
            return true;

        }

        private static bool CheckDespawn(ushort vehicleID, ref Vehicle vehicleData, bool isEmpty = false)
        {
            if (vehicleData.m_transportLine != 0)
            {
                int currentVehicleCount = TransportManager.instance.m_lines.m_buffer[vehicleData.m_transportLine].CountVehicles(vehicleData.m_transportLine);
                int targetVehicleCount = TransportLineOverrides.NewCalculateTargetVehicleCount(vehicleData.m_transportLine);
                if (currentVehicleCount > targetVehicleCount)
                {
                    if (isEmpty)
                    {
                        vehicleData.Info.m_vehicleAI.SetTransportLine(vehicleID, ref vehicleData, 0);
                    }
                    else
                    {
                        TLMVehicleUtils.DoSoftDespawn(vehicleID, ref vehicleData);
                    }
                    return true;
                }
            }
            return false;
        }

        private static int GetQuantityPassengerUnloadOnNextStop(ushort vehicleId, ref Vehicle data, out bool full, out bool empty)
        {
            var firstVehicle = data.GetFirstVehicle(vehicleId);
            if (firstVehicle != vehicleId)
            {
                return GetQuantityPassengerUnloadOnNextStop(firstVehicle, ref VehicleManager.instance.m_vehicles.m_buffer[firstVehicle], out full, out empty);
            }
            if (data.m_transportLine == 0)
            {
                full = false;
                empty = false;
                return -1;
            }
            var stopNodeId = data.m_targetBuilding;
            if (stopNodeId == 0)
            {
                full = false;
                empty = false;
                return 0;
            }
            NetManager nmInstance = NetManager.instance;
            Vector3 stopPos = nmInstance.m_nodes.m_buffer[stopNodeId].m_position;
            ushort nextStop = TransportLine.GetNextStop(stopNodeId);
            bool forceUnload = nextStop == 0;

            int serviceCounter = 0;
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint numCitizenUnits = instance.m_units.m_size;
            uint num2 = data.m_citizenUnits;
            int num3 = 0;
            while (num2 != 0U)
            {
                for (int i = 0; i < 5; i++)
                {
                    uint citizen = instance.m_units.m_buffer[(int)((UIntPtr)num2)].GetCitizen(i);
                    if (citizen != 0U)
                    {
                        ushort instance2 = instance.m_citizens.m_buffer[(int)((UIntPtr)citizen)].m_instance;
                        if (instance2 != 0)
                        {
                            if (!TransportArriveAtTarget(ref instance.m_instances.m_buffer[instance2], stopPos, forceUnload))
                            {
                                serviceCounter++;
                            }
                        }
                    }
                }
                num2 = instance.m_units.m_buffer[(int)((UIntPtr)num2)].m_nextUnit;
                if (++num3 > numCitizenUnits)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            data.Info.m_vehicleAI.GetBufferStatus(vehicleId, ref data, out _, out int passengers, out int capacity);
            full = capacity - passengers <= 0;
            empty = passengers == 0;
            return passengers - serviceCounter;
        }
        
        private static bool TransportArriveAtTarget(ref CitizenInstance citizenData, Vector3 stopPos, bool forceUnload)
        {
            PathManager instance = Singleton<PathManager>.instance;
            NetManager instance2 = Singleton<NetManager>.instance;
            if ((citizenData.m_flags & CitizenInstance.Flags.OnTour) == CitizenInstance.Flags.OnTour)
            {
                if ((citizenData.m_flags & CitizenInstance.Flags.TargetIsNode) == CitizenInstance.Flags.TargetIsNode)
                {
                    ushort targetBuilding = citizenData.m_targetBuilding;
                    if (targetBuilding != 0 && Vector3.SqrMagnitude(instance2.m_nodes.m_buffer[targetBuilding].m_position - stopPos) < 4f)
                    {
                        return false;
                    }
                }
                return true;
            }
            var pathPosIdx = citizenData.m_pathPositionIndex;
            var targetPath = citizenData.m_path;

            IncrmentPath(instance, ref pathPosIdx, ref targetPath);
            if (targetPath != 0U)
            {
                if (instance.m_pathUnits.m_buffer[(int)((UIntPtr)targetPath)].GetPosition(pathPosIdx >> 1, out PathUnit.Position pathPos2))
                {
                    uint laneID2 = PathManager.GetLaneID(pathPos2);

                    var pathPositionTarget2 = instance2.m_lanes.m_buffer[(int)((UIntPtr)laneID2)].CalculatePosition(1 - (pathPos2.m_offset / 255f));
                    var distNext2 = Vector3.SqrMagnitude(pathPositionTarget2 - stopPos);
                    if (TransportLinesManagerMod.DebugMode)
                    {
                        Vector3 pathPositionTarget = instance2.m_lanes.m_buffer[(int)((UIntPtr)laneID2)].CalculatePosition(pathPos2.m_offset / 255f);
                        float distNext = Vector3.SqrMagnitude(pathPositionTarget - stopPos);
                        LogUtils.DoLog($"pathOffset = {pathPos2.m_offset} ({pathPos2.m_offset / 255f}), lane = {pathPos2.m_lane} ({laneID2}), segment = {pathPos2.m_segment}, distNext = {distNext}, distNext2 = {distNext2}");
                    }

                    if (distNext2 < 4f)
                    {
                        if (!forceUnload)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static void IncrmentPath(PathManager instance, ref byte pathPosIdx, ref uint targetPath)
        {
            if (targetPath != 0U)
            {
                pathPosIdx += 2;
                if (pathPosIdx >> 1 >= instance.m_pathUnits.m_buffer[targetPath].m_positionCount)
                {
                    targetPath = instance.m_pathUnits.m_buffer[targetPath].m_nextPathUnit;
                    pathPosIdx = 0;
                }
            }
        }

    }

}
