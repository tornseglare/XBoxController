using System.Runtime.InteropServices;

namespace MASK.XInput
{
    /// <summary>
    /// Describes the library version.
    /// </summary>
    public enum XInputVersion
    {
        /// <summary>
        /// XInput 1.4 ships as part of Windows 8. Use this version for building Windows Store apps or if your desktop app requires Windows 8.
        /// </summary>
        Version14,
        /// <summary>
        /// XInput 9.1.0 ships as part of Windows Vista, Windows 7, and Windows 8. Use this version if your desktop app is intended to run on these versions of Windows and you are using basic XInput functionality.
        /// </summary>
        Version910,
        /// <summary>
        /// XInput 1.3 ships as a redistributable component in the DirectX SDK with support for Windows Vista, Windows 7, and Windows 8. Use this version if your desktop app is intended to run on these versions of Windows and you need functionality that is not supported by XInput 9.1.0.
        /// </summary>
        Version13,
        /// <summary>
        /// Invalid version, XInput failed to load.
        /// </summary>
        Invalid
    }

    /// <summary>
    /// This code is extracted from the great work in Vortice.Windows libraries! Learn more about amerkoleci and his friends good work here:
    /// https://github.com/amerkoleci/Vortice.Windows
    /// 
    /// Good reading about using the xinput library: 
    /// https://learn.microsoft.com/en-us/windows/win32/xinput/getting-started-with-xinput
    /// </summary>
    public static unsafe class xInput
    {
        private static readonly nint s_xinputLibrary;

        // Used to get the state of the controller.
        // https://learn.microsoft.com/en-us/windows/win32/api/xinput/nf-xinput-xinputgetstate
        private static readonly delegate* unmanaged<uint, out State, int> s_XInputGetState;

        // Used to set the vibration level of the controller.
        // https://learn.microsoft.com/en-us/windows/win32/api/xinput/nf-xinput-xinputsetstate
        private static readonly delegate* unmanaged<uint, Vibration*, int> s_XInputSetState;

        // Get type of controller and its capabilities.
        // https://learn.microsoft.com/en-us/windows/win32/api/xinput/nf-xinput-xinputgetcapabilities
        private static readonly delegate* unmanaged<uint, DeviceQueryType, out Capabilities, int> s_XInputGetCapabilities;

        // Deprecated in windows 10 or later.
        // If enable is set to FALSE, XInput will only send neutral data in response to XInputGetState. 
        // https://learn.microsoft.com/en-us/windows/win32/api/xinput/nf-xinput-xinputenable
        private static readonly delegate* unmanaged<int, void> s_XInputEnable;

        // Retrieves the battery type and charge status of a controller.
        // https://learn.microsoft.com/en-us/windows/win32/api/xinput/nf-xinput-xinputgetbatteryinformation
        private static readonly delegate* unmanaged<uint, BatteryDeviceType, out BatteryInformation, int> s_XInputGetBatteryInformation;

        // https://learn.microsoft.com/en-us/windows/win32/api/xinput/nf-xinput-xinputgetkeystroke
        private static readonly delegate* unmanaged<uint, uint, out Keystroke, int> s_XInputGetKeystroke;

        // https://learn.microsoft.com/en-us/windows/win32/api/xinput/nf-xinput-xinputgetaudiodeviceids
        private static readonly delegate* unmanaged<uint, IntPtr, IntPtr, IntPtr, IntPtr, uint> s_XInputGetAudioDeviceIds;

