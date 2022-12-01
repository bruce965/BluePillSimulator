using System.Collections.Concurrent;
using static Arduino;

namespace BluePillSimulator.Simulator;

class ArduinoSimulation : IDisposable
{
    // lock on this object for anything that might be accessed from multiple threads
    readonly object _guard = new();

    #region Event queue

    // lock on this object when enqueuing/dequeuing events
    readonly ConcurrentQueue<Action> _eventsQueue = new();

    void EnqueueEvent(Action a) {
        _eventsQueue.Enqueue(a);
    }

    void ProcessEventsQueue()
    {
        while (_eventsQueue.TryDequeue(out var a))
            a();
    }

    #endregion

    #region Arduino stuff

    public Serial Serial { get; } = new();

    public TwoWire Wire { get; } = new();

    public uint millis()
    {
        CheckSuspended();

        lock (_guard)
        {
            if (++_callsToTimeFunctionSinceLastAdvanceTime > 100)
                AdvanceTime();

            return unchecked((uint)(_currentTime.Ticks / TimeSpan.TicksPerMillisecond));
        }
    }

    public uint micros()
    {
        CheckSuspended();

        lock (_guard)
        {
            if (++_callsToTimeFunctionSinceLastAdvanceTime > 100)
                AdvanceTime();

            return unchecked((uint)(_currentTime.Ticks * 1000 / TimeSpan.TicksPerMillisecond));
        }
    }

    public void delay(uint ms) {
        CheckSuspended();

        // TODO: should use simulation time, not host time (causes incorrect timings when simulation is suspended).
        Thread.Sleep(TimeSpan.FromMilliseconds(ms));

        CheckSuspended();

        lock (_guard)
            AdvanceTime();
    }

    public void pinMode(uint pin, uint mode)
    {
        CheckSuspended();

        var highImpedance = mode != OUTPUT && mode != OUTPUT_OPEN_DRAIN;

        lock (_guard)
        {
            var p = GetPin(pin);

            if (p.Mode != mode)
            {
                p.Mode = mode;

                EnqueueEvent(() => SimulationPinModeChange?.Invoke(this, new(pin, mode)));
            }

            if (highImpedance && p.OutputVoltage != null)
            {
                p.OutputVoltage = null;

                EnqueueEvent(() => SimulationPinOutputVoltageChange?.Invoke(this, new(pin, null)));
            }
        }

        ProcessEventsQueue();
    }

    public uint digitalRead(uint pin)
    {
        CheckSuspended();

        float? v;
        uint m;
        lock (_guard)
        {
            var p = GetPin(pin);
            v = p.InputVoltage;
            m = p.Mode;
        }

        if (v == null)
            return m == INPUT_PULLUP ? HIGH : LOW;

        return v > 2 ? HIGH : LOW;
    }

    public void digitalWrite(uint pin, uint val)
    {
        CheckSuspended();

        lock (_guard)
        {
            var p = GetPin(pin);

            float? newVoltage = val == HIGH ? 3.3f : val == LOW ? 0f : null;

            if (p.OutputVoltage != newVoltage)
            {
                p.OutputVoltage = newVoltage;

                EnqueueEvent(() => SimulationPinOutputVoltageChange?.Invoke(this, new(pin, newVoltage)));
            }
        }

        ProcessEventsQueue();
    }

    #endregion

    #region Simulation threading magic

    Thread? _thread;
    bool _suspended = true;

    public bool IsSimulationRunning
    {
        get
        {
            lock (_guard)
                return !_suspended;
        }
    }

