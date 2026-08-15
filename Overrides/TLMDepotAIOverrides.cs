using ColossalFramework;
using ColossalFramework.Math;
using Commons.Utils;
using TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Data.DataContainers;
using TransportLinesManager.Data.Base.Enums;
using TransportLinesManager.Data.Extensions;

namespace TransportLinesManager.Overrides
{
    [HarmonyPatch(typeof(DepotAI))]
    public static class TLMDepotAIOverrides
    {
        private static readonly Dictionary<ushort, uint> DepotFrames = [];
        private static readonly Dictionary<ushort, uint> LineFrames = [];

        private static uint busFrame;
        private static uint tramFrame;
        private static uint trolleybusFrame;
        private static uint ferryFrame;
        private static uint blimpFrame;
        private static uint passengerHelicopterFrame;
        private static uint touristBusFrame;

        private static readonly TransferManager.TransferReason[] m_managedReasons = [
            TransferManager.TransferReason.Tram,
            TransferManager.TransferReason.PassengerTrain,
            TransferManager.TransferReason.PassengerShip,
            TransferManager.TransferReason.PassengerPlane,
            TransferManager.TransferReason.MetroTrain,
            TransferManager.TransferReason.Monorail,
            TransferManager.TransferReason.CableCar,
            TransferManager.TransferReason.Blimp,
            TransferManager.TransferReason.Bus,
            TransferManager.TransferReason.Ferry,
            TransferManager.TransferReason.Trolleybus,
            TransferManager.TransferReason.PassengerHelicopter,
            TransferManager.TransferReason.TouristBus,
        ];

        [HarmonyPatch(typeof(DepotAI), "StartTransfer")]
        [HarmonyPrefix]
        public static bool StartTransfer(DepotAI __instance, ushort buildingID, ref Building data, TransferManager.TransferReason reason, TransferManager.TransferOffer offer)
        {
            if (!m_managedReasons.Contains(reason) || offer.TransportLine == 0)
            {
                return true;
            }

            LogUtils.DoLog("START TRANSFER!!!!!!!!");
            TransportInfo m_transportInfo = __instance.m_transportInfo;
            BuildingInfo m_info = data.Info;

            LogUtils.DoLog("m_info {0} | m_transportInfo {1} | Line: {2}", m_info?.name, m_transportInfo?.name, offer.TransportLine);


            if ((m_transportInfo != null && reason == m_transportInfo.m_vehicleReason) || (__instance.m_secondaryTransportInfo != null && reason == __instance.m_secondaryTransportInfo.m_vehicleReason))
            {
                var tsd = TransportSystemDefinition.FromLocal(__instance.m_transportInfo);
                if (tsd is null)
                {
                    return true;
                }

                SetRandomBuilding(tsd, offer.TransportLine, ref buildingID);

                // If the Transit Vehicle Spawn Delay mod is not enabled, we enforce our own spawn limits
                if (!TLMController.IsTransitVehicleSpawnDelayEnabled && !CanSpawn(buildingID, offer.TransportLine, reason))
                {
                    return false;
                }

                LogUtils.DoLog("randomVehicleInfo");
                VehicleInfo randomVehicleInfo = DoModelDraw(offer.TransportLine);
                if (randomVehicleInfo != null)
                {
                    LogUtils.DoLog("randomVehicleInfo != null");
                    Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                    __instance.CalculateSpawnPosition(buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID], ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, out Vector3 position, out _);
                    if (Singleton<VehicleManager>.instance.CreateVehicle(out ushort vehicleID, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, position, reason, false, true))
                    {
                        LogUtils.DoLog("CreatedVehicle!!!");
                        TLMLineUtils.GetEffectiveExtensionForLine(offer.TransportLine).EditVehicleUsedCount(offer.TransportLine, randomVehicleInfo.name, "Add");
                        if (!TLMController.IsTransitVehicleSpawnDelayEnabled)
                        {
                            SetLastSpawnFrame(buildingID, offer.TransportLine, reason, Singleton<SimulationManager>.instance.m_currentFrameIndex);
                        }
                        randomVehicleInfo.m_vehicleAI.SetSource(vehicleID, ref vehicles.m_buffer[vehicleID], buildingID);
                        randomVehicleInfo.m_vehicleAI.StartTransfer(vehicleID, ref vehicles.m_buffer[vehicleID], reason, offer);
                    }
                    return false;
                }
                else
                {
                    LogUtils.DoErrorLog("DoModelDraw returned null for line {0}", offer.TransportLine);
                    return false;
                }
            }
            return true;
        }

        public static uint GetSpawnTime(ushort depotId, ushort lineId, TransferManager.TransferReason reason)
        {
            uint delay = GetSpawnDelay(reason);
            uint lastFrame = GetLastSpawnFrame(depotId, lineId, reason);

            return lastFrame + delay;
        }

        public static void ClearLineSpawnDelay(ushort lineId)
        {
            LineFrames.Remove(lineId);
        }

