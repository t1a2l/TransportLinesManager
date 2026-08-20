using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using Commons.Utils;
using TransportLinesManager.Data.Base.Enums;
using TransportLinesManager.Data.DataContainers;
using TransportLinesManager.Data.Extensions;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Overrides;
using UnityEngine;

namespace TransportLinesManager.Utils
{
    public class SpawnDelayUtils
    {
        private static readonly Dictionary<ushort, uint> DepotFrames = [];

        // For fair mode: one shared timer per depot
        private static readonly Dictionary<ushort, uint> FairDepotFrames = [];

        // Last spawn frame per line (used for priority, not as a gate)
        private static readonly Dictionary<ushort, uint> LastLineSpawnFrames = [];

        // Pending requests per depot
        private sealed class PendingLineRequest
        {
            public ushort DepotId;
            public ushort LineId;
            public TransferManager.TransferReason Reason;
            public TransferManager.TransferOffer Offer;
            public uint FirstSeenFrame;
        }

        private static readonly Dictionary<ushort, Dictionary<ushort, PendingLineRequest>> PendingRequests = [];

        private static uint busFrame;
        private static uint tramFrame;
        private static uint trolleybusFrame;
        private static uint ferryFrame;
        private static uint blimpFrame;
        private static uint passengerHelicopterFrame;
        private static uint touristBusFrame;

        public static bool IsFairLineMode() => TLMBaseConfigXML.CurrentContextConfig.SpawnDelayScope == SpawnDelayScope.DepotFairLine;

        public static bool IsSimpleDepotMode() => TLMBaseConfigXML.CurrentContextConfig.SpawnDelayScope == SpawnDelayScope.Depot;

        public static bool IsSimpleGlobalMode() => TLMBaseConfigXML.CurrentContextConfig.SpawnDelayScope == SpawnDelayScope.Global;

        public static bool CanSpawn(ushort buildingID, ushort lineId, TransferManager.TransferReason reason, uint currentFrame)
        {
            // Global mode
            if (IsSimpleGlobalMode())
            {
                if (!CanGlobalSpawn(reason))
                {
                    return false;
                }

                // Proceed to spawn
                return true;
            }
            // Simple per-depot mode
            else if (IsSimpleDepotMode())
            {
                if (!CanSimpleDepotSpawn(buildingID, reason))
                {
                    return false;
                }

                // Proceed to spawn
                return true;
            }
            // Fair line dispatch mode
            else if (IsFairLineMode())
            {
                // Depot cooldown check
                if (!CanFairDepotSpawn(buildingID, reason))
                {
                    return false;
                }

                // Proceed to spawn for the selected line
                return true;
            }

            // should not happen, but just in case
            return false;
        }

        public static void MarkSpawn(ushort buildingID, ushort lineId, TransferManager.TransferReason reason, uint currentFrame)
        {
            // Global mode
            if (IsSimpleGlobalMode())
            {
                MarkGlobalSpawn(reason, currentFrame);
            }
            // Simple per-depot mode
            else if (IsSimpleDepotMode())
            {
                MarkSimpleDepotSpawn(buildingID, currentFrame);
            }
            // Fair line dispatch mode
            else if (IsFairLineMode())
            {
                MarkFairDepotSpawn(buildingID, currentFrame);
                UpdateLastLineSpawn(lineId, currentFrame);
                RemovePendingRequest(buildingID, lineId);
            }
        }