        public static readonly XInputVersion Version = XInputVersion.Invalid;

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="PlatformNotSupportedException"></exception>
        /// <exception cref="EntryPointNotFoundException"></exception>
        static xInput()
        {
            s_xinputLibrary = LoadXInputLibrary(out Version);
            if (Version == XInputVersion.Invalid)
            {
                // Microsoft has DirectX in most windows installations by default, but make sure your game handles a missing dll properly.
                throw new PlatformNotSupportedException("XInput dll not found. Make sure DirectX is installed! You can download it from Microsofts web pages.");
            }

            try
            {
                s_XInputGetState = (delegate* unmanaged<uint, out State, int>)GetExport("XInputGetState");

                s_XInputSetState = (delegate* unmanaged<uint, Vibration*, int>)GetExport("XInputSetState");
                s_XInputGetCapabilities = (delegate* unmanaged<uint, DeviceQueryType, out Capabilities, int>)GetExport("XInputGetCapabilities");

                if (Version != XInputVersion.Version910)
                {
                    s_XInputEnable = (delegate* unmanaged<int, void>)GetExport("XInputEnable");
                    s_XInputGetBatteryInformation = (delegate* unmanaged<uint, BatteryDeviceType, out BatteryInformation, int>)GetExport("XInputGetBatteryInformation");
                    s_XInputGetKeystroke = (delegate* unmanaged<uint, uint, out Keystroke, int>)GetExport("XInputGetKeystroke");
                }

                if (Version == XInputVersion.Version14)
                {
                    s_XInputGetAudioDeviceIds = (delegate* unmanaged<uint, IntPtr, IntPtr, IntPtr, IntPtr, uint>)GetExport("XInputGetAudioDeviceIds");
                }
            }
            catch(EntryPointNotFoundException)
            {
                // Just here to make it clear it can happen. 
                throw;
            }
        }

        /// <summary>
        /// Retrieves the current state of the specified controller.
        /// </summary>
        /// <param name="userIndex">Index of the user's controller. Can be a value from 0 to 3.</param>
        /// <param name="state">Instance of <see cref="State"/> struct.</param>
        /// <returns>True if success, false if not connected or error.</returns>
        public static bool GetState(int userIndex, out State state)
        {
            return s_XInputGetState((uint)userIndex, out state) == 0;
        }

        /// <summary>
        /// The returned pState will have the proper value of wether or not the Guide button is pressed. 
        /// The normal GetState() function masks the value of the Guide button since it is considered not-a-game-button.
        /// As this is an unofficial API, use with care.
        /// </summary>
        /// <remarks>1.4 of the dll produces odd results, therefore 1.3 is used here.</remarks>
        [DllImport("xinput1_3.dll", EntryPoint = "#100")]
        public static extern int GuideButton(int dwUserIndex, ref State pState);

        /// <summary>
        /// Sets the gamepad vibration.
        /// </summary>
        /// <param name="userIndex">Index of the user's controller. Can be a value from 0 to 3.</param>
        /// <param name="leftMotor">The level of the left vibration motor. Valid values are between 0.0 and 1.0, where 0.0 signifies no motor use and 1.0 signifies max vibration.</param>
        /// <param name="rightMotor">The level of the right vibration motor. Valid values are between 0.0 and 1.0, where 0.0 signifies no motor use and 1.0 signifies max vibration.</param>
        /// <returns>True if succeed, false otherwise.</returns>
        public static bool SetVibration(int userIndex, float leftMotor, float rightMotor)
        {
            Vibration vibration = new((ushort)(leftMotor * ushort.MaxValue), (ushort)(rightMotor * ushort.MaxValue));
            return s_XInputSetState((uint)userIndex, &vibration) == 0;
        }

        /// <summary>
        /// Sets the gamepad vibration.
        /// </summary>
        /// <param name="userIndex">Index of the user's controller. Can be a value from 0 to 3.</param>
        /// <param name="leftMotorSpeed">The level of the left vibration motor speed.</param>
        /// <param name="rightMotorSpeed">The level of the right vibration motor speed.</param>
        /// <returns>True if succeed, false otherwise.</returns>
        public static bool SetVibration(uint userIndex, ushort leftMotorSpeed, ushort rightMotorSpeed)
        {
            Vibration vibration = new(leftMotorSpeed, rightMotorSpeed);
            return s_XInputSetState(userIndex, &vibration) == 0;
        }

        /// <summary>
        /// Sets the gamepad vibration.
        /// </summary>
        /// <param name="userIndex">Index of the user's controller. Can be a value from 0 to 3.</param>
        /// <param name="vibration">The <see cref="Vibration"/> to set.</param>
        /// <returns>True if succeed, false otherwise.</returns>
        public static bool SetVibration(uint userIndex, Vibration vibration)
        {
            return s_XInputSetState(userIndex, &vibration) == 0;
        }

        /// <summary>
        /// Set to false to disable all connected controllers. GetState() will return neutral data regardless
        /// of actual user input. Set to true to re-enable all connected controllers.
        /// Good to use if the app gets minimized for example. 
        /// </summary>
        /// <remarks>Deprecated in Windows 10 or later</remarks>
        /// <see cref="https://learn.microsoft.com/en-us/windows/win32/api/xinput/nf-xinput-xinputenable"/>
        public static void SetReporting(bool enableReporting)
        {
            if (Version == XInputVersion.Version910)
            {
                ThrowNotSupportedXInput91("XInputEnable");
            }

            s_XInputEnable(enableReporting ? 1 : 0);
        }

