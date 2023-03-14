# XBoxController

Using the Vortice.Windows nuget, see https://github.com/amerkoleci/Vortice.Windows for more examples.

# How to use

    using Vortice.XInput;
    using MASK;
    
    // Starts a task which periodically look for newly connected controllers.
    XBoxControllerPoller.StartPolling();
    
    // Somewhere in your game/app:
    if (XBoxControllerPoller.xBoxControllers.Count > 0)
    {
      // Once a controller has been detected and added by the XBoxControllerPoller, it will always remain. 
      XBoxController controller = XBoxControllerPoller.xBoxControllers[0];
      
      if (controller.Connected)
      {
        if (controller.Update())
        {
          if (controller.JustPressedA)
          {
            // A button pressed.
          }
          
          if (controller.RightTrigger > Gamepad.TriggerThreshold)
          {
            // Using the Vortice.XInput functionality here.
            if (XInput.SetVibration(controller.UserIndex, 0.0f, controller.RightTrigger / 255.0f) == false)
            {
              Console.WriteLine("Device not connected!");
            }
          }
        }
      }
      else
      {
        // Player/User has disconnected the controller. Urge them to reconnect it.
      }
    }
    
    
    // Somewhere in your code during exiting the app you should terminate the polling task.
    XBoxControllerPoller.StopPolling();

For a working demo, please see the XBoxController_Tests project here:
https://github.com/tornseglare/XBoxController/tree/main/XBoxController_Tests

