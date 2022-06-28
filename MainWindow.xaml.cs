using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ScreenCaptureClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient? _client;
        private NetworkStream? _serverStream = default(NetworkStream);
        private BitmapImage? _image;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            string host = txtHost.Text;
            int port = int.Parse(txtPort.Text);
            byte[] requestStream = Encoding.UTF8.GetBytes(txtInterval.Text);

            _client = new TcpClient();
            try
            {
                _client.Connect(host, port);
            }
            catch(SocketException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            txtConnectionStatus.Text = "Connected";
            txtConnectionStatus.Foreground = new SolidColorBrush(Colors.DarkGreen);

            _serverStream = _client.GetStream();
            _serverStream.Write(requestStream, 0, requestStream.Length);
            _serverStream.Flush();

            try
            {
                await getImage();
            }
            catch(IOException ex)
            {

            }
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            _client.Close();
            txtConnectionStatus.Text = "Disconnected";
            txtConnectionStatus.Foreground = new SolidColorBrush(Colors.DarkRed);
            imgScreenCapture.Source = null;
        }

        private async Task getImage()
        {
            while (true)
            {
                byte[] headerLengthBytes = new byte[4];
                int headerLength;
                byte[] bytesFrom = new byte[65536];
                int bytesLeft;
                MemoryStream messageStream = new MemoryStream();

                _serverStream = _client.GetStream();

                await _serverStream.ReadAsync(headerLengthBytes, 0, 4);

                headerLength = BitConverter.ToInt32(headerLengthBytes, 0);
                bytesLeft = headerLength;


                while (bytesLeft > 0)
                {
                    var read = await _serverStream.ReadAsync(bytesFrom, 0, _client.ReceiveBufferSize);
                    await messageStream.WriteAsync(bytesFrom, 0, read);
                    bytesLeft -= read;
                }

                _image = new BitmapImage();
                messageStream.Position = 0;
                _image.BeginInit();
                _image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                _image.CacheOption = BitmapCacheOption.OnLoad;
                _image.UriSource = null;
                _image.StreamSource = messageStream;
                _image.EndInit();

                imgScreenCapture.Source = _image;
            }
        }

        private void showImage()
        {
            imgScreenCapture.Source = _image;
        }
    }
}
