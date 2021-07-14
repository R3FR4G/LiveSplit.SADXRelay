using System;
using System.Windows.Forms;

namespace LiveSplit.SADXRelayReceiver
{
    class Receiver
    {
        public static ReceiverForm receiverForm;
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            receiverForm = new ReceiverForm();
            Application.Run(receiverForm);
        }
    }
}