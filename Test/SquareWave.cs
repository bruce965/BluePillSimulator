﻿using static Arduino;

public class SquareWave
{
    uint pin;
    bool high;
    TimerClock timer;

    public SquareWave(uint pin, float freq)
    {
        this.pin = pin;
        high = false;
        timer = new((uint)(1000000.0f / 2.0f / freq));
    }

    public void InitPins()
    {
        pinMode(pin, OUTPUT);
        digitalWrite(pin, high ? HIGH : LOW);
    }

    public void AdjustFreq(float freq)
    {
        timer = new((uint)(1000000.0f / 2.0f / freq));
    }

    public void Tick()
    {
        if (timer.Tick())
        {
            high = !high;
            digitalWrite(pin, high ? HIGH : LOW);
        }
    }
}
