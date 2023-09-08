using System;
using System.Linq;
using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using Commons.Utils;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Utils;
using UnityEngine;

namespace TransportLinesManager.Overrides
{
	[HarmonyPatch(typeof(TransportStationAI))]
    public static class TransportStationAIOverrides
	{
        public static readonly TransferManager.TransferReason[] m_managedReasons = new TransferManager.TransferReason[]   
        {
                TransferManager.TransferReason.DummyCar,
                TransferManager.TransferReason.DummyTrain,
                TransferManager.TransferReason.DummyShip,
                TransferManager.TransferReason.DummyPlane
        };

		[HarmonyPatch(typeof(TransportStationAI), "CreateIncomingVehicle")]
		[HarmonyPrefix]
		public static bool CreateIncomingVehicle(TransportStationAI __instance, ushort buildingID, ref Building buildingData, ushort startStop, int gateIndex, ref bool __result)
		{
			TransportInfo transportInfo = (!__instance.UseSecondaryTransportInfoForConnection()) ? __instance.m_transportInfo : __instance.m_secondaryTransportInfo;
			if (transportInfo != null && FindConnectionVehicle(__instance, buildingID, ref buildingData, startStop, 3000f) == 0)
			{
				VehicleInfo vehicleInfo = (__instance.m_overrideVehicleClass is null) ? TryGetRandomVehicleStation(Singleton<VehicleManager>.instance, ref Singleton<SimulationManager>.instance.m_randomizer, __instance.m_transportLineInfo.m_class.m_service, __instance.m_transportLineInfo.m_class.m_subService, __instance.m_transportLineInfo.m_class.m_level, transportInfo.m_vehicleType) : TryGetRandomVehicleStation(Singleton<VehicleManager>.instance, ref Singleton<SimulationManager>.instance.m_randomizer, __instance.m_overrideVehicleClass.m_service, __instance.m_overrideVehicleClass.m_subService, __instance.m_overrideVehicleClass.m_level, transportInfo.m_vehicleType);
				if (vehicleInfo != null)
				{
					ushort num = FindConnectionBuilding(__instance, startStop);
					if (num != 0)
					{
						Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
						BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[num].Info;
						Randomizer randomizer = default(Randomizer);
						randomizer.seed = (ulong)gateIndex;
						info.m_buildingAI.CalculateSpawnPosition(num, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[num], ref randomizer, vehicleInfo, out var position, out var _);
						if (vehicleInfo.m_vehicleAI.CanSpawnAt(position) && Singleton<VehicleManager>.instance.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, position, transportInfo.m_vehicleReason, transferToSource: true, transferToTarget: false))
						{
							vehicles.m_buffer[vehicle].m_gateIndex = (byte)gateIndex;
							Vehicle.Flags flags = ((vehicleInfo.m_class.m_subService != ItemClass.SubService.PublicTransportBus) ? (Vehicle.Flags.Importing | Vehicle.Flags.Exporting) : Vehicle.Flags.Importing);
							vehicles.m_buffer[vehicle].m_flags |= flags;
							vehicleInfo.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], num);
							vehicleInfo.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], buildingID);
							vehicleInfo.m_vehicleAI.SetTarget(vehicle, ref vehicles.m_buffer[vehicle], startStop);
							SetRegionalLine(vehicle, startStop);
							__result = true;
						}
						else
						{
							__result = false;
						}
					}
					else
					{
						__result = false;
					}
				}
				else
				{
					__result = false;
				}
			}
			else
			{
				__result = false;
			}
			return false;
		}

		[HarmonyPatch(typeof(TransportStationAI), "CreateOutgoingVehicle")]
		[HarmonyPrefix]
		public static bool CreateOutgoingVehicle(TransportStationAI __instance, ushort buildingID, ref Building buildingData, ushort startStop, int gateIndex, ref bool __result)
		{
			TransportInfo transportInfo = (!__instance.UseSecondaryTransportInfoForConnection()) ? __instance.m_transportInfo : __instance.m_secondaryTransportInfo;
			if (__instance.m_transportLineInfo != null && FindConnectionVehicle(__instance, buildingID, ref buildingData, startStop, 3000f) == 0)
			{
				VehicleInfo vehicleInfo = (__instance.m_overrideVehicleClass == null) ? TryGetRandomVehicleStation(Singleton<VehicleManager>.instance, ref Singleton<SimulationManager>.instance.m_randomizer, __instance.m_transportLineInfo.m_class.m_service, __instance.m_transportLineInfo.m_class.m_subService, __instance.m_transportLineInfo.m_class.m_level, transportInfo.m_vehicleType) : TryGetRandomVehicleStation(Singleton<VehicleManager>.instance, ref Singleton<SimulationManager>.instance.m_randomizer, __instance.m_overrideVehicleClass.m_service, __instance.m_overrideVehicleClass.m_subService, __instance.m_overrideVehicleClass.m_level, transportInfo.m_vehicleType);
				if (vehicleInfo != null)
				{
					Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
					Randomizer randomizer = default;
					randomizer.seed = (ulong)gateIndex;
					__instance.CalculateSpawnPosition(buildingID, ref buildingData, ref randomizer, vehicleInfo, out var position, out var _);
					if (vehicleInfo.m_vehicleAI.CanSpawnAt(position) && Singleton<VehicleManager>.instance.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, position, transportInfo.m_vehicleReason, transferToSource: false, transferToTarget: true))
					{
						vehicles.m_buffer[vehicle].m_gateIndex = (byte)gateIndex;
						Vehicle.Flags flags = ((vehicleInfo.m_class.m_subService != ItemClass.SubService.PublicTransportBus) ? (Vehicle.Flags.Importing | Vehicle.Flags.Exporting) : Vehicle.Flags.Exporting);
						vehicles.m_buffer[vehicle].m_flags |= flags;
						vehicleInfo.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], buildingID);
						vehicleInfo.m_vehicleAI.SetTarget(vehicle, ref vehicles.m_buffer[vehicle], startStop);
						SetRegionalLine(vehicle, startStop);
						__result = true;
					}
					else
					{
						__result = false;
					}
				}
				else
				{
					__result = false;
				}
			}
			else
			{
				__result = false;
			}
			return false;
		}

		private static ushort FindConnectionVehicle(TransportStationAI __instance, ushort buildingID, ref Building buildingData, ushort targetStop, float maxDistance)
        {
	        VehicleManager instance = Singleton<VehicleManager>.instance;
	        Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[targetStop].m_position;
	        ushort num = buildingData.m_ownVehicles;
	        int num2 = 0;
	        while (num != 0)
	        {
		        if (instance.m_vehicles.m_buffer[num].m_transportLine == 0)
		        {
			        VehicleInfo info = instance.m_vehicles.m_buffer[num].Info;
			        if (info.m_class.m_service == __instance.m_transportLineInfo.m_class.m_service && info.m_class.m_subService == __instance.m_transportLineInfo.m_class.m_subService && instance.m_vehicles.m_buffer[num].m_targetBuilding == targetStop && Vector3.SqrMagnitude(instance.m_vehicles.m_buffer[num].GetLastFramePosition() - position) < maxDistance * maxDistance)
			        {
				        return num;
			        }
		        }
		        num = instance.m_vehicles.m_buffer[num].m_nextOwnVehicle;
		        if (++num2 > 16384)
		        {
			        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
			        break;
		        }
	        }
	        return 0;
        }

		private static ushort FindConnectionBuilding(TransportStationAI __instance, ushort stop)
		{
			if ((object)__instance.m_transportLineInfo != null)
			{
				Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[stop].m_position;
				BuildingManager instance = Singleton<BuildingManager>.instance;
				FastList<ushort> outsideConnections = instance.GetOutsideConnections();
				ushort result = 0;
				float num = 40000f;
				for (int i = 0; i < outsideConnections.m_size; i++)
				{
					ushort num2 = outsideConnections.m_buffer[i];
					BuildingInfo info = instance.m_buildings.m_buffer[num2].Info;
					if ((info.m_class.m_service == __instance.m_transportLineInfo.m_class.m_service && info.m_class.m_subService == __instance.m_transportLineInfo.m_class.m_subService) || IsIntercityBusConnection(__instance, info))
					{
						float num3 = VectorUtils.LengthSqrXZ(instance.m_buildings.m_buffer[num2].m_position - position);
						if (num3 < num)
						{
							result = num2;
							num = num3;
						}
					}
				}
				return result;
			}
			return 0;
		}

		public static VehicleInfo TryGetRandomVehicle(VehicleManager vm, ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, VehicleInfo.VehicleType type)
        {
            var tsd = TransportSystemDefinition.FromOutsideConnection(subService, level, type);
            if (tsd is not null)
            {
                VehicleInfo randomVehicleInfo = tsd.GetTransportExtension().GetAModel(0);
                if (randomVehicleInfo != null)
                {
                    return randomVehicleInfo;
                }
            }
            return vm.GetRandomVehicleInfo(ref r, service, subService, level);
        }

		private static bool IsIntercityBusConnection(TransportStationAI __instance, BuildingInfo connectionInfo)
		{
			return connectionInfo.m_class.m_service == ItemClass.Service.Road && __instance.m_transportLineInfo.m_class.m_service == ItemClass.Service.PublicTransport && connectionInfo.m_class.m_subService == ItemClass.SubService.None && __instance.m_transportLineInfo.m_class.m_subService == ItemClass.SubService.PublicTransportBus;
		}

		private static void SetRegionalLine(ushort vehicleId, ushort stopId)
        {
            ref Vehicle veh = ref VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
            if (TransportSystemDefinition.From(veh.Info) == TransportSystemDefinition.TRAIN || TransportSystemDefinition.From(veh.Info) == TransportSystemDefinition.BUS)
            {
                if (TLMStationUtils.GetStationBuilding(stopId, 0, false) != veh.m_sourceBuilding)
                {
                    veh.m_custom = NetManager.instance.m_segments.m_buffer[NetManager.instance.m_nodes.m_buffer[stopId].GetSegment(0)].GetOtherNode(stopId);
                }
                else
                {
                    veh.m_custom = stopId;
                }
            }
        }

		private static VehicleInfo TryGetRandomVehicleStation(VehicleManager vm, ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, VehicleInfo.VehicleType type)
        {
            LogUtils.DoLog("START TRANSFER StationAI!!!!!!!!");
            return TryGetRandomVehicle(vm, ref r, service, subService, level, type);
        }

	}
}
