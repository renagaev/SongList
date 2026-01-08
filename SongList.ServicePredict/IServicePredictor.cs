namespace SongList.ServicePredict;

public record Slide(int SlideNumber, DateTimeOffset ShowedAt, DateTimeOffset HiddenAt, int TotalSlides = 0);

public record PredictionResult(bool Label, float Probability, string? Reason);
public interface IServicePredictor
{
    public PredictionResult Predict(ICollection<Slide> slides, TimeSpan gapPrev);
}
