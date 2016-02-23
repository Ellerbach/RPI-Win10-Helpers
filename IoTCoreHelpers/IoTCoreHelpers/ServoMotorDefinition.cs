using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTCoreHelpers
{
    /// <summary>
    /// Default period length expressed microseconds.
    /// </summary>
    public enum SerMotorPeriod
    {
        DefaultPeriod = 20000
    }
    /// <summary>
    /// Definition for a servo motor.
    /// </summary>
    public sealed class ServoMotorDefinition
    {

        private uint minimumDuration;
        private uint maximumDuration;
        private uint period;
        private float angleMax;
        private uint ismovingPeriod;

        /// <summary>
        /// Minimum duration expressed microseconds that servo supports.
        /// </summary>
        public uint MinimumDuration
        {
            get
            {
                return minimumDuration;
            }

            set
            {
                minimumDuration = value;
            }
        }
        /// <summary>
        /// Maximum duration expressed microseconds that servo supports.
        /// </summary>
        public uint MaximumDuration
        {
            get
            {
                return maximumDuration;
            }

            set
            {
                maximumDuration = value;
            }
        }
        /// <summary>
        /// Period length expressed microseconds.
        /// </summary>
        public uint Period
        {
            get
            {
                return period;
            }

            set
            {
                period = value;
            }
        }
        /// <summary>
        /// Period length expressed microseconds.
        /// </summary>
        public float AngleMax
        {
            get
            {
                return angleMax;
            }

            set
            {
                angleMax = value;
            }
        }

        public uint IsMovingPeriod
        {
            get
            {
                return ismovingPeriod;
            }
            internal set
            {
                ismovingPeriod = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ServoMotorDefinition class. This constructors uses 20 000 as default period.
        /// </summary>
        /// <param name="minimumDuration">Minimum duration of wave [microseconds]</param>
        /// <param name="maximumDuration">Maximum duration of wave [microseconds]</param>
        public ServoMotorDefinition(uint minimumDuration, uint maximumDuration)
            : this(minimumDuration, maximumDuration, (uint)SerMotorPeriod.DefaultPeriod, 360.0f, 0)
        { }
        /// <summary>
        /// Initializes a new instance of the ServoMotorDefinition class.
        /// </summary>
        /// <param name="minimumDuration">Minimum duration of wave [microseconds]</param>
        /// <param name="maximumDuration">Maximum duration of wave [microseconds]</param>
        /// <param name="period">Period length expressed of wave [microseconds]</param>
        public ServoMotorDefinition(uint minimumDuration, uint maximumDuration, uint period, float angleMax, uint ismovingperiod)
        {
            MinimumDuration = minimumDuration;
            MaximumDuration = maximumDuration;
            Period = period;
            AngleMax = angleMax;
            IsMovingPeriod = ismovingperiod;
        }
    }
}
