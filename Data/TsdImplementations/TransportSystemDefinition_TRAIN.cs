using Klyte.Commons.UI.SpriteNames;
using Klyte.TransportLinesManager.Data.Base;
using UnityEngine;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition TRAIN = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportTrain,
            VehicleInfo.VehicleType.Train,
            TransportInfo.TransportType.Train,
            ItemClass.Level.Level1,
            new TransferManager.TransferReason[] { TransferManager.TransferReason.PassengerTrain },
            new Color32(250, 104, 0, 255),
            240,
            LineIconSpriteNames.K45_CircleIcon,
            true,
            ItemClass.Level.Level1);
    }

}
