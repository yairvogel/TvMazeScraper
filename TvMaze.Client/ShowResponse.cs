namespace TvMaze.Client;

/// <summary>
/// Represents a response from tvmaze endpoint at /shows/{id}?embed=cast
/// </summary>
public record ShowResponse(
    int Id,
    string Name
);

public record Cast(Person Person);

public record Person(
    int Id,
    string Name,
    DateTime? Birthday);
