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
        
        public static string timeFormat = @"ss\.ff";
        public static string timeFormatWithMinutes = @"mm\:ss\.ff";
        public static string timeFormatWithHours = @"hh\:mm\:ss\.ff";
        
        public static TimeSpan EuTime = TimeSpan.Zero;
        public static TimeSpan NaTime = TimeSpan.Zero;

        public static TimeSpan[] CurrentTimes = new TimeSpan[] {EuTime, NaTime};
        
        public static TimeSpan EuFinalTime = TimeSpan.Zero;
        public static TimeSpan NaFinalTime = TimeSpan.Zero;
        
        public static TimeSpan[] FinalTimes = new TimeSpan[] {EuFinalTime, NaFinalTime};
        
        public static OutlineLabel EuTimeLabel;
        public static OutlineLabel NaTimeLabel;

        public static OutlineLabel[] TimeLabels;

        public static RelayStory CurrentEuStory = RelayStory.Sonic;
        public static RelayStory CurrentNaStory = RelayStory.Sonic;

        public static RelayStory[] CurrentStories = new RelayStory[] {CurrentEuStory, CurrentNaStory};

        public ReceiverForm()
        {
            InitializeComponent();

            InitSettings();

            MinimumSize = new Size(700, 200);
            
            grid.Size = ClientSize;
            grid.IsSplitterFixed = true;
            
            grid.Panel1.MinimumSize = new Size(ClientSize.Width / 2, ClientSize.Height);
            grid.Panel2.MaximumSize = new Size(ClientSize.Width / 2, ClientSize.Height);

            EuTimeLabel = new OutlineLabel()
            {
                Font = font,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                OutlineForeColor = leftOutline.Item1,
                OutlineWidth = leftOutline.Item2,
                Text = "00.00",
                TextAlign = ContentAlignment.MiddleCenter
            };

            NaTimeLabel = new OutlineLabel()
            {
                Font = font,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                OutlineForeColor = rightOutline.Item1,
                OutlineWidth = rightOutline.Item2,
                Text = "00.00",
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            EuTimeLabel.Location = new Point((grid.Panel1.Size.Width - EuTimeLabel.Size.Width) / 2, (grid.Panel1.Size.Height - EuTimeLabel.Size.Height) / 2);
            NaTimeLabel.Location = new Point((grid.Panel2.Size.Width - NaTimeLabel.Size.Width) / 2, (grid.Panel2.Size.Height - NaTimeLabel.Size.Height) / 2);

            grid.Panel1.Controls.Add(EuTimeLabel);
            grid.Panel2.Controls.Add(NaTimeLabel);

            TimeLabels = new OutlineLabel[] {EuTimeLabel, NaTimeLabel};
            
            ContextMenu ctxMenu = new ContextMenu();
            ctxMenu.MenuItems.Add("Change BackColor", OnClickBackColor);
            ctxMenu.MenuItems.Add("Change FontColor", OnClickFontColorChange);
            ctxMenu.MenuItems.Add("Change Font", OnClickFontChange);
            ctxMenu.MenuItems.Add("-");
            ctxMenu.MenuItems.Add("Edit Left Outline", EditLeftOutlineHandler);
            ctxMenu.MenuItems.Add("Edit Right Outline", EditRightOutlineHandler);
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

            Console.WriteLine($"{packet.Type.ToString()}\n{packet.PlayerTeamIndex.ToString()}\n{packet.PlayerIndex.ToString()}\n{packet.Time}");
            
            switch (packet.Type)
            {
                case PacketType.CurrentTimeToReceiver:
                {
                    byte teamIndex = (byte)packet.PlayerTeamIndex;
                    
                    if (packet.PlayerIndex != CurrentStories[teamIndex])
                        break;

                    CurrentTimes[teamIndex] = packet.Time;
                    string displayedTimeFormat = packet.Time.Hours != 0 ? timeFormatWithHours : packet.Time.Minutes != 0 ? timeFormatWithMinutes : timeFormat;

                    TimeSpan timeToDisplay = packet.Time + FinalTimes[teamIndex];
                    
                    TimeLabels[teamIndex].Text = timeToDisplay.ToString(displayedTimeFormat);
                    break;
                }
                case PacketType.RunUpdateToReceiver:
                {
                    byte teamIndex = (byte)packet.PlayerTeamIndex;

                    FinalTimes[teamIndex] = CurrentTimes[teamIndex];
                    
                    if (CurrentStories[teamIndex] == RelayStory.SuperSonic)
                    {
                        break;
                    }

                    CurrentStories[teamIndex]++;
                    break;
                }
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            grid.Size = ClientSize;
            grid.Panel1.MinimumSize = new Size(ClientSize.Width / 2, ClientSize.Height);
            grid.Panel2.MaximumSize = new Size(ClientSize.Width / 2, ClientSize.Height);
            EuTimeLabel.Location = new Point((grid.Panel1.Size.Width - EuTimeLabel.Size.Width) / 2, (grid.Panel1.Size.Height - EuTimeLabel.Size.Height) / 2);
            NaTimeLabel.Location = new Point((grid.Panel2.Size.Width - NaTimeLabel.Size.Width) / 2, (grid.Panel2.Size.Height - NaTimeLabel.Size.Height) / 2);
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
        
        private void EditLeftOutlineHandler(object sender, EventArgs e)
        {
            EditOutlineForm editForm = new EditOutlineForm(leftOutline);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                leftOutline = editForm.ResultOutline;
                EuTimeLabel.OutlineForeColor = editForm.ResultOutline.Item1;
                EuTimeLabel.OutlineWidth = editForm.ResultOutline.Item2;
                UserSettings.Default.LeftOutlineColor = editForm.ResultOutline.Item1;
                UserSettings.Default.LeftOutlineWidth = editForm.ResultOutline.Item2;
                UserSettings.Default.Save();
                EuTimeLabel.Invalidate();
            }
        }

        private void EditRightOutlineHandler(object sender, EventArgs e)
        {
            EditOutlineForm editForm = new EditOutlineForm(rightOutline);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                rightOutline = editForm.ResultOutline;
                NaTimeLabel.OutlineForeColor = editForm.ResultOutline.Item1;
                NaTimeLabel.OutlineWidth = editForm.ResultOutline.Item2;
                UserSettings.Default.RightOutlineColor = editForm.ResultOutline.Item1;
                UserSettings.Default.RightOutlineWidth = editForm.ResultOutline.Item2;
                UserSettings.Default.Save();
                NaTimeLabel.Invalidate();
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