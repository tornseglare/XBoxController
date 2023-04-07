using System.Diagnostics;
using Vortice.XInput;

namespace MASK
{
    /// <summary>
    /// One xbox controller.
    /// </summary>
    public class XBoxController : IController
    {
        State state;
        bool connected = false; // If user disconnect during play this goes false.
        private bool everConnected = false; // Set to true when the controller goes online for the first time.
        int lastPacketNumber = 0; // To see if anything has changed from previous call to Update(), compare the dwPacketNumber in State.
        Gamepad lastGamepad = new(); // Once anything has changed, lets compare with the last 'frames' state, to detect if any buttons etc. has been pressed, and so on.

        /// <summary>
        /// The id of the controller, a value between 0 and 3.
        /// </summary>
        public readonly int UserIndex;

        /// <summary>
        /// Is set to true when the controller goes online for the first time. 
        /// </summary>
        public bool EverConnected => everConnected;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Connected => connected;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool A => (state.Gamepad.Buttons & GamepadButtons.A) != GamepadButtons.None;
        public bool B => (state.Gamepad.Buttons & GamepadButtons.B) != GamepadButtons.None;
        public bool X => (state.Gamepad.Buttons & GamepadButtons.X) != GamepadButtons.None;
        public bool Y => (state.Gamepad.Buttons & GamepadButtons.Y) != GamepadButtons.None;
        public bool DPadDown => (state.Gamepad.Buttons & GamepadButtons.DPadDown) != GamepadButtons.None;
        public bool DPadUp => (state.Gamepad.Buttons & GamepadButtons.DPadUp) != GamepadButtons.None;
        public bool DPadLeft => (state.Gamepad.Buttons & GamepadButtons.DPadLeft) != GamepadButtons.None;
        public bool DPadRight => (state.Gamepad.Buttons & GamepadButtons.DPadRight) != GamepadButtons.None;
        public bool Start => (state.Gamepad.Buttons & GamepadButtons.Start) != GamepadButtons.None;
        public bool Back => (state.Gamepad.Buttons & GamepadButtons.Back) != GamepadButtons.None;
        public bool Guide => (state.Gamepad.Buttons & GamepadButtons.Guide) != GamepadButtons.None;
        public bool LeftShoulder => (state.Gamepad.Buttons & GamepadButtons.LeftShoulder) != GamepadButtons.None;
        public bool RightShoulder => (state.Gamepad.Buttons & GamepadButtons.RightShoulder) != GamepadButtons.None;
        public bool LeftThumb => (state.Gamepad.Buttons & GamepadButtons.LeftThumb) != GamepadButtons.None;
        public bool RightThumb => (state.Gamepad.Buttons & GamepadButtons.RightThumb) != GamepadButtons.None;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public byte RightTrigger => state.Gamepad.RightTrigger;
        public byte LeftTrigger => state.Gamepad.LeftTrigger;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int RightThumbX => state.Gamepad.RightThumbX;
        public int RightThumbY => state.Gamepad.RightThumbY;
        public int LeftThumbX => state.Gamepad.LeftThumbX;
        public int LeftThumbY => state.Gamepad.LeftThumbY;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool JustPressedA { get; private set; } = false;
        public bool JustPressedB { get; private set; } = false;
        public bool JustPressedX { get; private set; } = false;
        public bool JustPressedY { get; private set; } = false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool JustReleasedA { get; private set; } = false;
        public bool JustReleasedB { get; private set; } = false;
        public bool JustReleasedX { get; private set; } = false;
        public bool JustReleasedY { get; private set; } = false;

        /// <summary>
        /// Create a new XBoxController object and check if the controller is connected.
        /// </summary>
        /// <param name="dwUserIndex">a value between 0 and 3</param>
        /// <returns>a new XBoxController object</returns>
        /// <remarks>You should use the constructor directly instead.</remarks>
        [Obsolete("CreateController() is deprecated, please use constructor and UpdateConnectedState() instead.")]
        public static XBoxController CreateController(int dwUserIndex)
        {
            XBoxController controller = new(dwUserIndex);

            controller.UpdateConnectedState();
            return controller;
        }

