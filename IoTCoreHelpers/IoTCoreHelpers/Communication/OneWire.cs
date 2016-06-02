using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace IoTCoreHelpers.Communication
{
    public class OneWire : IDisposable
    {
        private SerialDevice serialPort = null;
        private DataWriter dataWriteObject = null;
        private DataReader dataReaderObject = null;

        private bool isReading = false;

        public void shutdown()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
                serialPort = null;
            }
        }

        string devID;

        public OneWire(string deviceId)
        {
            devID = deviceId;
        }

        public async Task<bool> onewireReset()
        {
            try
            {
                if (serialPort == null)
                {
                    serialPort = await SerialDevice.FromIdAsync(devID);
                    Debug.WriteLine($"Serial port initialized");
                    // Configure serial settings
                    serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                    serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                    serialPort.BaudRate = 9600; //115200;
                    serialPort.Parity = SerialParity.None;
                    serialPort.StopBits = SerialStopBitCount.One;
                    serialPort.DataBits = 8;
                    serialPort.Handshake = SerialHandshake.None;
                    dataWriteObject = new DataWriter(serialPort.OutputStream);
                    dataReaderObject = new DataReader(serialPort.InputStream);
                }
                serialPort.BaudRate = 9600;
                //changing the datanits forced to update the BaudRate, otherwaise, does not change
                serialPort.DataBits = 7;
                serialPort.DataBits = 8;
                dataWriteObject.WriteByte(0xF0);
                await dataWriteObject.StoreAsync();
                await dataReaderObject.LoadAsync(1);
                byte resp = dataReaderObject.ReadByte();
                if (resp == 0xFF)
                {
                    System.Diagnostics.Debug.WriteLine("Nothing connected to UART");
                    return false;
                }
                else if (resp == 0xF0)
                {
                    System.Diagnostics.Debug.WriteLine("No 1-wire devices are present");
                    return false;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Response: " + resp);
                    serialPort.BaudRate = 115200;
                    serialPort.DataBits = 7;
                    serialPort.DataBits = 8;
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
                return false;
            }
        }

        public async Task onewireWriteByte(byte b)
        {
            for (byte i = 0; i < 8; i++, b = (byte)(b >> 1))
            {
                // Run through the bits in the byte, extracting the
                // LSB (bit 0) and sending it to the bus
                await onewireBit((byte)(b & 0x01));
            }
        }

        public async Task<byte> onewireBit(byte b)
        {
            var bit = b > 0 ? 0xFF : 0x00;
            dataWriteObject.WriteByte((byte)bit);
            await dataWriteObject.StoreAsync();
            byte data = 0;
            await dataReaderObject.LoadAsync(1);
            data = dataReaderObject.ReadByte();
            return (byte)(data & 0xFF);
        }

        public async Task<byte> onewireReadByte()
        {
            byte b = 0;
            for (byte i = 0; i < 8; i++)
            {
                // Build up byte bit by bit, LSB first
                b = (byte)((b >> 1) + 0x80 * await onewireBit(1));
            }
            System.Diagnostics.Debug.WriteLine("onewireReadByte result: " + b);
            return b;
        }

        public void Dispose()
        {
            shutdown();
        }
    }

}
