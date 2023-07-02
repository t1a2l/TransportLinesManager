using Klyte.Commons.UI.SpriteNames;
using Klyte.TransportLinesManager.Data.Base;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition BLIMP = new TransportSystemDefinition(
            ItemClass.SubService.PublicTransportPlane,
            VehicleInfo.VehicleType.Blimp,
            TransportInfo.TransportType.Airplane,
            ItemClass.Level.Level2,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.Blimp },
            new Color32(0xd8, 0x01, 0xaa, 255),
            35,
            LineIconSpriteNames.K45_ParachuteIcon,
            true);
    }

}
