using System.Diagnostics;
using Vortice.XInput;

namespace MASK
{
    /// <summary>
    /// When a xbox controller event occurs.
    /// </summary>
    public enum XBoxControllerEventState
    {
        /// <summary>
        /// A controller has been connected to the pc.
        /// </summary>
        Connected,

        /// <summary>
        /// The controller has reconnected.
        /// </summary>
        Reconnected,

        /// <summary>
        /// The controller has been disconnected.
        /// </summary>
        Disconnected
    }

    /// <summary>
    /// The delegate for the event that a new xbox controller has been connected/reconnected to the pc.
    /// </summary>
    /// <param name="xBoxController">The controller's id</param>
    /// <param name="state">Type of event</param>
    public delegate void NotifyXBoxControllerStateChanged(XBoxController xBoxController, XBoxControllerEventState state);

    /// <summary>
    /// Poll for new controllers and reconnecting controllers.
    /// Use StartPolling() when your app starts and StopPolling() during app shut down.
    /// </summary>
    public static class XBoxControllerPoller
    {
        /// <summary>
        /// Keyed on the controls dwUserIndex, decided by windows when the control is connected, cable-wise.
        /// "The number corresponds to the port that the controller is plugged into, and is not modifiable."
        /// </summary>
        public static readonly Dictionary<int, XBoxController> xBoxControllers = new();

        /// <summary>
        /// This event fires when a controller has been connected/reconnected to the computer.
        /// Add your callback function to this event. 
        /// </summary>
        /// <example>XBoxControllerPoller.NotifyXBoxControllerStateChanged += YourEventConsumerFunction;</example>
        public static event NotifyXBoxControllerStateChanged? XBoxControllerEvent;

        /// <summary>
        /// The communication with the poll-thread happens trough this token.
        /// </summary>
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
                if (!xBoxControllers.ContainsKey(i))
                {
                    bool res = XInput.GetState(i, out _);

                    if (res)
                    {
                        // Add a new, disconnected controller object.
                        xBoxControllers.Add(i, new XBoxController(i));
                    }
                }

                if (xBoxControllers.ContainsKey(i) && xBoxControllers[i].Connected == false)
                {
                    bool everConnected = xBoxControllers[i].EverConnected;

                    // Controller just got added or was added earlier. We must check if it is connected and inform event consumers.
                    if (xBoxControllers[i].UpdateConnectedState())
                    {
                        if (everConnected == false)
                        {
                            Debug.WriteLine("We got contact! " + i);

                            newControllerConnected = true;

                            // If we have any event consumers, lets inform them we have a new controller connected.
                            XBoxControllerEvent?.Invoke(xBoxControllers[i], XBoxControllerEventState.Connected);
                        }
                        else
                        {
                            Debug.WriteLine("Reconnected! " + i);

                            // If we have any event consumers, lets inform them the controller has reconnected.
                            XBoxControllerEvent?.Invoke(xBoxControllers[i], XBoxControllerEventState.Reconnected);
                        }
                    }
                }
            }

            return newControllerConnected;
        }

        /// <summary>
        /// On XBoxController.Update() it detected the controller has been disconnected and so call this to invoke any listeners.
        /// </summary>
        internal static void ControllerDisconnected(int userIndex)
        {
            if (xBoxControllers.ContainsKey(userIndex))
            {
                XBoxControllerEvent?.Invoke(xBoxControllers[userIndex], XBoxControllerEventState.Disconnected);
            }
        }
    }
}
