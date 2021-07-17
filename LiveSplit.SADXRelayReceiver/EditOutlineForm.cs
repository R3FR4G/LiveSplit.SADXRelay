using System;
using System.Drawing;
using System.Windows.Forms;

namespace LiveSplit.SADXRelayReceiver
{
    public partial class EditOutlineForm : Form
    {
        public (Color, int) ResultOutline;
        
        private const int ControlMargin = 10;
        private const int ControlHeight = 25;

        private (Color, int) _outline;
        public EditOutlineForm((Color, int) outline)
        {
            InitializeComponent();

            _outline = outline;

            Button changeColor = new Button()
            {
                Location = new Point(ControlMargin, ControlMargin),
                Size = new Size(ClientSize.Width - (ControlMargin * 2), ControlHeight),
                Text = "Change Color ...",
                ForeColor = outline.Item1
            };
            changeColor.Click += ChangeColorOnClick;
            NumericUpDown changeWidth = new NumericUpDown()
            {
                Location = new Point(ControlMargin, 45),
                Size = new Size(ClientSize.Width - (ControlMargin * 2), ControlHeight),
                Value = outline.Item2,
                DecimalPlaces = 0
            };
            changeWidth.ValueChanged += ChangeWidthOnValueChanged;

            Button confirm = new Button()
            {
                Location = new Point(ControlMargin, ClientSize.Height - ControlHeight - ControlMargin),
                Size = new Size(ClientSize.Width / 2 - (ControlMargin * 2), ControlHeight),
                Text = "OK"
            };
            confirm.Click += (sender, args) => ConfirmOnClick();
            Button cancel = new Button()
            {
                Location = new Point(confirm.Location.X + confirm.Size.Width + 20, confirm.Location.Y),
                Size = new Size(ClientSize.Width / 2 - (ControlMargin * 2), ControlHeight),
                Text = "Cancel"
            };
            cancel.Click += (sender, args) => CancelOnClick();

            AcceptButton = confirm;
            CancelButton = cancel;
            
            Controls.Add(changeColor);
            Controls.Add(changeWidth);
            
            Controls.Add(confirm);
            Controls.Add(cancel);
        }

        private void ChangeColorOnClick(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog()
            {
                Color = _outline.Item1
            };

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                _outline.Item1 = colorDialog.Color;
                ((Button)sender).ForeColor = colorDialog.Color;
            }
        }

        private void ChangeWidthOnValueChanged(object sender, EventArgs e)
        {
            _outline.Item2 = (int) ((NumericUpDown)sender).Value;
        }

        private void CancelOnClick()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ConfirmOnClick()
        {
            DialogResult = DialogResult.OK;
            ResultOutline = _outline;
            Close();
        }
    }
}