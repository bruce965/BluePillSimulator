using System.Globalization;
using static Arduino;

namespace BluePillSimulator.Simulator;

partial class BoardForm : Form
{
    ArduinoSimulation _simulation;

    Dictionary<uint, PinControl> _pinToControl;
    Dictionary<PinControl, uint> _controlToPin;

    public BoardForm(ArduinoSimulation simulation)
    {
        _simulation = simulation;

        InitializeComponent();

        _pinToControl = new()
        {
            [PB2] = pinBoot1,
            [PB12] = pinB12,
            [PB13] = pinB13,
            [PB14] = pinB14,
            [PB15] = pinB15,
            [PA8] = pinA8,
            [PA9] = pinA9,
            [PA10] = pinA10,
            [PA11] = pinA11,
            [PA12] = pinA12,
            [PA15] = pinA15,
            [PB3] = pinB3,
            [PB4] = pinB4,
            [PB5] = pinB5,
            [PB6] = pinB6,
            [PB7] = pinB7,
            [PB8] = pinB8,
            [PB9] = pinB9,
            [PB11] = pinB11,
            [PB10] = pinB10,
            [PB1] = pinB1,
            [PB0] = pinB0,
            [PA7] = pinA7,
            [PA6] = pinA6,
            [PA5] = pinA5,
            [PA4] = pinA4,
            [PA3] = pinA3,
            [PA2] = pinA2,
            [PA1] = pinA1,
            [PA0] = pinA0,
            [PC15] = pinC15,
            [PC14] = pinC14,
            [PC13] = pinC13,
            [PA13] = pinA13,
            [PA14] = pinA14,
        };

        _controlToPin = _pinToControl.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        _simulation.SimulationPinModeChange += (sender, args) => Invoke(() => pinModeChange(sender, args));
        _simulation.SimulationPinOutputVoltageChange += (sender, args) => Invoke(() => pinOutputVoltageChange(sender, args));

        _simulation.Wire.Update += (sender, args) => Invoke(() => ssd1306.Display = _simulation.Wire._attachedDisplay);
    }

    void playPauseSimulation(object sender, EventArgs e)
    {
        if (_simulation.IsSimulationRunning)
        {
            buttonPlayPause.Text = "Resume simulation";
            _simulation.SimulationPause();
        }
        else
        {
            buttonPlayPause.Text = "Suspend simulation";
            _simulation.SimulationPlay();
        }
    }

    void toggleClockMode(object sender, EventArgs e)
    {
        switch (_simulation.SimulationClockMode)
        {
            case ArduinoSimulation.ClockMode.Realtime:
                buttonClockMode.Text = "Current clock mode: fixed step";
                _simulation.SimulationClockMode = ArduinoSimulation.ClockMode.FixedStep;
                break;

            case ArduinoSimulation.ClockMode.FixedStep:
                buttonClockMode.Text = "Current clock mode: frozen";
                _simulation.SimulationClockMode = ArduinoSimulation.ClockMode.Frozen;
                break;

            case ArduinoSimulation.ClockMode.Frozen:
                buttonClockMode.Text = "Current clock mode: realtime";
                _simulation.SimulationClockMode = ArduinoSimulation.ClockMode.Realtime;
                break;
        }
    }

    void inputChange(object sender, PinControl.InputChangeEventArgs e)
    {
        float? voltage = null;
        if (float.TryParse(e.Input.TrimEnd('V'), NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
            voltage = v;

        if (_controlToPin.TryGetValue((PinControl)sender, out var pin))
            _simulation.SimulationSetPinInput(pin, voltage);
    }

    void pinModeChange(object sender, ArduinoSimulation.PinModeChangeEventArgs e)
    {
        if (!_pinToControl.TryGetValue(e.Pin, out var control))
            return;

        control.PinMode = (PinMode)e.Mode;
    }

    void pinOutputVoltageChange(object sender, ArduinoSimulation.PinVoltageChangeEventArgs e)
    {
        if (!_pinToControl.TryGetValue(e.Pin, out var control))
            return;

        control.PinOutput = e.Voltage == null ? "" : $"{e.Voltage.Value.ToString("0.000", CultureInfo.InvariantCulture)}V";
    }
}
