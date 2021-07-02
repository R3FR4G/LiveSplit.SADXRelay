using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.UI;

namespace LiveSplit.SADXRelayClient
{
    public class ClientSettings : UserControl
    {
        public TextBox IdTextBox;
        public NumericUpDown RefreshRateUpDown;
        public const int IdTextBoxX = 45;
        public const int RefreshRateUpDownX = 157;
        public const int RefreshRateUpDownY = 50;

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
                Text = "Enter your unique ID here",
                Location = new Point(IdTextBoxX, TopMargin - 3),
                Size = new Size(Size.Width - IdTextBoxX - LeftMargin, ControlHeight)
            };

            
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
            
            Controls.Add(IdTextBox);
            Controls.Add(idLabel);
            Controls.Add(RefreshRateUpDown);
            Controls.Add(refreshRateLabel);
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