using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;

namespace TestingBluetoothNativeWin10
{
    public class MainWindowViewModel : BaseViewModel
    {
        public ObservableCollection<DeviceViewModel> Devices { get; } = new ObservableCollection<DeviceViewModel>();
        private readonly BluetoothLEAdvertisementWatcher _watcher;
       

        public MainWindowViewModel()
        {
            _watcher = new BluetoothLEAdvertisementWatcher {ScanningMode = BluetoothLEScanningMode.Active};
            _watcher.Received += OnAdvertisementReceived;
            _watcher.Stopped += OnAdvertisementWatcherStopped;

            _watcher.Start();
        }

        private void OnAdvertisementWatcherStopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
        {
            Debug.WriteLine($"{DateTime.Now} [{sender.Status}] [{sender.GetHashCode()}] stopped called");
        }

        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            //System.Diagnostics.Debug.WriteLine($"Got OnAdvertisementReceived");
            // We can obtain various information about the advertisement we just received by accessing 
            // the properties of the EventArgs class
            if (string.IsNullOrEmpty(eventArgs.Advertisement.LocalName)) return;
            // The timestamp of the event
            DateTimeOffset timestamp = eventArgs.Timestamp;

            // The type of advertisement
            BluetoothLEAdvertisementType advertisementType = eventArgs.AdvertisementType;

            // The received signal strength indicator (RSSI)
            Int16 rssi = eventArgs.RawSignalStrengthInDBm;

            // The local name of the advertising device contained within the payload, if any
            string localName = eventArgs.Advertisement.LocalName;

            // Check if there are any manufacturer-specific sections.
            // If there is, print the raw data of the first manufacturer section (if there are multiple).
            //string manufacturerDataString = "";
            //var manufacturerSections = eventArgs.Advertisement.ManufacturerData;
            //if (manufacturerSections.Count > 0)
            //{
            //    // Only print the first one of the list
            //    var manufacturerData = manufacturerSections[0];
            //    var data = new byte[manufacturerData.Data.Length];
            //    using (var reader = DataReader.FromBuffer(manufacturerData.Data))
            //    {
            //        reader.ReadBytes(data);
            //    }
            //    // Print the company ID + the raw data in hex format
            //    manufacturerDataString = string.Format("0x{0}: {1}",
            //        manufacturerData.CompanyId.ToString("X"),
            //        BitConverter.ToString(data));
            //}

            Debug.WriteLine($"[{DateTime.Now}] [{timestamp}] [{localName}] [{rssi}] [{advertisementType}]");

            if (Devices.Any(d => d.BluetoothAddress == eventArgs.BluetoothAddress))
                return;

            var device = new DeviceViewModel(eventArgs.Advertisement.LocalName, eventArgs.BluetoothAddress);
            Application.Current.Dispatcher.Invoke(() =>
            {
                Devices.Add(device);
            });
        }
    }

    public class DeviceViewModel : BaseViewModel
    {
        public DeviceViewModel(string name, ulong addr, string id = "")
        {
            Name = name;
            Id = id;
            BluetoothAddress = addr;
            Status = BluetoothConnectionStatus.Disconnected.ToString();
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => AssignProperty(ref _name, value);
        }

        private string _id;
        public string Id
        {
            get => _id;
            set => AssignProperty(ref _id, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => AssignProperty(ref _status, value);
        }

        private ulong _bluetoothAddress;
        public ulong BluetoothAddress
        {
            get => _bluetoothAddress;
            set => AssignProperty(ref _bluetoothAddress, value);
        }

        public string BtAddressDisplay => BluetoothAddress.ToString();
    }
}