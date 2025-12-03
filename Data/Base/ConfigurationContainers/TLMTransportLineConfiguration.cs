using Commons.Utils.StructExtensions;
using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Interfaces;
using System.Xml.Serialization;

namespace TransportLinesManager.Data.Base.ConfigurationContainers
{
    public class TLMTransportLineConfiguration : IBasicExtensionStorage
    {
        private string customIdentifier;
        private bool isCustom = false;

        [XmlAttribute("isCustom")]
        public bool IsCustom
        {
            get => isCustom; set
            {
                if (!value && isCustom)
                {
                    DisplayAbsoluteValues = false;
                    BudgetEntries = [];
                    AssetList = [];
                    TicketPriceEntries = [];
                    DepotsAllowed = [];
                }
                isCustom = value;
            }
        }
        [XmlAttribute("displayAbsoluteValues")]
        public bool DisplayAbsoluteValues { get; set; } = false;
        [XmlElement("Budget")]
        public TimeableList<BudgetEntryXml> BudgetEntries { get; set; } = [];
        [XmlElement("AssetsList")]
        public SimpleXmlList<string> AssetList { get; set; } = [];
        [XmlElement("TicketPrices")]
        public TimeableList<TicketPriceEntryXml> TicketPriceEntries { get; set; } = [];
        [XmlElement("DepotsAllowed")]
        public SimpleXmlHashSet<ushort> DepotsAllowed { get; set; } = [];

        [XmlAttribute("isZeroed")]
        public bool IsZeroed { get; set; }

        [XmlAttribute("customIdentifier")]
        public string CustomCode
        {
            get => customIdentifier; set
            {
                customIdentifier = value.TrimToNull();
                if (!LoadingManager.instance.m_currentlyLoading)
                {
                    TLMController.Instance.SharedInstance.OnLineSymbolParameterChanged();
                }
            }
        }
    }

}
