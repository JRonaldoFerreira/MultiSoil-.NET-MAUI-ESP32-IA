// Models/SensorMetric.cs
using System;

namespace MultiSoil_EdgeAI.Models;

[Flags]
public enum SensorMetric
{
    None = 0,
    N = 1 << 0,
    P = 1 << 1,
    K = 1 << 2,
    PH = 1 << 3,
    CE = 1 << 4,
    Temp = 1 << 5,
    Umid = 1 << 6,
    All = N | P | K | PH | CE | Temp | Umid
}

public record SensorReadings(
    double? N, double? P, double? K, double? PH,
    double? CE, double? Temp, double? Umid
);
