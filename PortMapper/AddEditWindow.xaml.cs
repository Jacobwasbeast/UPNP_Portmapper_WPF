using System.Windows;
using System.Windows.Controls;
using Portmapper;

namespace PortMapper
{
    public partial class AddEditWindow : Window
    {
        public PortMapping PortMapping { get; private set; }

        public AddEditWindow()
        {
            InitializeComponent();
            PortMapping = new PortMapping();
            DataContext = PortMapping;
        }

        public AddEditWindow(PortMapping existingMapping) : this()
        {
            PortMapping = existingMapping;
            DataContext = PortMapping;

            NameTextBox.Text = PortMapping.Name;
            InternalPortTextBox.Text = PortMapping.InternalPort.ToString();
            ExternalPortTextBox.Text = PortMapping.ExternalPort.ToString();
            ProtocolComboBox.SelectedItem = ProtocolComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == PortMapping.Protocol);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            PortMapping.Name = NameTextBox.Text;
            PortMapping.InternalPort = int.Parse(InternalPortTextBox.Text);
            PortMapping.ExternalPort = int.Parse(ExternalPortTextBox.Text);
            PortMapping.Protocol = (ProtocolComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            PortMapping.IPAddress = MainWindow._ipAddress;

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}