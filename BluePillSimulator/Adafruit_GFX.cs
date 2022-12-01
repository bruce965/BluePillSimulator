using BluePillSimulator;
using static Adafruit_GFX_h;

public class Adafruit_GFX
{
    internal int16_t WIDTH;        ///< This is the 'raw' display width - never changes
    internal int16_t HEIGHT;       ///< This is the 'raw' display height - never changes
    internal int16_t _width;       ///< Display width as modified by current rotation
    internal int16_t _height;      ///< Display height as modified by current rotation
    internal int16_t cursor_x;     ///< x location to start print()ing text
    internal int16_t cursor_y;     ///< y location to start print()ing text
    internal uint16_t textcolor;   ///< 16-bit background color for print()
    internal uint16_t textbgcolor; ///< 16-bit text color for print()
    internal uint8_t textsize_x;   ///< Desired magnification in X-axis of text to print()
    internal uint8_t textsize_y;   ///< Desired magnification in Y-axis of text to print()
    internal uint8_t rotation;     ///< Display rotation (0 thru 3)
    internal bool wrap;            ///< If set, 'wrap' text at right edge of display
    internal bool _cp437;          ///< If set, use correct CP437 charset (default is off)
    internal GFXfont gfxFont;      ///< Pointer to special font

    internal uint16_t[,] _buffer;
    internal bool _dimmed;

    public Adafruit_GFX(uint8_t w, uint8_t h)
    {
        _width = w;
        _height = h;

        _buffer = new uint16_t[w, h];
        gfxFont = GFXfont.Default;
    }

    public void setCursor(int16_t x, int16_t y)
    {
        cursor_x = x;
        cursor_y = y;
    }

    public void setTextSize(uint8_t s) => setTextSize(s, s);

    public void setTextSize(uint8_t s_x, uint8_t s_y)
    {
        textsize_x = (s_x > 0) ? s_x : (uint8_t)1;
        textsize_y = (s_y > 0) ? s_y : (uint8_t)1;
    }

    public void setTextColor(uint16_t c)
    {
        textcolor = textbgcolor = c;
    }

    public void cp437(bool x = true)
    {
        _cp437 = x;
    }

    public size_t write(int c) => write((uint8_t)c);

    public size_t write(uint8_t c) {
        return gfxFont.write(this, c);
    }

    public void writePixel(int16_t x, int16_t y, uint16_t color)
    {
        if (x < 0 || x >= _buffer.GetLength(0) || y < 0 || y >= _buffer.GetLength(1))
            return;

        _buffer[x, y] = color;
    }
}
