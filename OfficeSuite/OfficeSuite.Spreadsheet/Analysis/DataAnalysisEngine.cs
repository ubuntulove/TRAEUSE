using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeSuite.Spreadsheet.Analysis
{
    public class DataAnalysisEngine
    {
        private readonly Dictionary<string, AnalysisResult> _cachedResults;
        private readonly List<IDataTransform> _transforms;
        private readonly StatisticalEngine _statistics;
        private readonly RegressionEngine _regression;
        private readonly ClusteringEngine _clustering;

        public DataAnalysisEngine()
        {
            _cachedResults = new Dictionary<string, AnalysisResult>();
            _transforms = new List<IDataTransform>();
            _statistics = new StatisticalEngine();
            _regression = new RegressionEngine();
            _clustering = new ClusteringEngine();
        }

        public AnalysisResult AnalyzeData(DataRange range, AnalysisType type)
        {
            var cacheKey = $"{range.Address}_{type}";
            if (_cachedResults.TryGetValue(cacheKey, out var cached))
                return cached;

            AnalysisResult result = type switch
            {
                AnalysisType.Descriptive => ComputeDescriptiveStatistics(range),
                AnalysisType.Correlation => ComputeCorrelation(range),
                AnalysisType.Regression => ComputeRegression(range),
                AnalysisType.Clustering => ComputeClustering(range),
                AnalysisType.Sorting => ComputeSorting(range),
                AnalysisType.Filtering => ComputeFiltering(range),
                AnalysisType.Summarization => ComputeSummarization(range),
                AnalysisType.TrendAnalysis => ComputeTrendAnalysis(range),
                AnalysisType.OutlierDetection => ComputeOutlierDetection(range),
                AnalysisType.FrequencyAnalysis => ComputeFrequencyAnalysis(range),
                _ => throw new ArgumentException($"Unknown analysis type: {type}")
            };

            _cachedResults[cacheKey] = result;
            return result;
        }

        public DescriptiveStatistics ComputeDescriptiveStatistics(DataRange range)
        {
            var values = range.GetNumericValues().Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToList();
            
            if (!values.Any())
                return new DescriptiveStatistics();

            var sorted = values.OrderBy(v => v).ToList();
            var count = values.Count;
            var sum = values.Sum();
            var mean = sum / count;
            var median = ComputeMedian(sorted);
            var min = sorted.First();
            var max = sorted.Last();

            double variance = values.Sum(v => Math.Pow(v - mean, 2)) / (count - 1);
            double stdDev = Math.Sqrt(variance);
            double range_value = max - min;
            double skewness = ComputeSkewness(values, mean, stdDev, count);
            double kurtosis = ComputeKurtosis(values, mean, stdDev, count);
            double coefficientOfVariation = stdDev / mean * 100;

            var quartiles = ComputeQuartiles(sorted);
            var percentiles = ComputePercentiles(sorted);

            return new DescriptiveStatistics
            {
                Count = count,
                Sum = sum,
                Mean = mean,
                Median = median,
                Mode = ComputeMode(values),
                Min = min,
                Max = max,
                Range = range_value,
                Variance = variance,
                StandardDeviation = stdDev,
                CoefficientOfVariation = coefficientOfVariation,
                Skewness = skewness,
                Kurtosis = kurtosis,
                FirstQuartile = quartiles.Q1,
                ThirdQuartile = quartiles.Q3,
                InterquartileRange = quartiles.Q3 - quartiles.Q1,
                Percentiles = percentiles,
                ConfidenceInterval95 = ComputeConfidenceInterval(values, mean, stdDev, count, 0.95),
                ConfidenceInterval99 = ComputeConfidenceInterval(values, mean, stdDev, count, 0.99),
                GeometricMean = ComputeGeometricMean(values),
                HarmonicMean = ComputeHarmonicMean(values),
                TrimmedMean = ComputeTrimmedMean(sorted, 0.1)
            };
        }

        public CorrelationResult ComputeCorrelation(DataRange range)
        {
            var columns = range.GetColumns();
            var correlations = new Dictionary<string, Dictionary<string, double>>();
            var columnNames = columns.Keys.OrderBy(k => k).ToList();

            foreach (var col1 in columnNames)
            {
                correlations[col1] = new Dictionary<string, double>();
                foreach (var col2 in columnNames)
                {
                    if (col1 == col2)
                    {
                        correlations[col1][col2] = 1.0;
                        continue;
                    }

                    var values1 = columns[col1].Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToList();
                    var values2 = columns[col2].Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToList();

                    var minLength = Math.Min(values1.Count, values2.Count);
                    values1 = values1.Take(minLength).ToList();
                    values2 = values2.Take(minLength).ToList();

                    if (values1.Count < 2)
                    {
                        correlations[col1][col2] = double.NaN;
                        continue;
                    }

                    var correlation = ComputePearsonCorrelation(values1, values2);
                    correlations[col1][col2] = correlation;
                }
            }

            var strongest = FindStrongestCorrelation(correlations);
            var weakest = FindWeakestCorrelation(correlations);

            return new CorrelationResult
            {
                Correlations = correlations,
                StrongestPositive = strongest.positive,
                StrongestNegative = strongest.negative,
                WeakestCorrelation = weakest,
                AverageCorrelation = correlations.Values.SelectMany(d => d.Values)
                    .Where(v => !double.IsNaN(v) && v != 1.0)
                    .DefaultIfEmpty(0)
                    .Average()
            };
        }

        public RegressionResult ComputeRegression(DataRange range)
        {
            var values = range.GetNumericValues().ToList();
            return _regression.PerformRegression(values);
        }

        public ClusteringResult ComputeClustering(DataRange range)
        {
            var dataPoints = range.ToDataPoints();
            return _clustering.PerformClustering(dataPoints);
        }

        public SortedDataResult ComputeSorting(DataRange range)
        {
            var data = range.ToDataRecords();
            var sorted = data.OrderBy(d => d.SortKey).ToList();
            return new SortedDataResult
            {
                SortedRecords = sorted,
                OriginalIndices = data.Select((d, i) => new { Original = i, Sorted = sorted.IndexOf(d) }).ToList()
            };
        }

        public FilteredDataResult ComputeFiltering(DataRange range)
        {
            var data = range.ToDataRecords();
            var filtered = data.Where(d => d.MatchesCriteria).ToList();
            return new FilteredDataResult
            {
                FilteredRecords = filtered,
                FilteredIndices = data.Select((d, i) => filtered.Contains(d) ? i : -1).Where(i => i >= 0).ToList()
            };
        }

        public SummaryResult ComputeSummarization(DataRange range)
        {
            var columns = range.GetColumns();
            var summaries = new Dictionary<string, ColumnSummary>();

            foreach (var column in columns)
            {
                var values = column.Value.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToList();
                if (!values.Any())
                    continue;

                summaries[column.Key] = new ColumnSummary
                {
                    ColumnName = column.Key,
                    Count = values.Count,
                    Sum = values.Sum(),
                    Mean = values.Average(),
                    Min = values.Min(),
                    Max = values.Max(),
                    UniqueCount = values.Distinct().Count(),
                    NullCount = column.Value.Count(v => double.IsNaN(v)),
                    MostFrequent = values.GroupBy(v => v).OrderByDescending(g => g.Count()).First().Key
                };
            }

            return new SummaryResult
            {
                ColumnSummaries = summaries,
                TotalRows = range.RowCount,
                TotalColumns = range.ColumnCount,
                DataType = DetermineDataType(range)
            };
        }

        public TrendResult ComputeTrendAnalysis(DataRange range)
        {
            var values = range.GetNumericValues().ToList();
            var n = values.Count;
            
            var xValues = Enumerable.Range(1, n).Select(i => (double)i).ToList();
            
            var linearFit = _regression.LinearRegression(xValues, values);
            var exponentialFit = _regression.ExponentialRegression(xValues, values);
            var logarithmicFit = _regression.LogarithmicRegression(xValues, values);
            var polynomialFit = _regression.PolynomialRegression(xValues, values, 2);

            var linearError = ComputeRMSE(values, linearFit.Predicted);
            var exponentialError = ComputeRMSE(values, exponentialFit.Predicted);
            var logarithmicError = ComputeRMSE(values, logarithmicFit.Predicted);
            var polynomialError = ComputeRMSE(values, polynomialFit.Predicted);

            var bestFit = new[] {
                (linearFit, linearError, "Linear"),
                (exponentialFit, exponentialError, "Exponential"),
                (logarithmicFit, logarithmicError, "Logarithmic"),
                (polynomialFit, polynomialError, "Polynomial")
            }.OrderBy(x => x.Item2).First();

            return new TrendResult
            {
                LinearTrend = linearFit,
                ExponentialTrend = exponentialFit,
                LogarithmicTrend = logarithmicFit,
                PolynomialTrend = polynomialFit,
                BestFitType = bestFit.Item3,
                BestFitModel = bestFit.Item1,
                BestFitError = bestFit.Item2,
                Seasonality = DetectSeasonality(values),
                CyclicalPattern = DetectCyclicalPattern(values)
            };
        }

        public OutlierResult ComputeOutlierDetection(DataRange range)
        {
            var values = range.GetNumericValues().ToList();
            var outliers = new List<OutlierInfo>();

            var sorted = values.OrderBy(v => v).ToList();
            var quartiles = ComputeQuartiles(sorted);
            var iqr = quartiles.Q3 - quartiles.Q1;
            var lowerBound = quartiles.Q1 - 1.5 * iqr;
            var upperBound = quartiles.Q3 + 1.5 * iqr;

            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] < lowerBound || values[i] > upperBound)
                {
                    outliers.Add(new OutlierInfo
                    {
                        Index = i,
                        Value = values[i],
                        Type = values[i] < lowerBound ? OutlierType.Low : OutlierType.High,
                        Severity = Math.Abs(values[i] - (values[i] < lowerBound ? lowerBound : upperBound)) / iqr
                    });
                }
            }

            var zScores = ComputeZScores(values);
            var zScoreOutliers = zScores.Where(z => Math.Abs(z) > 3).Select((z, i) => new OutlierInfo
            {
                Index = i,
                Value = values[i],
                Type = OutlierType.ZScore,
                Severity = Math.Abs(z) - 3
            }).ToList();

            outliers.AddRange(zScoreOutliers);

            return new OutlierResult
            {
                Outliers = outliers.DistinctBy(o => o.Index).ToList(),
                LowerBound = lowerBound,
                UpperBound = upperBound,
                InterquartileRange = iqr,
                ModifiedZScoreOutliers = zScoreOutliers
            };
        }

        public FrequencyResult ComputeFrequencyAnalysis(DataRange range)
        {
            var values = range.GetNumericValues().ToList();
            var bins = CreateHistogramBins(values, 10);
            
            var frequency = new Dictionary<string, long>();
            foreach (var value in values)
            {
                var bin = GetBin(value, bins);
                frequency[bin] = frequency.GetValueOrDefault(bin) + 1;
            }

            var cumulativeFrequency = new Dictionary<string, long>();
            long cumSum = 0;
            foreach (var bin in bins)
            {
                cumSum += frequency.GetValueOrDefault(bin);
                cumulativeFrequency[bin] = cumSum;
            }

            return new FrequencyResult
            {
                Bins = bins,
                Frequency = frequency,
                RelativeFrequency = frequency.ToDictionary(f => f.Key, f => (double)f.Value / values.Count),
                CumulativeFrequency = cumulativeFrequency,
                CumulativeRelativeFrequency = cumulativeFrequency.ToDictionary(f => f.Key, f => (double)f.Value / values.Count),
                Mode = ComputeMode(values),
                Median = ComputeMedian(sorted),
                Mean = values.Average()
            };
        }

        private double ComputeMedian(List<double> sorted)
        {
            int count = sorted.Count;
            if (count == 0) return double.NaN;
            if (count % 2 == 0)
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
            return sorted[count / 2];
        }

        private double ComputeMode(List<double> values)
        {
            var groups = values.GroupBy(v => v);
            var max = groups.Max(g => g.Count());
            return groups.First(g => g.Count() == max).Key;
        }

        private double ComputeSkewness(List<double> values, double mean, double stdDev, int count)
        {
            if (stdDev == 0) return 0;
            return values.Sum(v => Math.Pow((v - mean) / stdDev, 3)) / count;
        }

        private double ComputeKurtosis(List<double> values, double mean, double stdDev, int count)
        {
            if (stdDev == 0) return 0;
            return values.Sum(v => Math.Pow((v - mean) / stdDev, 4)) / count - 3;
        }

        private (double Q1, double Q2, double Q3) ComputeQuartiles(List<double> sorted)
        {
            int n = sorted.Count;
            double q1, q2, q3;

            if (n % 2 == 0)
            {
                q2 = (sorted[n / 2 - 1] + sorted[n / 2]) / 2;
                int half = n / 2;
                if (half % 2 == 0)
                {
                    q1 = (sorted[half / 2 - 1] + sorted[half / 2]) / 2;
                    q3 = (sorted[half + half / 2 - 1] + sorted[half + half / 2]) / 2;
                }
                else
                {
                    q1 = sorted[half / 2];
                    q3 = sorted[half + half / 2];
                }
            }
            else
            {
                q2 = sorted[n / 2];
                int half = n / 2;
                if (half % 2 == 0)
                {
                    q1 = (sorted[half / 2 - 1] + sorted[half / 2]) / 2;
                    q3 = (sorted[half + half / 2] + sorted[half + half / 2 + 1]) / 2;
                }
                else
                {
                    q1 = sorted[half / 2];
                    q3 = sorted[half + half / 2 + 1];
                }
            }

            return (q1, q2, q3);
        }

        private Dictionary<int, double> ComputePercentiles(List<double> sorted)
        {
            var percentiles = new Dictionary<int, double>();
            for (int p = 1; p <= 99; p++)
            {
                int index = (int)Math.Ceiling(p / 100.0 * sorted.Count) - 1;
                percentiles[p] = sorted[Math.Max(0, Math.Min(index, sorted.Count - 1))];
            }
            return percentiles;
        }

        private double ComputeConfidenceInterval(List<double> values, double mean, double stdDev, int count, double confidence)
        {
            if (count < 2) return 0;
            double t = GetTCriticalValue(count - 1, confidence);
            return t * stdDev / Math.Sqrt(count);
        }

        private double ComputeGeometricMean(List<double> values)
        {
            var positive = values.Where(v => v > 0);
            if (!positive.Any()) return double.NaN;
            return Math.Pow(positive.Product(), 1.0 / positive.Count());
        }

        private double ComputeHarmonicMean(List<double> values)
        {
            var positive = values.Where(v => v > 0);
            if (!positive.Any()) return double.NaN;
            return positive.Count() / positive.Sum(1.0 / v);
        }

        private double ComputeTrimmedMean(List<double> sorted, double proportion)
        {
            int trim = (int)(sorted.Count * proportion);
            if (trim == 0) return sorted.Average();
            var trimmed = sorted.Skip(trim).Take(sorted.Count - 2 * trim);
            return trimmed.Average();
        }

        private double ComputePearsonCorrelation(List<double> x, List<double> y)
        {
            int n = Math.Min(x.Count, y.Count);
            if (n < 2) return double.NaN;

            double meanX = x.Average();
            double meanY = y.Average();

            double sumXY = 0, sumX2 = 0, sumY2 = 0;
            for (int i = 0; i < n; i++)
            {
                double dx = x[i] - meanX;
                double dy = y[i] - meanY;
                sumXY += dx * dy;
                sumX2 += dx * dx;
                sumY2 += dy * dy;
            }

            if (sumX2 == 0 || sumY2 == 0) return 0;
            return sumXY / Math.Sqrt(sumX2 * sumY2);
        }

        private (string positive, string negative) FindStrongestCorrelation(Dictionary<string, Dictionary<string, double>> correlations)
        {
            string strongestPos = null, strongestNeg = null;
            double maxPos = 0, maxNeg = double.MinValue;

            foreach (var col1 in correlations)
            {
                foreach (var col2 in col1.Value)
                {
                    if (col1.Key == col2.Key) continue;
                    if (!double.IsNaN(col2.Value))
                    {
                        if (col2.Value > 0 && col2.Value > maxPos)
                        {
                            maxPos = col2.Value;
                            strongestPos = $"{col1.Key}-{col2.Key}: {col2.Value:F3}";
                        }
                        else if (col2.Value < 0 && col2.Value > maxNeg)
                        {
                            maxNeg = col2.Value;
                            strongestNeg = $"{col1.Key}-{col2.Key}: {col2.Value:F3}";
                        }
                    }
                }
            }

            return (strongestPos, strongestNeg);
        }

        private string FindWeakestCorrelation(Dictionary<string, Dictionary<string, double>> correlations)
        {
            string weakest = null;
            double minAbs = double.MaxValue;

            foreach (var col1 in correlations)
            {
                foreach (var col2 in col1.Value)
                {
                    if (col1.Key == col2.Key) continue;
                    if (!double.IsNaN(col2.Value) && Math.Abs(col2.Value) < minAbs)
                    {
                        minAbs = Math.Abs(col2.Value);
                        weakest = $"{col1.Key}-{col2.Key}: {col2.Value:F3}";
                    }
                }
            }

            return weakest;
        }

        private double GetTCriticalValue(int degreesOfFreedom, double confidence)
        {
            var tValues = new Dictionary<(int, double), double>
            {
                {(1, 0.95), 12.706}, {(1, 0.99), 63.657},
                {(2, 0.95), 4.303}, {(2, 0.99), 9.925},
                {(5, 0.95), 2.571}, {(5, 0.99), 4.032},
                {(10, 0.95), 2.228}, {(10, 0.99), 3.169},
                {(20, 0.95), 2.086}, {(20, 0.99), 2.845},
                {(30, 0.95), 2.042}, {(30, 0.99), 2.750},
                {(60, 0.95), 2.000}, {(60, 0.99), 2.660},
                {(120, 0.95), 1.980}, {(120, 0.99), 2.617},
                {(int.MaxValue, 0.95), 1.960}, {(int.MaxValue, 0.99), 2.576}
            };

            foreach (var kvp in tValues)
            {
                if (degreesOfFreedom <= kvp.Key.Item1 && Math.Abs(kvp.Key.Item2 - confidence) < 0.001)
                    return kvp.Value;
            }

            return 1.96;
        }

        private double[] ComputeZScores(List<double> values)
        {
            double mean = values.Average();
            double std = Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / values.Count);
            return values.Select(v => std > 0 ? (v - mean) / std : 0).ToArray();
        }

        private List<string> CreateHistogramBins(List<double> values, int binCount)
        {
            double min = values.Min();
            double max = values.Max();
            double binWidth = (max - min) / binCount;
            var bins = new List<string>();

            for (int i = 0; i < binCount; i++)
            {
                double lower = min + i * binWidth;
                double upper = lower + binWidth;
                bins.Add($"{lower:F2}-{upper:F2}");
            }

            return bins;
        }

        private string GetBin(double value, List<string> bins)
        {
            return bins.LastOrDefault() ?? "N/A";
        }

        private SeasonalityResult DetectSeasonality(List<double> values)
        {
            int[] periods = { 4, 7, 12, 52 };
            SeasonalityResult result = null;
            double maxCorrelation = 0;

            foreach (var period in periods)
            {
                if (values.Count < 2 * period) continue;
                
                var autocorr = ComputeAutocorrelation(values, period);
                if (autocorr > maxCorrelation)
                {
                    maxCorrelation = autocorr;
                    result = new SeasonalityResult
                    {
                        Period = period,
                        Strength = autocorr,
                        Detected = autocorr > 0.5
                    };
                }
            }

            return result ?? new SeasonalityResult { Period = 0, Strength = 0, Detected = false };
        }

        private double ComputeAutocorrelation(List<double> values, int lag)
        {
            if (values.Count <= lag) return 0;
            var lagged = values.Skip(lag).ToList();
            var baseValues = values.Take(values.Count - lag).ToList();
            return ComputePearsonCorrelation(lagged, baseValues);
        }

        private CyclicalResult DetectCyclicalPattern(List<double> values)
        {
            var spectrum = ComputeSpectrum(values);
            var peakIndex = spectrum.ToList().IndexOf(spectrum.Max());
            var cycleLength = peakIndex > 0 ? values.Count / peakIndex : 0;

            return new CyclicalResult
            {
                CycleLength = cycleLength,
                Strength = spectrum.Max(),
                Detected = cycleLength > 1 && spectrum.Max() > 0.3
            };
        }

        private double[] ComputeSpectrum(List<double> values)
        {
            int n = values.Count;
            var spectrum = new double[n / 2];
            
            for (int k = 0; k < spectrum.Length; k++)
            {
                double real = 0, imag = 0;
                for (int t = 0; t < n; t++)
                {
                    double angle = 2 * Math.PI * k * t / n;
                    real += values[t] * Math.Cos(angle);
                    imag -= values[t] * Math.Sin(angle);
                }
                spectrum[k] = Math.Sqrt(real * real + imag * imag) / n;
            }

            return spectrum;
        }

        private double ComputeRMSE(List<double> actual, List<double> predicted)
        {
            if (actual.Count != predicted.Count) return double.MaxValue;
            return Math.Sqrt(actual.Zip(predicted, (a, p) => Math.Pow(a - p, 2)).Average());
        }

        private string DetermineDataType(DataRange range)
        {
            return "Numeric";
        }

        public void ClearCache()
        {
            _cachedResults.Clear();
        }

        public void RegisterTransform(IDataTransform transform)
        {
            _transforms.Add(transform);
        }
    }

    public class StatisticalEngine
    {
        public double ComputeVariance(IEnumerable<double> values, bool sample = true)
        {
            var list = values.ToList();
            double mean = list.Average();
            int n = list.Count;
            if (n < 2) return 0;
            return list.Sum(v => Math.Pow(v - mean, 2)) / (sample ? n - 1 : n);
        }

        public double ComputeStandardDeviation(IEnumerable<double> values, bool sample = true)
        {
            return Math.Sqrt(ComputeVariance(values, sample));
        }

        public double ComputeCovariance(IEnumerable<double> x, IEnumerable<double> y, bool sample = true)
        {
            var xList = x.ToList();
            var yList = y.ToList();
            int n = Math.Min(xList.Count, yList.Count);
            if (n < 2) return 0;

            double meanX = xList.Take(n).Average();
            double meanY = yList.Take(n).Average();
            double sum = 0;

            for (int i = 0; i < n; i++)
            {
                sum += (xList[i] - meanX) * (yList[i] - meanY);
            }

            return sum / (sample ? n - 1 : n);
        }

        public double ComputePercentile(IEnumerable<double> values, double percentile)
        {
            var sorted = values.OrderBy(v => v).ToList();
            if (!sorted.Any()) return double.NaN;

            int index = (int)Math.Ceiling(percentile / 100 * sorted.Count) - 1;
            return sorted[Math.Max(0, Math.Min(index, sorted.Count - 1))];
        }
    }

    public class RegressionEngine
    {
        public RegressionModel LinearRegression(List<double> x, List<double> y)
        {
            int n = Math.Min(x.Count, y.Count);
            if (n < 2) return new RegressionModel();

            double sumX = x.Take(n).Sum();
            double sumY = y.Take(n).Sum();
            double sumXY = x.Take(n).Zip(y.Take(n), (a, b) => a * b).Sum();
            double sumX2 = x.Take(n).Sum(v => v * v);

            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double intercept = (sumY - slope * sumX) / n;

            var predicted = x.Take(n).Select(v => slope * v + intercept).ToList();
            double rSquared = ComputeRSquared(y.Take(n).ToList(), predicted);

            return new RegressionModel
            {
                Type = "Linear",
                Coefficients = new[] { slope, intercept },
                Predicted = predicted,
                RSquared = rSquared,
                StandardError = ComputeStandardError(y.Take(n).ToList(), predicted, 2)
            };
        }

        public RegressionModel ExponentialRegression(List<double> x, List<double> y)
        {
            var positiveY = y.Where(v => v > 0).ToList();
            if (!positiveY.Any()) return new RegressionModel();

            var logY = positiveY.Select(Math.Log).ToList();
            var result = LinearRegression(x.Take(logY.Count).ToList(), logY);

            return new RegressionModel
            {
                Type = "Exponential",
                Coefficients = new[] { Math.Exp(result.Coefficients[1]), result.Coefficients[0] },
                Predicted = x.Take(positiveY.Count).Select(v => Math.Exp(result.Coefficients[1]) * Math.Exp(result.Coefficients[0] * v)).ToList(),
                RSquared = result.RSquared
            };
        }

        public RegressionModel LogarithmicRegression(List<double> x, List<double> y)
        {
            var logX = x.Select(Math.Log).ToList();
            var result = LinearRegression(logX, y);

            return new RegressionModel
            {
                Type = "Logarithmic",
                Coefficients = new[] { result.Coefficients[0], result.Coefficients[1] },
                Predicted = x.Select(v => result.Coefficients[0] + result.Coefficients[1] * Math.Log(v)).ToList(),
                RSquared = result.RSquared
            };
        }

        public RegressionModel PolynomialRegression(List<double> x, List<double> y, int degree)
        {
            int n = Math.Min(x.Count, y.Count);
            if (n < degree + 1) return new RegressionModel();

            var coefficients = SolvePolynomial(x.Take(n).ToList(), y.Take(n).ToList(), degree);
            var predicted = x.Take(n).Select(v => EvaluatePolynomial(v, coefficients)).ToList();
            double rSquared = ComputeRSquared(y.Take(n).ToList(), predicted);

            return new RegressionModel
            {
                Type = $"Polynomial (degree {degree})",
                Coefficients = coefficients,
                Predicted = predicted,
                RSquared = rSquared
            };
        }

        private double[] SolvePolynomial(List<double> x, List<double> y, int degree)
        {
            var coefficients = new double[degree + 1];
            for (int i = 0; i <= degree; i++)
            {
                coefficients[i] = 1;
            }
            return coefficients;
        }

        private double EvaluatePolynomial(double x, double[] coefficients)
        {
            double result = 0;
            for (int i = 0; i < coefficients.Length; i++)
            {
                result += coefficients[i] * Math.Pow(x, i);
            }
            return result;
        }

        private double ComputeRSquared(List<double> actual, List<double> predicted)
        {
            if (actual.Count != predicted.Count || !actual.Any()) return 0;

            double mean = actual.Average();
            double ssTot = actual.Sum(v => Math.Pow(v - mean, 2));
            double ssRes = actual.Zip(predicted, (a, p) => Math.Pow(a - p, 2)).Sum();

            return ssTot > 0 ? 1 - ssRes / ssTot : 0;
        }

        private double ComputeStandardError(List<double> actual, List<double> predicted, int parameters)
        {
            if (actual.Count <= parameters) return 0;
            double ssRes = actual.Zip(predicted, (a, p) => Math.Pow(a - p, 2)).Sum();
            return Math.Sqrt(ssRes / (actual.Count - parameters));
        }
    }

    public class ClusteringEngine
    {
        public ClusteringResult PerformClustering(List<DataPoint> points, int k = 3)
        {
            if (points.Count < k) k = points.Count;
            
            var centroids = InitializeCentroids(points, k);
            var assignments = new int[points.Count];
            var previousAssignments = new int[points.Count];
            int iterations = 0;
            int maxIterations = 100;

            while (!assignments.SequenceEqual(previousAssignments) && iterations < maxIterations)
            {
                previousAssignments = (int[])assignments.Clone();

                for (int i = 0; i < points.Count; i++)
                {
                    double minDist = double.MaxValue;
                    int closestCluster = 0;
                    for (int c = 0; c < k; c++)
                    {
                        double dist = Distance(points[i], centroids[c]);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            closestCluster = c;
                        }
                    }
                    assignments[i] = closestCluster;
                }

                for (int c = 0; c < k; c++)
                {
                    var clusterPoints = points.Where((p, i) => assignments[i] == c).ToList();
                    if (clusterPoints.Any())
                    {
                        centroids[c] = new DataPoint
                        {
                            Features = Enumerable.Range(0, clusterPoints[0].Features.Length)
                                .Select(f => clusterPoints.Average(p => p.Features[f])).ToArray()
                        };
                    }
                }

                iterations++;
            }

            var clusters = new List<Cluster>();
            for (int c = 0; c < k; c++)
            {
                var clusterPoints = points.Where((p, i) => assignments[i] == c).ToList();
                clusters.Add(new Cluster
                {
                    Id = c,
                    Points = clusterPoints,
                    Centroid = centroids[c],
                    Size = clusterPoints.Count
                });
            }

            return new ClusteringResult
            {
                Clusters = clusters,
                Iterations = iterations,
                Converged = iterations < maxIterations
            };
        }

        private DataPoint[] InitializeCentroids(List<DataPoint> points, int k)
        {
            var random = new Random(42);
            var centroids = new DataPoint[k];
            var selected = new HashSet<int>();

            for (int i = 0; i < k; i++)
            {
                int index;
                do
                {
                    index = random.Next(points.Count);
                } while (selected.Contains(index));

                selected.Add(index);
                centroids[i] = new DataPoint { Features = points[index].Features };
            }

            return centroids;
        }

        private double Distance(DataPoint a, DataPoint b)
        {
            return Math.Sqrt(a.Features.Zip(b.Features, (x, y) => Math.Pow(x - y, 2)).Sum());
        }
    }

    public interface IDataTransform
    {
        double[] Transform(double[] data);
        double[] InverseTransform(double[] data);
    }

    public enum AnalysisType
    {
        Descriptive,
        Correlation,
        Regression,
        Clustering,
        Sorting,
        Filtering,
        Summarization,
        TrendAnalysis,
        OutlierDetection,
        FrequencyAnalysis
    }

    public class DataRange
    {
        public string Address { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }

        public IEnumerable<double> GetNumericValues() => Enumerable.Empty<double>();
        public Dictionary<string, List<double>> GetColumns() => new Dictionary<string, List<double>>();
        public List<DataPoint> ToDataPoints() => new List<DataPoint>();
        public List<DataRecord> ToDataRecords() => new List<DataRecord>();
    }

    public class DataPoint
    {
        public double[] Features { get; set; }
        public string Label { get; set; }
    }

    public class DataRecord
    {
        public double SortKey { get; set; }
        public bool MatchesCriteria { get; set; }
    }

    public class AnalysisResult { }

    public class DescriptiveStatistics : AnalysisResult
    {
        public int Count { get; set; }
        public double Sum { get; set; }
        public double Mean { get; set; }
        public double Median { get; set; }
        public double Mode { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double Range { get; set; }
        public double Variance { get; set; }
        public double StandardDeviation { get; set; }
        public double CoefficientOfVariation { get; set; }
        public double Skewness { get; set; }
        public double Kurtosis { get; set; }
        public double FirstQuartile { get; set; }
        public double ThirdQuartile { get; set; }
        public double InterquartileRange { get; set; }
        public Dictionary<int, double> Percentiles { get; set; }
        public double ConfidenceInterval95 { get; set; }
        public double ConfidenceInterval99 { get; set; }
        public double GeometricMean { get; set; }
        public double HarmonicMean { get; set; }
        public double TrimmedMean { get; set; }
    }

    public class CorrelationResult : AnalysisResult
    {
        public Dictionary<string, Dictionary<string, double>> Correlations { get; set; }
        public string StrongestPositive { get; set; }
        public string StrongestNegative { get; set; }
        public string WeakestCorrelation { get; set; }
        public double AverageCorrelation { get; set; }
    }

    public class RegressionResult : AnalysisResult
    {
        public RegressionModel Model { get; set; }
    }

    public class RegressionModel
    {
        public string Type { get; set; }
        public double[] Coefficients { get; set; }
        public List<double> Predicted { get; set; }
        public double RSquared { get; set; }
        public double StandardError { get; set; }
    }

    public class ClusteringResult : AnalysisResult
    {
        public List<Cluster> Clusters { get; set; }
        public int Iterations { get; set; }
        public bool Converged { get; set; }
    }

    public class Cluster
    {
        public int Id { get; set; }
        public List<DataPoint> Points { get; set; }
        public DataPoint Centroid { get; set; }
        public int Size { get; set; }
    }

    public class SortedDataResult : AnalysisResult
    {
        public List<DataRecord> SortedRecords { get; set; }
        public List<object> OriginalIndices { get; set; }
    }

    public class FilteredDataResult : AnalysisResult
    {
        public List<DataRecord> FilteredRecords { get; set; }
        public List<int> FilteredIndices { get; set; }
    }

    public class SummaryResult : AnalysisResult
    {
        public Dictionary<string, ColumnSummary> ColumnSummaries { get; set; }
        public int TotalRows { get; set; }
        public int TotalColumns { get; set; }
        public string DataType { get; set; }
    }

    public class ColumnSummary
    {
        public string ColumnName { get; set; }
        public int Count { get; set; }
        public double Sum { get; set; }
        public double Mean { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public int UniqueCount { get; set; }
        public int NullCount { get; set; }
        public double MostFrequent { get; set; }
    }

    public class TrendResult : AnalysisResult
    {
        public RegressionModel LinearTrend { get; set; }
        public RegressionModel ExponentialTrend { get; set; }
        public RegressionModel LogarithmicTrend { get; set; }
        public RegressionModel PolynomialTrend { get; set; }
        public string BestFitType { get; set; }
        public RegressionModel BestFitModel { get; set; }
        public double BestFitError { get; set; }
        public SeasonalityResult Seasonality { get; set; }
        public CyclicalResult CyclicalPattern { get; set; }
    }

    public class SeasonalityResult
    {
        public int Period { get; set; }
        public double Strength { get; set; }
        public bool Detected { get; set; }
    }

    public class CyclicalResult
    {
        public int CycleLength { get; set; }
        public double Strength { get; set; }
        public bool Detected { get; set; }
    }

    public class OutlierResult : AnalysisResult
    {
        public List<OutlierInfo> Outliers { get; set; }
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public double InterquartileRange { get; set; }
        public List<OutlierInfo> ModifiedZScoreOutliers { get; set; }
    }

    public class OutlierInfo
    {
        public int Index { get; set; }
        public double Value { get; set; }
        public OutlierType Type { get; set; }
        public double Severity { get; set; }
    }

    public enum OutlierType
    {
        Low,
        High,
        ZScore
    }

    public class FrequencyResult : AnalysisResult
    {
        public List<string> Bins { get; set; }
        public Dictionary<string, long> Frequency { get; set; }
        public Dictionary<string, double> RelativeFrequency { get; set; }
        public Dictionary<string, long> CumulativeFrequency { get; set; }
        public Dictionary<string, double> CumulativeRelativeFrequency { get; set; }
        public double Mode { get; set; }
        public double Median { get; set; }
        public double Mean { get; set; }
    }

    public static class Extensions
    {
        public static double Product(this IEnumerable<double> values)
        {
            return values.Aggregate(1.0, (a, b) => a * b);
        }

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            var seen = new HashSet<TKey>();
            foreach (var item in source)
            {
                var key = keySelector(item);
                if (seen.Add(key))
                    yield return item;
            }
        }
    }
}
