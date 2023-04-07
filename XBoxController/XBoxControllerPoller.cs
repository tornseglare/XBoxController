using System.Diagnostics;
using Vortice.XInput;

namespace MASK
{
    public static class XBoxControllerPoller
    {
        // Keyed on the controls dwUserIndex, decided by windows when the control is connected, cable-wise.
        // "The number corresponds to the port that the controller is plugged into, and is not modifiable."
        // 
        public static readonly Dictionary<int, XBoxController> xBoxControllers = new();

        static CancellationTokenSource? tokenSource = null;

        /// <summary>
        /// Starts a task which will poll for new controllers periodically. 
        /// </summary>
        public static async void StartPolling()
        {
            if(tokenSource == null)
            {
                tokenSource = new();
            }

            CancellationToken ct = tokenSource.Token;

            try
            {
                Task theTask = new(() =>
                {
                    // This task loops until the ct got a cancellation request.
                    double lastTime = 0;
                    Stopwatch stopWatch = new();
                    stopWatch.Start();

                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            break;
                        }

                        // Poll for new controllers every two seconds, maybe three. 
                        if (stopWatch.Elapsed.TotalSeconds - lastTime >= 2)
                        {
                            lastTime = stopWatch.Elapsed.TotalSeconds;

                            if (XBoxControllerPoller.IterateControllers())
                            {
                                Debug.WriteLine("One or more controllers found and connected!");
                            }
                        }

                        Thread.Sleep(0);
                    }
                });

                // Start the task, then await it.
                theTask.Start();

                await theTask;
            }
            finally
            {
                tokenSource.Dispose();
                tokenSource = null;
            }
        }

        /// <summary>
        /// Cancels the polling task.
        /// </summary>
        public static void StopPolling()
        {
            tokenSource?.Cancel();
        }

        /// <summary>
        /// Polls for new controllers. 
        /// Also reconnect disconnected controllers once they return to life.
        /// For performance reasons, don't call IterateControllers() every frame. We recommend that you space out checks for new controllers every few seconds instead, or use the StartPolling() to setup a running task.
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
                        Console.WriteLine("Packet number: " + state.PacketNumber);
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
