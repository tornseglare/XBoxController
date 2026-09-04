using System.Runtime.InteropServices;

namespace MASK.XInput
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vibration(ushort leftMotorSpeed, ushort rightMotorSpeed)
    {
        public readonly ushort LeftMotorSpeed = leftMotorSpeed;
        public readonly ushort RightMotorSpeed = rightMotorSpeed;
    }
}