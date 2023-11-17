using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Data.Base;
using System.Collections.Generic;
using UnityEngine;
using TransportLinesManager.Utils;

namespace TransportLinesManager.Interfaces
{
    public interface ILineNumberToIndexable
    {
        uint LineToIndex(ushort lineId);
    }

    public interface IBudgetableExtension : ISafeGettable<IBudgetStorage>, ILineNumberToIndexable
    {
    }
    public interface IBudgetStorage
    {
        TimeableList<BudgetEntryXml> BudgetEntries { get; set; }
    }

    public interface INameableExtension : ISafeGettable<INameableStorage>, ILineNumberToIndexable
    {
    }

    public interface INameableStorage
    {
        string Name { get; set; }
    }

    public interface ITicketPriceExtension : ISafeGettable<ITicketPriceStorage>, ILineNumberToIndexable
    {
        uint GetDefaultTicketPrice(uint rel);
    }

    public interface ITicketPriceStorage
    {
         TimeableList<TicketPriceEntryXml> TicketPriceEntries { get; set; }
    }

    public interface IAssetSelectorExtension : ISafeGettable<IAssetSelectorStorage>, ILineNumberToIndexable
    {
        Dictionary<TransportAsset, string> GetAllBasicAssetsForLine(ushort lineId);
        List<TransportAsset> GetBasicAssetListForLine(ushort lineId);
        VehicleInfo GetAModel(ushort lineId, string status);
        void EditVehicleUsedCount(ushort lineID, string selectedModel, string status);
    }

    public interface IAssetSelectorStorage
    {
        SimpleXmlList<TransportAsset> AssetTransportList { get; set; }
        SimpleXmlList<string> AssetList { get; set; }
    }

    public interface IColorSelectableExtension : ISafeGettable<IColorSelectableStorage>, ILineNumberToIndexable
    {
    }
    public interface IColorSelectableStorage
    {
        Color Color { get; set; }
    }
    public interface ISafeGettable<T>
    {
        T SafeGet(uint index);
    }

    public interface IDepotSelectableExtension : ISafeGettable<IDepotSelectionStorage>, ILineNumberToIndexable
    {
    }

    public interface IDepotSelectionStorage
    {
        SimpleXmlHashSet<ushort> DepotsAllowed { get; set; }
    }

    public interface IBasicExtension : IAssetSelectorExtension, IBudgetableExtension, ITicketPriceExtension, IDepotSelectableExtension, ISafeGettable<IBasicExtensionStorage>
    {
    }
    public interface IBasicExtensionStorage : IAssetSelectorStorage, IBudgetStorage, ITicketPriceStorage, IDepotSelectionStorage
    { }

}