        /// <summary>
        /// Retrieves the battery type and charge status of a wireless controller.
        /// </summary>
        /// <param name="userIndex">Index of the user's controller. Can be a value in the range 0–3. </param>
        /// <param name="batteryDeviceType">Type of the battery device.</param>
        /// <returns>Instance of <see cref="BatteryInformation"/>.</returns>
        public static BatteryInformation GetBatteryInformation(uint userIndex, BatteryDeviceType batteryDeviceType)
        {
            if (Version == XInputVersion.Version910)
            {
                ThrowNotSupportedXInput91("XInputGetBatteryInformation");
            }

            s_XInputGetBatteryInformation(userIndex, batteryDeviceType, out BatteryInformation result);
            return result;
        }

        /// <summary>
        /// Retrieves the battery type and charge status of a wireless controller.
        /// </summary>
        /// <param name="userIndex">Index of the user's controller. Can be a value in the range 0–3. </param>
        /// <param name="batteryDeviceType">Type of the battery device.</param>
        /// <param name="batteryInformation">The battery information.</param>
        /// <returns>True if succeed, false otherwise.</returns>
        public static bool GetBatteryInformation(uint userIndex, BatteryDeviceType batteryDeviceType, out BatteryInformation batteryInformation)
        {
            return s_XInputGetBatteryInformation(userIndex, batteryDeviceType, out batteryInformation) == 0;
        }

        /// <summary>
        /// Retrieves the capabilities and features of a connected controller.
        /// </summary>
        /// <param name="userIndex">Index of the user's controller. Can be a value in the range 0–3. </param>
        /// <param name="deviceQueryType">Type of the device query.</param>
        /// <param name="capabilities">The capabilities of this controller.</param>
        /// <returns>True if the controller is connected and succeed, false otherwise.</returns>
        public static bool GetCapabilities(uint userIndex, DeviceQueryType deviceQueryType, out Capabilities capabilities)
        {
            return s_XInputGetCapabilities(userIndex, deviceQueryType, out capabilities) == 0;
        }

        /// <summary>
        /// Retrieves a gamepad input event.
        /// </summary>
        /// <param name="userIndex">Index of the user's controller. Can be a value in the range 0–3. </param>
        /// <param name="keystroke">The keystroke.</param>
        /// <returns>False if the controller is not connected and no new keys have been pressed, true otherwise.</returns>
        public static bool GetKeystroke(uint userIndex, out Keystroke keystroke)
        {
            if (Version == XInputVersion.Version910)
            {
                ThrowNotSupportedXInput91("XInputGetKeystroke");
            }

            return s_XInputGetKeystroke(userIndex, 0u, out keystroke) == 0;
        }

        public static uint GetAudioDeviceIds(uint userIndex, IntPtr renderDeviceId, IntPtr renderCount, IntPtr captureDeviceId, IntPtr captureCount)
        {
            if (Version != XInputVersion.Version14)
            {
                throw new NotSupportedException($"XInputGetAudioDeviceIds is only supported on XInput 1.4");
            }

            return s_XInputGetAudioDeviceIds(userIndex, renderDeviceId, renderCount, captureDeviceId, captureCount);
        }

        private static void ThrowNotSupportedXInput91(string name)
        {
            throw new NotSupportedException($"{name} is not supported on XInput9.1.0");
        }

        private static nint LoadXInputLibrary(out XInputVersion version)
        {
            if (NativeLibrary.TryLoad("xinput1_4.dll", out IntPtr libraryHandle))
            {
                version = XInputVersion.Version14;
                return libraryHandle;
            }
            else if (NativeLibrary.TryLoad("xinput1_3.dll", out libraryHandle))
            {
                version = XInputVersion.Version13;
                return libraryHandle;
            }
            else if (NativeLibrary.TryLoad("xinput9_1_0.dll", out libraryHandle))
            {
                version = XInputVersion.Version910;
                return libraryHandle;
            }

            version = XInputVersion.Invalid;
            return 0;
        }

        private static nint GetExport(string name) => NativeLibrary.GetExport(s_xinputLibrary, name);
    }
}
