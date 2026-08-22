using Commons.Utils;

namespace TransportLinesManager.WorldInfoPanels.Components.TLMVehicleSelector
{
    public class VehicleItem
    {
        // Vehicle prefab.
        private VehicleInfo m_vehicleInfo;
        private string m_vehicleName;

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleItem"/> class.
        /// </summary>
        /// <param name="prefab">Vehicle prefab for this item.</param>
        public VehicleItem(VehicleInfo prefab)
        {
            Info = prefab;
        }

        /// <summary>
        /// Gets the vehicles's name (empty string if none).
        /// </summary>
        public string Name => m_vehicleName;

        /// <summary>
        /// Gets or sets the vehicle prefab for this record.
        /// </summary>
        public VehicleInfo Info
        {
            get => m_vehicleInfo;

            set
            {
                m_vehicleInfo = value;

                // Set display name.
                m_vehicleName = PrefabUtils.GetDisplayName(value);
            }
        }
    }
}
