using Commons.UI.SpriteNames;
using UnityEngine;

namespace TransportLinesManager.Data.Tsd
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition FERRY = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportShip,
            VehicleInfo.VehicleType.Ferry,
            TransportInfo.TransportType.Ship,
            ItemClass.Level.Level2,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Ferry },
            new Color32(0xe3, 0xf0, 0, 255),
            50,
            LineIconSpriteNames.S08StarIcon,
            true);
    }

}
