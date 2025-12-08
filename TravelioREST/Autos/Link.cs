namespace TravelioREST.Autos;

public sealed class Link
{
    public required string Rel { get; set; }
    public required string Href { get; set; }
    public required string Method { get; set; }
}
