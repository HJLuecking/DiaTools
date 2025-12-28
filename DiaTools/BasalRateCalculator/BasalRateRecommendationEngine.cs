namespace BasalRateCalculator
{
    using System;
    using System.Collections.Generic;

    public class BasalRateRecommendationEngine
    {
        private const double MaxHourlyChangePercent = 0.30;
        private const double MinBasalRate = 0.1;
        private const double MaxBasalRate = 5.0;
        private const double RoundingStep = 0.05;

        /// <summary>
        /// Calculates recommended basal insulin rates for each hour of the day based on provided hourly average values,
        /// applying smoothing and safety constraints.
        /// </summary>
        /// <remarks>The recommended rates are smoothed using a moving average and constrained to prevent
        /// abrupt hourly changes and to stay within allowed minimum and maximum basal rates. The output rates are
        /// rounded to the nearest supported increment.</remarks>
        /// <param name="hourlyAverages">A dictionary mapping each hour of the day (0–23) to the average basal rate
        /// for that hour. Must contain exactly 24 entries, one for each hour.</param>
        /// <returns>A dictionary containing the recommended basal rate for each hour of the day.
        /// Each key is an hour (0–23), and each value is the adjusted basal rate for that hour.</returns>
        public Dictionary<int, double> RecommendBasalRates(Dictionary<int, double> hourlyAverages)
        {
            var smoothed = ApplyMovingAverage(hourlyAverages);
            var recommended = new Dictionary<int, double>();

            var previousRate = smoothed[0];

            for (var hour = 0; hour < 24; hour++)
            {
                var target = smoothed[hour];

                var maxIncrease = previousRate * (1 + MaxHourlyChangePercent);
                var maxDecrease = previousRate * (1 - MaxHourlyChangePercent);

                var adjusted = Math.Clamp(target, maxDecrease, maxIncrease);
                adjusted = Math.Clamp(adjusted, MinBasalRate, MaxBasalRate);
                adjusted = RoundToStep(adjusted, RoundingStep);

                recommended[hour] = adjusted;
                previousRate = adjusted;
            }

            return recommended;
        }

        /// <summary>
        /// Calculates a simple moving average for each hour in a 24-hour period using the values from the previous,
        /// current, and next hours.
        /// </summary>
        /// <remarks>The calculation wraps around at the boundaries: for hour 0, the previous hour is 23;
        /// for hour 23, the next hour is 0. The input dictionary must not be null and must include all 24 hours to
        /// avoid a KeyNotFoundException.</remarks>
        /// <param name="input">A dictionary mapping each hour of the day (0 to 23) to its corresponding value.
        /// Must contain an entry for every hour from 0 to 23.</param>
        /// <returns>A dictionary containing the moving average for each hour, where each value is the average of
        /// the previous, current, and next hour's values.</returns>
        private Dictionary<int, double> ApplyMovingAverage(
            Dictionary<int, double> input)
        {
            var result = new Dictionary<int, double>();

            for (var hour = 0; hour < 24; hour++)
            {
                var prev = (hour + 23) % 24;
                var next = (hour + 1) % 24;

                var avg = (input[prev] + input[hour] + input[next]) / 3.0;
                result[hour] = avg;
            }

            return result;
        }

        /// <summary>
        /// Rounds the specified value to the nearest multiple of the given step.
        /// </summary>
        /// <remarks>If the step is less than or equal to zero, the result may not be meaningful. The
        /// method uses standard rounding, so values exactly halfway between two multiples of step are rounded to the
        /// nearest even multiple.</remarks>
        /// <param name="value">The numeric value to round.</param>
        /// <param name="step">The step size to which the value is rounded. Must be a non-zero positive number.</param>
        /// <returns>The value rounded to the nearest multiple of the specified step.</returns>
        private double RoundToStep(double value, double step)
        {
            return Math.Round(value / step) * step;
        }
    }

}