        /// <summary>
        /// Create a new XBoxController object. 
        /// </summary>
        /// <param name="dwUserIndex">a value between 0 and 3</param>
        public XBoxController(int dwUserIndex)
        {
            this.UserIndex = dwUserIndex;

            connected = false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Update()
        {
            if (!connected) return false; // Call Reconnected() once it is reconnected.

            ResetJusts();

            if (!XInput.GetState(UserIndex, out state))
            {
                // Ouch, controller disconnected!
                Debug.WriteLine($"Controller {UserIndex} disconnected!");
                connected = false;
                return true;
            }
            else
            {
                connected = true;

                if (state.PacketNumber == lastPacketNumber)
                {
                    // Nothing has changed since last time.
                }
                else
                {
                    // Things has happened! Player is actually using the controller.
                    CompareLastGamepad();

                    lastPacketNumber = state.PacketNumber;
                    lastGamepad = state.Gamepad;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Mark the controller as connected.
        /// Should only be called from XBoxControllerPoller when it detects the controller is once again connected.
        /// </summary>
        [Obsolete("Reconnected() is deprecated, please use UpdateConnectedState() instead.")]
        public void Reconnected()
        {
            connected = true;
            Debug.WriteLine($"Controller {UserIndex} reconnected!");
        }

        /// <summary>
        /// If controller was disconnected but is again connected, this will set the controller to connected.
        /// </summary>
        /// <returns>True if connected.</returns>
        public bool UpdateConnectedState()
        {
            // From the docs: Note that the return value of XInputGetState can be used to determine if the controller is connected.
            if (XInput.GetState(UserIndex, out state))
            {
                everConnected = true;
                connected = true;
                lastPacketNumber = state.PacketNumber;
            }
            else
            {
                connected = false;
            }

            return connected;
        }

        private void CompareLastGamepad()
        {
            if (lastGamepad.Buttons != state.Gamepad.Buttons)
            {
                // Buttons are still binary, so lets record when they get pressed and released.
                // NOTE: This can be coded for all the buttons as well as for the directional pad, if needed.
                // See https://stackoverflow.com/questions/3261451/using-a-bitmask-in-c-sharp for the messybestialities of getting the bitmasks right.

                // Previous loop the button was _not_ pressed, but are now.
                if (A && (lastGamepad.Buttons & GamepadButtons.A) == GamepadButtons.None)
                {
                    JustPressedA = true;
                }
                if (B && (lastGamepad.Buttons & GamepadButtons.B) == GamepadButtons.None)
                {
                    JustPressedB = true;
                }
                if (X && (lastGamepad.Buttons & GamepadButtons.X) == GamepadButtons.None)
                {
                    JustPressedX = true;
                }
                if (Y && (lastGamepad.Buttons & GamepadButtons.Y) == GamepadButtons.None)
                {
                    JustPressedY = true;
                }

                // Previous loop the button _was_ pressed, but are no longer.
                if (!A && (lastGamepad.Buttons & GamepadButtons.A) != GamepadButtons.None)
                {
                    JustReleasedA = true;
                }
                if (!B && (lastGamepad.Buttons & GamepadButtons.B) != GamepadButtons.None)
                {
                    JustReleasedB = true;
                }
                if (!X && (lastGamepad.Buttons & GamepadButtons.X) != GamepadButtons.None)
                {
                    JustReleasedX = true;
                }
                if (!Y && (lastGamepad.Buttons & GamepadButtons.Y) != GamepadButtons.None)
                {
                    JustReleasedY = true;
                }
            }
        }

        private void ResetJusts()
        {
            JustPressedA = false;
            JustPressedB = false;
            JustPressedX = false;
            JustPressedY = false;

            JustReleasedA = false;
            JustReleasedB = false;
            JustReleasedX = false;
            JustReleasedY = false;
        }
    }
}