using Klyte.Commons.UI.SpriteNames;
using Klyte.TransportLinesManager.Data.Base;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition METRO = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportMetro,
            VehicleInfo.VehicleType.Metro,
            TransportInfo.TransportType.Metro,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.MetroTrain },
            new Color32(58, 224, 50, 255),
            180,
            LineIconSpriteNames.K45_SquareIcon,
            true);
    }

}
