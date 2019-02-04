using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Windows.Devices.Bluetooth.Advertisement;

namespace TestingBluetoothNativeWin10
{
    public class MainWindowViewModel : BaseViewModel
    {
        public ObservableCollection<DeviceViewModel> Devices { get; } = new ObservableCollection<DeviceViewModel>();
        private readonly BluetoothLEAdvertisementWatcher _watcher;
        private object _locker = new object();

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
            if (string.IsNullOrEmpty(eventArgs.Advertisement.LocalName)) return;

            var timestamp = eventArgs.Timestamp;
            var advertisementType = eventArgs.AdvertisementType;
            var rssi = eventArgs.RawSignalStrengthInDBm;
            var localName = eventArgs.Advertisement.LocalName;

            Debug.WriteLine($"[{DateTime.Now}] [{timestamp}] [{localName}] [{rssi}] [{advertisementType}]");

            lock(_locker)
            {
                var foundDevice = Devices.FirstOrDefault(d => d.BluetoothAddress == eventArgs.BluetoothAddress.ToString());
                if (foundDevice != null)
                {
                    foundDevice.LastSeen = timestamp.ToString();
                    return;
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (_locker)
                {
                    var device = new DeviceViewModel(eventArgs.Advertisement.LocalName, eventArgs.BluetoothAddress, timestamp);
                    Devices.Add(device);
                }
            });
        }
    }

    public class DeviceViewModel : BaseViewModel
    {
        public DeviceViewModel(string name, ulong addr, DateTimeOffset lastSeen)
        {
            Name = name;
            BluetoothAddress = addr.ToString();
            LastSeen = lastSeen.ToString();
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => AssignProperty(ref _name, value);
        }

        private string _bluetoothAddress;
        public string BluetoothAddress
        {
            get => _bluetoothAddress;
            set => AssignProperty(ref _bluetoothAddress, value);
        }

        private string _lastSeen;
        public string LastSeen
        {
            get => _lastSeen;
            set => AssignProperty(ref _lastSeen, value);
        }
    }
}