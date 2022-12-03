using ButtonsTest;
using System.Runtime.CompilerServices;
using static Arduino;
using static Adafruit_SSD1306_h;

OpenSimulator(() =>
{
    const uint32_t BATTERY_PIN = PB0; // optional: connected to battery through a 1:1 resistive divider
    const uint32_t BATTERY_CHARGE_PIN = PB1; // optional: high input when battery is charged
    DigitalOutput keepPowered = new(PB3, LOW, true); // MOSFET force to stay powered when output = low
    SquareWave ledBlink = new(LED_BUILTIN, 0.5f); // blink LED twice every second (debug, to show it's still running)
    SquareWave buzzer = new(PB9, 2400); // used to make the buzzer buzz
    RotaryEncoder encoder = new(PB12, PB13); // rotary encoder input
    Adafruit_SSD1306 display = new(128, 32, Wire, -1); // OLED display output
    Button timerButton = new(PB10, INPUT_PULLUP); // button input to start timer (rotary encoder button)
    Button powerButton = new(PB11, INPUT_PULLUP); // button input to power on and to switch timer
    TimerClock inactivityTimer = new(60000000); // consider inactive after 60 seconds

    const int32_t TICKS_PER_SECOND = 16;  // 16 ticks every second
    const int32_t TIMER_MIN = TICKS_PER_SECOND * 5; // 5s
    const int32_t TIMER_MAX = TICKS_PER_SECOND * 3 * 60 * 60; // 3h
    const int32_t TIMER_INITIAL = TICKS_PER_SECOND * 3; // 5m  // DEBUG: 3 seconds

    int8_t focusedTimerIndex = 0;
    int8_t lowestBeepingTimerIndex = -1;
    TimerStatus[] timers = {
      new() { ticks = TIMER_INITIAL, running = false, clock = new(1000000 / TICKS_PER_SECOND) },
      new() { ticks = TIMER_INITIAL, running = false, clock = new(1000000 / TICKS_PER_SECOND) },
      new() { ticks = TIMER_INITIAL, running = false, clock = new(1000000 / TICKS_PER_SECOND) },
    };
    const int32_t TIMERS_COUNT = 3;

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

        buzzer.ForceLowOutput(true);

        if (!display.begin(SSD1306_SWITCHCAPVCC, 0x3C))
        {
            Serial.println("SSD1306 fail");
        }

        Serial.println("ready");

        pinMode(BATTERY_PIN, INPUT_PULLUP);
        uint32_t bat = analogRead(BATTERY_PIN);
        bool lowBatt = bat > 155 && bat < 543; // 155 = 0.5V (probably not connected), 543 = 1.75V; assuming a 1:1 resistive divider to battery with 3.5V cutoff

        pinMode(BATTERY_CHARGE_PIN, INPUT_PULLDOWN);
        bool charging = digitalRead(BATTERY_CHARGE_PIN) == HIGH;

        if (charging)
        {
            Serial.println("charging");

            for (int i = 0; i < 2; i++)
            {
                drawBatteryIcon(1);
                delay(500);
                drawBatteryIcon(2);
                delay(500);
                drawBatteryIcon(3);
                delay(500);
                drawBatteryIcon(4);
                delay(500);
            }
        }
        else if (lowBatt)
        {
            Serial.println("low batt");

            for (int i = 0; i < 3; i++)
            {
                drawBatteryIcon(1);
                delay(500);
                drawBatteryIcon(0);
                delay(500);
            }
        }

        updateDisplay(true);
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

            timer.ticks += getTimeStep(timer.ticks);

            if (timer.ticks > TIMER_MAX)
                timer.ticks = TIMER_MAX;

            resetElapsedTimers();
            updateDisplay(false);
        }
        else if (rotation < 0)
        {
            Serial.println("enc: -1");
            inactivityTimer.Reset();
            display.dim(false);

            timer.ticks -= getTimeStep(timer.ticks);

            if (timer.ticks < TIMER_MIN)
                timer.ticks = TIMER_MIN;

            resetElapsedTimers();
            updateDisplay(false);
        }

        // timer button start/pauses currently focused timer
        timerButton.Tick();
        if (timerButton.IsPress())
        {
            Serial.println("btn: timer");
            inactivityTimer.Reset();
            display.dim(false);

            timer.clock.Reset(); // avoid jump when resuming timer

            // toggle status
            timer.running = !timer.running;

            resetElapsedTimers();
            updateDisplay(false);
        }

        // power button toggles currently focused timer
        powerButton.Tick();
        if (powerButton.IsPress())
        {
            Serial.println("btn: power");
            inactivityTimer.Reset();
            display.dim(false);

            // focus next timer
            if (++focusedTimerIndex >= TIMERS_COUNT)
                focusedTimerIndex = 0;

            resetElapsedTimers();
            updateDisplay(true); // force update because selected timer changed
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
                t.ticks--;

                // if it just started beeping, update beeping status
                if (t.ticks == 0)
                    updateLowestBeepingTimerIndex();

                // if it's the focused timer and the time just decreased, update display
                if (Unsafe.AreSame(ref t, ref timer))
                    updateDisplay(false);
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

        // update buzzer
        if (lowestBeepingTimerIndex == -1)
        {
            buzzer.ForceLowOutput(true);
        }
        else
        {
            int ticks = (uint8_t)(-timers[lowestBeepingTimerIndex].ticks) % 16;

            // buzzer does a [_-_-_-_-________] pattern using the timer's ticks
            buzzer.ForceLowOutput(ticks >= 8 || (ticks % 2) != 0);
        }
    }

    void drawBatteryIcon(uint8_t level)
    {
        display.clearDisplay();

        display.drawRect(56, 11, 15, 8, SSD1306_WHITE);
        display.drawRect(57, 12, 13, 6, SSD1306_BLACK);
        display.drawRect(71, 13, 1, 4, SSD1306_WHITE);

        // NOTE: max 4 levels
        for (uint8_t i = 0; i < level; i++)
            display.drawRect((int16_t)(58 + i * 3), 13, 2, 4, SSD1306_WHITE);

        display.display();
    }

    int _lastH = 0;
    int _lastM = 0;
    int _lastS = 0;
    bool _lastShowDots = false;
    void updateDisplay(bool force)
    {
        ref TimerStatus timer = ref timers[focusedTimerIndex];

        int seconds = timer.ticks / TICKS_PER_SECOND;

        if (seconds < 0)
            seconds = -seconds;

        int h = seconds / 3600;
        int m = (seconds - h * 3600) / 60;
        int s = (seconds - h * 3600 - m * 60);

        bool blink = (((uint32_t)timer.ticks * 2 / TICKS_PER_SECOND) % 2) != 0; // blink once every second
        bool showDots = blink || !timer.running;
        bool showTime = timer.ticks >= 0 || blink;

        if (!force && h == _lastH && m == _lastM && s == _lastS && showDots == _lastShowDots)
            return;

        _lastH = h;
        _lastM = m;
        _lastS = s;
        _lastShowDots = showDots;

        display.clearDisplay();
        display.setCursor(0, 0);

        display.setTextSize(4);
        display.setTextColor(SSD1306_WHITE);
        display.cp437(true);

        if (showTime)
        {
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
        }

        if (TIMERS_COUNT > 1)
        {
            display.setTextWrap(false);
            display.setCursor(128 - 5, 0);
            display.setTextSize(1);
            display.write('1' + focusedTimerIndex);
            display.setTextWrap(true);
        }

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

    void updateLowestBeepingTimerIndex()
    {
        lowestBeepingTimerIndex = -1;
        for (int i = 0; i < TIMERS_COUNT; i++)
        {
            ref TimerStatus t = ref timers[i];
            
            // not running or still has time left? not beeping
            if (!t.running || t.ticks > 0)
                continue;

            lowestBeepingTimerIndex = (int8_t)i;
            break;
        }
    }

    void resetElapsedTimers()
    {
        for (int i = 0; i < TIMERS_COUNT; i++)
        {
            ref TimerStatus t = ref timers[i];

            // only resets elapsed timers
            if (t.ticks > 0)
                continue;

            t.ticks = TIMER_INITIAL;
            t.running = false;
            break;
        }

        lowestBeepingTimerIndex = -1;
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
    public int32_t ticks;
    public bool running;
    public TimerClock clock;
};
