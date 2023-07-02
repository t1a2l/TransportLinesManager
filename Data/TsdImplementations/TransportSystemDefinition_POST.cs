using Klyte.TransportLinesManager.Data.Base;

namespace Klyte.TransportLinesManager.Data.TsdImplementations
{
    public partial class TransportSystemDefinitionType
    {
        public static readonly TransportSystemDefinition POST = new TransportSystemDefinition(
                    ItemClass.SubService.PublicTransportPost,
            VehicleInfo.VehicleType.None,
            TransportInfo.TransportType.Post,
            ItemClass.Level.Level2,
            new TransferManager.TransferReason[] { },
            default,
            1,
            default,
            false);  
    }

}
