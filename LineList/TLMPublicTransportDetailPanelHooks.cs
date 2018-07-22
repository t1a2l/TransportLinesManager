﻿using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using Klyte.TransportLinesManager.UI;
using Klyte.TransportLinesManager.Utils;
using System.Reflection;

namespace Klyte.TransportLinesManager.LineList
{
    class TLMPublicTransportDetailPanelHooks : Redirector<TLMPublicTransportDetailPanelHooks>
    {

        private static bool OpenDetailPanel(int idx)
        {
            TransportSystemDefinition def;
            UiCategoryTab cat = UiCategoryTab.LineListing;
            switch (idx)
            {
                case 0:
                    def = TransportSystemDefinition.BUS;
                    break;
                case 1:
                    def = TransportSystemDefinition.TRAM;
                    break;
                case 2:
                    def = TransportSystemDefinition.METRO;
                    break;
                case 3:
                    def = TransportSystemDefinition.TRAIN;
                    break;
                case 4:
                    def = TransportSystemDefinition.FERRY;
                    break;
                case 5:
                    def = TransportSystemDefinition.BLIMP;
                    break;
                case 6:
                    def = TransportSystemDefinition.MONORAIL;
                    break;
                case 8:
                    cat = UiCategoryTab.TourListing;
                    def = TransportSystemDefinition.TOUR_PED;
                    break;
                case 9:
                    cat = UiCategoryTab.TourListing;
                    def = TransportSystemDefinition.TOUR_BUS;
                    break;
                default:
                    def = TransportSystemDefinition.BUS;
                    break;
            }

            TLMPublicTransportManagementPanel.instance?.OpenAt(cat, def);
            return false;
        }

        public static bool OpenDetailPanelDefaultTab()
        {
            OpenDetailPanel(0);
            return false;
        }

        #region Hooking

        public static bool preventDefault()
        {
            return false;
        }

        public override void AwakeBody()
        {

            MethodInfo OpenDetailPanel = typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OpenDetailPanel", allFlags);
            MethodInfo OpenDetailPanelDefaultTab = typeof(TLMPublicTransportDetailPanelHooks).GetMethod("OpenDetailPanelDefaultTab", allFlags);

            TLMUtils.doLog("Loading PublicTransportInfoViewPanel Hooks!");
            AddRedirect(typeof(PublicTransportInfoViewPanel).GetMethod("OpenDetailPanel", allFlags), OpenDetailPanel);
            AddRedirect(typeof(PublicTransportInfoViewPanel).GetMethod("OpenDetailPanelDefaultTab", allFlags), OpenDetailPanelDefaultTab);
            AddRedirect(typeof(ToursInfoViewPanel).GetMethod("OpenDetailPanel", allFlags), OpenDetailPanel);

            var preventDefault = typeof(TLMPublicTransportDetailPanelHooks).GetMethod("preventDefault", allFlags);
            var from3 = typeof(PublicTransportLineInfo).GetMethod("RefreshData", allFlags);
            TLMUtils.doErrorLog("Muting PublicTransportLineInfo: {0} ({1}=>{2}})", typeof(PublicTransportLineInfo), from3, preventDefault);
            AddRedirect(from3, preventDefault);
        }

        private string getOrdinal(int nth)
        {
            if (nth % 10 == 1 && nth % 100 != 11)
            {
                return "st";
            }
            else if (nth % 10 == 2 && nth % 100 != 12)
            {
                return "nd";
            }
            else if (nth % 10 == 3 && nth % 100 != 13)
            {
                return "rd";
            }
            else
            {
                return "th";
            }
        }
        public override void doLog(string text, params object[] param)
        {
            TLMUtils.doLog(text, param);
        }


        #endregion
    }

}
