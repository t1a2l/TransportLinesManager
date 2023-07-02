using ColossalFramework.Math;
using HarmonyLib;
using UnityEngine;
using System;

namespace Klyte.TransportLinesManager.Overrides
{
    [HarmonyPatch(typeof(NetManager))]
    public static class NetManagerOverrides
    {

        public static event Action<ushort> EventNodeChanged;
        public static event Action<ushort> EventSegmentChanged;
        public static event Action<ushort> EventSegmentReleased;
        public static event Action<ushort> EventSegmentNameChanged;
  
        [HarmonyPatch(typeof(NetManager), "CreateNode")]
        [HarmonyPostfix]
        public static void CreateNode(ref ushort node)
        {
            ushort node_ = node;
            SimulationManager.instance.AddAction(() => EventNodeChanged?.Invoke(node_)).Execute();
        }

        [HarmonyPatch(typeof(NetManager), "ReleaseNode")]
        [HarmonyPostfix]
        public static void ReleaseNode(ref ushort node)
        {
            ushort node_ = node;
            SimulationManager.instance.AddAction(() => EventNodeChanged?.Invoke(node_)).Execute();
        }

        [HarmonyPatch(typeof(NetManager), "CreateSegment", new Type[] { typeof(ushort), typeof(Randomizer), typeof(NetInfo), typeof(TreeInfo), typeof(ushort), typeof(ushort), typeof(Vector3), typeof(Vector3), typeof(uint), typeof(uint), typeof(bool) }, new ArgumentType[] { ArgumentType.Ref, ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void CreateSegment(ref ushort segment, ref ushort startNode, ref ushort endNode)
        {
            ushort startNode_ = startNode;
            ushort segment_ = segment;
            ushort endNode_ = endNode;

            SimulationManager.instance.AddAction(() =>
            {
                EventNodeChanged?.Invoke(startNode_);
                EventNodeChanged?.Invoke(endNode_);
                EventSegmentChanged?.Invoke(segment_);
            }).Execute();
        }

        [HarmonyPatch(typeof(NetManager), "ReleaseSegment")]
        [HarmonyPrefix]
        public static void ReleaseSegment(ref ushort segment)
        {
            ushort segment_ = segment;
            SimulationManager.instance.AddAction(() =>
            {
                EventNodeChanged?.Invoke(NetManager.instance.m_segments.m_buffer[segment_].m_startNode);
                EventNodeChanged?.Invoke(NetManager.instance.m_segments.m_buffer[segment_].m_endNode);
                EventSegmentChanged?.Invoke(segment_);
                EventSegmentReleased?.Invoke(segment_);
            }).Execute();
        }

        [HarmonyPatch(typeof(NetManager), "SetSegmentNameImpl")]
        [HarmonyPostfix]
        public static void SetSegmentNameImpl(ref ushort segmentID)
        {
            ushort segment_ = segmentID;
            SimulationManager.instance.AddAction(() => EventSegmentNameChanged?.Invoke(segment_)).Execute();
        }

    }
}
