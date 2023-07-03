using Commons.UI.SpriteNames;
using UnityEngine;

namespace TransportLinesManager.Data.Tsd
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition METRO = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportMetro,
            VehicleInfo.VehicleType.Metro,
            TransportInfo.TransportType.Metro,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.MetroTrain },
            new Color32(58, 224, 50, 255),
            180,
            LineIconSpriteNames.SquareIcon,
            true);
    }

}
