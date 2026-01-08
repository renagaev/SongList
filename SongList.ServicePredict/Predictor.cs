namespace SongList.ServicePredict;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

public sealed class OnnxServicePredictor : IDisposable, IServicePredictor
{
    private readonly InferenceSession _session;
    private const string _inputName = "input"; // from onnx_meta.json
    private readonly int _nFeatures = FeatureOrder.Length;

    // Feature order (matches onnx_meta.json) without future-only columns.
    private static readonly string[] FeatureOrder =
    [
        "dow_pg", "hour", "length_sec", "n_slides", "unique_slides", "total_time", "total_over_length",
        "mean_dur", "median_dur", "std_dur", "min_dur", "max_dur", "span", "cv_dur", "repeat_ratio",
        "unique_ratio", "shown_ratio", "first_dur", "last_dur", "max_pos", "max_pos_norm", "long_tail_mean",
        "long_tail_ratio", "gap_prev", "delta_to_11m", "delta_to_19m", "is_sunday", "is_saturday",
        "dist_to_service_min", "dist_to_rehearsal_min"
    ];

    public OnnxServicePredictor()
    {
        var path = Path.Combine(Path.GetDirectoryName(typeof(OnnxServicePredictor).Assembly.Location), "model.onnx");
        if (!Path.Exists(path))
            throw new FileNotFoundException($"model.onnx not found. Checked: {path}");

        _session = new InferenceSession(path);
    }

