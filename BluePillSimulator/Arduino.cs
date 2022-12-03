using BluePillSimulator;
using BluePillSimulator.Simulator;

public static class Arduino
{
    public const uint LOW = 0x0;
    public const uint HIGH = 0x1;
    public const uint CHANGE = 0x2;
    public const uint FALLING = 0x3;
    public const uint RISING = 0x4;

    // Official Arduino
    public const uint INPUT = 0x0;
    public const uint OUTPUT = 0x1;
    public const uint INPUT_PULLUP = 0x2;
    // STM32 extension
    public const uint INPUT_FLOATING = INPUT;
    public const uint INPUT_PULLDOWN = 0x3;
    public const uint INPUT_ANALOG = 0x4;
    public const uint OUTPUT_OPEN_DRAIN = 0x5;

    // Left Side
    public const uint PB9 = 0;
    public const uint PB8 = 1;
    public const uint PB7 = 2;
    public const uint PB6 = 3;
    public const uint PB5 = 4;
    public const uint PB4 = 5;
    public const uint PB3 = 6;
    public const uint PA15 = 7;
    public const uint PA12 = 8; // USB DP
    public const uint PA11 = 9; // USB DM
    public const uint PA10 = 10;
    public const uint PA9 = 11;
    public const uint PA8 = 12;
    public const uint PB15 = 13;
    public const uint PB14 = 14;
    public const uint PB13 = 15;
    public const uint PB12 = 16; // LED Blackpill
    // Right side
    public const uint PC13 = 17; // LED Bluepill
    public const uint PC14 = 18;
    public const uint PC15 = 19;
    public const uint PA0  = 0xC0;
    public const uint PA1  = 0xC1;
    public const uint PA2  = 0xC2;
    public const uint PA3  = 0xC3;
    public const uint PA4  = 0xC4;
    public const uint PA5  = 0xC5;
    public const uint PA6  = 0xC6;
    public const uint PA7  = 0xC7;
    public const uint PB0  = 0xC8;
    public const uint PB1  = 0xC9;
    public const uint PB10 = 30;
    public const uint PB11 = 31;
    // Other
    public const uint PB2  = 32; // BOOT1
    public const uint PA13 = 33; // SWDI0
    public const uint PA14 = 34; // SWCLK

    public const uint LED_BUILTIN = PC13;

    static ArduinoSimulation? s_instance;

    static ArduinoSimulation Sim => s_instance ?? throw new InvalidOperationException("Simulation not started.");

    public static Serial Serial => Sim.Serial;

    public static TwoWire Wire => Sim.Wire;

    public static uint millis() => Sim.millis();

    public static uint micros() => Sim.micros();

    public static void delay(uint ms) => Sim.delay(ms);

    public static void pinMode(uint pin, uint mode) => Sim.pinMode(pin, mode);

    public static uint digitalRead(uint pin) => Sim.digitalRead(pin);

    public static uint analogRead(uint pin) => Sim.analogRead(pin);

    public static void digitalWrite(uint pin, uint val) => Sim.digitalWrite(pin, val);

    /// <summary>
    /// Open a window to run simulation (does not halt current thread),
    /// should be the first call after the <see langword="using"/> statements.
    /// </summary>
    public static void OpenSimulator(Func<(Action, Action)> prepare)
    {
        if (s_instance != null)
            throw new InvalidOperationException("Simulation already started.");

        Action? setup = null;
        Action? loop = null;

        ArduinoSimulation simulation = new(() => { (setup, loop) = prepare(); setup(); }, () => loop!.Invoke());
        try
        {
            s_instance = simulation;

            var thread = new Thread(() => {
                try
                {
                    //ApplicationConfiguration.Initialize();
                    Application.Run(new BoardForm(simulation));
                }
                finally
                {
                    try
                    {
                        simulation.SimulationDisposeAndWait(TimeSpan.FromSeconds(1));
                    }
                    finally
                    {
                        // the simulation is still running, it must be disposed before setting global reference to null
                        s_instance = null;
                    }
                }
            });

            thread.Start();
        }

        // if thread construction failed, dispose immediately,
        // otherwise let the thread dispose the simulation
        catch (Exception)
        {
            s_instance = null;
            simulation.Dispose();

            throw;
        }
    }
}
