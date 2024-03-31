using ColossalFramework.Math;
using Commons.Interfaces;
using Commons.Utils;
using TransportLinesManager.Data.Base.ConfigurationContainers.OutsideConnections;
using TransportLinesManager.Data.DataContainers;
using System;
using UnityEngine;

namespace TransportLinesManager.Cache.BuildingData
{
    public class InnerBuildingLine : IIdentifiable
    {
        public long? Id { get => SrcStop; set { } }
        public TransportInfo Info { get; set; }
        public ushort SrcBuildingId { get; set; }
        public ushort DstBuildingId { get; set; }
        public ushort SrcStop { get; set; }
        public ushort DstStop { get; set; }
        public bool BrokenFromSrc { get; set; }
        public bool BrokenFromDst { get; set; }

        private bool m_needsToBeCalculated;
        private uint m_lastCheckTick;

        private Mesh[] m_lineMeshes;
        private RenderGroup.MeshData[] m_lineMeshData;

        public int CountStops()
        {
            int num = 0;
            ushort stops = SrcStop;
            ushort num2 = stops;
            int num3 = 0;
            while (num2 != 0)
            {
                num++;
                num2 = TransportLine.GetNextStop(num2);
                if (num2 == stops)
                {
                    break;
                }
                if (++num3 >= 32768)
                {
                    LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return num;
        }
        public ushort GetStop(int index)
        {
            if (index == -1)
            {
                return GetLastStop();
            }
            ushort stops = SrcStop;
            ushort num = stops;
            int num2 = 0;
            while (num != 0)
            {
                if (index-- == 0)
                {
                    return num;
                }
                num = TransportLine.GetNextStop(num);
                if (num == stops)
                {
                    break;
                }
                if (++num2 >= 32768)
                {
                    LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return 0;
        }
        public ushort GetLastStop()
        {
            NetManager instance = NetManager.instance;
            ushort num = SrcStop;
            int num2 = 0;
            for (; ; )
            {
                bool flag = false;
                int i = 0;
                while (i < 8)
                {
                    ushort segment = instance.m_nodes.m_buffer[num].GetSegment(i);
                    if (segment != 0 && instance.m_segments.m_buffer[segment].m_startNode == num)
                    {
                        num = instance.m_segments.m_buffer[segment].m_endNode;
                        if (num == SrcStop)
                        {
                            return num;
                        }
                        flag = true;
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
                if (++num2 >= 32768)
                {
                    LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                    return num;
                }
                if (!flag)
                {
                    return num;
                }
            }
        }

        private OutsideConnectionLineInfo m_cachedLineInfoRef;
        public OutsideConnectionLineInfo LineDataObject 
            => !TLMBuildingDataContainer.Instance.SafeGet(SrcBuildingId).TlmManagedRegionalLines 
            ? null 
            : m_cachedLineInfoRef ?? (m_cachedLineInfoRef = TLMBuildingDataContainer.Instance.SafeGet(SrcBuildingId).PlatformMappings[NetManager.instance.m_nodes.m_buffer[SrcStop].m_lane].TargetOutsideConnections[DstBuildingId]);

        public void RenderLine(RenderManager.CameraInfo cameraInfo)
        {
            if (Info.m_transportType != TransportInfo.TransportType.Train && Info.m_transportType != TransportInfo.TransportType.Bus) return;
            if (m_needsToBeCalculated && SimulationManager.instance.m_currentTickIndex - m_lastCheckTick > 50)
            {
                UpdateMeshData();
            }
            if (m_lineMeshData != null)
            {
                UpdateMesh();
            }
            RenderLine_internal(cameraInfo);
        }
        public bool UpdateMeshData()
        {
            NetManager instance2 = NetManager.instance;
            PathManager instance3 = PathManager.instance;
            TerrainManager instance4 = TerrainManager.instance;
            TransportLine.TempUpdateMeshData[] data = Info.m_requireSurfaceLine ? (new TransportLine.TempUpdateMeshData[81]) : (new TransportLine.TempUpdateMeshData[1]);
            bool flag = true;
            int num = 0;
            int curveCount = 0;
            float totalLength = 0f;
            ushort stops = SrcStop;
            ushort num2 = stops;
            int num3 = 0;
            m_needsToBeCalculated = false;
            while (num2 != 0)
            {
                ushort num4 = 0;
                for (int i = 0; i < 8; i++)
                {
                    ushort segment = instance2.m_nodes.m_buffer[num2].GetSegment(i);
                    if (segment != 0 && instance2.m_segments.m_buffer[segment].m_startNode == num2)
                    {
                        uint path = instance2.m_segments.m_buffer[segment].m_path;
                        if (path != 0U)
                        {
                            byte pathFindFlags = instance3.m_pathUnits.m_buffer[path].m_pathFindFlags;
                            if ((pathFindFlags & 4u) != 0)//not calculated
                            {
                                Vector3 position = Vector3.zero;
                                if (!TransportLine.CalculatePathSegmentCount(path, 0, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, VehicleInfo.VehicleCategory.All, ref data, ref curveCount, ref totalLength, ref position))
                                {
                                    TransportLineAI.StartPathFind(segment, ref instance2.m_segments.m_buffer[segment], Info.m_netService, Info.m_secondaryNetService, Info.m_vehicleType, Info.vehicleCategory, false);
                                    flag = false;
                                    m_needsToBeCalculated = true;
                                }
                            }
                            else if ((pathFindFlags & 8) == 0) //invalid
                            {
                                if (num2 == stops)
                                {
                                    BrokenFromSrc = true;
                                }
                                else
                                {
                                    BrokenFromDst = true;
                                }
                                flag = false;
                            }
                        }
                        else
                        {
                            TransportLineAI.StartPathFind(segment, ref instance2.m_segments.m_buffer[segment], Info.m_netService, Info.m_secondaryNetService, Info.m_vehicleType, Info.vehicleCategory, false);
                            flag = false;
                            m_needsToBeCalculated = true;
                        }
                        num4 = instance2.m_segments.m_buffer[segment].m_endNode;
                        break;
                    }
                }
                if (Info.m_requireSurfaceLine)
                {
                    TransportLine.TempUpdateMeshData[] data2 = data;
                    int patchIndex = instance4.GetPatchIndex(instance2.m_nodes.m_buffer[num2].m_position);
                    data2[patchIndex].m_pathSegmentCount++;
                }
                else
                {
                    TransportLine.TempUpdateMeshData[] data3 = data;
                    data3[0].m_pathSegmentCount++;
                }
                num++;
                num2 = num4;
                if (num2 == stops)
                {
                    break;
                }
                if (++num3 >= 32768)
                {
                    LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            int num5 = 0;
            for (int j = 0; j < data.Length; j++)
            {
                int pathSegmentCount = data[j].m_pathSegmentCount;
                if (pathSegmentCount != 0)
                {
                    RenderGroup.MeshData meshData = new()
                    {
                        m_vertices = new Vector3[pathSegmentCount * 8],
                        m_normals = new Vector3[pathSegmentCount * 8],
                        m_tangents = new Vector4[pathSegmentCount * 8],
                        m_uvs = new Vector2[pathSegmentCount * 8],
                        m_uvs2 = new Vector2[pathSegmentCount * 8],
                        m_colors = new Color32[pathSegmentCount * 8],
                        m_triangles = new int[pathSegmentCount * 30]
                    };
                    data[j].m_meshData = meshData;
                    num5++;
                }
            }
            TransportManager.LineSegment[] array = new TransportManager.LineSegment[num];
            Bezier3[] curves = new Bezier3[curveCount];
            int num6 = 0;
            int curveIndex = 0;
            float lengthScale = Mathf.Ceil(totalLength / 64f) / totalLength;
            float currentLength = 0f;
            num2 = stops;
            Vector3 vector = new(100000f, 100000f, 100000f);
            Vector3 vector2 = new(-100000f, -100000f, -100000f);
            num3 = 0;
            while (num2 != 0)
            {
                ushort num7 = 0;
                for (int k = 0; k < 8; k++)
                {
                    ushort segment2 = instance2.m_nodes.m_buffer[num2].GetSegment(k);
                    if (segment2 != 0 && instance2.m_segments.m_buffer[segment2].m_startNode == num2)
                    {
                        uint path2 = instance2.m_segments.m_buffer[segment2].m_path;
                        if(curveIndex >= curveCount)
                        {
                            continue;
                        }
                        if (path2 != 0U && (instance3.m_pathUnits.m_buffer[(int)(UIntPtr)path2].m_pathFindFlags & 4u) != 0)
                        {
                            array[num6].m_curveStart = curveIndex;
                            TransportLine.FillPathSegments(path2, 0, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, VehicleInfo.VehicleCategory.All, ref data, curves, null, ref curveIndex, ref currentLength, lengthScale, out Vector3 minPos, out Vector3 maxPos, Info.m_requireSurfaceLine, true);
                            vector = Vector3.Min(vector, minPos);
                            vector2 = Vector3.Max(vector2, maxPos);
                            array[num6].m_bounds.SetMinMax(minPos, maxPos);
                            array[num6].m_curveEnd = curveIndex;
                        }
                        num7 = instance2.m_segments.m_buffer[segment2].m_endNode;
                        break;
                    }
                }
                if (Info.m_requireSurfaceLine)
                {
                    int patchIndex = instance4.GetPatchIndex(instance2.m_nodes.m_buffer[num2].m_position);
                    TransportLine.FillPathNode(instance2.m_nodes.m_buffer[num2].m_position, data[patchIndex].m_meshData, data[patchIndex].m_pathSegmentIndex, 4f, 20f, true);
                    TransportLine.TempUpdateMeshData[] data4 = data;
                    data4[patchIndex].m_pathSegmentIndex++;
                }
                else
                {
                    TransportLine.FillPathNode(instance2.m_nodes.m_buffer[num2].m_position, data[0].m_meshData, data[0].m_pathSegmentIndex, 4f, 5f, false);
                    TransportLine.TempUpdateMeshData[] data5 = data;
                    data[0].m_pathSegmentIndex++;
                }
                num6++;
                num2 = num7;
                if (num2 == stops)
                {
                    break;
                }
                if (++num3 >= 32768)
                {
                    LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            RenderGroup.MeshData[] array3 = new RenderGroup.MeshData[num5];
            int num8 = 0;
            for (int l = 0; l < data.Length; l++)
            {
                if (data[l].m_meshData != null)
                {
                    data[l].m_meshData.UpdateBounds();
                    if (Info.m_requireSurfaceLine)
                    {
                        Vector3 min = data[l].m_meshData.m_bounds.min;
                        Vector3 max = data[l].m_meshData.m_bounds.max;
                        max.y += 1024f;
                        data[l].m_meshData.m_bounds.SetMinMax(min, max);
                    }
                    array3[num8++] = data[l].m_meshData;
                }
            }
            m_lineMeshData = array3;
            m_lastCheckTick = SimulationManager.instance.m_currentTickIndex;
            return flag;
        }

        private void RenderLine_internal(RenderManager.CameraInfo cameraInfo)
        {
            Material material = Info.m_lineMaterial2;
            TerrainManager instance2 = TerrainManager.instance;
            if (m_lineMeshes != null)
            {
                int num = m_lineMeshes.Length;
                for (int i = 0; i < num; i++)
                {
                    Mesh mesh = m_lineMeshes[i];
                    if (mesh != null && cameraInfo.Intersect(mesh.bounds))
                    {
                        material.color = LineDataObject?.LineColor ?? TLMController.COLOR_ORDER[SrcStop % TLMController.COLOR_ORDER.Length];
                        material.SetFloat(TransportManager.instance.ID_StartOffset, -1000f);
                        if (Info.m_requireSurfaceLine)
                        {
                            instance2.SetWaterMaterialProperties(mesh.bounds.center, material);
                        }
                        if (material.SetPass(0))
                        {
                            TransportManager.instance.m_drawCallData.m_overlayCalls++;
                            Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                        }
                    }
                }
            }
        }

        private void UpdateMesh()
        {
            RenderGroup.MeshData[] array;
            array = m_lineMeshData;
            m_lineMeshData = null;
            if (array != null)
            {
                int num = 0;
                if (m_lineMeshes != null)
                {
                    num = m_lineMeshes.Length;
                }
                if (num != array.Length)
                {
                    Mesh[] array3 = new Mesh[array.Length];
                    int num2 = Mathf.Min(num, array3.Length);
                    for (int i = 0; i < num2; i++)
                    {
                        array3[i] = m_lineMeshes[i];
                    }
                    for (int j = num2; j < array3.Length; j++)
                    {
                        array3[j] = new Mesh();
                    }
                    for (int k = num2; k < num; k++)
                    {
                        UnityEngine.Object.Destroy(m_lineMeshes[k]);
                    }
                    m_lineMeshes = array3;
                }
                for (int l = 0; l < array.Length; l++)
                {
                    m_lineMeshes[l].Clear();
                    m_lineMeshes[l].vertices = array[l].m_vertices;
                    m_lineMeshes[l].normals = array[l].m_normals;
                    m_lineMeshes[l].tangents = array[l].m_tangents;
                    m_lineMeshes[l].uv = array[l].m_uvs;
                    m_lineMeshes[l].uv2 = array[l].m_uvs2;
                    m_lineMeshes[l].colors32 = array[l].m_colors;
                    m_lineMeshes[l].triangles = array[l].m_triangles;
                    m_lineMeshes[l].bounds = array[l].m_bounds;
                }
            }
        }

    }
}