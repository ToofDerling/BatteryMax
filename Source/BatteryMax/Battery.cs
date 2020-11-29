using System;
using System.Windows.Forms;

namespace BatteryMax
{
    public class Battery
    {
        /// <summary>
        /// If true all other properties will have their default value
        /// </summary>
        public bool IsNotAvailable { get; private set; }

        /// <summary>
        /// Typically 5% or lower.
        /// </summary>
        public bool IsCriticalCharge { get; protected set; }

        /// <summary>
        /// Current charge percent
        /// </summary>
        public int CurrentCharge { get; protected set; }

        /// <summary>
        /// Calculated time to Minimum- or MaximumCharge
        /// </summary>
        public TimeSpan CurrentTime { get; private set; }

        /// <summary>
        /// True if battery is charging. Note that we can run on AC power and not be charging. 
        /// </summary>
        public bool IsCharging { get; protected set; }

        /// <summary>
        /// If system runs on AC power. This can be true even if not charging.
        /// </summary>
        public bool IsPluggedInNotCharging { get; protected set; }

        /// <summary>
        /// Above the user defined maximum charge
        /// </summary>
        public bool IsAboveMaximumCharge { get; protected set; }

        /// <summary>
        /// Below the user defined minimum charge
        /// </summary>
        public bool IsBelowMinimumCharge { get; protected set; }

        // Remaining seconds as reported by Windows (is -1 if charging)
        private int TotalSecondsRemaining { get; set; }

        // Only set if charging
        private int Rate { get; set; }

        // Only set if charging
        private int FullCapacity { get; set; }

        // Only set if charging
        private uint CurrentCapacity { get; set; }

        // Only set if charging
        private TimeSpan TimeToFullCapacity { get; set; }

        public Battery(bool initialize = true)
        {
            if (!initialize)
            {
                return;
            }

            var status = SystemInformation.PowerStatus;
            var chargeStatus = status.BatteryChargeStatus;

            if (IsNotAvailable = chargeStatus.HasFlag(BatteryChargeStatus.NoSystemBattery) || chargeStatus.HasFlag(BatteryChargeStatus.Unknown))
            {
                return;
            }

            IsCriticalCharge = chargeStatus.HasFlag(BatteryChargeStatus.Critical);
            TotalSecondsRemaining = status.BatteryLifeRemaining;

            CurrentCharge = Convert.ToInt32(status.BatteryLifePercent * 100);
            IsAboveMaximumCharge = CurrentCharge > Settings.MaximumCharge;
            IsBelowMinimumCharge = CurrentCharge < Settings.MinimumCharge;

            if (IsCharging = status.BatteryChargeStatus.HasFlag(BatteryChargeStatus.Charging))
            {
                var details = BatteryInfo.GetBatteryInformation();

                Rate = details.Rate;
                FullCapacity = details.FullChargeCapacity;
                CurrentCapacity = details.CurrentCapacity;

                CalculateChargingTime();
            }
            else
            {
                IsPluggedInNotCharging = status.PowerLineStatus == PowerLineStatus.Online;
             
                CalculateRemainingTime();
            }
        }

        private void CalculateChargingTime()
        {
            if (IsAboveMaximumCharge || Rate == 0)
            {
                CurrentTime = TimeSpan.FromSeconds(0);
            }

            // double cast to prevent inaccurate int calculations
            var totalRequiredCapacity = FullCapacity - (double)CurrentCapacity;

            var hoursToTotalCapacity = totalRequiredCapacity / Rate;
            TimeToFullCapacity = TimeSpan.FromHours(hoursToTotalCapacity);

            var maximumCapacity = FullCapacity / 100d * Settings.MaximumCharge;
            var maximumRequiredCapacity = maximumCapacity - CurrentCapacity;

            var hoursToMaximumCapacity = maximumRequiredCapacity / Rate;
            CurrentTime = TimeSpan.FromHours(hoursToMaximumCapacity);
        }

        private void CalculateRemainingTime()
        {
            if (IsBelowMinimumCharge
                || TotalSecondsRemaining <= 0 // Will be -1 if charging
                || IsPluggedInNotCharging) // This can happen if charging is stopped by an utility like ASUS Battery Health Charging
            {
                CurrentTime = TimeSpan.FromSeconds(0);
            }

            // double cast to prevent inaccurate int calculations
            var percentOfRemaining = Settings.MinimumCharge / (double)CurrentCharge * 100;

            var difference = TotalSecondsRemaining * percentOfRemaining / 100;
            var remainingSeconds = TotalSecondsRemaining - difference;

            CurrentTime = TimeSpan.FromSeconds(remainingSeconds);
        }

        public override string ToString()
        {
            if (IsNotAvailable)
            {
                return "No battery";
            }

            if (IsCharging)
            {
                if (IsAboveMaximumCharge)
                {
                    return $"Charging {CurrentCharge}% - above maximum {Settings.MaximumCharge}";
                }

                return $"Charging {CurrentCharge}% - {CurrentTime.TotalMinutes.ToInt()} min to {Settings.MaximumCharge} - {TimeToFullCapacity.TotalMinutes.ToInt()} to 100";
            }

            if (IsPluggedInNotCharging)
            {
                return $"Not charging {CurrentCharge}% - running on AC power";
            }

            if (IsBelowMinimumCharge)
            {
                return $"Remaining {CurrentCharge}% - below minimum {Settings.MinimumCharge}";
            }

            var toZero = TimeSpan.FromSeconds(TotalSecondsRemaining).TotalMinutes.ToInt();

            return $"Remaining {CurrentCharge}% - {CurrentTime.TotalMinutes.ToInt()} min to {Settings.MinimumCharge} - {toZero} to 0";
        }
    }
}
