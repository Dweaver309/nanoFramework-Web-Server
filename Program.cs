using System;
using System.Threading;
using System.Text;
using System.Net.Sockets;

/// <summary>
/// To use the webserver type in the IP address in any brower the output should look like this
/// Hello World Utc central date and time is 08/13/2019 07:14:35
/// </summary>
namespace Webserver
{
    public class Program
    {

         

        public static void Main()
        {
          
         Console.WriteLine("Hello world!");

         Double TimeOffSet = -5;

        //Replace with your SSID and Password
         string MySSID = "SSID";

         string MYPassword = "Password";

         // Create the web server
         HttpWebServer  webServer = new HttpWebServer(MySSID, MYPassword,TimeOffSet);

         // Ceate a method to used when a ServerRequest event is raised
         webServer.ServerRequest += ServerRequest;
          
           void ServerRequest()
            {
                Console.WriteLine(webServer.RequestString);

                //Process request string and send the response string

                //Compose a response
                string response = "Hello World Utc central date and time is " + DateTime.UtcNow.AddHours(TimeOffSet);

                string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";

                webServer.clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                webServer.clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);
                
            }
         
            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T

        }
       
    }

}
