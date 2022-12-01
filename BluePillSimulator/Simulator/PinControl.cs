using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace BluePillSimulator.Simulator;

partial class PinControl : UserControl
{
    public class InputChangeEventArgs : EventArgs
    {
        public InputChangeEventArgs(string input)
        {
            Input = input;
        }

        public string Input { get; }
    }

    public delegate void InputChangeEventHandler(object sender, InputChangeEventArgs e);

    static readonly object s_inputChangeEvent = new();

    [Category("Appearance"), Description("Pin name"), DefaultValue(null)]
    public string PinName
    {
        get => labelPin.Text;
        set => labelPin.Text = value;
    }

    [Category(category: "Data"), Description("Pin input"), DefaultValue("NC")]
    public string PinInput
    {
        get => comboBoxInput.Text;
        set
        {
            comboBoxInput.Text = value;
            UpdateVoltageControl(comboBoxInput);
        }
    }

    [Category(category: "Data"), Description("Pin output"), DefaultValue("")]
    public string PinOutput
    {
        get => textBoxOutput.Text;
        set
        {
            textBoxOutput.Text = value;
            UpdateVoltageControl(textBoxOutput);
        }
    }

    [Category(category: "Data"), Description("Pin mode"), DefaultValue(default(PinMode))]
    public PinMode PinMode
    {
        //get => textBoxMode.Text;
        set => textBoxMode.Text = typeof(PinMode).GetField(Enum.GetName(value) ?? "")?.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? Enum.GetName(value) ?? value.ToString();
    }

    protected override Size DefaultSize => new(300, 29);

    [Category("Action"), Description("Input change")]
    public event InputChangeEventHandler InputChange
    {
        add => Events.AddHandler(s_inputChangeEvent, value);
        remove => Events.RemoveHandler(s_inputChangeEvent, value);
    }

    public PinControl()
    {
        InitializeComponent();
    }

    void inputChanged(object sender, EventArgs e)
    {
        UpdateVoltageControl(comboBoxInput);
        ((InputChangeEventHandler?)Events[s_inputChangeEvent])?.Invoke(this, new(comboBoxInput.Text));
    }

    void toggleInput(object sender, EventArgs e)
    {
        // index 0 = not connected
        // index 1 = low
        // index 2 = high
        if (comboBoxInput.SelectedIndex == 1)
            comboBoxInput.SelectedIndex = 2;
        else
            comboBoxInput.SelectedIndex = 1;
    }

    void UpdateVoltageControl(Control control)
    {
        if (float.TryParse(control.Text.TrimEnd('V'), NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
        {
            control.BackColor = Color.FromArgb(VoltageToByte(v, 3.3f, 0f), VoltageToByte(v, 0f, 3.3f) / 2, 0);
            control.ForeColor = Color.White;
        }
        else
        {
            control.BackColor = SystemColors.Window;
            control.ForeColor = SystemColors.ControlText;
        }
    }

    static byte VoltageToByte(float v, float min = 0f, float max = 3.3f)
    {
        var flip = false;
        if (min > max)
        {
            var buff = max;
            max = min;
            min = buff;

            flip = true;
        }

        if (v > max)
            v = max;

        if (v < min)
            v = min;

        var norm = (v - min) / (max - min);

        var b = (byte)(norm * 255f);
        return flip ? (byte)(255 - b) : b;
    }
}
