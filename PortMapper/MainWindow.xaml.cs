using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using Portmapper;
using Mono.Nat;
namespace PortMapper
{
    public partial class MainWindow : Window
    {
        private Config _config;
        public static NatProtocol _natProtocol;
        public static string _ipAddress;
        public static INatDevice _device;

        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            // check to see if the app has admin rights
            if (!IsAdministrator())
            {
                MessageBox.Show("This application runs better with administrator rights. Please restart the application as an administrator if you experience issues");
            }
            PortMappingList.ItemsSource = _config.PortMappings;
            Trace.WriteLine("Starting PortMapper");
            NatUtility.DeviceFound += DeviceFound;
            NatUtility.StartDiscovery ();
            Trace.WriteLine("Discovery started");
        }
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        readonly SemaphoreSlim locker = new SemaphoreSlim (1, 1);
        private async void DeviceFound(object sender, DeviceEventArgs args)
        {
            await locker.WaitAsync ();
            try
            {
                INatDevice device = args.Device;
                Trace.WriteLine(("Device found: " + device.ToString()));
                _natProtocol = device.NatProtocol;
                Trace.WriteLine(("Type: " + device.GetType().Name));
                locker.Release ();
                _device = device;
                _ipAddress = device.GetExternalIP().ToString();
                ThreadPool.QueueUserWorkItem (new WaitCallback (UpdateMappings));
                Trace.WriteLine("---");
                foreach (var mapping in _device.GetAllMappingsAsync().Result)
                {
                    // check to see if mapping is in the config
                    bool found = false;
                    foreach (PortMapping portMapping in _config.PortMappings)
                    {
                        if (portMapping.InternalPort == mapping.PrivatePort && portMapping.ExternalPort == mapping.PublicPort)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        // add mapping to config
                        PortMapping portMapping = new PortMapping
                        {
                            Name = mapping.Description,
                            InternalPort = mapping.PrivatePort,
                            ExternalPort = mapping.PublicPort,
                            Protocol = mapping.Protocol == Protocol.Tcp ? "TCP" : "UDP",
                            IPAddress = _ipAddress
                        };
                        _config.PortMappings.Add(portMapping);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private void UpdateMappings(object state)
        {
            string status = "External IP: " + _ipAddress;
            foreach (PortMapping mapping in _config.PortMappings)
            {
                try
                {
                    Protocol protocol = mapping.Protocol == "TCP" ? Protocol.Tcp : Protocol.Udp;
                    applyChange(true, mapping);
                    status += $"\nAdded Port Mapping: {mapping.Name} - {mapping.Protocol} - {mapping.InternalPort} -> {mapping.ExternalPort}";
                }
                catch (Exception ex)
                {
                    status += $"\nFailed to add Port Mapping: {mapping.Name} - {mapping.Protocol} - {mapping.InternalPort} -> {mapping.ExternalPort}";
                    Trace.WriteLine(ex);
                }
            }
            Dispatcher.Invoke(() => StatusText.Text = status);
        }
        private void applyChange(bool shouldAdd, PortMapping mapping)
        {
            ThreadPool.QueueUserWorkItem (state =>
            {
                string status = shouldAdd ? "Adding" : "Removing";
                try
                {
                    Protocol protocol = mapping.Protocol == "TCP" ? Protocol.Tcp : Protocol.Udp;
                    if (shouldAdd)
                    {
                        _device.CreatePortMap(new Mapping(protocol, mapping.InternalPort, mapping.ExternalPort, 0, mapping.Name));
                    }
                    else
                    {
                        _device.DeletePortMap(new Mapping(protocol, mapping.InternalPort, mapping.ExternalPort, 0, mapping.Name));
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    status += $" Failed to {status} Port Mapping: {mapping.Name} - {mapping.Protocol} - {mapping.InternalPort} -> {mapping.ExternalPort}";
                }

                try
                {
                    bool isFound = false;
                    if (shouldAdd)
                    {
                        foreach (var mapping1 in _device.GetAllMappingsAsync().Result)
                        {
                            if (mapping1.PrivatePort == mapping.InternalPort && mapping1.PublicPort == mapping.ExternalPort)
                            {
                                isFound = true;
                                break;
                            }
                        }
                        if (!isFound)
                        {
                            status += $" Failed to {status} Port Mapping: {mapping.Name} - {mapping.Protocol} - {mapping.InternalPort} -> {mapping.ExternalPort}";
                        }
                        // try to add again
                        if (!isFound)
                        {
                            Protocol protocol = mapping.Protocol == "TCP" ? Protocol.Tcp : Protocol.Udp;
                            _device.CreatePortMap(new Mapping(protocol, mapping.InternalPort, mapping.ExternalPort));
                            status += $"\nAdded Port Mapping: {mapping.Name} - {mapping.Protocol} - {mapping.InternalPort} -> {mapping.ExternalPort}";
                        }
                    }
                    else
                    {
                        foreach (var mapping1 in _device.GetAllMappingsAsync().Result)
                        {
                            if (mapping1.PrivatePort == mapping.InternalPort && mapping1.PublicPort == mapping.ExternalPort)
                            {
                                isFound = false;
                                break;
                            }
                        }
                        if (isFound)
                        {
                            status += $" Failed to {status} Port Mapping: {mapping.Name} - {mapping.Protocol} - {mapping.InternalPort} -> {mapping.ExternalPort}";
                        }
                        // try to remove again
                        if (isFound)
                        {
                            Protocol protocol = mapping.Protocol == "TCP" ? Protocol.Tcp : Protocol.Udp;
                            _device.DeletePortMapAsync(new Mapping(protocol, mapping.InternalPort, mapping.ExternalPort));
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    status += $" Failed to {status} Port Mapping: {mapping.Name} - {mapping.Protocol} - {mapping.InternalPort} -> {mapping.ExternalPort}";
                }

                Trace.WriteLine(status);
                Dispatcher.Invoke(() => StatusText.Text = status);
            });
        }
        

        private void LoadConfig()
        {
            _config = ConfigManager.LoadConfig();
            StatusText.Text = $"Loaded {_config.PortMappings.Count} Port Mappings for IP: {_ipAddress}";
        }

        private void SaveConfig()
        {
            ConfigManager.SaveConfig(_config);
            StatusText.Text = "Configuration Saved";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditWindow();
            if (addWindow.ShowDialog() == true)
            {
                _config.PortMappings.Add(addWindow.PortMapping);
                applyChange(true, addWindow.PortMapping);
                SaveConfig();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (PortMappingList.SelectedItem is PortMapping selectedMapping)
            {
                var editWindow = new AddEditWindow(selectedMapping);
                if (editWindow.ShowDialog() == true)
                {
                    int index = _config.PortMappings.IndexOf(selectedMapping);
                    _config.PortMappings[index] = editWindow.PortMapping;
                    SaveConfig();
                    applyChange(false, selectedMapping);
                    applyChange(true, editWindow.PortMapping);
                }
            }
            else
            {
                MessageBox.Show("Please select a port mapping to edit.");
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (PortMappingList.SelectedItem is PortMapping selectedMapping)
            {
                _config.PortMappings.Remove(selectedMapping);
                SaveConfig();
                applyChange(false, selectedMapping);
            }
            else
            {
                MessageBox.Show("Please select a port mapping to remove.");
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            if (settingsWindow.ShowDialog() == true)
            {
                SaveConfig();
            }
        }
    }
}
