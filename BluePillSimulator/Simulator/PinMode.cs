using System.Runtime.Serialization;
using static Arduino;

namespace BluePillSimulator.Simulator;

enum PinMode
{
    [EnumMember(Value = "IN")]
    Input = (int)INPUT,

    [EnumMember(Value = "OUT")]
    Output = (int)OUTPUT,

    [EnumMember(Value = "IN (pull-up)")]
    InputPullup = (int)INPUT_PULLUP,

    [EnumMember(Value = "IN (floating)")]
    InputFloating = (int)INPUT_FLOATING,

    [EnumMember(Value = "IN (pull-down)")]
    InputPulldown = (int)INPUT_PULLDOWN,

    [EnumMember(Value = "IN (analog)")]
    InputAnalog = (int)INPUT_ANALOG,

    [EnumMember(Value = "OUT (open-drain)")]
    OutputOpenDrain = (int)OUTPUT_OPEN_DRAIN,
}
