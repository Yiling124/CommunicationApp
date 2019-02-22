using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChattingClient
{
    public interface IDeliverable
    {
        bool SendOut(IChattingService Server);
    }
}
