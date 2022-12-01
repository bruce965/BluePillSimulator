using static Arduino;

public class Button
{
    uint pin;
    uint dwMode;
    bool down;
    uint downStartMicros;
    bool press;
    uint holdMicros;
    Debouncer debounce;

    public Button(uint pin, uint dwMode)
    {
        this.pin = pin;
        this.dwMode = dwMode;
        down = false;
        press = false;
        //downStartMicros = 0;
        //holdMicros = 0;
        debounce = new(10000);
    }

    public void InitPins()
    {
        pinMode(pin, dwMode);
    }

    public void DeinitPins()
    {
        pinMode(pin, INPUT_FLOATING);
    }

    public void Tick()
    {
        debounce.Tick();

        uint now = micros();

        bool isHigh = digitalRead(pin) == HIGH;
        bool triggerOnHigh = dwMode == INPUT_PULLDOWN;

        // press status only lasts for one tick
        press = false;

        // is currently being pressed?
        if (isHigh == triggerOnHigh)
        {
            // wait debounce period before actually marking as pressed
            if (debounce.Trigger())
            {
                down = true;
                downStartMicros = now;

                press = true;
            }

            // how long has it been held for?
            holdMicros = now - downStartMicros;
        }

        // released or not pressed?
        else
        {
            down = false;
        }
    }

    public bool IsDown()
    {
        return down;
    }

    public bool IsPress()
    {
        return press;
    }

    public bool IsHeld(int micros = 1000000)
    {
        if (!down)
            return false;

        return holdMicros >= micros;
    }
}
