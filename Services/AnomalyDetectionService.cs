using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;

namespace PCOptimizer.Services
{
    // Data model for performance metrics
    public class PerformanceData
    {
        public float Value { get; set; }
    }

    // Result from spike detection
    public class SpikePrediction
    {
        [VectorType(3)]
        public double[] Prediction { get; set; } = new double[3];
    }

    // Result from change point detection
    public class ChangePointPrediction
    {
        [VectorType(4)]
        public double[] Prediction { get; set; } = new double[4];
    }

    // Anomaly types
    public enum AnomalyType
    {
        None,
        Spike,        // Temporary burst (e.g., brief CPU spike)
        ChangePoint   // Persistent shift (e.g., memory leak causing RAM to climb)
    }

    // Anomaly detection result
    public class AnomalyResult
    {
        public AnomalyType Type { get; set; }
        public string MetricName { get; set; } = "";
        public float Value { get; set; }
        public double Confidence { get; set; }  // 0.0 to 1.0
        public string Description { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class AnomalyDetectionService
    {
        private readonly MLContext _mlContext;

        // Separate pipelines for each metric
        private ITransformer? _cpuSpikeModel;
        private ITransformer? _gpuSpikeModel;
        private ITransformer? _ramSpikeModel;

        private ITransformer? _cpuChangePointModel;
        private ITransformer? _gpuChangePointModel;
        private ITransformer? _ramChangePointModel;

        private PredictionEngine<PerformanceData, SpikePrediction>? _cpuSpikeEngine;
        private PredictionEngine<PerformanceData, SpikePrediction>? _gpuSpikeEngine;
        private PredictionEngine<PerformanceData, SpikePrediction>? _ramSpikeEngine;

        private PredictionEngine<PerformanceData, ChangePointPrediction>? _cpuChangePointEngine;
        private PredictionEngine<PerformanceData, ChangePointPrediction>? _gpuChangePointEngine;
        private PredictionEngine<PerformanceData, ChangePointPrediction>? _ramChangePointEngine;

        // Historical data buffers (need min 12-20 points for effective detection)
        private readonly Queue<float> _cpuHistory = new Queue<float>();
        private readonly Queue<float> _gpuHistory = new Queue<float>();
        private readonly Queue<float> _ramHistory = new Queue<float>();

        private const int MIN_DATA_POINTS = 20;  // Minimum data points before starting detection
        private const int MAX_HISTORY_SIZE = 100; // Keep last 100 data points

        public AnomalyDetectionService()
        {
            _mlContext = new MLContext(seed: 1);
            InitializeModels();
        }

        private void InitializeModels()
        {
            // CPU spike detection
            var cpuSpikeTrainingData = _mlContext.Data.LoadFromEnumerable(new List<PerformanceData>());
            var cpuSpikePipeline = _mlContext.Transforms.DetectIidSpike(
                outputColumnName: nameof(SpikePrediction.Prediction),
                inputColumnName: nameof(PerformanceData.Value),
                confidence: 95,  // 95% confidence level
                pvalueHistoryLength: 20  // Size of sliding window
            );
            _cpuSpikeModel = cpuSpikePipeline.Fit(cpuSpikeTrainingData);
            _cpuSpikeEngine = _mlContext.Model.CreatePredictionEngine<PerformanceData, SpikePrediction>(_cpuSpikeModel);

            // GPU spike detection
            var gpuSpikeTrainingData = _mlContext.Data.LoadFromEnumerable(new List<PerformanceData>());
            var gpuSpikePipeline = _mlContext.Transforms.DetectIidSpike(
                outputColumnName: nameof(SpikePrediction.Prediction),
                inputColumnName: nameof(PerformanceData.Value),
                confidence: 95,
                pvalueHistoryLength: 20
            );
            _gpuSpikeModel = gpuSpikePipeline.Fit(gpuSpikeTrainingData);
            _gpuSpikeEngine = _mlContext.Model.CreatePredictionEngine<PerformanceData, SpikePrediction>(_gpuSpikeModel);

            // RAM spike detection
            var ramSpikeTrainingData = _mlContext.Data.LoadFromEnumerable(new List<PerformanceData>());
            var ramSpikePipeline = _mlContext.Transforms.DetectIidSpike(
                outputColumnName: nameof(SpikePrediction.Prediction),
                inputColumnName: nameof(PerformanceData.Value),
                confidence: 95,
                pvalueHistoryLength: 20
            );
            _ramSpikeModel = ramSpikePipeline.Fit(ramSpikeTrainingData);
            _ramSpikeEngine = _mlContext.Model.CreatePredictionEngine<PerformanceData, SpikePrediction>(_ramSpikeModel);

            // CPU change point detection
            var cpuChangePointTrainingData = _mlContext.Data.LoadFromEnumerable(new List<PerformanceData>());
            var cpuChangePointPipeline = _mlContext.Transforms.DetectIidChangePoint(
                outputColumnName: nameof(ChangePointPrediction.Prediction),
                inputColumnName: nameof(PerformanceData.Value),
                confidence: 95,
                changeHistoryLength: 20  // Number of points to look back for change detection
            );
            _cpuChangePointModel = cpuChangePointPipeline.Fit(cpuChangePointTrainingData);
            _cpuChangePointEngine = _mlContext.Model.CreatePredictionEngine<PerformanceData, ChangePointPrediction>(_cpuChangePointModel);

            // GPU change point detection
            var gpuChangePointTrainingData = _mlContext.Data.LoadFromEnumerable(new List<PerformanceData>());
            var gpuChangePointPipeline = _mlContext.Transforms.DetectIidChangePoint(
                outputColumnName: nameof(ChangePointPrediction.Prediction),
                inputColumnName: nameof(PerformanceData.Value),
                confidence: 95,
                changeHistoryLength: 20
            );
            _gpuChangePointModel = gpuChangePointPipeline.Fit(gpuChangePointTrainingData);
            _gpuChangePointEngine = _mlContext.Model.CreatePredictionEngine<PerformanceData, ChangePointPrediction>(_gpuChangePointModel);

            // RAM change point detection
            var ramChangePointTrainingData = _mlContext.Data.LoadFromEnumerable(new List<PerformanceData>());
            var ramChangePointPipeline = _mlContext.Transforms.DetectIidChangePoint(
                outputColumnName: nameof(ChangePointPrediction.Prediction),
                inputColumnName: nameof(PerformanceData.Value),
                confidence: 95,
                changeHistoryLength: 20
            );
            _ramChangePointModel = ramChangePointPipeline.Fit(ramChangePointTrainingData);
            _ramChangePointEngine = _mlContext.Model.CreatePredictionEngine<PerformanceData, ChangePointPrediction>(_ramChangePointModel);
        }

        public List<AnomalyResult> DetectAnomalies(PerformanceMetrics metrics)
        {
            var anomalies = new List<AnomalyResult>();

            // Add current metrics to history
            AddToHistory(_cpuHistory, metrics.CpuUsage);
            AddToHistory(_gpuHistory, metrics.GpuUsage);
            AddToHistory(_ramHistory, (float)metrics.RamPercent);

            // Only start detection after we have enough data points
            if (_cpuHistory.Count < MIN_DATA_POINTS)
            {
                return anomalies; // Not enough data yet
            }

            // Detect CPU anomalies
            var cpuAnomalies = DetectMetricAnomalies(
                "CPU Usage",
                metrics.CpuUsage,
                _cpuSpikeEngine,
                _cpuChangePointEngine
            );
            anomalies.AddRange(cpuAnomalies);

            // Detect GPU anomalies
            var gpuAnomalies = DetectMetricAnomalies(
                "GPU Usage",
                metrics.GpuUsage,
                _gpuSpikeEngine,
                _gpuChangePointEngine
            );
            anomalies.AddRange(gpuAnomalies);

            // Detect RAM anomalies
            var ramAnomalies = DetectMetricAnomalies(
                "RAM Usage",
                (float)metrics.RamPercent,
                _ramSpikeEngine,
                _ramChangePointEngine
            );
            anomalies.AddRange(ramAnomalies);

            return anomalies;
        }

        private List<AnomalyResult> DetectMetricAnomalies(
            string metricName,
            float value,
            PredictionEngine<PerformanceData, SpikePrediction>? spikeEngine,
            PredictionEngine<PerformanceData, ChangePointPrediction>? changePointEngine)
        {
            var anomalies = new List<AnomalyResult>();

            if (spikeEngine == null || changePointEngine == null)
                return anomalies;

            var dataPoint = new PerformanceData { Value = value };

            // Spike detection
            var spikePrediction = spikeEngine.Predict(dataPoint);
            // Prediction[0] = Alert (0 = no spike, 1 = spike)
            // Prediction[1] = Raw score
            // Prediction[2] = P-value (confidence)
            if (spikePrediction.Prediction[0] == 1)
            {
                anomalies.Add(new AnomalyResult
                {
                    Type = AnomalyType.Spike,
                    MetricName = metricName,
                    Value = value,
                    Confidence = 1.0 - spikePrediction.Prediction[2], // P-value to confidence
                    Description = $"{metricName} spike detected: {value:F1}%",
                    Timestamp = DateTime.Now
                });
            }

            // Change point detection
            var changePointPrediction = changePointEngine.Predict(dataPoint);
            // Prediction[0] = Alert (0 = no change, 1 = change detected)
            // Prediction[1] = Raw score
            // Prediction[2] = P-value
            // Prediction[3] = Martingale score (confidence measure)
            if (changePointPrediction.Prediction[0] == 1)
            {
                anomalies.Add(new AnomalyResult
                {
                    Type = AnomalyType.ChangePoint,
                    MetricName = metricName,
                    Value = value,
                    Confidence = changePointPrediction.Prediction[3] / 100.0, // Normalize martingale score
                    Description = $"{metricName} change point detected: Performance pattern shifted to {value:F1}%",
                    Timestamp = DateTime.Now
                });
            }

            return anomalies;
        }

        private void AddToHistory(Queue<float> history, float value)
        {
            history.Enqueue(value);

            // Maintain max size
            while (history.Count > MAX_HISTORY_SIZE)
            {
                history.Dequeue();
            }
        }

        public int GetHistoryCount(string metricName)
        {
            return metricName.ToLower() switch
            {
                "cpu" => _cpuHistory.Count,
                "gpu" => _gpuHistory.Count,
                "ram" => _ramHistory.Count,
                _ => 0
            };
        }

        public bool IsReady()
        {
            // System is ready when we have enough data points
            return _cpuHistory.Count >= MIN_DATA_POINTS;
        }
    }
}
