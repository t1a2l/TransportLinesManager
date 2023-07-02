using Klyte.Commons.UI.SpriteNames;
using Klyte.TransportLinesManager.Data.Base;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition TAXI = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportTaxi,
            VehicleInfo.VehicleType.Car,
            TransportInfo.TransportType.Taxi,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Taxi },
            new Color32(60, 184, 120, 255),
            1,
            LineIconSpriteNames.K45_TriangleIcon,
            false);
    }

}
