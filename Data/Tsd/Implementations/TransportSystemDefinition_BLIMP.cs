using Commons.UI.SpriteNames;
using UnityEngine;

namespace TransportLinesManager.Data.Tsd
{
    public partial class TransportSystemDefinition
    {
        public static readonly TransportSystemDefinition BLIMP = new TransportSystemDefinition(
            ItemClass.SubService.PublicTransportPlane,
            VehicleInfo.VehicleType.Blimp,
            TransportInfo.TransportType.Airplane,
            ItemClass.Level.Level2,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Blimp },
            new Color32(0xd8, 0x01, 0xaa, 255),
            35,
            LineIconSpriteNames.ParachuteIcon,
            true);
    }

}
