using System.Diagnostics;
using Vortice.XInput;

namespace XBoxController
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Console.WriteLine(XInput.Version);

            double lastTime = 0;   
            Stopwatch stopWatch = new();
            stopWatch.Start();
            while (true)
            {
                if(XBoxControllerPoller.xBoxControllers.Count > 0)
                {
                    if (XBoxControllerPoller.xBoxControllers[0].Connected)
                    {
                        // Just print anything if a value has changed.
                        if (XBoxControllerPoller.xBoxControllers[0].Update())
                        {
                            if (XBoxControllerPoller.xBoxControllers[0].JustPressedA)
                            {
                                Console.WriteLine("You just pressed the A button!");
                            }

                            if (XBoxControllerPoller.xBoxControllers[0].RightThumb)
                            {
                                Console.WriteLine("You just pressed the right thumb button!");
                            }

                            if (XBoxControllerPoller.xBoxControllers[0].RightTrigger > Gamepad.TriggerThreshold)
                            {
                                Console.WriteLine("Touching the right trigger! Value: " + XBoxControllerPoller.xBoxControllers[0].RightTrigger);
                            }
                            if (Math.Abs(XBoxControllerPoller.xBoxControllers[0].RightThumbX) > Gamepad.RightThumbDeadZone)
                            {
                                Console.WriteLine("Moving along the x-axis! Value: " + XBoxControllerPoller.xBoxControllers[0].RightThumbX);
                            }
                            if (Math.Abs(XBoxControllerPoller.xBoxControllers[0].RightThumbY) > Gamepad.RightThumbDeadZone)
                            {
                                Console.WriteLine("Moving along the y-axis! Value: " + XBoxControllerPoller.xBoxControllers[0].RightThumbY);
                            }
                        }
                    }
                }

                if (stopWatch.Elapsed.TotalSeconds - lastTime >= 2)
                {
                    lastTime = stopWatch.Elapsed.TotalSeconds;

                    if(XBoxControllerPoller.IterateControllers())
                    {
                        Console.WriteLine("One or more controllers found and connected!");
                    }
                }

                // It's sleeping waaay more than a millisecond here. (According to spec it sleep minimum of 1 milliseconds, but probably/always more than 15)
                Thread.Sleep(1);
            }
        }
    }
}