        private static VehicleInfo DoModelDraw(ushort lineId)
        {
            return TLMLineUtils.GetEffectiveExtensionForLine(lineId).GetAModel(lineId);
        }

        private static void SetRandomBuilding(TransportSystemDefinition tsd, ushort lineId, ref ushort currentId)
        {
            Interfaces.IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(lineId);
            List<ushort> allowedDepots = config.GetAllowedDepots(tsd, lineId);
            if (allowedDepots.Count == 0)
            {
                if (TransportLinesManagerMod.DebugMode)
                {
                    LogUtils.DoLog("allowedDepots.Count --{0}-- == 0", allowedDepots.Count);
                }
                return;
            }
            var r = new Randomizer(new System.Random().Next());
            if (TransportLinesManagerMod.DebugMode)
            {
                LogUtils.DoLog("DEPOT POSSIBLE VALUES FOR {2} LINE {1}: {0} ", string.Join(",", [.. allowedDepots.Select(x => x.ToString())]), lineId, tsd);
            }

            currentId = allowedDepots[r.Int32(0, allowedDepots.Count - 1)];
            if (TransportLinesManagerMod.DebugMode)
            {
                LogUtils.DoLog("DEPOT FOR {2} LINE {1}: {0} ", currentId, lineId, tsd);
            }
        }

        private static uint GetSpawnDelay(TransferManager.TransferReason reason)
        {
            var config = TLMBaseConfigXML.CurrentContextConfig;

            return reason switch
            {
                TransferManager.TransferReason.Bus => config.BusDelay,
                TransferManager.TransferReason.Tram => config.TramDelay,
                TransferManager.TransferReason.Trolleybus => config.TrolleybusDelay,
                TransferManager.TransferReason.Ferry => config.FerryDelay,
                TransferManager.TransferReason.Blimp => config.BlimpDelay,
                TransferManager.TransferReason.PassengerHelicopter => config.PassengerHelicopterDelay,
                TransferManager.TransferReason.TouristBus => config.TouristBusDelay,
                _ => 0,
            };
        }

        private static uint GetLastSpawnFrame(ushort depotId, ushort lineId, TransferManager.TransferReason reason)
        {
            switch (TLMBaseConfigXML.CurrentContextConfig.SpawnDelayScope)
            {
                case SpawnDelayScope.Line:
                    return LineFrames.TryGetValue(lineId, out uint lineFrame) ? lineFrame : 0;

                case SpawnDelayScope.Depot:
                    return DepotFrames.TryGetValue(depotId, out uint depotFrame) ? depotFrame : 0;

                default:
                    return GetGlobalFrame(reason);
            }
        }

        private static void SetLastSpawnFrame(ushort depotId, ushort lineId, TransferManager.TransferReason reason, uint frame)
        {
            switch (TLMBaseConfigXML.CurrentContextConfig.SpawnDelayScope)
            {
                case SpawnDelayScope.Line:
                    LineFrames[lineId] = frame;
                    return;

                case SpawnDelayScope.Depot:
                    DepotFrames[depotId] = frame;
                    return;

                default:
                    SetGlobalFrame(reason, frame);
                    return;
            }
        }

        private static bool CanSpawn(ushort depotId, ushort lineId, TransferManager.TransferReason reason)
        {
            uint delay = GetSpawnDelay(reason);
            if (delay == 0)
            {
                return true;
            }

            uint lastFrame = GetLastSpawnFrame(depotId, lineId, reason);
            uint currentFrame = Singleton<SimulationManager>.instance.m_currentFrameIndex;

            return currentFrame - lastFrame >= delay;
        }

        private static uint GetGlobalFrame(TransferManager.TransferReason reason)
        {
            return reason switch
            {
                TransferManager.TransferReason.Bus => busFrame,
                TransferManager.TransferReason.Tram => tramFrame,
                TransferManager.TransferReason.Trolleybus => trolleybusFrame,
                TransferManager.TransferReason.Ferry => ferryFrame,
                TransferManager.TransferReason.Blimp => blimpFrame,
                TransferManager.TransferReason.PassengerHelicopter => passengerHelicopterFrame,
                TransferManager.TransferReason.TouristBus => touristBusFrame,
                _ => 0,
            };
        }

        private static void SetGlobalFrame(TransferManager.TransferReason reason, uint frame)
        {
            switch (reason)
            {
                case TransferManager.TransferReason.Bus:
                    busFrame = frame;
                    break;

                case TransferManager.TransferReason.Tram:
                    tramFrame = frame;
                    break;

                case TransferManager.TransferReason.Trolleybus:
                    trolleybusFrame = frame;
                    break;

                case TransferManager.TransferReason.Ferry:
                    ferryFrame = frame;
                    break;

                case TransferManager.TransferReason.Blimp:
                    blimpFrame = frame;
                    break;

                case TransferManager.TransferReason.PassengerHelicopter:
                    passengerHelicopterFrame = frame;
                    break;

                case TransferManager.TransferReason.TouristBus:
                    touristBusFrame = frame;
                    break;
            }
        }
    }
}
