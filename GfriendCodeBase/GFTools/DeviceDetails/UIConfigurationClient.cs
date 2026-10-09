using HP.DeviceAutomation;
using HP.DeviceAutomation.Jedi.Oxpd.UIConfiguration;
using OXPd.Service.UIConfiguration;
using System;
using System.Net;
using System.ServiceModel;

namespace DeviceDetails
{
    public sealed class UIConfigurationClient : ClientBase<IUIConfigurationService>
    {
        /// <summary>
        /// Gets the address of the device this <see cref="UIConfigurationClient" /> will connect to.
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// Gets or sets the device admin password.
        /// </summary>
        public string AdminPassword
        {
            get => ClientCredentials.UserName.Password;
            set => ClientCredentials.UserName.Password = value;
        }
        public string Port { get; internal set; }
        public string Language { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OxpdUIConfigurationClient" /> class.
        /// </summary>
        /// <param name="address">The device address.</param>
        /// <exception cref="ArgumentNullException"><paramref name="address" /> is null.</exception>
        public UIConfigurationClient(string address)
            : base(new BsiHttpBinding(SecurityMode.Transport), BuildEndpointAddress(address))
        {
            Address = address;
            ClientCredentials.UserName.UserName = "admin";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OxpdUIConfigurationClient" /> class.
        /// </summary>
        /// <param name="address">The device address.</param>
        /// <exception cref="ArgumentNullException"><paramref name="address" /> is null.</exception>
        public UIConfigurationClient(IPAddress address)
            : this(address?.ToString() ?? throw new ArgumentNullException(nameof(address)))
        {
        }

        private static EndpointAddress BuildEndpointAddress(string address)
        {
            if (address is null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            UriBuilder builder = new UriBuilder(Uri.UriSchemeHttps, address, 7627, "hp/device/webservices/OXPd/UIConfigurationService");
            return new EndpointAddress(builder.Uri);
        }

        /// <summary>
        /// Gets the user interface profile of the device.
        /// </summary>
        /// <returns>A <see cref="UIProfile" /> object containing the UI profile information.</returns>
        public GetApplicationAccessPointsResponse GetApplicationAccessPoint(String languageCode)
        {
            GetApplicationAccessPointsRequest request = new GetApplicationAccessPointsRequest(languageCode);
            return Channel.GetApplicationAccessPoints(request);
        }

    }
}
