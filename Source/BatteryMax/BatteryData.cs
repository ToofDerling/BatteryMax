using System;
using System.Windows.Forms;
using Windows.Devices.Power;

namespace BatteryMax
{
    public class BatteryData
    {
        /// <summary>
        /// If true all other properties will have their default value
        /// </summary>
        public bool IsNotAvailable { get; protected set; }

        /// <summary>
        /// Typically 5% or lower.
        /// </summary>
        public bool IsCriticalCharge { get; protected set; }

        /// <summary>
        /// Current charge percent
        /// </summary>
        public int CurrentCharge { get; protected set; }

        /// <summary>
        /// This represents one of four different values:
        /// If IsCharging and <= MaximumCharge it's the time to MaximumCharge.
        /// If IsCharging and > MaximumCharge it's the time to full charge.
        /// If not IsCharging and >= MinimumCharge it's the time to MinimumCharge.
        /// If not IsCharging and < MinimumCharge it's the time to zero charge.
        /// </summary>
        public TimeSpan CurrentTime { get; protected set; }

        /// <summary>
        /// True if battery is charging. Note that we can run on AC power and not be charging. 
        /// </summary>
        public bool IsCharging { get; protected set; }

        /// <summary>
        /// If system runs on AC power but isn't charging.
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

        // Remaining seconds to zero charge as reported by Windows (is -1 if charging)
        private int TotalSecondsRemaining { get; set; }

        // Only set if charging
        private int ChargeRate { get; set; }

        // Only set if charging
        private int FullCapacity { get; set; }

        // Only set if charging
        private int RemainingCapacity { get; set; }

        // Only set if charging
        private TimeSpan TimeToFullCapacity { get; set; }

        public BatteryData(Battery battery)
        {
            if (IsNotAvailable = battery == null)
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
                var report = battery.GetReport();

                ChargeRate = report.ChargeRateInMilliwatts.GetValueOrDefault();
                FullCapacity = report.FullChargeCapacityInMilliwattHours.GetValueOrDefault();
                RemainingCapacity = report.RemainingCapacityInMilliwattHours.GetValueOrDefault();

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
            if (ChargeRate == 0)
            {
                CurrentTime = TimeSpan.FromSeconds(0);
                return;
            }

            // double cast to prevent inaccurate int calculations
            var totalRequiredCapacity = FullCapacity - (double)RemainingCapacity;

            var hoursToTotalCapacity = totalRequiredCapacity / ChargeRate;
            TimeToFullCapacity = TimeSpan.FromHours(hoursToTotalCapacity);

            if (IsAboveMaximumCharge)
            {
                CurrentTime = TimeToFullCapacity;
                return;
            }

            var maximumCapacity = FullCapacity / 100d * Settings.MaximumCharge;
            var maximumRequiredCapacity = maximumCapacity - RemainingCapacity;

            var hoursToMaximumCapacity = maximumRequiredCapacity / ChargeRate;
            CurrentTime = TimeSpan.FromHours(hoursToMaximumCapacity);
        }

        private void CalculateRemainingTime()
        {
            if (TotalSecondsRemaining <= 0 // Will be -1 if charging
                || IsPluggedInNotCharging) // This can happen if charging is stopped by an utility like ASUS Battery Health Charging
            {
                CurrentTime = TimeSpan.FromSeconds(0);
                return;
            }

            if (IsBelowMinimumCharge)
            {
                CurrentTime = TimeSpan.FromSeconds(TotalSecondsRemaining);
                return;
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
