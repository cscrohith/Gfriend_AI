using System;
using System.Net;

namespace DeviceDetails
{
    public sealed class DeviceNetworkAddress
    {
        public string StringAddress { get; }
        public IPAddress IPAddress { get; }

        public DeviceNetworkAddress(string address)
        {
            StringAddress = address ?? throw new ArgumentNullException(nameof(address));
        }

        public DeviceNetworkAddress(IPAddress address)
        {
            IPAddress = address ?? throw new ArgumentNullException(nameof(address));
            StringAddress = address.ToString();
        }

        public override string ToString()
        {
            return StringAddress;
        }
    }
}
