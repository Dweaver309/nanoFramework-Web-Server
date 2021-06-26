using System;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using nanoFramework.Runtime.Native;
using nanoFramework.Networking;




namespace Webserver
{

    /// <summary>
    /// Thanks to Laurent Ellerbach who wrote the networker helper that simplified in improved this project.
    /// HttpWebServer is a simple way to use web pages to communicate with your project
    /// This project has been tested with the ESP DevKit C and Pico D4
    /// The nanoFramework samples are a great resource for this project https://github.com/nanoframework/Samples
    /// For more details on using a web server see my ESP8266 Serial Wi-Fi https://github.com/Dweaver309/nanoFramework.serial.wifi.esp8266
    /// To learn about parsing GET and POST requests and much more see this https://archive.codeplex.com/?p=netmfwebserver
    /// </summary>
    public class HttpWebServer : IDisposable
    {
        private Socket socket = null;

        public Socket clientSocket = null;

        public string RequestString = string.Empty;

        public delegate void dgEventRaiser();

        public event dgEventRaiser ServerRequest;
       
        /// <summary>
        /// Constructor for creating the web server
        /// Example: HttpWebServer  webServer = new HttpWebServer("SSID", "password", -4);
        /// </summary>
        public HttpWebServer(string SSID, string Password, Double TimeOffSet = -5)
        {
          
            bool success;

            CancellationTokenSource cs = new(60000);

            success = NetworkHelper.ConnectWifiDhcp(SSID, Password, setDateTime: true, token: cs.Token);
                       
            // Initialize Socket class
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Request and bind to an IP from DHCP server
            socket.Bind(new IPEndPoint(IPAddress.Any, 80));

            Debug.WriteLine("IP " + NetworkInterface.GetAllNetworkInterfaces()[0].IPv4Address);

            String macString = BitConverter.ToString(NetworkInterface.GetAllNetworkInterfaces()[0].PhysicalAddress);

            Debug.WriteLine("Mac " + macString);
           
            // Start listen for web requests
            socket.Listen(10);

            // SMTP connects automatically to get time
            Rtc.SetSystemTime(DateTime.UtcNow.AddHours(TimeOffSet));

            Debug.WriteLine("System time is:" + DateTime.UtcNow.ToString());

            // Create and start a thead for listening for server requests
            Thread tListenforRequest = new Thread(ListenForRequest);

            tListenforRequest.Start();

        }

        /// <summary>
        /// When a server request is made the clientSocket accepts the request
        /// The ServerRequest() event is raised and the string received is 
        /// decoded into the RequestSring 
        /// The event can be consumed from another class  
        /// </summary>
        private void ListenForRequest()
        {
            while (true)
            {
                using (clientSocket = socket.Accept())
                {
                    // Get clients IP
                    IPEndPoint clientIP = clientSocket.RemoteEndPoint as IPEndPoint;

                    EndPoint clientEndPoint = clientSocket.RemoteEndPoint;

                    int bytesReceived = clientSocket.Available;

                    if (bytesReceived > 0)
                    {
                        // Get server request
                        byte[] buffer = new byte[bytesReceived];

                        int byteCount = clientSocket.Receive(buffer, bytesReceived, SocketFlags.None);

                        // Put server request in public RequestString
                        RequestString = new string(Encoding.UTF8.GetChars(buffer));

                        // Raise event
                        ServerRequest();

                        Thread.Sleep(150);

                    }
                }
            }
        }

       

        ~HttpWebServer()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (socket != null)
                socket.Close();
        }
    }
}
