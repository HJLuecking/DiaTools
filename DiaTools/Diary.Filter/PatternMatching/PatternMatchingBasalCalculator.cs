using System.Numerics;
using Diary.Aggregator;
using Diary.Model;

namespace Diary.Aggregator.PatternMatching
{
    /// <summary>
    /// Pattern-Matching based basal rate calculator.
    /// Uses similarity of daily glucose/basal patterns and
    /// weighted averaging based on glucose stability.
    /// </summary>
    public class PatternMatchingBasalCalculator
    {
        // Distance function weights
        private const double Alpha = 1.0;   // Glucose level weight
        private const double Beta = 0.5;    // Glucose trend weight
        private const double Gamma = 0.2;   // Basal rate weight

        // Glucose weighting parameters
        private const double GlucoseTarget = 110.0;
        private const double Sigma = 30.0;

        private const double Threshold = 6000.0;

        public List<DailyProfile> ConvertToDailyProfiles(List<InsulinInfusionGlucoseEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return new List<DailyProfile>();

            return entries
                .GroupBy(e => e.Time.Date)
                .Select(dayGroup =>
                {
                    var dailyProfile = new DailyProfile
                    {
                        Date = dayGroup.Key
                    };

                    // Group existing entries by hour
                    var hourlyGroups = dayGroup
                        .GroupBy(e => e.Time.Hour)
                        .ToDictionary(g => g.Key, g => g);

                    // Ensure all 24 hours exist
                    for (int hour = 0; hour < 24; hour++)
                    {
                        if (hourlyGroups.TryGetValue(hour, out var hourGroup))
                        {
                            // Use real data
                            dailyProfile.Hours.Add(new HourlyData
                            {
                                Hour = hour,
                                BasalUnits = hourGroup.Average(e => e.InsulinUnitsPerHour),
                                Glucose = hourGroup.Average(e => e.GlucoseMgPerLitre) / 10.0
                            });
                        }
                        else
                        {
                            // Insert empty/default values
                            dailyProfile.Hours.Add(new HourlyData
                            {
                                Hour = hour,
                                BasalUnits = 0.0,
                                Glucose = 0.0
                            });
                        }
                    }

                    return dailyProfile;
                })
                .OrderBy(dp => dp.Date)
                .ToList();
        }



        /// <summary>
        /// Computes a 24-hour basal profile (E/h) from multiple daily profiles
        /// </summary>
        public Dictionary<int, double> ComputeBasalProfile(List<DailyProfile> days)
        {
            if (days == null || days.Count == 0)
                throw new ArgumentException("No daily profiles provided.");

            // Step 1: Filter days with complete data
            var validDays = days;
                //.Where(d => d.Hours != null && d.Hours.Count == 24)
                //.ToList();

            // if (validDays.Count == 0) throw new InvalidOperationException("No valid days with 24 hours of data.");

            // Step 2: Cluster days by similarity (simple threshold-based clustering)
            var clusters = ClusterDays(validDays, threshold: Threshold);

            // Step 3: Select the most stable cluster (lowest glucose variance)
            var bestCluster = clusters
                .OrderBy(c => GlucoseVariance(c))
                .First();

            //var orderedCluster = bestCluster
            //    .OrderBy(d => d.Date)
            //    .ToList();
            //var combinedCluster = new List<DailyProfile>();
            //for (int i = 0; i < 3; i++)
            //{
            //        combinedCluster.Add(orderedCluster[i]);
            //}
            //return ComputeWeightedBasal(combinedCluster);

            // Step 4: Compute weighted basal profile
            var result = ComputeWeightedBasal(bestCluster);
            var total = result.Values.Sum();
            return result;
        }

        /// <summary>
        /// Groups days into clusters based on pairwise distance
        /// </summary>
        private List<List<DailyProfile>> ClusterDays(List<DailyProfile> days, double threshold)
        {
            var clusters = new List<List<DailyProfile>>();

            foreach (var day in days)
            {
                bool assigned = false;

                foreach (var cluster in clusters)
                {
                    // Compare with first element as cluster representative
                    if (Distance(day, cluster[0]) < threshold)
                    {
                        cluster.Add(day);
                        assigned = true;
                        break;
                    }
                }

                if (!assigned)
                {
                    clusters.Add(new List<DailyProfile> { day });
                }
            }

            return clusters;
        }

        /// <summary>
        /// Distance function between two daily profiles
        /// Combines glucose level, glucose trend and basal rate similarity
        /// </summary>
        private double Distance(DailyProfile d1, DailyProfile d2)
        {
            double sum = 0.0;

            for (int h = 0; h < 23; h++)
            {
                double g1 = d1.Hours[h].Glucose;
                double g2 = d2.Hours[h].Glucose;

                double dg1 = d1.Hours[h + 1].Glucose - g1;
                double dg2 = d2.Hours[h + 1].Glucose - g2;

                double b1 = d1.Hours[h].BasalUnits;
                double b2 = d2.Hours[h].BasalUnits;

                sum += Alpha * Math.Pow(g1 - g2, 2)
                     + Beta * Math.Pow(dg1 - dg2, 2)
                     + Gamma * Math.Pow(b1 - b2, 2);
            }

            return sum;
        }

        /// <summary>
        /// Computes glucose variance across all hours and days in a cluster
        /// Used to select the most stable pattern
        /// </summary>
        private double GlucoseVariance(List<DailyProfile> cluster)
        {
            var glucoseValues = cluster
                .SelectMany(d => d.Hours)
                .Select(h => h.Glucose)
                .ToList();

            double mean = glucoseValues.Average();

            return glucoseValues
                .Average(g => Math.Pow(g - mean, 2));
        }

        /// <summary>
        /// Computes the final 24h basal profile using glucose-weighted averaging
        /// </summary>
        private Dictionary<int, double> ComputeWeightedBasal(
            List<DailyProfile> cluster)
        {
            var result = new Dictionary<int, double>();

            for (int h = 0; h < 24; h++)
            {
                double weightedSum = 0.0;
                double weightTotal = 0.0;

                foreach (var day in cluster)
                {
                    double glucose = day.Hours[h].Glucose;
                    double basal = day.Hours[h].BasalUnits;

                    // Weight hours with glucose close to target higher
                    double weight = Math.Exp(
                        -Math.Abs(glucose - GlucoseTarget) / Sigma);

                    weightedSum += weight * basal;
                    weightTotal += weight;
                }

                result[h] = weightTotal > 0
                    ? weightedSum / weightTotal
                    : 0.0;
            }

            for (int h = 0; h < 24; h++)
            {
                var p2 = h - 2;
                var p1 = h - 1;
                var c = h;
                var n1 = h + 1;
                var n2 = h + 2;
                if (h == 0) { p1 = 23; p2 = 22; }
                if (h == 1) { p2 = 23; }
                if (h == 23) { n1 = 0; n2 = 1; }
                if (h == 22) { n2 = 0;}
                result[h] = (result[p2] + result[p1]*2 + result[c]*3 + result[n1]*2 + result[n2]) / 9.0;
            }

            return result;
        }
    }
}

