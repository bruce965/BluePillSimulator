namespace BluePillSimulator.Simulator;

public partial class SSD1306Display : Control
{
    Adafruit_SSD1306? _display;
    public Adafruit_SSD1306? Display
    {
        get => _display;
        set
        {
            if (_display != null)
                _display.Update -= _listener;

            _display = value;
            
            if (_display != null)
                _display.Update += _listener;
        }
    }

    readonly Brush _unlit = new SolidBrush(Color.Black);
    readonly Brush _lit = new SolidBrush(Color.Cyan);
    readonly Brush _dim = new SolidBrush(Color.DarkCyan);

    EventHandler _listener;

    public SSD1306Display()
    {
        InitializeComponent();

        _listener = (sender, e) => Invoke(() => Invalidate());
    }

    protected override void OnPaint(PaintEventArgs pe)
    {
        pe.Graphics.FillRectangle(_unlit, pe.ClipRectangle);

        if (_display == null)
            return;

        for (var y = pe.ClipRectangle.Top; y < pe.ClipRectangle.Bottom;)
        {
            var y1 = y * _display._height / Height;
            var ySize = Height / _display._height;

            for (var x = pe.ClipRectangle.Left; x < pe.ClipRectangle.Right;)
            {
                var x1 = x * _display._width / Width;
                var xSize = Width / _display._width;

                var b = _display._buffer[x1, y1] == 0 ? _unlit : (_display._dimmed ? _dim : _lit);
                pe.Graphics.FillRectangle(b, new(x, y, xSize, ySize));

                x += xSize;
            }

            y += ySize;
        }
    }
}
