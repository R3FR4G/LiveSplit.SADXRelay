using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveSplit.SADXRelayPacketLib;
using UserSettings = LiveSplit.SADXRelayReceiver.Properties.Settings;

namespace LiveSplit.SADXRelayReceiver
{
    public partial class ReceiverForm : Form
    {
        private static Font font;

        private static LineSeparatedSplitContainer grid = new LineSeparatedSplitContainer(new Pen(Brushes.Black, 10));

        private static (Color, int) leftOutline;
        private static (Color, int) rightOutline;

        public static OutlineLabel EuTime;
        public static OutlineLabel NaTime;

        public ReceiverForm()
        {
            InitializeComponent();

            InitSettings();

            MinimumSize = new Size(700, 200);
            
            grid.Size = ClientSize;
            grid.IsSplitterFixed = true;
            
            grid.Panel1.MinimumSize = new Size(ClientSize.Width / 2, ClientSize.Height);
            grid.Panel2.MaximumSize = new Size(ClientSize.Width / 2, ClientSize.Height);

            EuTime = new OutlineLabel()
            {
                Font = font,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                OutlineForeColor = leftOutline.Item1,
                OutlineWidth = leftOutline.Item2,
                Text = "00:00.00",
                TextAlign = ContentAlignment.MiddleCenter
            };

            NaTime = new OutlineLabel()
            {
                Font = font,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                OutlineForeColor = rightOutline.Item1,
                OutlineWidth = rightOutline.Item2,
                Text = "00:00.00",
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            EuTime.Location = new Point((grid.Panel1.Size.Width - EuTime.Size.Width) / 2, (grid.Panel1.Size.Height - EuTime.Size.Height) / 2);
            NaTime.Location = new Point((grid.Panel2.Size.Width - NaTime.Size.Width) / 2, (grid.Panel2.Size.Height - NaTime.Size.Height) / 2);

            grid.Panel1.Controls.Add(EuTime);
            grid.Panel2.Controls.Add(NaTime);
            
            ContextMenu ctxMenu = new ContextMenu();
            ctxMenu.MenuItems.Add("Change BackColor", OnClickBackColor);
            ctxMenu.MenuItems.Add("Change FontColor", OnClickFontColorChange);
            ctxMenu.MenuItems.Add("Change Font", OnClickFontChange);
            ctxMenu.MenuItems.Add("-");
            ctxMenu.MenuItems.Add("Quit", QuitHandler);
            ContextMenu = ctxMenu;

            Resize += OnResize;
            
            Controls.Add(grid);

            BackWorker.backWorker.WorkerReportsProgress = true;
            BackWorker.backWorker.WorkerSupportsCancellation = true;
            BackWorker.backWorker.DoWork += (sender, args) => BackWorker.DoWork();
            BackWorker.backWorker.ProgressChanged += BackWorkerOnProgressChanged;
            BackWorker.backWorker.RunWorkerAsync();
        }

        private void BackWorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Packet packet = (Packet)e.UserState;

            Console.WriteLine($"{packet.Type.ToString()}\n{packet.PlayerIndex}\n{packet.Time}");
            
            switch (packet.Type)
            {
                case PacketType.CurrentTimeToReceiver:
                {
                    if (packet.PlayerTeamIndex == 0)
                    {
                        EuTime.Text = packet.Time.ToString();
                        break;
                    }
                    NaTime.Text = packet.Time.ToString();
                    break;
                }
                case PacketType.RunUpdateToReceiver:
                {
                    break;
                }
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            grid.Size = ClientSize;
            grid.Panel1.MinimumSize = new Size(ClientSize.Width / 2, ClientSize.Height);
            grid.Panel2.MaximumSize = new Size(ClientSize.Width / 2, ClientSize.Height);
            EuTime.Location = new Point((grid.Panel1.Size.Width - EuTime.Size.Width) / 2, (grid.Panel1.Size.Height - EuTime.Size.Height) / 2);
            NaTime.Location = new Point((grid.Panel2.Size.Width - NaTime.Size.Width) / 2, (grid.Panel2.Size.Height - NaTime.Size.Height) / 2);
        }

        public void InitSettings()
        {
            BackColor = UserSettings.Default.BackColor;
            ForeColor = UserSettings.Default.FontColor;
            font = UserSettings.Default.Font;

            leftOutline = (UserSettings.Default.LeftOutlineColor, UserSettings.Default.LeftOutlineWidth);
            rightOutline = (UserSettings.Default.RightOutlineColor, UserSettings.Default.RightOutlineWidth);
        }
        
        private void OnClickBackColor(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog() { Color = BackColor };

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                BackColor = colorDialog.Color;
                UserSettings.Default.BackColor = colorDialog.Color;
                UserSettings.Default.Save();
            }
        }

        private void OnClickFontColorChange(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog() { Color = ForeColor };

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                ForeColor = colorDialog.Color;
                UserSettings.Default.FontColor = colorDialog.Color;
                UserSettings.Default.Save();
            }
        }
        
        private void OnClickFontChange(object sender, EventArgs e)
        {
            FontDialog fontDialog = new FontDialog() { Font = font };

            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                font = fontDialog.Font;
                UserSettings.Default.Font = fontDialog.Font;
                UserSettings.Default.Save();
                MessageBox.Show("Restart for the changes to take effect");
            }
        }
        
        private void QuitHandler(object sender, EventArgs e)
        {
            BackWorker.backWorker.CancelAsync();
            BackWorker.udpClient.Dispose();
            Exit();
        }

        public static void Exit()
        {
            Environment.Exit(0);
        }
    }
}