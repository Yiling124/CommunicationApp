using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChattingServer
{
    public class TextMessage
    {
        public string TextMessageContent { get; set; }
        public string TextsenderName { get; set; }
        public Tuple<string, int> TextSessionOwnerAds { get; set; }
        public Tuple<string, int> TextReceiverAds { get; set; }

        public TextMessage(string textConent, string sderName, Tuple<string, int> receiverIp, Tuple<string, int> ownerIp)
        {
            this.TextMessageContent = textConent;
            this.TextsenderName = sderName;
            this.TextReceiverAds = receiverIp;
            this.TextSessionOwnerAds = ownerIp;
        }
    }


}
