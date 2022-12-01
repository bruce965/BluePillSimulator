using static Arduino;

namespace ButtonsTest;

class DigitalOutput
{
    uint pin;
    uint trueDwVal;
    bool highImpedance;

    public DigitalOutput(uint pin, uint trueDwVal)
    {
        this.pin = pin;
        this.trueDwVal = trueDwVal;
        highImpedance = false;
    }

    public DigitalOutput(uint pin, uint trueDwVal, bool highImpedance)
    {
        this.pin = pin;
        this.trueDwVal = trueDwVal;
        this.highImpedance = highImpedance;
    }

    public void InitPins()
    {
        pinMode(pin, OUTPUT);
        digitalWrite(pin, (trueDwVal == LOW) ? HIGH : LOW);
    }

    public void Set(bool val)
    {
        if (highImpedance)
        {
            if (val)
            {
                pinMode(pin, OUTPUT);
                digitalWrite(pin, trueDwVal);
            }
            else
            {
                pinMode(pin, INPUT_FLOATING);
            }
        }
        else
        {
            if (val)
            {
                digitalWrite(pin, trueDwVal);
            }
            else
            {
                digitalWrite(pin, (trueDwVal == LOW) ? HIGH : LOW);
            }
        }
    }
}
