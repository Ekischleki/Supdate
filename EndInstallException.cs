using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supdate
{
    internal class EndInstallException : Exception
    {
        public EndInstallException(string message, Updater.UpdateEndCode updateEndCode) : base(message)
        {
            UpdateEndCode = updateEndCode;
        }

        public Updater.UpdateEndCode UpdateEndCode { get; set; }

    }
}
