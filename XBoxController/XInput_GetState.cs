using System.Runtime.InteropServices;

namespace MASK.XInput
{
    [Flags]
    public enum GamepadButtons : ushort
    {
        None = 0,
        DPadUp = 0x0001,
        DPadDown = 0x0002,
        DPadLeft = 0x0004,
        DPadRight = 0x0008,
        Start = 0x0010,
        Back = 0x0020,
        LeftThumb = 0x0040,
        RightThumb = 0x0080,
        LeftShoulder = 0x0100,
        RightShoulder = 0x0200,
        Guide = 0x0400,
        A = 0x1000,
        B = 0x2000,
        X = 0x4000,
        Y = 0x8000
    }

    /// <summary>
    /// Describes the current state of the Xbox 360 Controller.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Gamepad
    {
        public const short LeftThumbDeadZone = 7849;
        public const short RightThumbDeadZone = 8689;
        public const byte TriggerThreshold = 30;

        /// <summary>
        /// Bitmask of the device digital buttons, as follows. 
        /// A set bit indicates that the corresponding button is pressed. 
        /// </summary>
        public GamepadButtons Buttons;

        /// <summary>
        /// The current value of the left trigger analog control. 
        /// The value is between 0 and 255.
        /// </summary>
        public byte LeftTrigger;

        /// <summary>
        /// The current value of the right trigger analog control. 
        /// The value is between 0 and 255.
        /// </summary>
        public byte RightTrigger;

        /// <summary>
        /// Left thumbstick x-axis value. 
        /// Each of the thumbstick axis members is a signed value between -32768 and 32767 describing the position of the thumbstick. 
        /// A value of 0 is centered. 
        /// Negative values signify down or to the left. 
        /// Positive values signify up or to the right. 
        /// The constants <see cref="LeftThumbDeadZone" /> or <see cref="RightThumbDeadZone" /> can be used as a positive and negative value to filter a thumbstick input. 
        /// </summary>
        public short LeftThumbX;

        /// <summary>
        /// Left thumbstick y-axis value. The value is between -32768 and 32767.
        /// </summary>
        public short LeftThumbY;

        /// <summary>
        /// Right thumbstick x-axis value. The value is between -32768 and 32767.
        /// </summary>
        public short RightThumbX;

        /// <summary>
        /// Right thumbstick y-axis value. The value is between -32768 and 32767.
        /// </summary>
        public short RightThumbY;

        public override readonly string ToString() => $"Buttons: {Buttons}, LeftTrigger: {LeftTrigger}, RightTrigger: {RightTrigger}, LeftThumbX: {LeftThumbX}, LeftThumbY: {LeftThumbY}, RightThumbX: {RightThumbX}, RightThumbY: {RightThumbY}";
    }

    /// <summary>
    /// Represents the state of a controller.
    /// </summary>
    /// <remarks>
    /// The <see cref="PacketNumber"/> member is incremented only if the status of the controller has changed since the controller was last polled.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct State
    {
        /// <summary>
        /// The packet number indicates whether there have been any changes in the state of the controller.
        /// If the <see cref="PacketNumber"/> member is the same in sequentially returned <see cref="State"/> structures, the controller state has not changed.
        /// </summary>
        public int PacketNumber;

        /// <summary>
        /// <dd> <p> <strong><see cref="T:Vortice.XInput.Gamepad" /></strong> structure containing the current state of an Xbox 360 Controller.</p> </dd>
        /// </summary>
        public Gamepad Gamepad;
    }
}