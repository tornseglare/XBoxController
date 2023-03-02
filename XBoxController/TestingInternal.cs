using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBoxController
{
    public class TestingInternal
    {
        public void test()
        {
            XBoxController? xBoxController = XBoxController.CreateController(0);

            // Works. Same namespace.
            xBoxController?.Reconnected();
        }
    }
}

namespace AnotherNamespace
{
    public class TestingInternal
    {
        public void test()
        {
            XBoxController.XBoxController? xBoxController = XBoxController.XBoxController.CreateController(0);

            // Works. Same assembly. That's why 'internal' is pretty useless, I cannot protect a function so it can be used by just 'my' classes..
            // It WOULD work if I put the XBoxController class inside the XBoxControllerPoller (which is the only one in need to access Reconnected()), but that looks a bit messy to me.
            // I set it to public again and forget my valiant attempt.
            xBoxController?.Reconnected();
        }
    }
}
