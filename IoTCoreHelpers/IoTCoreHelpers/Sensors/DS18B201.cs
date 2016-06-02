using IoTCoreHelpers.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTCoreHelpers.Sensors
{
    public class DS18B201:OneWire
    {
        private bool isReading = false;

        public DS18B201(string deviceId) : base(deviceId)
        {
            
        }

        public async Task<double> getTemperature()
        {
            double tempCelsius = -200;

            if (isReading)
                return tempCelsius;

            isReading = true;
            if (await onewireReset())
            {
                await onewireWriteByte(0xCC); //1-Wire SKIP ROM command (ignore device id)
                await onewireWriteByte(0x44); //DS18B20 convert T command 
                                              // (initiate single temperature conversion)
                                              // thermal data is stored in 2-byte temperature 
                                              // register in scratchpad memory

                // Wait for at least 750ms for data to be collated
                await Task.Delay(750);
                // Get the data
                await onewireReset();
                await onewireWriteByte(0xCC); //1-Wire Skip ROM command (ignore device id)
                await onewireWriteByte(0xBE); //DS18B20 read scratchpad command
                                              // DS18B20 will transmit 9 bytes to master (us)
                                              // starting with the LSB

                byte tempLSB = await onewireReadByte(); //read lsb
                byte tempMSB = await onewireReadByte(); //read msb
                //Reset bus to stop sensor sending unwanted data
                await onewireReset();
                // Log the Celsius temperature
                tempCelsius = ((tempMSB * 256) + tempLSB) / 16.0;
                var temp2 = ((tempMSB << 8) + tempLSB) * 0.0625; //just another way of calculating it

                System.Diagnostics.Debug.WriteLine("Temperature: " + tempCelsius + " degrees C " + temp2);
            }
            isReading = false;

            return tempCelsius;
        }

    }
}
