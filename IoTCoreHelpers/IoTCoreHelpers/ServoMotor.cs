using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;

namespace IoTCoreHelpers
{
    class ServoMotor
    {
        private GpioPin servoPin;
        private double PulseFrequency = 20;

        double currentPulseWidth = 0;

        private float angle;
        /// <summary>
        /// Servo motor definition.
        /// </summary>
        private ServoMotorDefinition definition;
        /// <summary>
        /// The duration per angle's degree.
        /// </summary>
        private double rangePerDegree;
        private Helpers.Waiting myCounter;
        private CancellationTokenSource tockenAgle;

        public ServoMotor(int PinNumber, ServoMotorDefinition definition)
        {
            tockenAgle = new CancellationTokenSource();
            myCounter = new Helpers.Waiting();
            this.definition = definition;
            PulseFrequency = definition.Period / 1000.0;
            UpdateRange();

            GpioController controller = GpioController.GetDefault();
            if (controller == null)
            {
                Debug.WriteLine("GPIO does not exist on the current system.");
                return;
            }
            servoPin = controller.OpenPin(PinNumber);
            servoPin.SetDriveMode(GpioPinDriveMode.Output);
            Debug.WriteLine("GPIO initialized in ServoMotor");
            //You do not need to await this, as your goal is to have this run for the lifetime of the application

            Windows.System.Threading.ThreadPool.RunAsync(this.MotorThread, Windows.System.Threading.WorkItemPriority.High);
            Debug.WriteLine("MotorThread initialized in ServoMotor");
        }

        private void MotorThread(IAsyncAction action)
        {
            Debug.WriteLine("MotorThread lunched ServoMotor");
            while (true)
            {
                //Write the pin high for the appropriate length of time
                if (currentPulseWidth != 0)
                {
                    if (servoPin != null)
                        servoPin.Write(GpioPinValue.High);
                }
                //Use the wait helper method to wait for the length of the pulse
                myCounter.Wait(currentPulseWidth);
                //The pulse if over and so set the pin to low and then wait until it's time for the next pulse
                if (servoPin != null)
                    servoPin.Write(GpioPinValue.Low);
                myCounter.Wait(PulseFrequency - currentPulseWidth);
            }
        }

        public void SetPulse(uint period, uint duration)
        {
            PulseFrequency = period / 1000.0;
            currentPulseWidth = duration / 1000.0;
        }
        /// <summary>
        /// Is it moving?
        /// </summary>
        /// <remarks>Return if the Servo Motor is currently moving or not.</remarks>
        public bool IsMoving
        {
            get
            {
                if (currentPulseWidth != 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Current angle.
        /// </summary>
        /// <remarks>Motor won't move until Angle is set explicitly.</remarks>
        public float Angle
        {
            get
            {
                return angle;
            }
            set
            {
                SetAngle(value);
            }
        }

        private async Task SetAngle(float value)
        {
            if (definition.IsMovingPeriod != 0)
            {
                if(tockenAgle!=null)
                    if (!tockenAgle.IsCancellationRequested)
                    {
                        tockenAgle.Cancel();
                        tockenAgle.Dispose();
                        tockenAgle = null;
                    }
            }
            float newAngle;
            if (value > definition.AngleMax)
                newAngle = definition.AngleMax;
            else if (value < 0)
                newAngle = 0;
            else
                newAngle = value;
            if (newAngle != Angle)
            {
                angle = newAngle;
                Rotate();
                if (definition.IsMovingPeriod != 0)
                {
                    var t = Task.Run(async delegate
                    {
                        if(tockenAgle==null)
                            tockenAgle = new CancellationTokenSource();
                        await Task.Delay((int)definition.IsMovingPeriod, tockenAgle.Token);
                        currentPulseWidth = 0;
                        if(tockenAgle!=null)
                        {
                            tockenAgle.Dispose();
                            tockenAgle = null;
                        }
                    });
                    t.Wait();
                }
            }
        }

        /// <summary>
        /// Rotate the motor to current <see cref="Angle"/>.
        /// </summary>
        public void Rotate()
        {
            double duration = (definition.MinimumDuration + rangePerDegree * Angle) / 1000.0;
            currentPulseWidth = duration;
        }

        /// <summary>
        /// Updates the temporary variables when definition changes.
        /// </summary>
        private void UpdateRange()
        {
            if (definition.AngleMax != 0)
                rangePerDegree = (definition.MaximumDuration - definition.MinimumDuration) / definition.AngleMax;
        }

        /// <summary>
        /// Minimum duration expressed microseconds that servo supports.
        /// </summary>
        public uint MinimumDuration
        {
            get { return definition.MinimumDuration; }
            set
            {
                if (definition.MinimumDuration != value)
                {
                    definition.MinimumDuration = value;
                    UpdateRange();
                }
            }
        }

        /// <summary>
        /// Maximum duration expressed microseconds that servo supports.
        /// </summary>
        public uint MaximumDuration
        {
            get { return definition.MaximumDuration; }
            set
            {
                if (definition.MaximumDuration != value)
                {
                    definition.MaximumDuration = value;
                    UpdateRange();
                }
            }
        }

        /// <summary>
        /// Period length expressed microseconds.
        /// </summary>
        public uint Period
        {
            get { return definition.Period; }
            set
            {
                definition.Period = value;
                if (!IsMoving)
                    Rotate();
            }
        }
    }
}
