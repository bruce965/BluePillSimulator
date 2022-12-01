using static Arduino;

public class RotaryEncoder
{
    const byte ROT_STATE_FLOATING = 0; // both floating, ready for next event
    const byte ROT_STATE_A_SHORTED = 1; // A shorted and B floating, wheel rotating clockwise
    const byte ROT_STATE_B_SHORTED = 2; // B shorted and A floating, wheel rotating counter-clockwise
    const byte ROT_STATE_SHORTED = 3; // something recently shorted, waiting to return floating

    uint pinA;
    uint pinB;
    byte state;

    public RotaryEncoder(uint pinA, uint pinB)
    {
        this.pinA = pinA;
        this.pinB = pinB;
        state = ROT_STATE_FLOATING;
    }

    public void InitPins()
    {
        pinMode(pinA, INPUT_PULLUP);
        pinMode(pinB, INPUT_PULLUP);

        Tick();
    }

    public sbyte Tick()
    {
        bool shortedA = digitalRead(pinA) == LOW;
        bool shortedB = digitalRead(pinB) == LOW;

        if (state == ROT_STATE_SHORTED)
        {
            if (!shortedA && !shortedB)
            {
                state = ROT_STATE_FLOATING;
            }

            return 0;
        }

        sbyte rotation = 0;
        if (shortedA)
        {
            if (shortedB)
            {
                switch (state)
                {
                    case ROT_STATE_A_SHORTED:
                        rotation = +1;
                        break;

                    case ROT_STATE_B_SHORTED:
                        rotation = -1;
                        break;
                }

                state = ROT_STATE_SHORTED;
            }
            else
            {
                state = ROT_STATE_A_SHORTED;
            }
        }
        else
        {
            if (shortedB)
            {
                state = ROT_STATE_B_SHORTED;
            }
            else
            {
                state = ROT_STATE_FLOATING;
            }
        }

        return rotation;
    }
}
