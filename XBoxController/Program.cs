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

            IterateControllers();

            Console.WriteLine(XInput.Version);

            /*while(true)
            {
                Thread.Sleep(1);
            }*/
        }

        static void IterateControllers()
        {
            // Egentligen 0-3, men kunde inte låta bli att testa. Inget kraschar. :)
            for(int i=0;i<7;i++)
            {
                State state;
                bool res = XInput.GetState(i, out state);

                if(res)
                {
                    // Har testat med en inkopplad kontroll, testa med två!
                    Debug.WriteLine("We got contact! " +  i);
                    Debug.WriteLine(state.Gamepad.GetHashCode());
                }
            }
        }
    }
}