        public static void ProcessPendingQueues(uint currentFrame)
        {
            if (!IsFairLineMode())
                return;

            // Iterate over all depots that have pending requests
            var depotsToProcess = PendingRequests.Keys.ToList();

            foreach (var depotId in depotsToProcess)
            {
                if (!PendingRequests.TryGetValue(depotId, out var requests))
                    continue;

                if (requests.Count == 0)
                    continue;

                // Group by reason (bus, tram, etc.)
                var byReason = requests.Values.GroupBy(x => x.Reason).ToList();

                foreach (var group in byReason)
                {
                    var reason = group.Key;

                    if (!CanFairDepotSpawn(depotId, reason))
                        continue;

                    // Select best line among pending for this depot+reason
                    var bestRequest = SelectFairRequest(depotId, reason, currentFrame);

                    if (bestRequest == null)
                        continue;

                    // Force a spawn for this line at this depot
                    if (TrySpawnVehicleAtDepot(depotId, bestRequest.LineId, reason, bestRequest.Offer, currentFrame))
                    {
                        // Success: update timers and remove from queue
                        MarkFairDepotSpawn(depotId, currentFrame);
                        UpdateLastLineSpawn(bestRequest.LineId, currentFrame);
                        RemovePendingRequest(depotId, bestRequest.LineId);
                    }
                }
            }
        }

        public static void RegisterPendingRequest(ushort depotId, ushort lineId, TransferManager.TransferReason reason, TransferManager.TransferOffer offer, uint frame)
        {
            if (!PendingRequests.TryGetValue(depotId, out var depotRequests))
            {
                depotRequests = [];
                PendingRequests[depotId] = depotRequests;
            }

            if (!depotRequests.ContainsKey(lineId))
            {
                depotRequests[lineId] = new PendingLineRequest
                {
                    DepotId = depotId,
                    LineId = lineId,
                    Reason = reason,
                    Offer = offer,
                    FirstSeenFrame = frame
                };
            }
        }

