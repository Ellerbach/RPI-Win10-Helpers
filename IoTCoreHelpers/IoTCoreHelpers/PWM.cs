using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;

namespace IoTCoreHelpers
{
    public sealed class PWM
    {
        // use to determine the freqncy of the PWM
        // PulseFrequency = total frenquency
        //curent pulse width = when the signal is hig
        private double currentPulseWidth;
        private double pulseFrequency;
        //use to determine the length of the pulse
        //100 % = full output. 0%= nothing as output
        private int percentage;
        private bool precisionPWM;

        private bool isRunning;
        private bool istopped = true;
        private GpioPin servoPin;

        private Stopwatch stopwatch = Stopwatch.StartNew();

        /// <summary>
        /// Set the percentage for the pulse width
        /// 100 = full pulse
        /// 0 = no pulse
        /// 50 = half pulse
        /// </summary>
        public int Percentage
        {
            get
            {
                return percentage;
            }

            set
            {
                if (value > 100)
                    value = 100;
                if (value < 0)
                    value = 0;
                percentage = value;
                UpdateRange();
            }
        }

        /// <summary>
        /// Set the Frequency of the pulse in micro seconds
        /// </summary>
        public uint PulsePeriod
        {
            get
            {
                return (uint)(pulseFrequency * 1000);
            }

            set
            {
                pulseFrequency = value / 1000.0;
                UpdateRange();
            }
        }

        /// <summary>
        /// Use or not the precision timer
        /// when using the precision timer, the thread is high priority and no async is used
        /// When using the non precision timer, the thred is high priority but using classical async to wait
        /// It is recommended to use precision timer for servo motors
        /// </summary>
        public bool PrecisionPWM
        {
            get
            {
                return precisionPWM;
            }

            set
            {
                precisionPWM = value;
            }
        }

        /// <summary>
        /// Initialize the PWM
        /// </summary>
        /// <param name="pin">GPIO pin number to use</param>
        /// <param name="Period">Period of the PWM in micro seconds</param>
        /// <param name="Percentage">Initial percentage of PWM</param>
        /// <param name="Precision">True to use the precision timer</param>
        public PWM(int pin, uint Period = 20000, int Percentage = 0, bool Precision = true)
        {
            if (Period >= 0)
                pulseFrequency = Period / 1000.0;
            else
                pulseFrequency = 0.0;
            percentage = Percentage;
            precisionPWM = Precision;
            isRunning = false;
            UpdateRange();

            GpioController controller = GpioController.GetDefault();
            if (controller == null)
            {
                Debug.WriteLine("GPIO does not exist on the current system.");
                return;
            }
            servoPin = controller.OpenPin(pin);
            servoPin.SetDriveMode(GpioPinDriveMode.Output);
            Debug.WriteLine("GPIO initialized in PWM");
            //You do not need to await this, as your goal is to have this run for the lifetime of the application

            Windows.System.Threading.ThreadPool.RunAsync(this.PWMThread, Windows.System.Threading.WorkItemPriority.High);
            Debug.WriteLine("MotorThread initialized in PWM");

        }

        //A synchronous wait is used to avoid yielding the thread 
        //This method calculates the number of CPU ticks will elapse in the specified time and spins
        //in a loop until that threshold is hit. This allows for very precise timing.
        public void Wait(double milliseconds)
        {
            long initialTick = stopwatch.ElapsedTicks;
            long initialElapsed = stopwatch.ElapsedMilliseconds;
            double desiredTicks = milliseconds / 1000.0 * Stopwatch.Frequency;
            double finalTick = initialTick + desiredTicks;
            while (stopwatch.ElapsedTicks < finalTick)
            {
                //nothing than waiting
            }
        }

        private async void PWMThread(IAsyncAction action)
        {
            Debug.WriteLine("PWMThread lunched");
            while (true)
            {
                //Write the pin high for the appropriate length of time
                if (isRunning)
                {
                    if (currentPulseWidth != 0)
                    {
                        if (servoPin != null)
                            servoPin.Write(GpioPinValue.High);
                    }
                    //Use the wait helper method to wait for the length of the pulse
                    if (precisionPWM)
                        Wait(currentPulseWidth);
                    else
                        Task.Delay(TimeSpan.FromMilliseconds(currentPulseWidth)).Wait();
                    //The pulse if over and so set the pin to low and then wait until it's time for the next pulse
                    if (servoPin != null)
                        servoPin.Write(GpioPinValue.Low);
                    if (precisionPWM)
                        Wait(pulseFrequency - currentPulseWidth);
                    else
                        Task.Delay(TimeSpan.FromMilliseconds(pulseFrequency - currentPulseWidth)).Wait();
                }
                else
                {
                    if (!istopped)
                    {
                        if (servoPin != null)
                            servoPin.Write(GpioPinValue.Low);
                        istopped = true;
                    }
                }
            }
        }

        private void UpdateRange()
        {
            currentPulseWidth = percentage * pulseFrequency / 100;
        }

        /// <summary>
        /// Start the PWM
        /// </summary>
        public void Start()
        {
            //set to running mode
            isRunning = true;
            istopped = false;
        }

        /// <summary>
        /// Stop the PWM
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            //it will stop when the cycle will finish
        }

    }
}
