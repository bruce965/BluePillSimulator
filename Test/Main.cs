using ButtonsTest;
using System.Runtime.CompilerServices;
using static Arduino;
using static Adafruit_SSD1306_h;

OpenSimulator(() =>
{
    DigitalOutput keepPowered = new(PB3, LOW, true); // force to stay powered when output = low
    SquareWave ledBlink = new(LED_BUILTIN, 0.5f); // blink led twice every second (debug, to show it's still running)
    SquareWave buzzer = new(PB9, 2400); // used to make the buzzer buzz
    RotaryEncoder encoder = new(PB12, PB13); // rotary encoder input
    Adafruit_SSD1306 display = new(128, 32, Wire, -1); // OLED display output
    Button timerButton = new(PB10, INPUT_PULLUP); // button input to start timer (rotary encoder button)
    Button powerButton = new(PB11, INPUT_PULLUP); // button input to power on and to switch timer
    TimerClock inactivityTimer = new(60000000); // consider inactive after 60 seconds

    const int32_t TIMER_MIN = 5; // 5s
    const int32_t TIMER_MAX = 3 * 60 * 60; // 3h
    const int32_t TIMER_INITIAL = 300; // 5m

    sbyte focusedTimerIndex = 0;
    TimerStatus[] timers = {
      new() { seconds = TIMER_INITIAL, running = false, clock = new(1000000) },
      new() { seconds = TIMER_INITIAL, running = false, clock = new(1000000) },
    };
    const int32_t TIMERS_COUNT = 2;

    void setup()
    {
        keepPowered.InitPins();
        keepPowered.Set(true);

        Serial.begin(9600);
        Serial.println("init");

        ledBlink.InitPins();
        buzzer.InitPins();
        encoder.InitPins();
        timerButton.InitPins();
        powerButton.InitPins();

        if (!display.begin(SSD1306_SWITCHCAPVCC, 0x3C))
        {
            Serial.println("SSD1306 fail");
        }

        Serial.println("ready");

        updateDisplay();
    }

    void loop()
    {
        ref TimerStatus timer = ref timers[focusedTimerIndex];

        ledBlink.Tick();
        buzzer.Tick();

        int8_t rotation = encoder.Tick();
        if (rotation > 0)
        {
            Serial.println("enc: +1");
            inactivityTimer.Reset();
            display.dim(false);

            timer.seconds += getTimeStep(timer.seconds);

            if (timer.seconds > TIMER_MAX)
                timer.seconds = TIMER_MAX;

            updateDisplay();
        }
        else if (rotation < 0)
        {
            Serial.println("enc: -1");
            inactivityTimer.Reset();
            display.dim(false);

            timer.seconds -= getTimeStep(timer.seconds);

            if (timer.seconds < TIMER_MIN)
                timer.seconds = TIMER_MIN;

            updateDisplay();
        }

        // timer button start/pauses currently focused timer
        timerButton.Tick();
        if (timerButton.IsPress())
        {
            Serial.println("btn: timer");
            inactivityTimer.Reset();
            display.dim(false);

            timer.clock.Reset();

            timer.running = !timer.running;
            updateDisplay();
        }

        // power button toggles currently focused timer
        powerButton.Tick();
        if (powerButton.IsPress())
        {
            Serial.println("btn: power");
            inactivityTimer.Reset();
            display.dim(false);

            if (++focusedTimerIndex >= TIMERS_COUNT)
                focusedTimerIndex = 0;

            updateDisplay();
        }
        if (powerButton.IsHeld())
        {
            powerOff();
        }

        // update timers
        bool anyTimerRunning = false;
        for (int i = 0; i < TIMERS_COUNT; i++)
        {
            ref TimerStatus t = ref timers[i];
            if (!t.running)
                continue;

            anyTimerRunning = true;

            if (t.clock.Tick())
            {
                t.seconds--;

                // if it's the focused timer and the time just decreased, update display
                if (Unsafe.AreSame(ref t, ref timer))
                    updateDisplay();
            }
        }

        // self-shutdown after 60 seconds of inactivity
        if (inactivityTimer.Tick())
        {
            if (anyTimerRunning)
            {
                Serial.println("enter low power mode");
                display.dim(true);
            }
            else
            {
                powerOff();
            }
        }
    }

    void updateDisplay()
    {
        ref TimerStatus timer = ref timers[focusedTimerIndex];

        int h = timer.seconds / 3600;
        int m = (timer.seconds - h * 3600) / 60;
        int s = (timer.seconds - h * 3600 - m * 60);

        bool even = (timer.seconds % 2) != 0;
        bool showDots = !even || !timer.running;

        display.clearDisplay();
        display.setCursor(0, 0);

        display.setTextSize(4);
        display.setTextColor(SSD1306_WHITE);
        display.cp437(true);

        if (h > 0)
        {
            display.setCursor(0, 4);
            display.setTextSize(3);

            display.write('0' + h);
            display.write(showDots ? ':' : ' ');
        }
        else
        {
            display.setCursor(0, 0);
            display.setTextSize(4);
        }

        if (h > 0 || m >= 10)
            display.write('0' + (m / 10));
        else
            display.write(' ');

        display.write('0' + (m % 10));

        display.write(showDots ? ':' : ' ');

        display.write('0' + (s / 10));
        display.write('0' + (s % 10));

        display.display();
    }

    void powerOff()
    {
        Serial.println("power off...");

        // FIX: since this button is indirectly connected to "keepPowered" MOSFET,
        // it would feed 2.4V to the MOSFET if not de-inited before shutting down.
        powerButton.DeinitPins();

        display.clearDisplay();
        display.display();

        keepPowered.Set(false);

        delay(100);
        Serial.println("power off failed");

        while (true) ; // freeze
    }

    int32_t getTimeStep(int32_t time)
    {
        if (time >= 30 * 60) // 30m
            return 5 * 60; // 5m

        if (time >= 6 * 60) // 6m
            return 60; // 1m

        if (time >= 3 * 60) // 3m
            return 30; // 30s

        if (time >= 60) // 1m
            return 15; // 15s

        return 5; // 5s
    }

    return (setup, loop);
});

struct TimerStatus
{
    public int32_t seconds;
    public bool running;
    public TimerClock clock;
};
