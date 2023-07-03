using Commons.UI.SpriteNames;
using UnityEngine;

namespace TransportLinesManager.Data.Tsd
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition PLANE = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportPlane,
            VehicleInfo.VehicleType.Plane,
            TransportInfo.TransportType.Airplane,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerPlane },
            new Color32(0xa8, 0x01, 0x7a, 255),
            200,
            LineIconSpriteNames.PentagonIcon,
            true,
            ItemClass.Level.Level1);
    }

}
