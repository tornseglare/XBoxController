using System.Runtime.InteropServices;

namespace MASK.XInput
{
    /// <summary>
    /// Retrieves the battery type and charge status of a wireless controller.
    /// </summary>
    public enum BatteryDeviceType
    {
        /// <summary>
        /// Index of the signed-in gamer associated with the device. 
        /// Can be a value in the range 0-4 ? 1.
        /// </summary>
        Gamepad,
        /// <summary>
        /// Specifies which device associated with this user index should be queried. Must be <strong>BATTERY_DEVTYPE_GAMEPAD</strong> or <strong>BATTERY_DEVTYPE_HEADSET</strong>.
        /// </summary>
        Headset
    }

    /// <summary>
    /// Describes the battery type.
    /// </summary>
    public enum BatteryType : byte
    {
        /// <summary>
        /// The device is not connected. 
        /// </summary>
        Disconnected = 0,
        /// <summary>
        /// The device is a wired device and does not have a battery. 
        /// </summary>
        Wired = 1,
        /// <summary>
        /// The device has an alkaline battery. 
        /// </summary>
        Alkaline = 2,
        /// <summary>
        /// The device has a nickel metal hydride battery. 
        /// </summary>
        Nimh = 3,
        /// <summary>
        /// The device has an unknown battery type. 
        /// </summary>
        Unknown = byte.MaxValue
    }

    /// <summary>
    /// Describes The charge state of the battery.
    /// </summary>
    public enum BatteryLevel : byte
    {
        Empty,
        Low,
        Medium,
        Full
    }

    /// <summary>
    /// Contains information on battery type and charge state.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public readonly struct BatteryInformation
    {
        /// <summary>
        /// The type of battery.
        /// </summary>
        public readonly BatteryType BatteryType;

        /// <summary>
        /// The charge state of the battery.
        /// </summary>
        public readonly BatteryLevel BatteryLevel;
    }
}