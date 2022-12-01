namespace BluePillSimulator;

public class TwoWire
{
    internal Adafruit_SSD1306? _attachedDisplay { get; private set; }

    internal event EventHandler? Update;

    public void Attach(Adafruit_SSD1306? display)
    {
        _attachedDisplay = display;
        Update?.Invoke(this, EventArgs.Empty);
    }
}
