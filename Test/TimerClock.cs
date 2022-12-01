using static Arduino;

public class TimerClock
{
    uint every;
    uint last;
    uint next;

    public TimerClock(uint everyMicros)
    {
        every = everyMicros;
        last = micros();
        next = last + every;
    }

    public bool Tick()
    {
        uint now = micros();

        if (now < next)
            return false;

        // handle overflow
        if (last > next && now > last)
            return false;

        last = now;
        next = now + every;

        return true;
    }

    public void Reset()
    {
        last = micros();
        next = last + every;
    }
}
