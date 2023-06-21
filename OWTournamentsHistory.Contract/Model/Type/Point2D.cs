namespace OWTournamentsHistory.Contract.Model.Type;

public class Point2D<T>
{
    public required T X { get; set; }
    public decimal? Y { get; set; }

    public static explicit operator Point2D<T> ((T, decimal?) value) => new() { X = value.Item1, Y = value.Item2 };
}
