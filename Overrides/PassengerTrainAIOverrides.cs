using System;
using UnityEngine;
using HarmonyLib;
using Klyte.TransportLinesManager.Cache.BuildingData;
using Klyte.TransportLinesManager.Data.Base.ConfigurationContainers.OutsideConnections;

namespace Klyte.TransportLinesManager.Overrides
{
    [HarmonyPatch(typeof(PassengerTrainAI))]
    public class PassengerTrainAIOverrides
    {
        [HarmonyPatch(typeof(PassengerTrainAI), "GetColor", new Type[] { typeof(ushort), typeof(Vehicle), typeof(InfoManager.InfoMode), typeof(InfoManager.SubInfoMode) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal })]
        [HarmonyPrefix]
        
        public static bool GetColor(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode, ref Color __result)
        {
            if (data.m_leadingVehicle == 0 
                && infoMode == InfoManager.InfoMode.None 
                && data.m_transportLine == 0
                && data.m_custom != 0
                && TransportLinesManagerMod.Controller.BuildingLines[data.m_custom] is InnerBuildingLine cacheItem
                && cacheItem.LineDataObject is OutsideConnectionLineInfo ocli)
            {
                __result = ocli.LineColor;
                return false;
            }
            return true;
        }

    }
}