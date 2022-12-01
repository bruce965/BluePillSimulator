namespace BluePillSimulator.Simulator;

class Pin
{
    public uint Mode { get; set; }

    /// <summary>
    /// External voltage, or <see langword="null"/> if not connected.
    /// </summary>
    public float? InputVoltage { get; set; }

    /// <summary>
    /// Output voltage, or <see langword="null"/> if high impedence.
    /// </summary>
    public float? OutputVoltage { get; set; }
}
