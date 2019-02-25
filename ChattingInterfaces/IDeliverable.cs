using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ChattingInterfaces
{
    public interface IDeliverable
    {
        bool SendOut(IChattingService Server);
    }
}
