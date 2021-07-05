using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.SADXRelayPacketLib;
using LiveSplit.UI;

namespace LiveSplit.SADXRelayClient
{
    public class ClientSettings : UserControl
    {
        public TextBox IdTextBox;
        public NumericUpDown RefreshRateUpDown;
        public Button ConnectButton;

        public const string ConnectButtonDefaultText = "Verify ID and connect to the server";
        
        public const int IdTextBoxX = 45;
        public const int RefreshRateUpDownX = 157;
        public const int RefreshRateUpDownY = 50;
        public const int ConnectButtonY = 80;

        public const int TopMargin = 20;
        public const int LeftMargin = 20;
        public const int ControlHeight = 20;

        public ClientSettings()
        {
            Size = new Size(476, 512);
            
            Label idLabel = new Label()
            {
                Text = "ID:",
                AutoSize = true,
                Location = new Point(LeftMargin, TopMargin)
            };

            IdTextBox = new TextBox()
            {
                Location = new Point(IdTextBoxX, TopMargin - 3),
                Size = new Size(Size.Width - IdTextBoxX - LeftMargin, ControlHeight),
                MaxLength = 4,
                CharacterCasing = CharacterCasing.Upper
            };
            IdTextBox.TextChanged += IdTextBoxOnTextChanged;
            
            Label refreshRateLabel = new Label()
            {
                Text = "Updates sent per second:",
                AutoSize = true,
                Location = new Point(LeftMargin, RefreshRateUpDownY)
            };

            RefreshRateUpDown = new NumericUpDown()
            {
                DecimalPlaces = 0,
                Location = new Point(RefreshRateUpDownX, RefreshRateUpDownY - 3), 
                Size = new Size(Size.Width - RefreshRateUpDownX - LeftMargin, ControlHeight),
                Value = (decimal)(1000 / ClientComponent.updateTimer.Interval)
            };
            RefreshRateUpDown.ValueChanged += RefreshRateUpDownOnValueChanged;

            ConnectButton = new Button()
            {
                Location = new Point(LeftMargin, ConnectButtonY),
                Size = new Size(Size.Width - (LeftMargin * 2), ControlHeight),
                Text = ConnectButtonDefaultText,
                Enabled = !ClientComponent.IsAuthenticated
            };
            ConnectButton.Click += ConnectButtonOnClick;

            Controls.Add(IdTextBox);
            Controls.Add(idLabel);
            Controls.Add(RefreshRateUpDown);
            Controls.Add(refreshRateLabel);
            Controls.Add(ConnectButton);
        }

        private void RefreshRateUpDownOnValueChanged(object sender, EventArgs e)
        {
            ClientComponent.updateTimer.Interval = 1000.0 / (double)((NumericUpDown)sender).Value;
        }

        private void IdTextBoxOnTextChanged(object sender, EventArgs e)
        {
            ClientComponent.IsAuthenticated = false;
            ConnectButton.Enabled = true;
        }

        private void ConnectButtonOnClick(object sender, EventArgs e)
        {
            Button caller = (Button)sender;
            caller.Enabled = false;

            if (IdTextBox.Text.Length != IdTextBox.MaxLength)
            {
                Task.Factory.StartNew(() => MessageBox.Show("The ID must be 4 characters long"));
                return;
            }

            int retryCount = 1;
            ResponseCode response = ResponseCode.Error; //build error if no default code 
            
            while (retryCount < 4)
            {
                caller.Text = "Verifying ID... (Attempt n°" + retryCount + ")";
                response = ClientComponent.VerifyId(IdTextBox.Text);
                if (response != ResponseCode.Error)
                    break;
                retryCount++;
            }
            
            if (response == ResponseCode.Ok)
            {
                ClientComponent.IsAuthenticated = true;
                Task.Factory.StartNew(() => MessageBox.Show("Authenticated to the server CatBoo"));
            }
            if (response == ResponseCode.BadId)
            {
                Task.Factory.StartNew(() => MessageBox.Show("Wrong ID"));
                ConnectButton.Enabled = true;
            }
            if (response == ResponseCode.Error)
            {
                Task.Factory.StartNew(() => MessageBox.Show("Could not connect to the server"));
                ConnectButton.Enabled = true;
            }
                
            caller.Text = ConnectButtonDefaultText;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            XmlElement settingsNode = document.CreateElement("Settings");

            settingsNode.AppendChild(SettingsHelper.ToElement(document, "ID", IdTextBox.Text));
            settingsNode.AppendChild(SettingsHelper.ToElement(document, "RefreshRate", RefreshRateUpDown.Value));
            
            return settingsNode;
        }
        
        public void SetSettings(XmlNode settings)
        {
            XmlElement element = (XmlElement)settings;
            if (string.IsNullOrEmpty(element["ID"].InnerText) || string.IsNullOrEmpty(element["RefreshRate"].InnerText))
                return;
            
            IdTextBox.Text = element["ID"].InnerText;
            RefreshRateUpDown.Value = decimal.Parse(element["RefreshRate"].InnerText);

            ClientComponent.updateTimer.Interval = 1000.0 / (double)RefreshRateUpDown.Value;
        }
    }
}