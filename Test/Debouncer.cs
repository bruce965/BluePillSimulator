public class Debouncer
{
    const byte DEBOUNCE_STATE_READY = 0; // has not been triggered for a while
    const byte DEBOUNCE_STATE_TRIGGERED = 1; // has just been triggered and not released yet
    const byte DEBOUNCE_STATE_DEBOUNCING = 2; // has just been released, waiting debounce period

    TimerClock timer;
    byte state;

    public Debouncer(uint everyMicros) {
        timer = new(everyMicros);
        state = DEBOUNCE_STATE_READY;
    }

    public void Tick()
    {
        // NOTE: if state is "triggered", timer doesn't tick
        if (state == DEBOUNCE_STATE_DEBOUNCING)
        {
            if (timer.Tick())
                state = DEBOUNCE_STATE_READY;
        }
        else if (state == DEBOUNCE_STATE_TRIGGERED)
        {
            // it was still triggered during previous iteration, give it a chance to continue debouncing
            // if "Trigger()" will be called again, state will return to triggered before next "Tick()"
            state = DEBOUNCE_STATE_DEBOUNCING;
        }
    }

    public bool Trigger()
    {
        bool ready = state == DEBOUNCE_STATE_READY;

        // as long as it will stay pressed, timer won't tick
        timer.Reset();
        state = DEBOUNCE_STATE_TRIGGERED;

        return ready;
    }
}
