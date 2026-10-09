using HP.GFriend.GFLogger;
using RobotClient;
using RobotClient.Endpoints;
using RobotClient.Endpoints.Data;
using System;
using System.Linq;
using System.Collections.Generic;

namespace HP.GFriend.Keywords
{
    [LibraryDescription("Badge box address should be given in device additional capabilites. Use 'BadgeBoxAddress' as a key.")]
    public class BadgeBox : IGFLibrary
    {
        private DeviceUnderTest _dut;
        private string _address;
        private RobotControllerClient _badgeBox;
        private RfidSwitch _badgeBoxEndPoint;
        public void Dispose()
        {
            
            if(_badgeBoxEndPoint !=null)
            {
                _badgeBoxEndPoint.Dispose();
                _badgeBoxEndPoint = null;
            }
            
            if(_badgeBox != null)
            {
                _badgeBox = null;
            }
            
        }

        public bool DutUsed()
        {
            return true;
        }

        public List<string> GetDependencies()
        {
            return null;
        }

        public string GetName()
        {
            return "BadgeBox";
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
        
            _dut = dut;
            if(_dut.AdditionalCapabilites.Count(c=> c.Key.Equals("BadgeBoxAddress", StringComparison.CurrentCultureIgnoreCase)) <1)
            {
                throw new LibraryInitializeException("Address of Badge Box should be given in device additional capabilites with key 'BadgeBoxAddress'");
            }


            _badgeBox = new RobotControllerClient(_dut.AdditionalCapabilites.Where(c=>c.Key.Equals("BadgeBoxAddress", StringComparison.CurrentCultureIgnoreCase)).First().Value);

            _badgeBoxEndPoint = _badgeBox.GetEndpoint<RfidSwitch>();
            
        }


        [KeywordDescription("Scan badge at index. This keyword does not ensure if login is successful or not. Need to check login after scan.")]
        [KeywordDisplayName("Scan Badge")]
        [KeywordParameters("index", "index of the badge to scan (0~3)")]
        [SampleScript("BadgeBox.Scan Badge(0)\r\n Usage:\r\nBadge Box\r\n{\r\n  //Badge Box Auth\r\nBadgeBox.Scan Badge(0)\r\nSleep (5)\r\nAndroid.Touch ID (com.hp.print.authagent.secureaccess:id/edtPasscode)\r\n Android.Input Text (com.hp.print.authagent.secureaccess:id/edtPasscode,123456)\r\n Android.Wait For Object (com.hp.print.authagent.secureaccess:id/btnOK,5)\r\n Android.Touch ID (com.hp.print.authagent.secureaccess:id/btnOK)\r\n Sleep (30)\r\n }")]
        public KeywordResult ScanBadge(string index)
        {
            if(!int.TryParse(index, out int iIndex))
            {
                return new KeywordResult(KeywordResults.Error, "Index should be a number");
            }

            RfidActivateResult result = _badgeBoxEndPoint.Activate(iIndex);
            
            if (result.EndOffset != null)
            {
                Logger.Debug("Scan successful.");
                return new KeywordResult(KeywordResults.Pass);
            }
            return new KeywordResult(KeywordResults.Fail, "Badge scan failed");
        }
    }
}
