using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChattingClient
{
    class DisplayableTextMessage 
    {
        public void Display()
        {
        }
    }
}


//public class DisplayableTextMessage implements IDisplayable;

//    void GetMessage(string contents, string sender)
//    {
//        IDisplayble text_message_displayable = new DisplayableTextMessage(contents, sender);
//        BlockingQueue.enqueue(text_message_displayable);
//    }

//    void GetUML(string uml_param1, string uml_param2, ...)
//    {
//        IDisplayble uml_displayable = new DisplayableUML(uml_param1, uml_param2);
//        uml_displayable.Display();

//    }