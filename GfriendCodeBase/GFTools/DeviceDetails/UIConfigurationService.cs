using HP.DeviceAutomation;
using OXPd.Service.UIConfiguration;
using System;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace DeviceDetails
{
    internal class UIConfigurationService : IDisposable
    {
        private readonly string _address;
        private readonly string _adminPassword;
        private readonly string _port;
        private readonly string _language;
        private readonly Lazy<UIConfigurationClient> _oxpdTest;

        /// <summary>
        /// Initializes a new instance of the <see cref="OxpdTestService" /> class.
        /// </summary>
        /// <param name="address">The device address.</param>
        /// <param name="adminPassword">The device admin password.</param>
        /// <param name="portnumber">The device port.</param>
        /// <param name="languageCode">The device language.</param>
        /// <exception cref="ArgumentNullException"><paramref name="address" /> is null.</exception>
        public UIConfigurationService(DeviceNetworkAddress address, string adminPassword, int portnumber, string languageCode)
        {
            _address = address?.StringAddress ?? throw new ArgumentNullException(nameof(address));
            _adminPassword = adminPassword;
            _port = portnumber.ToString();
            _language = languageCode;
            _oxpdTest = new Lazy<UIConfigurationClient>(CreateOxpdTestClient);

            // If allowed, accept unsigned device certificates
            NetworkConfiguration.ConfigureCertificateAcceptance();
        }

        private UIConfigurationClient CreateOxpdTestClient()
        {
            UIConfigurationClient client = new UIConfigurationClient(_address)
            {
                AdminPassword = _adminPassword,
                Port = _port,
            };
            return client;
        }

        public GetApplicationAccessPointsResponse GetApplicationAccessPoint()
        {
            return CallService(n => n.GetApplicationAccessPoint(_language));
        }

        private void CallService(Action<UIConfigurationClient> action)
        {
            bool callActionAsFunc(UIConfigurationClient client)
            {
                action(client);
                return true;
            }

            CallService(callActionAsFunc);
        }

        private T CallService<T>(Func<UIConfigurationClient, T> action, bool allowRetry = true)
        {
            try
            {
                return action(_oxpdTest.Value);
            }
            //catch (FaultException ex) when (allowRetry && ex.Code.SubCode?.Name == "IllegalOperation"
            //                                           && StringMatcher.IsMatch("disabled", ex.Message, StringMatch.Contains, true))
            //{
            //    //Logger.Warn($"OXPd Test service at {_address} is disabled.  Enabling test service.");
            //    bool enableService(OxpdTestClient client)
            //    {
            //        client.EnableService();
            //        return true;
            //    }
            //    CallService(enableService, allowRetry: false);
            //    return CallService(action, allowRetry: false);
            //}
            catch (FaultException ex)
            {
                //LogException(ex);
                throw new DeviceInvalidOperationException(ex.Message);
            }
            catch (MessageSecurityException ex)
            {
                //LogException(ex);
                throw new DeviceSecurityException($"Admin password for device at {_address} is not valid.", ex);
            }
            catch (SecurityNegotiationException ex)
            {
                //LogException(ex);
                throw new NetworkConfigurationException($"Device at {_address} does not have a trusted certificate.");
            }
            catch (TimeoutException ex)
            {
                //LogException(ex);
                throw new DeviceCommunicationException($"OXPd Test service at {_address} did not respond.", ex);
            }
            catch (EndpointNotFoundException ex)
            {
                //LogException(ex);
                throw new DeviceCommunicationException($"OXPd Test service could not be found at {_address}.", ex);
            }
            catch (CommunicationException ex)
            {
                //LogException(ex);
                throw new DeviceCommunicationException($"OXPd Test service at {_address} could not be contacted.", ex);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
