//Actually useless

using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

[StructLayout(LayoutKind.Explicit, Size = 96)]
struct JoyconInputReport : IInputStateTypeInfo
{
    public FourCC format => new FourCC('H', 'I', 'D');

    [FieldOffset(1)] public byte reportId;

    [InputControl(name = "dpad/left", bit = 0)]
    [InputControl(name = "dpad/up", bit = 1)]
    [InputControl(name = "dpad/down", bit = 2)]
    [InputControl(name = "dpad/right", bit = 3)]
    [InputControl(name = "side/up", bit = 4)]
    [InputControl(name = "side/down", bit = 5)]
    [InputControl(name = "bumper", bit = 6)]
    [InputControl(name = "trigger", bit = 7)]
    [FieldOffset(4)] public byte buttons1;

    [InputControl(name = "special", bit = 1)]
    [InputControl(name = "home", bit = 2)]
    [InputControl(name = "joystickDown", bit = 4)]
    [FieldOffset(5)] public byte buttons2;
}