        public static void RemovePendingRequest(ushort depotId, ushort lineId)
        {
            if (PendingRequests.TryGetValue(depotId, out var depotRequests))
            {
                depotRequests.Remove(lineId);
                if (depotRequests.Count == 0)
                {
                    PendingRequests.Remove(depotId);
                }
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

        private static bool CanGlobalSpawn(TransferManager.TransferReason reason)
        {
            uint delay = GetSpawnDelay(reason);

            if (delay == 0)
            {
                return true;
            }

            uint lastFrame = GetGlobalFrame(reason);

            uint currentFrame = Singleton<SimulationManager>.instance.m_currentFrameIndex;

            return currentFrame - lastFrame >= delay;
        }

        private static void MarkGlobalSpawn(TransferManager.TransferReason reason, uint frame)
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

        private static bool CanSimpleDepotSpawn(ushort depotId, TransferManager.TransferReason reason)
        {
            uint delay = GetSpawnDelay(reason);

            if (delay == 0)
            {
                return true;
            }

            uint lastFrame = DepotFrames.TryGetValue(depotId, out uint frame) ? frame : 0;

            uint currentFrame = Singleton<SimulationManager>.instance.m_currentFrameIndex;

            return currentFrame - lastFrame >= delay;
        }

        private static void MarkSimpleDepotSpawn(ushort depotId, uint frame)
        {
            DepotFrames[depotId] = frame;
        }

        private static bool CanFairDepotSpawn(ushort depotId, TransferManager.TransferReason reason)
        {
            uint delay = GetSpawnDelay(reason);

            if (delay == 0)
            {
                return true;
            }

            uint lastFrame = FairDepotFrames.TryGetValue(depotId, out uint frame) ? frame : 0;

            uint currentFrame = Singleton<SimulationManager>.instance.m_currentFrameIndex;

            return currentFrame - lastFrame >= delay;
        }

        private static void MarkFairDepotSpawn(ushort depotId, uint frame)
        {
            FairDepotFrames[depotId] = frame;
        }

        private static bool TryGetLineNeed(ushort lineId, out int currentVehicles, out int targetVehicles)
        {
            currentVehicles = 0;
            targetVehicles = 0;

            if (lineId == 0 || lineId >= TransportManager.instance.m_lines.m_size)
            {
                return false;
            }

            ref TransportLine line = ref TransportManager.instance.m_lines.m_buffer[lineId];

            if ((line.m_flags & TransportLine.Flags.Created) == 0)
            {
                return false;
            }

            currentVehicles = line.CountVehicles(lineId);
            targetVehicles = TransportLineOverrides.NewCalculateTargetVehicleCount(lineId);

            return targetVehicles > 0;
        }

        private static float GetLinePriority(PendingLineRequest request, uint currentFrame)
        {
            if (!TryGetLineNeed(request.LineId, out int currentVehicles, out int targetVehicles))
            {
                return float.MinValue;
            }

            if (currentVehicles >= targetVehicles)
            {
                return float.MinValue;
            }

            uint lastSpawnFrame = LastLineSpawnFrames.TryGetValue(request.LineId, out uint frame) ? frame : request.FirstSeenFrame;

            uint elapsedFrames = currentFrame - lastSpawnFrame;

            float deficitRatio = (targetVehicles - currentVehicles) / Mathf.Max(1f, targetVehicles);

            uint lineDelay = GetSpawnDelayForLine(request.LineId);

            float overdueRatio = lineDelay == 0 ? 0f : elapsedFrames / (float)lineDelay;

            return overdueRatio * 0.65f + deficitRatio * 0.35f;
        }

        private static uint GetSpawnDelayForLine(ushort lineId)
        {
            if (lineId == 0)
            {
                return 0;
            }

            ref TransportLine line =
                ref TransportManager.instance.m_lines.m_buffer[lineId];

            if (line.Info == null)
            {
                return 0;
            }

            TransferManager.TransferReason reason = line.Info.m_vehicleReason;

            return GetSpawnDelay(reason);
        }

        private static bool IsLineAllowedAtDepot(ushort lineId, ushort depotId)
        {
            var tsd = TransportSystemDefinition.GetDefinitionForLine(lineId, false);

            if (tsd == null)
            {
                return false;
            }

            var ext = TLMLineUtils.GetEffectiveExtensionForLine(lineId);
            var allowed = ext.GetAllowedDepots(tsd, lineId);

            if (allowed == null || allowed.Count == 0)
            {
                return true; // no restriction
            }

            return allowed.Contains(depotId);
        }

        private static PendingLineRequest SelectFairRequest(ushort depotId, TransferManager.TransferReason reason, uint currentFrame)
        {
            if (!PendingRequests.TryGetValue(depotId, out var requests))
            {
                return null;
            }

            return requests.Values
                .Where(x => x.Reason == reason)
                .Where(x => IsLineAllowedAtDepot(x.LineId, depotId))
                .Where(x => TryGetLineNeed(x.LineId, out int current, out int target) && current < target)
                .OrderByDescending(x => GetLinePriority(x, currentFrame))
                .ThenBy(x => x.FirstSeenFrame)
                .ThenBy(x => x.LineId)
                .FirstOrDefault();
        }

        private static void UpdateLastLineSpawn(ushort lineId, uint frame)
        {
            LastLineSpawnFrames[lineId] = frame;
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

        private static bool TrySpawnVehicleAtDepot(ushort depotId, ushort lineId, TransferManager.TransferReason reason, TransferManager.TransferOffer offer, uint currentFrame)
        {
            ref Building depot = ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[depotId];

            if (depot.Info?.GetAI() is not DepotAI depotAi)
                return false;

            // Reuse your existing spawn logic, but do not call CanSpawn again
            // You already know this depot+line+reason is allowed.
            VehicleInfo vehicleInfo = TLMLineUtils.GetEffectiveExtensionForLine(lineId).GetAModel(lineId);

            if (vehicleInfo == null)
            {
                LogUtils.DoErrorLog("No vehicle model for line {0}", lineId);
                return false;
            }
               
            Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;

            depotAi.CalculateSpawnPosition(depotId, ref depot, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, out Vector3 position, out _);

            if (!Singleton<VehicleManager>.instance.CreateVehicle(out ushort vehicleID, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, position, reason, false, true))
            {
                return false;
            }

            TLMLineUtils.GetEffectiveExtensionForLine(lineId).EditVehicleUsedCount(lineId, vehicleInfo.name, "Add");

            vehicleInfo.m_vehicleAI.SetSource(vehicleID, ref vehicles.m_buffer[vehicleID], depotId);
            vehicleInfo.m_vehicleAI.StartTransfer(vehicleID, ref vehicles.m_buffer[vehicleID], reason, offer);

            return true;
        }
    }
}
