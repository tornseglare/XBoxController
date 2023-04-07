using Vortice.XInput;
using MASK;
using XBoxController_Tests;

namespace SomeNameSpace
{
    internal class Program
    {
        private static XBoxController? daController;

        static void Main()
        {
            Console.WriteLine("Hello, World! Press the controllers Start button to exit app.");
            Console.WriteLine(XInput.Version);

            // This line would allow us to add a derived class object. It must be done before calling StartPolling() or IterateControllers() to ensure the list xBoxControllers are empty.
            //XBoxControllerPoller.xBoxControllers.Add(0, new FancierXBoxController(0));

            XBoxControllerPoller.XBoxControllerEvent += XBoxControllerPoller_XBoxControllerEvent;
            XBoxControllerPoller.StartPolling();
            bool connected = false;

            try
            {
                while (true)
                {
                    if (daController != null)
                    {
                        // Just print something if a value has changed.
                        if (daController.Update())
                        {
                            if (daController.JustPressedA)
                            {
                                Console.WriteLine("You just pressed the A button!");
                            }

                            if (daController.RightThumb)
                            {
                                Console.WriteLine("You just pressed the right thumb button!");
                            }

                            // The right motor is the high-frequency motor, the left motor is the low-frequency motor. 
                            if (daController.RightTrigger > Gamepad.TriggerThreshold)
                            {
                                Console.WriteLine("Touching the right trigger! Value: " + daController.RightTrigger);

                                if (XInput.SetVibration(daController.UserIndex, 0.0f, daController.RightTrigger / 255.0f) == false)
                                {
                                    Console.WriteLine("Not vibrating!");
                                }
                            }
                            if (daController.LeftTrigger > Gamepad.TriggerThreshold)
                            {
                                Console.WriteLine("Touching the left trigger! Value: " + daController.LeftTrigger);

                                if (XInput.SetVibration(0, daController.LeftTrigger / 255.0f, 0.0f) == false)
                                {
                                    Console.WriteLine("Not vibrating!");
                                }
                            }

                            if (Math.Abs(daController.RightThumbX) > Gamepad.RightThumbDeadZone)
                            {
                                Console.WriteLine("Moving along the x-axis! Value: " + daController.RightThumbX);
                            }
                            if (Math.Abs(daController.RightThumbY) > Gamepad.RightThumbDeadZone)
                            {
                                Console.WriteLine("Moving along the y-axis! Value: " + daController.RightThumbY);
                            }

                            if (daController.Start)
                            {
                                Console.WriteLine("You just pressed start button! Terminating app..");

                                XBoxControllerPoller.StopPolling();
                                break;
                            }
                        }
                        else if(daController.Connected)
                        {
                            connected = true;
                        }
                        else if(connected)
                        {
                            // Somewhat complicated way to detect it was connected last time we checked, but are not anymore.
                            connected = false;
                            Console.WriteLine($"Controller {daController.UserIndex} disconnected!");
                        }
                    }

                    // You can always check for controllers this way instead of using the event EventXBoxControllerAdded, but is a bit cumbersome.
                    /*if (XBoxControllerPoller.xBoxControllers.Count > 0)
                    {
                        if (XBoxControllerPoller.xBoxControllers[0].Connected)
                        {
                            XBoxController controller = XBoxControllerPoller.xBoxControllers[0];
                        }
                    }*/

                    // Support for many controllers goes here. :) 
                    foreach(KeyValuePair<int,XBoxController> kvp in XBoxControllerPoller.xBoxControllers)
                    {
                        if(kvp.Value.Update())
                        {
                            // Things updated. 
                        }
                    }

                    // It's sleeping waaay more than a millisecond here. (According to spec it sleep minimum of 1 milliseconds, but probably/always more than 15)
                    Thread.Sleep(1);
                }
            }
            finally
            {
                // Attempt to bug in and stop the polling task when closing this console app. (Not working though, it seem necessary to setup a message pump and the whole circus, which I wont do for this simple example.)
                XBoxControllerPoller.StopPolling();
            }
        }

        /// <summary>
        /// Gets called when a controller is connected or reconnected.
        /// </summary>
        private static void XBoxControllerPoller_XBoxControllerEvent(XBoxController xBoxController, XBoxControllerEventState state)
        {
            // We just ignore any other controls being added. 
            if (xBoxController.UserIndex == 0)
            {
                if(state == XBoxControllerEventState.Connected)
                {
                    daController = xBoxController;
                    Console.WriteLine($"Controller {daController.UserIndex} connected!");
                }
                else if(state == XBoxControllerEventState.Reconnected)
                {
                    Console.WriteLine($"Controller {daController!.UserIndex} reconnected!");
                }
            }
        }
    }
}