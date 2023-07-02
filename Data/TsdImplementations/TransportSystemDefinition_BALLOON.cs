using Klyte.TransportLinesManager.Data.Base;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition BALLOON = new TransportSystemDefinition(
            ItemClass.SubService.PublicTransportTours,
            VehicleInfo.VehicleType.Balloon,
            TransportInfo.TransportType.HotAirBalloon,
            ItemClass.Level.Level4,
            new TransferManager.TransferReason[]
                {
                    TransferManager.TransferReason.TouristA,
                    TransferManager.TransferReason.TouristB,
                    TransferManager.TransferReason.TouristC,
                    TransferManager.TransferReason.TouristD
                },
            default,
            1,
            default,
            false);
     
    }

}
