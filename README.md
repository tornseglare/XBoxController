# XBoxController

Using the Vortice.Windows nuget, see https://github.com/amerkoleci/Vortice.Windows for more examples.

For a working and more up-to-date demo, please see the XBoxController_Tests project here:
https://github.com/tornseglare/XBoxController/tree/main/XBoxController_Tests

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
            // A button just got pressed.
          }
		  
		  if(controller.JustReleasedA)
		  {
		    // A button just got released.
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
		
		// Regardless of if Update() returns true or false you can always check the current state. 
		// (Not with the JustPressed/ReleasedXYZs though, these changes only when Update() has detected a change and returns true, naturally.)
		if(controller.A)
		{
		  // The A button is pressed.
		}
      }
    }
    
    
    // Somewhere in your code during exiting the app you should terminate the polling task.
    XBoxControllerPoller.StopPolling();

# Use the connected/reconnected event

    using Vortice.XInput;
    using MASK;

    // Somewhere in your setup.
    XBoxControllerPoller.XBoxControllerEvent += XBoxControllerPoller_XBoxControllerEvent;
    XBoxControllerPoller.StartPolling();

    // Your event listener function.
    void XBoxControllerPoller_XBoxControllerEvent(XBoxController xBoxController, XBoxControllerEventState state)
    {
      if(state == XBoxControllerEventState.Connected)
      {
        Console.WriteLine($"Controller {xBoxController.UserIndex} connected!");
      }
      else if(state == XBoxControllerEventState.Reconnected)
      {
        Console.WriteLine($"Controller {xBoxController.UserIndex} reconnected!");
      }
	  else if(state == XBoxControllerEventState.Disconnected)
	  {
	    // Player/User has disconnected the controller. Urge them to reconnect it.
	    Console.WriteLine($"Controller {daController!.UserIndex} disconnected!");
	  }	  
    }