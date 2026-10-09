using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Logger = HP.GFriend.GFLogger.Logger;

namespace HP.GFriend.Keywords
{
    public class Preparation : IGFLibrary
    {
        private DeviceUnderTest _dut;
        private static string _outputDir;
        public string GetName()
        {
            return "Preparation";
        }

        public List<string> GetDependencies()
        {
            return null;
        }

        public void Initialize(DeviceUnderTest dut, string outputDir)
        {
            _dut = dut;
            _outputDir = outputDir;
        }

        public void Dispose()
        {
            
        }

        public bool DutUsed()
        {
            return true;
        }

        [KeywordDescription("Install hpk to device")]
        [KeywordDisplayName("Install HPK")]
        [KeywordParameters("hpkPath", "Path of hpk file to install. If path is folder which contains hpk files, install all hpk files in the folder.")]
        [SampleScript("<p>Pass scenario : Preparation.Install HPK(C:\\Users\\BaPr519\\Downloads\\HP_Service_Now-release-white-1.1.0.hpk) <p>Fail scenario 1 :  Preparation.Install HPK(C:\\Users\\BaPr519\\Downloads\\HP_Service_Now-release-white.hpk) //output : Can not find hpk file with given path <p>Fail scenario 2 : If any issue occurs while installing the hpk file , the keyword fails with message - [ HP_Service_Now-release-white-1.1.0.hpk : FAIL ]")]
        public KeywordResult InstallHPK(string hpkPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            bool result = true;
            hpkPath = Support.Utils.GetAbsolutePath(hpkPath, CommonExecutionInfo.ScriptFolder);
            if (Directory.Exists(hpkPath))
            {
                // Install hpk files in directory
                string[] hpkFIles = Directory.GetFiles(hpkPath,"*.hpk");
                foreach (string hpkFile in hpkFIles)
                {
                    HpkInstallData hpkInstaller = new HpkInstallData(hpkFile);
                    if (hpkInstaller.ExecuteInstall(_dut))
                    {
                        result &= true;
                        kr.Output += $"[ {hpkFile.Split('\\').Last()} : PASS ]\r\n";
                        Logger.Debug($"[ {hpkFile.Split('\\').Last()} : is installed ]");
                    }
                    else
                    {
                        result &= false;
                        kr.Output += $"[ {hpkFile.Split('\\').Last()} : FAIL ]\r\n";
                        Logger.Debug($"[ {hpkFile.Split('\\').Last()} : is not installed ]");
                    }
                }
            }
            else if(File.Exists(hpkPath))
            {
                HpkInstallData hpkInstaller = new HpkInstallData(hpkPath);
                if (hpkInstaller.ExecuteInstall(_dut))
                {
                    result &= true;
                }
                else
                {
                    result &= false;
                }
            }
            else
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = "Can not find hpk file with given path";
                return kr;
            }

            if (result)
            {
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            else
            {
                kr.Result = KeywordResults.Fail;
                return kr;
            }
        }

        [KeywordDescription("Uninstall hpk to device")]
        [KeywordDisplayName("Uninstall HPK")]
        [KeywordParameters("hpkPath", "Path of hpk file to uninstall. If path is folder which contains hpk files, uninstall all hpk files in the folder.")]
        [SampleScript("<p>Pass scenario : Preparation.Uninstall HPK(C:\\Users\\BaPr519\\Downloads\\HP_Service_Now-release-white-1.1.0.hpk) <p>Fail scenario :  Preparation.Uninstall HPK(C:\\Users\\BaPr519\\Downloads\\HP_Service_Now-release-white.hpk) //output : Can not find hpk file with given path.<p>Fail scenario 2 : If any issue occurs while uninstalling the hpk file , the keyword fails with message - [ HP_Service_Now-release-white-1.1.0.hpk : FAIL ]")]
        public KeywordResult UninstallHPK(string hpkPath)
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            bool result = true;
            hpkPath = Support.Utils.GetAbsolutePath(hpkPath, CommonExecutionInfo.ScriptFolder);
            if (Directory.Exists(hpkPath))
            {
                // Install hpk files in directory
                string[] hpkFIles = Directory.GetFiles(hpkPath, "*.hpk");
                foreach (string hpkFile in hpkFIles)
                {
                    HpkInstallData hpkInstaller = new HpkInstallData(hpkFile);
                    if (hpkInstaller.ExecuteUninstall(_dut))
                    {
                        result &= true;
                        kr.Output += $"[ {hpkFile.Split('\\').Last()} : PASS ]\r\n";
                        Logger.Debug($"[ {hpkFile.Split('\\').Last()} : is uninstalled ]");
                    }
                    else
                    {
                        result &= false;
                        kr.Output += $"[ {hpkFile.Split('\\').Last()} : FAIL ]\r\n";
                        Logger.Debug($"[ {hpkFile.Split('\\').Last()} : is not uninstalled ]");
                    }
                }
            }
            else if (File.Exists(hpkPath))
            {
                HpkInstallData hpkInstaller = new HpkInstallData(hpkPath);
                if (hpkInstaller.ExecuteUninstall(_dut))
                {
                    result &= true;
                }
                else
                {
                    result &= false;
                }
            }
            else
            {
                kr.Result = KeywordResults.Fail;
                kr.Output = "Can not find hpk file with given path";
                return kr;
            }

            if (result)
            {
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            else
            {
                kr.Result = KeywordResults.Fail;
                return kr;
            }
        }

        [KeywordDescription("Uninstall all hpk packages to device")]
        [KeywordDisplayName("Uninstall all HPK")]
        [SampleScript("<p>Pass Scenario : Preparation.Uninstall all HPK //Output: Uninstall all hpk : PASS ] <p>Fail Scenario : Preparation.Uninstall all HPK //Output: All hpk packages are not uninstalled ]")]
        public KeywordResult UninstallAllHPK()
        {
            KeywordResult kr = new KeywordResult(KeywordResults.Pass);
            bool result = true;
            HpkInstallData hpkInstaller = new HpkInstallData();
            if (hpkInstaller.ExecuteClear(_dut))
            {
                result &= true;
                kr.Output += $"Uninstall all hpk : PASS ]\r\n";
                Logger.Debug($"All hpk packages are uninstalled ]");
            }
            else
            {
                result &= false;
                kr.Output += $"Uninstall all hpk : FAIL ]\r\n";
                Logger.Debug($"All hpk packages are not uninstalled ]");
            }            

            if (result)
            {
                kr.Result = KeywordResults.Pass;
                return kr;
            }
            else
            {
                kr.Result = KeywordResults.Fail;
                return kr;
            }
        }

    }
}
