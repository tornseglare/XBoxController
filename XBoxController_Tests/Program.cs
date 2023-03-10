using Vortice.XInput;
using MASK;

namespace SomeNameSpace
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine("Hello, World! Press the controllers Start button to exit app.");
            Console.WriteLine(XInput.Version);

            XBoxControllerPoller.StartPolling();

            try
            {
                while (true)
                {
                    if (XBoxControllerPoller.xBoxControllers.Count > 0)
                    {
                        if (XBoxControllerPoller.xBoxControllers[0].Connected)
                        {
                            XBoxController controller = XBoxControllerPoller.xBoxControllers[0];

                            // Just print anything if a value has changed.
                            if (controller.Update())
                            {
                                if (controller.JustPressedA)
                                {
                                    Console.WriteLine("You just pressed the A button!");
                                }

                                if (controller.RightThumb)
                                {
                                    Console.WriteLine("You just pressed the right thumb button!");
                                }

                                // The right motor is the high-frequency motor, the left motor is the low-frequency motor. 
                                if (controller.RightTrigger > Gamepad.TriggerThreshold)
                                {
                                    Console.WriteLine("Touching the right trigger! Value: " + controller.RightTrigger);

                                    if (XInput.SetVibration(controller.UserIndex, 0.0f, controller.RightTrigger / 255.0f) == false)
                                    {
                                        Console.WriteLine("Not vibrating!");
                                    }
                                }
                                if (controller.LeftTrigger > Gamepad.TriggerThreshold)
                                {
                                    Console.WriteLine("Touching the left trigger! Value: " + controller.LeftTrigger);

                                    if (XInput.SetVibration(0, controller.LeftTrigger / 255.0f, 0.0f) == false)
                                    {
                                        Console.WriteLine("Not vibrating!");
                                    }
                                }

                                if (Math.Abs(controller.RightThumbX) > Gamepad.RightThumbDeadZone)
                                {
                                    Console.WriteLine("Moving along the x-axis! Value: " + controller.RightThumbX);
                                }
                                if (Math.Abs(controller.RightThumbY) > Gamepad.RightThumbDeadZone)
                                {
                                    Console.WriteLine("Moving along the y-axis! Value: " + controller.RightThumbY);
                                }

                                if (controller.Start)
                                {
                                    Console.WriteLine("You just pressed start button! Terminating app..");

                                    XBoxControllerPoller.StopPolling();
                                    break;
                                }
                            }
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
    }
}