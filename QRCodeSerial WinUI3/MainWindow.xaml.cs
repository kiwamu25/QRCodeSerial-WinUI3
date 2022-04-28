using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QRCodeSerial_WinUI3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        MainPageViewMode ViewModel = new MainPageViewMode();
        DataReader dr;
        public MainWindow()
        {
            this.InitializeComponent();
            GetPorts();

        }

        private async void GetPorts()
        {
            ViewModel.Reload = Visibility.Visible;
            if (ViewModel.SerialDevices != null)
            {
                foreach (var device in ViewModel.SerialDevices)
                {
                    device.device.Dispose();
                }
                ViewModel.SerialDevices.Clear();
            }

            DeviceInformationCollection serialDeviceInfos = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector());

            foreach (DeviceInformation serialDeviceInfo in serialDeviceInfos)
            {
                try
                {
                    var com = await SerialDevice.FromIdAsync(serialDeviceInfo.Id);
                    if (com != null)
                    {
                        var item = new SerialDeviceItem();
                        item.PortName = com.PortName;
                        item.DeviceName = serialDeviceInfo.Name;
                        item.device = com;
                        ViewModel.SerialDevices.Add(item);
                    }
                }
                catch (Exception)
                {
                    // Couldn't instantiate the device
                }

            }
            ViewModel.Reload = Visibility.Collapsed;
        }


        public string ReadBuffer = "";
        private async void ReadStanby()
        {
            try
            {
                uint bytesRead = await dr.LoadAsync(1);
                var str = dr.ReadString(bytesRead);
                switch (str)
                {
                    case "\n":
                        break;
                    case "\r":
                        ViewModel.ReadCode = ReadBuffer;
                        ReadBuffer = string.Empty;
                        break;
                    default:
                        ReadBuffer += str;
                        break;
                }
                ReadStanby();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                dr.Dispose();
                ViewModel.IsSerialOpen = false;
            }
        }

        private async void OpenSerial()
        {
            DeviceInformationCollection serialDeviceInfos = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector());

            foreach (DeviceInformation serialDeviceInfo in serialDeviceInfos)
            {
                try
                {
                    if (ViewModel.SelectedDevice == null)
                    {
                        return;
                    }
                    ViewModel.IsSerialOpen = true;
                    dr = new DataReader(ViewModel.SelectedDevice.device.InputStream);
                    ReadStanby();
                    return;
                }
                catch (Exception)
                {
                    // Couldn't instantiate the device
                }

            }
            ViewModel.IsSerialOpen = false;
        }

        private void Button_Reload_Click(object sender, RoutedEventArgs e)
        {
            GetPorts();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OpenSerial();
        }
    }

    public class MainPageViewMode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly PropertyChangedEventArgs SerialDevicesPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(SerialDevices));
        private ObservableCollection<SerialDeviceItem> serialDevices = new ObservableCollection<SerialDeviceItem>();
        public ObservableCollection<SerialDeviceItem> SerialDevices
        {
            get { return this.serialDevices; }
            set
            {
                if (this.serialDevices == value) { return; }
                this.serialDevices = value;
                this.PropertyChanged?.Invoke(this, SerialDevicesPropertyChangedEventArgs);
            }
        }

        private static readonly PropertyChangedEventArgs SelectedDevicePropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(SelectedDevice));
        private SerialDeviceItem selectedDevice;
        public SerialDeviceItem SelectedDevice
        {
            get { return this.selectedDevice; }
            set
            {
                if (this.selectedDevice == value) { return; }
                this.selectedDevice = value;
                this.PropertyChanged?.Invoke(this, SelectedDevicePropertyChangedEventArgs);
            }
        }

        private static readonly PropertyChangedEventArgs ReloadPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(Reload));
        private Visibility reload = Visibility.Visible;
        public Visibility Reload
        {
            get { return this.reload; }
            set
            {
                if (this.reload == value) { return; }
                this.reload = value;
                this.PropertyChanged?.Invoke(this, ReloadPropertyChangedEventArgs);
            }
        }


        private static readonly PropertyChangedEventArgs ReadCodePropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(ReadCode));
        private string readCode;
        public string ReadCode
        {
            get { return this.readCode; }
            set
            {
                if (this.readCode == value) { return; }
                this.readCode = value;
                this.PropertyChanged?.Invoke(this, ReadCodePropertyChangedEventArgs);
            }
        }

        private static readonly PropertyChangedEventArgs IsSerialOpenPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(IsSerialOpen));
        private bool isSerialOpen = false;
        public bool IsSerialOpen
        {
            get { return this.isSerialOpen; }
            set
            {
                if (this.isSerialOpen == value) { return; }
                this.isSerialOpen = value;
                this.PropertyChanged?.Invoke(this, IsSerialOpenPropertyChangedEventArgs);
            }
        }

    }

    public class SerialDeviceItem
    {
        public string PortName { get; set; }
        public string DeviceName { get; set; }
        public SerialDevice device { get; set; }
    }

}
