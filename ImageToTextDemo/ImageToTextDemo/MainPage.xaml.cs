using Acr.UserDialogs;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ImageToTextDemo
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        private async void RealOcr_Clicked(object sender, EventArgs e)
        {
            OcrResult text1;
            var media = Plugin.Media.CrossMedia.Current;
            await media.Initialize();
            var file = await media.TakePhotoAsync(new StoreCameraMediaOptions
            {
                SaveToAlbum = false
            });
            UserDialogs.Instance.ShowLoading("Wait Till We extract Text");
            Img.Source = ImageSource.FromStream(() => file.GetStream());

            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add(
                "Ocp-Apim-Subscription-Key", "<Your COMPUTER_VISION_SUBSCRIPTION_KEY>");

            // Request parameters. 
            // The language parameter doesn't specify a language, so the 
            // method detects it automatically.
            // The detectOrientation parameter is set to true, so the method detects and
            // and corrects text orientation before detecting text.
            string requestParameters = "language=unk&detectOrientation=true";

            // Assemble the URI for the REST API method.
            string uri = "https://westcentralus.api.cognitive.microsoft.com/vision/v2.1/ocr" + "?" + requestParameters;

            HttpResponseMessage response;

            // Read the contents of the specified local image
            // into a byte array.

            BinaryReader binaryReader = new BinaryReader(file.GetStream());
            byte[] byteData = binaryReader.ReadBytes((int)file.GetStream().Length);



           

            // Add the byte array as an octet stream to the request body.
            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses the "application/octet-stream" content type.
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                // Asynchronously call the REST API method.
                response = await client.PostAsync(uri, content);
            }

            // Asynchronously get the JSON response.
            string contentString = await response.Content.ReadAsStringAsync();

            // Display the JSON response.
            Console.WriteLine("\nResponse:\n\n{0}\n",
                JToken.Parse(contentString).ToString());


            var data = JToken.Parse(contentString).ToString();

            
            text1 = JsonConvert.DeserializeObject<OcrResult>(data);

            LblResult.Text = "";

            //var visionclient = new VisionServiceClient("Subscription Key", "https://westcentralus.api.cognitive.microsoft.com/vision/v2.0/recognizeText?Printed" /*"https://westcentralus.api.cognitive.microsoft.com/vision/v1.0"*/);
            //text1 = await visionclient.RecognizeTextAsync(file.GetStream());
            foreach (var region in text1.Regions)
            {
                foreach (var line in region.Lines)
                {
                    foreach (var word in line.Words)
                    {
                        LblResult.Text += Convert.ToString(word.Text)+" ";
                    }
                    LblResult.Text += "\n";
                }
            }
            UserDialogs.Instance.HideLoading();
        }
    }
   
}
