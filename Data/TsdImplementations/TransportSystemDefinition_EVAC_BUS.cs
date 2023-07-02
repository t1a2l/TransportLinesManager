using Klyte.Commons.UI.SpriteNames;
using Klyte.TransportLinesManager.Data.Base;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition EVAC_BUS = new TransportSystemDefinition(
                    ItemClass.SubService.None,
            VehicleInfo.VehicleType.Car,
            TransportInfo.TransportType.EvacuationBus,
            ItemClass.Level.Level4,
            new TransferManager.TransferReason[] {
                        TransferManager.TransferReason.EvacuateA,
                        TransferManager.TransferReason.EvacuateB,
                        TransferManager.TransferReason.EvacuateC,
                        TransferManager.TransferReason.EvacuateD,
                        TransferManager.TransferReason.EvacuateVipA,
                        TransferManager.TransferReason.EvacuateVipB,
                        TransferManager.TransferReason.EvacuateVipC,
                        TransferManager.TransferReason.EvacuateVipD
                    },
            new Color32(202, 162, 31, 255),
            50,
            LineIconSpriteNames.K45_CrossIcon,
            false);
    }

}
