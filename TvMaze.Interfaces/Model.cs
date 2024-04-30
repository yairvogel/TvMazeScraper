using System.Runtime.CompilerServices;

namespace TvMaze.Interfaces;

public record Show(
    int Id,
    string Name,
    List<Cast> Cast);

public record Cast(
    int Id,
    string Name,
    DateTime? Birthday);