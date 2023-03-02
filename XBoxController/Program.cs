﻿using System.Data;
using System.Diagnostics;
using Vortice.XInput;

namespace XBoxController
{
    // https://github.com/amerkoleci/Vortice.Windows
    // Källkoden hittar du här: C:\Users\tornseglare\Documents\Vortice.Windows
    // 
    // Liten repetition om hur man kopplar till en dll:
    // private static readonly nint s_xinputLibrary;
    // private static readonly delegate* unmanaged<int, out State, int> s_XInputGetState;
    // 
    // Ladda in xinput dll:
    // s_xinputLibrary = LoadXInputLibrary(out Version);
    //   NativeLibrary.TryLoad("xinput1_4.dll", out IntPtr libraryHandle)
    // 
    // Sen letar vi upp XInputGetState i xinput dllen:
    // s_XInputGetState = (delegate* unmanaged<int, out State, int>)GetExport("XInputGetState");
    //   private static nint GetExport(string name) => NativeLibrary.GetExport(s_xinputLibrary, name);
    // 
    //  <-System.Runtime.InteropServices.NativeLibrary är microsofts lilla sköna. 
    //    Man får en pekare till funktionen XInputGetState i dllen.

    // https://learn.microsoft.com/en-us/windows/win32/xinput/getting-started-with-xinput
    // <-Han har gjort en riktigt tajjt koppling till detta c++ bibliotek.


    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Console.WriteLine(XInput.Version);

            double lastTime = 0;   
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            while (true)
            {
                if(XBoxControllerPoller.xBoxControllers.Count > 0)
                {
                    XBoxControllerPoller.xBoxControllers[0].Update();

                    if(XBoxControllerPoller.xBoxControllers[0].JustPressedA)
                    {
                        Console.WriteLine("You just pressed the A button!");
                    }
                }

                if (stopWatch.Elapsed.TotalSeconds - lastTime >= 2)
                {
                    lastTime = stopWatch.Elapsed.TotalSeconds;
                    // Every 3 seconds.
                    if(XBoxControllerPoller.IterateControllers())
                    {
                        Console.WriteLine("One or more controllers found and connected! *Showing some fantastic user feedback here*");
                    }
                }

                // It's sleeping waaay more than a millisecond here. (According to spec it sleep minimum of 1 milliseconds, but probably/always more than 15)
                Thread.Sleep(1);
            }
        }
    }

    static class XBoxControllerPoller
    {
        // Keyed on the controls dwUserIndex, decided by windows when the control is connected, cable-wise.
        // "The number corresponds to the port that the controller is plugged into, and is not modifiable."
        // 
        public static readonly Dictionary<int, XBoxController> xBoxControllers = new();

        /// <summary>
        /// Polls for new controllers. 
        /// Also reconnect disconnected controllers once they return to life.
        /// For performance reasons, don't call XInputGetState for an 'empty' user slot every frame. We recommend that you space out checks for new controllers every few seconds instead.
        /// </summary>
        public static bool IterateControllers()
        {
            bool newControllerConnected = false;

            // 0 to 3. 
            for (int i = 0; i < 3; i++)
            {
                bool res = XInput.GetState(i, out State state);

                if (res)
                {
                    // Har testat med en inkopplad kontroll, testa med två!
                    // Har inte två usb-kablar, men mina handkontroller har i varje fall varsin hash code: :-)
                    // 246684740
                    // 1695051208
                    //  <-Försökte med alla medel ansluta via usb, det SKA gå, men windows hittade aldrig min kontroll. 
                    //  <-XBoxen uppdaterar handkontrollernas mjukvara själv, visste inte ens de hade det. Men appen 'XBox Accessories' låter dig göra det på windows med.
                    // 
                    XBoxController? xBoxController = XBoxController.CreateController(i);

                    if(xBoxController != null)
                    {
                        if (!xBoxControllers.ContainsKey(i))
                        {
                            Debug.WriteLine("We got contact! " + i);
                            Debug.WriteLine(state.Gamepad.GetHashCode());

                            xBoxControllers.Add(i, xBoxController);
                            newControllerConnected = true;
                        }
                        else
                        {
                            // Already in the list, just set it to connected by calling Update().
                            if (xBoxControllers[i].Connected == false)
                            {
                                xBoxControllers[i].Reconnected();
                            }
                        }
                    }
                }
            }

            return newControllerConnected;
        }
    }

    class XBoxController
    {
        int dwUserIndex;
        State state;
        bool connected = false; // If user disconnect during play this goes false.
        int lastPacketNumber = 0; // To see if anything has changed from previous call to Update(), compare the dwPacketNumber in State.
        Gamepad lastGamepad = new(); // Once anything has changed, lets compare with the last 'frames' state, to detect if any buttons etc. has been pressed, and so on.

        // Always check this before reading any states.
        public bool Connected { get { return connected; } }

        // These are immediate states, right-now-states.
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

        // These goes true when the previous state was false, but current state is true. NOTE: These are reset to false in every call to Update(), so make sure you check these every loop if you are using them!
        public bool JustPressedA { get; private set; } = false;
        public bool JustPressedB { get; private set; } = false;
        public bool JustPressedX { get; private set; } = false;
        public bool JustPressedY { get; private set; } = false;

        // These goes true when the previous state was true, but current state is false. Same as JustPressed, these are reset in Update().
        public bool JustReleasedA { get; private set; } = false;
        public bool JustReleasedB { get; private set; } = false;
        public bool JustReleasedX { get; private set; } = false;
        public bool JustReleasedY { get; private set; } = false;

        /// <summary>
        /// Returns a new XBoxController object. 
        /// </summary>
        /// <param name="dwUserIndex"></param>
        /// <returns></returns>
        public static XBoxController? CreateController(int dwUserIndex)
        {
            XBoxController controller = new(dwUserIndex);

            // From the docs: Note that the return value of XInputGetState can be used to determine if the controller is connected.
            if (XInput.GetState(dwUserIndex, out controller.state))
            {
                controller.connected = true;
                controller.lastPacketNumber = controller.state.PacketNumber;
            }
            else
            {
                controller.connected = false;
            }

            return controller;
        }

        private XBoxController(int dwUserIndex)
        {
            this.dwUserIndex = dwUserIndex;
        }

        /// <summary>
        /// Call every as often to update the state of the controller.
        /// If returns true, a change of some kind has happened since last call to Update().
        /// </summary>
        public bool Update()
        {
            if(!connected) return false; // Call Reconnected() once it is reconnected.

            ResetJusts();

            if (!XInput.GetState(dwUserIndex, out state))
            {
                // Ouch, controller disconnected!
                Debug.WriteLine($"Controller {dwUserIndex} disconnected!");
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
        public void Reconnected()
        {
            connected = true;
            Debug.WriteLine($"Controller {dwUserIndex} reconnected!");
        }

        private void CompareLastGamepad()
        {
            if(lastGamepad.Buttons != state.Gamepad.Buttons)
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