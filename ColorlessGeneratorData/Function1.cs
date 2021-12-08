using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Devices.Client;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using DotNetty.Codecs;


namespace ColorlessGeneratorData
{
    public class Function1
    {
        private static DeviceClient s_deviceClient;
        private readonly static string s_connectionString01 = "HostName=colorless-iot-hub.azure-devices.net;DeviceId=mydecive-colorless;SharedAccessKey=g8c4iCaOe7WgujxOq62T5vJ60l2KxSHz+9btMNhLc78=";

        [FunctionName("Function1")]
        public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString01, TransportType.Mqtt);
            SendDeviceToCloudMessagesAsync(s_deviceClient);
            Console.ReadLine();

        }

        private static async void SendDeviceToCloudMessagesAsync(DeviceClient s_deviceClient)
        {
            try
            {
                double minTemperature = 20;
                double minOxigenlevel = 2;
                Random rand = new Random();
                int length = 10;


                while (true)
                {
                    const string chars = "ABCDEFGHIJKLMOPQRSTUVWXYZ0123456789";
                    var random = new Random();
                    var currentdeviceId = new string(Enumerable.Repeat(chars, length)
                        .Select(s => s[random.Next(s.Length)]).ToArray());

                    double currentTemperature = minTemperature + rand.NextDouble() * 15;
                    double currentOxigenLevel = minOxigenlevel + rand.NextDouble() * 20;
                    DateTime currentimestamp;
                    currentimestamp = Convert.ToDateTime(DateTime.Now.ToString("dd-MMM-yyyy"));
                    // var currentimestamp = (int)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;

                    // Create JSON message  

                    var telemetryDataPoint = new
                    {
                        deviceID = currentdeviceId,
                        temperature = currentTemperature,
                        oxigenlevel = currentOxigenLevel,
                        timestamp = currentimestamp
                    };

                    string messageString = "";



                    messageString = JsonConvert.SerializeObject(telemetryDataPoint);

                    var message = new Message(Encoding.ASCII.GetBytes(messageString));

                    // Add a custom application property to the message.  
                    // An IoT hub can filter on these properties without access to the message body.  
                    //message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");  

                    // Send the telemetry message  
                    await s_deviceClient.SendEventAsync(message);
                    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
                    await Task.Delay(1000 * 10);

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }

}
