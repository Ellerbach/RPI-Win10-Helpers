using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTCoreHelpers;
using Windows.Storage;
using Windows.Devices.WiFi;
using Windows.Security.Credentials;

namespace IoTCoreHelpers.Networking
{
    public class Wifi
    {
        static public async Task UpdateConnectivity(string filename, StorageFolder localFolder = null)
        {
            System.IO.FileStream fileToRead = null;
            try
            {
                if (localFolder == null)
                    localFolder = ApplicationData.Current.LocalFolder;
                fileToRead = new FileStream(await Helpers.GetFilePathAsync(filename, localFolder), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                long fileLength = fileToRead.Length;

                byte[] buf = new byte[fileLength];
                fileToRead.Read(buf, 0, (int)fileLength);

                var str = new string(Encoding.UTF8.GetChars(buf));
                WifiConfig wifi = Newtonsoft.Json.JsonConvert.DeserializeObject<WifiConfig>(str);

                var access = await WiFiAdapter.RequestAccessAsync();
                if (access != WiFiAccessStatus.Allowed)
                    return;
                var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                WiFiAdapter adapter = null;
                if (result.Count >= 1)
                    adapter = await WiFiAdapter.FromIdAsync(result[0].Id);
                else
                    return;
                string connectedSsid = null;
                PasswordCredential tst = new PasswordCredential();
                tst.Password = wifi.pwd;
                foreach (var network in adapter.NetworkReport.AvailableNetworks)
                    if (network.Ssid == wifi.ssid)
                    {
                        var res = await adapter.ConnectAsync(network, WiFiReconnectionKind.Automatic, tst, wifi.ssid);
                        if (res.ConnectionStatus == WiFiConnectionStatus.Success)
                            return;
                    }
                var connectedProfile = await adapter.NetworkAdapter.GetConnectedProfileAsync();
                if (connectedProfile != null &&
                connectedProfile.IsWlanConnectionProfile &&
                connectedProfile.WlanConnectionProfileDetails != null)
                {
                    connectedSsid = connectedProfile.WlanConnectionProfileDetails.GetConnectedSsid();
                }
            }
            catch
            { }
        }
    }
}
