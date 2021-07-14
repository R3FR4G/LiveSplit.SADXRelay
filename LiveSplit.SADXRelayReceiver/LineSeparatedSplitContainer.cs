using System.Drawing;
using System.Windows.Forms;

namespace LiveSplit.SADXRelayReceiver
{
    public class LineSeparatedSplitContainer : SplitContainer
    {
        private Pen _lineDrawPen;
        public LineSeparatedSplitContainer(Pen lineDrawPen) : base()
        {
            _lineDrawPen = lineDrawPen;
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            float x = ClientSize.Width / 2f;
            e.Graphics.DrawLine(_lineDrawPen, x, 0f, x, Height);
        }
    }
}