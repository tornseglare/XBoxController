using MASK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBoxController_Tests
{
    public class FancierXBoxController : XBoxController
    {
        public int IHaveFancySpecifics = 42;

        public FancierXBoxController(int dwUserIndex) : base(dwUserIndex)
        {
        }
    }
}
