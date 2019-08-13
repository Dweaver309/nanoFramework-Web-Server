using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using nanoFramework.Runtime.Native;


namespace Webserver
{

    /// <summary>
    /// HttpWebServer is a simple way to use web pages to communicate with your project
    /// This project has been tested with the ESP DevKit C and Pico D4
    /// The nanoFramework samples are a great resource for this project https://github.com/nanoframework/Samples
    /// For more details on using a web server see my ESP8266 Serial Wi-Fi https://github.com/Dweaver309/nanoFramework.serial.wifi.esp8266
    /// To learn about parsing GET and POST requests and much more see this https://archive.codeplex.com/?p=netmfwebserver
    /// </summary>
    class HttpWebServer : IDisposable
    {
        private  Socket socket = null;
      
        public  Socket clientSocket = null;

        public string RequestString = string.Empty;

        public delegate void dgEventRaiser();

        public  event dgEventRaiser ServerRequest;
        
        private string _SSID = string.Empty;

        private string _Password = string.Empty;

        private   Double _TimeOffSet = 0;

        /// <summary>
        /// Constructor for creating the web server
        /// Example: HttpWebServer  webServer = new HttpWebServer("SSID", "password", -4);
        /// </summary>
        public HttpWebServer( string SSID, string Password, Double TimeOffSet = -5)
        {
            _SSID = SSID;

            _Password = Password;

            _TimeOffSet = TimeOffSet;
              
            ConnectNetwork();

            // Initialize Socket class
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Request and bind to an IP from DHCP server
            socket.Bind(new IPEndPoint(IPAddress.Any, 80));

            Console.WriteLine(NetworkInterface.GetAllNetworkInterfaces()[0].IPv4Address);

            // Start listen for web requests
            socket.Listen(10);

            // SMTP connects automatically to get time
            Rtc.SetSystemTime(DateTime.UtcNow.AddHours(_TimeOffSet));
            
            Console.WriteLine("System time is:" + DateTime.UtcNow.ToString());

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
        private  void ListenForRequest()
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

        /// <summary>
        /// Connect to the SSID network and login using the Password
        /// </summary>
        private  void ConnectNetwork()
        {
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();

            if (nis.Length > 0)
            {
                // Get the first interface
                NetworkInterface ni = nis[0];


                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    
                    // Network interface is Wi-Fi
                    Console.WriteLine("Network connection is: Wi-Fi");

                    Wireless80211Configuration wc = Wireless80211Configuration.GetAllWireless80211Configurations()[ni.SpecificConfigId];
                   
                        wc.Ssid = _SSID;

                        wc.Password = _Password;
                 
                 }

                else
                {
                    
                    // Network interface is Ethernet
                    Console.WriteLine("Network connection is: Ethernet");

                    ni.EnableDhcp();

                }

                // Wait for DHCP to complete
                WaitIP();

            }

            else
            {
                throw new NotSupportedException("ERROR: there is no network interface configured.\r\nOpen the 'Edit Network Configuration' in Device Explorer and configure one.");
            }

        }

        /// <summary>
        /// Wait for the IP address
        /// </summary>
        private void WaitIP()
        {
            Console.WriteLine("Waiting for IP...");


            while (true)
            {

                NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];
                
                if (ni.IPv4Address != null && ni.IPv4Address.Length > 0)
                {

                    if (ni.IPv4Address[0] != '0')
                    {

                        Console.WriteLine($"We have an IP: {ni.IPv4Address}");
                       
                        break;

                    }

                }

                Thread.Sleep(1000);

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