    public PredictionResult Predict(ICollection<Slide> slides, TimeSpan gapPrev)
    {
        if (slides == null || slides.Count == 0)
            return new PredictionResult(false, 0.0f, "no slides");

        // Sort slides by time and drop the first technical slide.
        var ordered = slides.OrderBy(s => s.ShowedAt).ToList();
        ordered = ordered.Skip(1).ToList();
        if (ordered.Count == 0)
            return new PredictionResult(false, 0.0f, "only tech slide");

        // Deduplicate slide_number: keep the last occurrence.
        ordered = ordered.GroupBy(s => s.SlideNumber)
            .Select(g => g.OrderBy(s => s.ShowedAt).Last())
            .OrderBy(s => s.ShowedAt)
            .ToList();
        if (ordered.Count == 0)
            return new PredictionResult(false, 0.0f, "all deduped");

        // Segment by long gaps (>10 minutes); keep the largest segment.
        const double longGapSec = 600;
        var segments = new List<List<Slide>> { new() { ordered[0] } };
        for (var i = 1; i < ordered.Count; i++)
        {
            var gap = (ordered[i].ShowedAt - ordered[i - 1].ShowedAt).TotalSeconds;
            if (gap > longGapSec) segments.Add(new List<Slide>());
            segments[^1].Add(ordered[i]);
        }

        ordered = segments.OrderByDescending(g => g.Count).First();
        if (ordered.Count == 0)
            return new PredictionResult(false, 0.0f, "empty after segment");

        // Durations (clip at 600s).
        var durations = ordered.Select(s =>
        {
            var dur = (s.HiddenAt - s.ShowedAt).TotalSeconds;
            return Math.Max(0, Math.Min(600, dur));
        }).ToList();

        var nSlides = ordered.Count;
        var uniqueSlides = ordered.Select(s => s.SlideNumber).Distinct().Count();
        if (uniqueSlides <= 2)
            return new PredictionResult(false, 0.0f, "unique<=2");
        var totalSlides = ordered.Max(s => s.TotalSlides);
        var shownRatio = totalSlides > 0 ? uniqueSlides / (double)totalSlides : 0.0;

        var startAt = ordered.First().ShowedAt;
        var endAt = ordered.Last().HiddenAt;
        var lengthSec = Math.Max(1e-6, (endAt - startAt).TotalSeconds);

        var totalTime = durations.Sum();
        var mean = durations.Average();
        var median = Median(durations);
        var variance = durations.Count > 1
            ? durations.Select(x => Math.Pow(x - mean, 2)).Sum() / durations.Count
            : 0;
        var std = Math.Sqrt(variance);
        var min = durations.Min();
        var max = durations.Max();
        var maxPos = durations.IndexOf(max) + 1;
        var maxPosNorm = maxPos / (double)nSlides;
        double span = ordered.Max(s => s.SlideNumber) - ordered.Min(s => s.SlideNumber);
        var repeatRatio = 1 - uniqueSlides / (double)nSlides;
        var uniqueRatio = uniqueSlides / (double)nSlides;
        var firstDur = durations.First();
        var lastDur = durations.Last();
        var top3 = durations.OrderByDescending(x => x).Take(3).ToList();
        var longTailMean = top3.Average();
        var longTailRatio = totalTime > 0 ? top3.Sum() / totalTime : 0;
        var totalOverLength = totalTime / lengthSec;

        // Time features
        var hour = startAt.Hour;
        var dowPg = ((int)startAt.DayOfWeek + 6) % 7; // Sunday=0
        var isSunday = dowPg == 0;
        var isSaturday = dowPg == 6;
        double delta11 = Math.Abs(startAt.Hour * 60 + startAt.Minute - 11 * 60);
        double delta19 = Math.Abs(startAt.Hour * 60 + startAt.Minute - 19 * 60);

        // Soft distances to schedule anchors
        var distService = MinDistToAnchors(startAt, [
            (DayOfWeek.Wednesday, 19, 0),
            (DayOfWeek.Friday, 19, 0),
            (DayOfWeek.Sunday, 11, 0),
            (DayOfWeek.Sunday, 19, 0)
        ]);
        var distRehearsal = MinDistToAnchors(startAt, [
            (DayOfWeek.Saturday, 20, 0),
            (DayOfWeek.Sunday, 9, 30),
            (DayOfWeek.Sunday, 20, 30)
        ]);

        // Assemble feature vector
        var feats = new Dictionary<string, float>
        {
            ["dow_pg"] = dowPg,
            ["hour"] = hour,
            ["length_sec"] = (float)lengthSec,
            ["n_slides"] = nSlides,
            ["unique_slides"] = uniqueSlides,
            ["total_time"] = (float)totalTime,
            ["total_over_length"] = (float)totalOverLength,
            ["mean_dur"] = (float)mean,
            ["median_dur"] = (float)median,
            ["std_dur"] = (float)std,
            ["min_dur"] = (float)min,
            ["max_dur"] = (float)max,
            ["span"] = (float)span,
            ["cv_dur"] = (float)(mean == 0 ? 0 : std / mean),
            ["repeat_ratio"] = (float)repeatRatio,
            ["unique_ratio"] = (float)uniqueRatio,
            ["shown_ratio"] = (float)shownRatio,
            ["first_dur"] = (float)firstDur,
            ["last_dur"] = (float)lastDur,
            ["max_pos"] = (float)maxPos,
            ["max_pos_norm"] = (float)maxPosNorm,
            ["long_tail_mean"] = (float)longTailMean,
            ["long_tail_ratio"] = (float)longTailRatio,
            ["gap_prev"] = (float)gapPrev.TotalSeconds,
            ["delta_to_11m"] = (float)delta11,
            ["delta_to_19m"] = (float)delta19,
            ["is_sunday"] = isSunday ? 1f : 0f,
            ["is_saturday"] = isSaturday ? 1f : 0f,
            ["dist_to_service_min"] = (float)distService,
            ["dist_to_rehearsal_min"] = (float)distRehearsal
        };

        var data = new DenseTensor<float>(new[] { 1, _nFeatures });
        for (var i = 0; i < FeatureOrder.Length; i++)
            data[0, i] = feats[FeatureOrder[i]];

        var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(_inputName, data) };
        using var outputs = _session.Run(inputs);
        var probs = outputs.First(o => o.Name == "probabilities").AsEnumerable<float>().ToArray();
        var probTrue = probs.Length == 2 ? probs[1] : probs[0];
        var label = probTrue >= 0.5f;
        return new PredictionResult(label, probTrue, null);
    }

    private static double MinDistToAnchors(DateTimeOffset ts, IEnumerable<(DayOfWeek dow, int h, int m)> anchors)
    {
        var best = double.MaxValue;
        var naive = ts.UtcDateTime;
        foreach (var (dow, h, m) in anchors)
        {
            var targetDow = (int)dow;
            var baseDate = naive.AddDays(targetDow - (int)naive.DayOfWeek);
            foreach (int delta in new[] { -7, 0, 7 })
            {
                var cand = baseDate.AddDays(delta).Date.AddHours(h).AddMinutes(m);
                best = Math.Min(best, Math.Abs((naive - cand).TotalMinutes));
            }
        }

        return best;
    }

    private static double Median(List<double> values)
    {
        if (values.Count == 0) return 0;
        var sorted = values.OrderBy(x => x).ToList();
        var mid = sorted.Count / 2;
        if (sorted.Count % 2 == 0) return (sorted[mid - 1] + sorted[mid]) / 2.0;
        return sorted[mid];
    }

    public void Dispose() => _session.Dispose();
}
