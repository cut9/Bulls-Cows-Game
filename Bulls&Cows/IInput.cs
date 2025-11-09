using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulls_Cows
{
    internal interface IInput
    {
        public GameCommand GetUserChar(out char ch);
        public GameCommand GetCommand();
        public void WaitUserInput();
        public string GetUserString();
    }
}
