using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Src
{
    public class GolemUIException : Exception
    {
        public string? ContextDetail { get; protected set; }

        public GolemUIException(string message) : base(message)
        { }

        public GolemUIException(string message, string contextDetail) : base(message)
        {
            this.ContextDetail = contextDetail;
        }
    }
}
