using System.Diagnostics;
using Vortice.XInput;

namespace XBoxController
{
    public static class XBoxControllerPoller
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
                    XBoxController xBoxController = XBoxController.CreateController(i);

                    if (!xBoxControllers.ContainsKey(i))
                    {
                        Console.WriteLine("We got contact! " + i);
                        Console.WriteLine("Hash code: " + state.Gamepad.GetHashCode());

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

            return newControllerConnected;
        }
    }
}
