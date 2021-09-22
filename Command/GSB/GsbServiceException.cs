using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Command.GSB
{
    class GsbServiceException : Exception
    {
        public string? Input { get; }
        public string Output { get; }

        public GsbServiceException(string message, string output, string? input = null, Exception? cause = null) : base(message, cause)
        {
            Input = input;
            Output = output;
        }
    }
}
