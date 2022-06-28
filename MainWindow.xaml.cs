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

        private void btnConnect_Click(object sender, RoutedEventArgs e)
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

            //Thread clientThread = new Thread(getImage);
            //clientThread.Start();
            getImage();

        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            _client.Close();
            txtConnectionStatus.Text = "Disconnected";
            txtConnectionStatus.Foreground = new SolidColorBrush(Colors.DarkRed);
        }

        private void getImage()
        {
            // Add while(true)

            byte[] headerLengthBytes = new byte[4];
            int headerLength;
            byte[] bytesFrom = new byte[65536];
            int bytesLeft;
            MemoryStream messageStream = new MemoryStream();

            _serverStream = _client.GetStream();

            _serverStream.Read(headerLengthBytes, 0, 4);

            headerLength = BitConverter.ToInt32(headerLengthBytes, 0);
            bytesLeft = headerLength;


            while (bytesLeft > 0)
            {
                var read = _serverStream.Read(bytesFrom, 0, _client.ReceiveBufferSize);
                messageStream.Write(bytesFrom, 0, read);
                bytesLeft -= read;
            }


            //string hexString = Convert.ToHexString(bytesFrom);


            _image = new BitmapImage();
            messageStream.Position = 0;
            _image.BeginInit();
            _image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            _image.CacheOption = BitmapCacheOption.OnLoad;
            _image.UriSource = null;
            _image.StreamSource = messageStream;
            _image.EndInit();



            //_image = new BitmapImage();
            //using (var mem = new MemoryStream(bytesFrom))
            //{
            //    mem.Position = 0;
            //    _image.BeginInit();
            //    _image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            //    _image.CacheOption = BitmapCacheOption.OnLoad;
            //    _image.UriSource = null;
            //    _image.StreamSource = mem;
            //    _image.EndInit();
            //}

            imgScreenCapture.Source = _image;
            //Application.Current.Dispatcher.BeginInvoke(() => showImage());

            //MessageBox.Show(hexString);
            //MessageBox.Show(Encoding.UTF8.GetString(bytesFrom));

        }

        private void showImage()
        {
            imgScreenCapture.Source = _image;
        }
    }
}
