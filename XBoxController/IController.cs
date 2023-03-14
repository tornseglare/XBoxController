
namespace MASK
{
    /// <summary>
    /// If you intend to write for example a keyboard 'fake' xbox controller, this interface might help you accomplish this.
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// Returns true if the controller is connected.
        /// </summary>

        bool Connected { get; }

        /// <summary>
        /// A B X Y are immediate states, right-now-states.
        /// </summary>
        bool A { get; }
        bool B { get; }
        bool X { get; }
        bool Y { get; }

        bool DPadDown { get; }
        bool DPadUp { get; }
        bool DPadLeft { get; }
        bool DPadRight { get; }
        bool Start { get; }
        bool Back { get; }
        bool Guide { get; }
        bool LeftShoulder { get; }
        bool RightShoulder { get; }
        bool LeftThumb { get; }
        bool RightThumb { get; }

        /// <summary>
        /// The trigger buttons on top of the controller have a pressed-value between 0 and 255.
        /// </summary>
        byte RightTrigger { get; }
        byte LeftTrigger { get; }

        /// <summary>
        /// Each of the thumbstick axis members is a signed value between -32768 and 32767 describing the position of the thumbstick.
        /// Casting to int to avoid the silly Math.Abs(short.MinValue) crash. (-32768 cannot be abs'ed to 32768 since it is an overflow)
        /// </summary>
        int RightThumbX { get; }
        int RightThumbY { get; }
        int LeftThumbX { get; }
        int LeftThumbY { get; }

        /// <summary>
        /// These goes true when the previous state was false, but current state is true. 
        /// NOTE: These are reset to false in every call to Update(), so make sure you check these every loop if you are using them!
        /// </summary>
        bool JustPressedA { get; }
        bool JustPressedB { get; }
        bool JustPressedX { get; }
        bool JustPressedY { get; }

        /// <summary>
        /// These goes true when the previous state was true, but current state is false. Same as JustPressed, these are reset in Update().
        /// </summary>
        bool JustReleasedA { get; }
        bool JustReleasedB { get; }
        bool JustReleasedX { get; }
        bool JustReleasedY { get; }

        /// <summary>
        /// Call every as often to update the state of the controller.
        /// If returns true, a change of some kind has happened since last call to Update().
        /// </summary>
        bool Update();
    }
}
