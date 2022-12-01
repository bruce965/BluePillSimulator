using BluePillSimulator;
using static Adafruit_SSD1306_h;

public class Adafruit_SSD1306 : Adafruit_GFX
{
    internal event EventHandler? Update;

    internal uint16_t[,] _visibleBuffer;

    public Adafruit_SSD1306(uint8_t w, uint8_t h, TwoWire? twi = null,
                   int8_t rst_pin = -1, uint32_t clkDuring = 400000,
                   uint32_t clkAfter = 100000) : base(w, h)
    {
        _visibleBuffer = (uint16_t[,])_buffer.Clone();

        twi ??= Arduino.Wire;
        twi.Attach(this);
    }

    public bool begin(uint8_t switchvcc = SSD1306_SWITCHCAPVCC, uint8_t i2caddr = 0,
             bool reset = true, bool periphBegin = true)
    {
        return true;
    }

    public void clearDisplay()
    {
        _buffer = new uint16_t[_width, _height];
    }

    public void dim(bool dim)
    {
        _dimmed = dim;
        Update?.Invoke(this, EventArgs.Empty);
    }

    public void display()
    {
        _visibleBuffer = (uint16_t[,])_buffer.Clone();
        Update?.Invoke(this, EventArgs.Empty);
    }
}
