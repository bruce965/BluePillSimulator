using System.Drawing.Drawing2D;

namespace BluePillSimulator.Simulator;

public partial class Oscilloscope : Control, IDisposable
{
    readonly float[] _samples = new float[4096];
    int _nextSample = 0;

    bool _hold = true;
    public bool Hold
    {
        get => _hold;
        set
        {
            var held = _hold;
            _hold = value;

            if (_hold != held)
                Invalidate();
        }
    }

    public TimeSpan SimulationTime { private get; set; }

    readonly Brush _bg = new SolidBrush(Color.DarkCyan);
    readonly Brush _fg = new SolidBrush(Color.Lime);
    readonly Pen _dash = new(new HatchBrush(HatchStyle.SmallCheckerBoard, Color.Lime, Color.DarkCyan));
    readonly Pen _full = new(new SolidBrush(Color.Lime));

    public Oscilloscope()
    {
        InitializeComponent();
    }

    public void PushSampleThreadSafe(float value)
    {
        lock (_samples)
        {
            if (++_nextSample >= _samples.Length)
                _nextSample = 0;

            _samples[_nextSample] = value;
        }
    }

    protected override void OnPaint(PaintEventArgs pe)
    {
        var hFactor = Height / 11f;
        var hOffset = 8f;

        pe.Graphics.FillRectangle(_bg, 0, 0, Width, Height);
        pe.Graphics.DrawLine(_dash, 0, hOffset * hFactor, Width, hOffset * hFactor);

        if (Hold)
            pe.Graphics.DrawString("HOLD", Font, _fg, 5, 5);
        else
            pe.Graphics.FillRectangle(_bg, 5, 5, 20, 10);

        //var previousSample = _samples[0];
        for (var i = 0; i < _samples.Length; i++)
        {
            //var thisSample = _samples[i];

            //pe.Graphics.DrawLine(
            //    _full,
            //    (i-1) * Width / _samples.Length,
            //    (hOffset - previousSample) * hFactor,
            //    i * Width / _samples.Length,
            //    (hOffset - thisSample) * hFactor);

            //previousSample = thisSample;

            var j = reverseBits4096(i);
            pe.Graphics.FillRectangle(_fg, j * Width / _samples.Length, (hOffset - _samples[j]) * hFactor, 1, 1);
        }
    }

    static int reverseBits4096(int x)
    {
        int y = 0;
        for (var i = 0; i < 12; i++)
        {
            y <<= 1;
            y |= x & 1;
            x >>= 1;
        }

        return y;
    }
}
