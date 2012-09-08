
using System.Text;
using System;
namespace stekelvarken.xmpproxy.client
{
    class Program
    {
        static string ConvertMsgToString(myList<byte> msgs)
        {
            return Encoding.UTF8.GetString(msgs.ToArray(), 0, msgs.Count);
        }

        static void Main(string[] args)
        {
            myList<byte> msgs;
            string strmsg;
            Connection cc = new Connection();
            cc.Connect();
            Connection sc = new Connection("chat.facebook.com");
            sc.Connect();
            while (true)
            {
                strmsg = "";
                msgs = cc.GetMessages();
                if ((msgs != null) && (msgs.Count > 0))
                {
                    strmsg = ConvertMsgToString(msgs);
                    sc.Send(msgs);
                }
                msgs = sc.GetMessages();
                if ((msgs != null) && (msgs.Count > 0))
                {
                    strmsg = ConvertMsgToString(msgs);
                    cc.Send(msgs);
                }
                if (strmsg.Length > 0)
                {
                    Console.WriteLine(strmsg);
                }
                if (strmsg.Contains("proceed xmlns=\"urn:ietf:params:xml:ns:xmpp-tls\""))
                {
                    cc.StartTls();
                    sc.StartTls();
                }
            }
        }
    }
}
