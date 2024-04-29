using System.Text.Json.Serialization;

namespace TvMaze.Client;

/// <summary>
/// Represents a response from tvmaze endpoint at /shows/{id}?embed=cast
/// </summary>
public record ShowResponse(
    int Id,
    string Name,
    [property: JsonPropertyName("_embedded")] Embedded Embedded
);

public record Embedded(List<Cast> Cast);

public record Cast(Person Person);

public record Person(
    int Id,
    string Name,
    DateTime? Birthday);