    public ArduinoSimulation(Action setup, Action loop)
    {
        _thread = new(() =>
        {
            _callsToTimeFunctionSinceLastAdvanceTime = 0;
            _lastAdvanceTime = DateTime.UtcNow;

            try
            {
                CheckSuspended();

                setup();

                CheckSuspended();

                while (true)
                {
                    CheckSuspended();

                    loop();

                    AdvanceTime();
                }
            }
            catch (ThreadInterruptedException) when (_thread == null)
            {
                // simulation is over, nothing to do
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        });

        _thread.Name = nameof(ArduinoSimulation);
        _thread.Start();
    }

    public void SimulationPlay()
    {
        CheckDisposed();

        lock (_guard)
        {
            SkipTime();

            _suspended = false;
        }
    }

    public void SimulationPause()
    {
        CheckDisposed();

        lock (_guard)
        {
            _suspended = true;
        }
    }

    public void SimulationDisposeAndWait(TimeSpan timeout)
    {
        var t = _thread;

        Dispose();

        if (t != null)
            t.Join(timeout);
    }

    /// <summary>
    /// Call this method from any Arduino API to suspend simulation if requested from outside.
    /// </summary>
    void CheckSuspended()
    {
        var suspended = false;

        lock (_guard)
        {
            if (_suspended)
            {
                if (_thread == null)
                    Thread.Sleep(-1); // accept thread interruption

                suspended = true;
            }
        }

        // TODO: use something smarter to suspend, like a signal.
        while (suspended)
        {
            lock (_guard)
                suspended = _suspended;

            if (suspended)
                Thread.Sleep(10);
        }
    }

    #endregion

    #region Time tracking

    public enum ClockMode
    {
        Realtime,
        FixedStep,
        Frozen,
    }

    TimeSpan _currentTime = TimeSpan.Zero;
    ClockMode _clockMode = ClockMode.Realtime;

    int _callsToTimeFunctionSinceLastAdvanceTime;
    DateTime _lastAdvanceTime;

    public ClockMode SimulationClockMode
    {
        get
        {
            lock (_guard)
                return _clockMode;
        }
        set
        {
            lock (_guard)
                _clockMode = value;
        }
    }

    void AdvanceTime()
    {
        var now = DateTime.UtcNow;

        _currentTime += _clockMode switch
        {
            ClockMode.Realtime => now - _lastAdvanceTime,
            ClockMode.FixedStep => TimeSpan.FromMilliseconds(100),
            _ => TimeSpan.Zero
        };

        _lastAdvanceTime = now;

        if (_clockMode != ClockMode.Realtime)
            Thread.Sleep(0);
    }

    void SkipTime()
    {
        _lastAdvanceTime = DateTime.UtcNow;
    }

    #endregion

    #region Pins

    public class PinModeChangeEventArgs : EventArgs
    {
        public PinModeChangeEventArgs(uint pin, uint mode)
        {
            Pin = pin;
            Mode = mode;
        }

        public uint Pin { get; }
        public uint Mode { get; }
    }

    public class PinVoltageChangeEventArgs : EventArgs
    {
        public PinVoltageChangeEventArgs(uint pin, float? voltage)
        {
            Pin = pin;
            Voltage = voltage;
        }

        public uint Pin { get; }
        public float? Voltage { get; }
    }

    public delegate void PinModeChangeEventHandler(object sender, PinModeChangeEventArgs e);

    public delegate void PinVoltageChangeEventHandler(object sender, PinVoltageChangeEventArgs e);

    readonly Dictionary<uint, Pin> _pins = new();

    Pin GetPin(uint pin) => _pins.TryGetValue(pin, out var p) ? p : (_pins[pin] = new());

    public void SimulationSetPinInput(uint pin, float? voltage)
    {
        lock (_guard)
            GetPin(pin).InputVoltage = voltage;
    }

    public event PinModeChangeEventHandler? SimulationPinModeChange;

    public event PinVoltageChangeEventHandler? SimulationPinOutputVoltageChange;

    #endregion

    #region IDisposable

    bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            lock (_guard)
            {
                if (_thread != null)
                {
                    _suspended = true;

                    if (_thread.IsAlive)
                    {
                        _thread.Interrupt();
                        _thread = null;
                    }
                }
            }

            // set large fields to null

            _disposed = true;
        }
    }

    // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~ArduinoSimulation()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected void CheckDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ArduinoSimulation));
    }

    #endregion
}
