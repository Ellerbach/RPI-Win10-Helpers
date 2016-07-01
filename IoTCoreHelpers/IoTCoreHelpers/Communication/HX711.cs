using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTCoreHelpers.Communication
{
    public enum InputAndGainOption : int
    {
        A128 = 1, B32 = 2, A64 = 3
    }

    //24-Bit Analog-to-Digital Converter (ADC) for Weigh Scales
    public class HX711
    {
        #region setup

        //PD_SCK
        private GpioPin PowerDownAndSerialClockInput;

        //DOUT
        private GpioPin SerialDataOutput;
        private int retry;
        private bool isOn = false;

        public HX711(GpioPin powerDownAndSerialClockInput, GpioPin serialDataOutput, int retrymillisec)
        {
            PowerDownAndSerialClockInput = powerDownAndSerialClockInput;
            powerDownAndSerialClockInput.SetDriveMode(GpioPinDriveMode.Output);

            SerialDataOutput = serialDataOutput;
            SerialDataOutput.SetDriveMode(GpioPinDriveMode.Input);
            retry = retrymillisec;
        }
        public HX711(GpioPin powerDownAndSerialClockInput, GpioPin serialDataOutput) : this(powerDownAndSerialClockInput, serialDataOutput, 10000)
        { }

        #endregion

        #region data retrieval

        //When output data is not ready for retrieval,
        //digital output pin DOUT is high.
        private bool IsReady()
        {
            return SerialDataOutput.Read() == GpioPinValue.Low;
        }
        //By applying 25~27 positive clock pulses at the
        //PD_SCK pin, data is shifted out from the DOUT
        //output pin.Each PD_SCK pulse shifts out one bit,
        //starting with the MSB bit first, until all 24 bits are
        //shifted out.
        public int Read()
        {
            if (!isOn)
                PowerOn();
            DateTime dtinit = DateTime.Now;
            dtinit = dtinit.AddMilliseconds(retry);
            while (!IsReady())
            {
                if (dtinit.CompareTo(DateTime.Now) < 0)
                    return int.MaxValue;
            }
            string binaryData = "";
            for (int pulses = 0; pulses < 25 + (int)InputAndGainSelection; pulses++)
            {
                PowerDownAndSerialClockInput.Write(GpioPinValue.High);
                PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
                if (pulses < 25)
                    binaryData += (int)SerialDataOutput.Read();
            }
            return Convert.ToInt32(binaryData, 2);
        }

        public int ReadA128()
        {
            InputAndGainSelection = InputAndGainOption.A128;
            return Read();
        }
        public int ReadB32()
        {
            InputAndGainSelection = InputAndGainOption.B32;
            return Read();
        }

        public int ReadA64()
        {
            InputAndGainSelection = InputAndGainOption.A128;
            return Read();
        }

        #endregion

        #region input selection/ gain selection

        private InputAndGainOption _InputAndGainSelection = InputAndGainOption.A128;

        public InputAndGainOption InputAndGainSelection
        {
            get
            {
                return _InputAndGainSelection;
            }
            set
            {
                _InputAndGainSelection = value;
                Read();
            }
        }

        #endregion

        #region power

        //When PD_SCK pin changes from low to high
        //and stays at high for longer than 60µs, HX711
        //enters power down mode
        public void PowerDown()
        {
            PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
            PowerDownAndSerialClockInput.Write(GpioPinValue.High);
            isOn = false;
            //wait 60 microseconds
        }

        //When PD_SCK returns to low,
        //chip will reset and enter normal operation mode
        public void PowerOn()
        {           
            PowerDownAndSerialClockInput.Write(GpioPinValue.Low);
            _InputAndGainSelection = InputAndGainOption.A128;
            isOn = true;
        }
        //After a reset or power-down event, input
        //selection is default to Channel A with a gain of 128. 

        #endregion

    }
}
