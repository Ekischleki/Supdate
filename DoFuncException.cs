using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supdate
{
    internal class DoFuncException : Exception
    {
        public Action action;

        public DoFuncException(Action action)
        {
            this.action = action;
        }
    }
}
