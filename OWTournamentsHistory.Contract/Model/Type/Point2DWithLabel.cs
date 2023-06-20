namespace OWTournamentsHistory.Contract.Model.Type;

public class Point2DWithLabel<T>
{
    public string? Label { get; set; }
    public required T X { get; set; }
    public decimal? Y { get; set; }
}
