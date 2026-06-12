using System.Linq;
using ColossalFramework.UI;
using Commons.Utils;
using TransportLinesManager.Utils;
using UnityEngine;

namespace TransportLinesManager.WorldInfoPanels.NearLines
{
    public class TLMNearLineRowControl : UICustomControl
    {
        public const string ROW_TEMPLATE = "TLM_NearLinesRowTemplate";

        private UIPanel m_root;
        private UIButton m_badgeHost;
        private TLMLineItemButtonControl m_badge;
        private UILabel m_name;
        private UILabel m_waiting;

        private bool m_fromBuilding;
        private ushort m_lineId;
        private Vector3 m_position;
        private bool m_initialized;

        public static void EnsureTemplate()
        {
            if (UITemplateUtils.GetTemplateDict().ContainsKey(ROW_TEMPLATE))
            {
                return;
            }

            var go = new GameObject(ROW_TEMPLATE);
            var panel = go.AddComponent<UIPanel>();
            panel.name = ROW_TEMPLATE;
            panel.width = 285;
            panel.height = 42;
            panel.autoLayout = false;
            panel.name = ROW_TEMPLATE;
            panel.isVisible = false;
            panel.canFocus = true;
            panel.isInteractive = true;
            panel.eventMouseHover += (c, p) => panel.color = new Color32(255, 255, 255, 20);
            panel.eventEnterFocus += (c, p) => panel.color = new Color32(255, 255, 255, 30);

            var badgeTemplate = Instantiate(UITemplateUtils.GetTemplateDict()[TLMLineItemButtonControl.LINE_ITEM_TEMPLATE].gameObject);
            badgeTemplate.transform.SetParent(panel.transform, false);
            badgeTemplate.name = "LineBadge";
            var badgeButton = badgeTemplate.GetComponent<UIButton>();
            badgeButton.width = 40;
            badgeButton.height = 40;
            badgeButton.relativePosition = new Vector3(2, 2);

            MonoUtils.CreateUIElement(out UILabel name, panel.transform);
            name.name = "LineName";
            name.relativePosition = new Vector3(50, 12);
            name.width = 190;
            name.height = 20;
            name.textAlignment = UIHorizontalAlignment.Left;
            name.verticalAlignment = UIVerticalAlignment.Middle;
            name.textScale = 0.75f;
            name.autoSize = false;

            MonoUtils.CreateUIElement(out UILabel waiting, panel.transform);
            waiting.name = "Waiting";
            waiting.relativePosition = new Vector3(230, 12);
            waiting.width = 50;
            waiting.height = 20;
            waiting.textAlignment = UIHorizontalAlignment.Center;
            waiting.verticalAlignment = UIVerticalAlignment.Middle;
            waiting.textScale = 0.75f;
            waiting.autoSize = false;

            go.AddComponent<TLMNearLineRowControl>();
            UITemplateUtils.GetTemplateDict()[ROW_TEMPLATE] = panel;
        }


        private bool EnsureReferences()
        {
            if (m_initialized)
            {
                return true;
            }

            m_root = GetComponent<UIPanel>();
            m_badgeHost = GetComponentsInChildren<UIButton>(true).FirstOrDefault(x => x.name == "LineBadge");
            m_name = GetComponentsInChildren<UILabel>(true).FirstOrDefault(x => x.name == "LineName");
            m_waiting = GetComponentsInChildren<UILabel>(true).FirstOrDefault(x => x.name == "Waiting");

            if (m_badgeHost != null)
            {
                m_badge = m_badgeHost.GetComponent<TLMLineItemButtonControl>();
                if (m_badge == null)
                {
                    m_badge = m_badgeHost.gameObject.AddComponent<TLMLineItemButtonControl>();
                }
            }

            if (m_root != null)
            {
                m_root.eventClick -= OnRowClick;
                m_root.eventClick += OnRowClick;
            }
            if (m_name != null)
            {
                m_name.eventClick -= OnAnyClick;
                m_name.eventClick += OnAnyClick;
            }
            if (m_waiting != null)
            {
                m_waiting.eventClick -= OnAnyClick;
                m_waiting.eventClick += OnAnyClick;
            }
            if (m_badgeHost != null)
            {
                m_badgeHost.eventClick -= OnAnyClick;
                m_badgeHost.eventClick += OnAnyClick;
            }

            m_initialized = m_root != null && m_badgeHost != null && m_badge != null && m_name != null && m_waiting != null;
            return m_initialized;
        }

        public void ResetData(bool fromBuilding, ushort lineId, Vector3 position, string lineName, int waiting)
        {
            if (!EnsureReferences())
            {
                Debug.LogError($"TLMNearLineRowControl not initialized: obj={gameObject.name}");
                return;
            }

            m_fromBuilding = fromBuilding;
            m_lineId = lineId;
            m_position = position;

            m_badge.ResetData(fromBuilding, lineId, position);
            m_badge.Resize(40);

            m_name.text = Shorten(lineName, 24);
            m_name.tooltip = lineName ?? "";

            m_waiting.text = waiting >= 0 ? waiting.ToString("N0") : "-";
            m_waiting.tooltip = waiting >= 0 ? $"{waiting:N0} waiting" : "Waiting count unavailable";
            m_root.isVisible = true;
        }

        private void OnRowClick(UIComponent c, UIMouseEventParameter p) => OpenWorldInfo();
        private void OnAnyClick(UIComponent c, UIMouseEventParameter p) => OpenWorldInfo();

        private void OpenWorldInfo()
        {
            if (!m_fromBuilding)
            {
                if (m_lineId == 0) return;
                InstanceID iid = InstanceID.Empty;
                iid.TransportLine = m_lineId;
                WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(m_position, iid);
            }
            else
            {
                InstanceID iid = InstanceID.Empty;
                iid.Set(TLMInstanceType.BuildingLines, m_lineId);
                WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(m_position, iid);
            }
        }

        private static string Shorten(string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s) || s.Length <= maxLen)
            {
                return s ?? "";
            }
            return s.Substring(0, maxLen - 1) + "…";
        }
    }
}
