using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Watson.LanguageTranslator.v3;
using IBM.Watson.LanguageTranslator.v3.Model;
using IBM.Watson.TextToSpeech.v1;
using IBM.Watson.TextToSpeech.v1.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Topicos_ProyectoFinal
{
    public partial class Form1 : Form
    {
        TextToSpeechService textToSpeechService;
        LanguageTranslatorService languageTranslatorService;
        List<Voice> listVoices;
        string textToSpeechKey = "g7w6YxO0ZTH-8-EE0Iwrj7ORk1eKXRim4n_b8iKKqU4-";
        string textToSpeechUrl = "https://api.us-south.text-to-speech.watson.cloud.ibm.com/instances/89d5a008-47c4-4025-8b5d-4ca26d532bd1";
        string languageTranslatorKey = "cWxrMKlB9HcMaxrTdSSN-x60rc8_zXBMHsMcdEzH-V-D";
        string languageTranslatorUrl = "https://api.us-south.language-translator.watson.cloud.ibm.com/instances/80e7539f-db52-46fd-95c4-1b3e3f5c27fc";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string modelId = "es-" + comboBox4.Text.Split('(', ')')[1];

                var result = languageTranslatorService.Translate(
                text: new List<string>() { textBox1.Text },
                modelId: modelId
                );
                dynamic data = JsonConvert.DeserializeObject<ExpandoObject>(result.Response);
                textBox2.Text = data.translations[0].translation;
            }
            catch (Exception)
            {
                MessageBox.Show("Ingrese Texto Y Seleccione Idioma");
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IamAuthenticator textToSpeechAuth = new IamAuthenticator(apikey: textToSpeechKey);
            textToSpeechService = new TextToSpeechService(textToSpeechAuth);
            textToSpeechService.SetServiceUrl(textToSpeechUrl);

            IamAuthenticator languageTranslatorAuth = new IamAuthenticator(apikey:languageTranslatorKey);

            languageTranslatorService = new LanguageTranslatorService("2018-05-01", languageTranslatorAuth);
            languageTranslatorService.SetServiceUrl(languageTranslatorUrl);

            var result = textToSpeechService.ListVoices();

            listVoices = result.Result._Voices;

            foreach (var voice in listVoices.Where(voice => voice.Language.StartsWith("es")))
            {
                comboBox1.Items.Add(voice.Name);
            }

            comboBox4.Items.Add("Inglés (en)");
            comboBox4.Items.Add("Francés (fr)");
        }


        private void playAudio(string text, string voice, string errorMessage, int mediaPlayer)
        {
            try
            {
                if(mediaPlayer == 1)
                    axWindowsMediaPlayer1.URL = null;
                else
                    axWindowsMediaPlayer2.URL = null;

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(textToSpeechUrl + "/v1/synthesize?voice=" + voice);
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/json";

                var username = "apikey";
                var password = textToSpeechKey;
                string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                                               .GetBytes(username + ":" + password));

                httpWebRequest.Headers.Add("Authorization", "Basic " + encoded);
                httpWebRequest.Accept = "audio/wav";

                object data = new
                {
                    text = text
                };

                var jsonString = JsonConvert.SerializeObject(data);

                byte[] postBytes = Encoding.UTF8.GetBytes(jsonString);

                httpWebRequest.ContentLength = postBytes.Length;

                Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();

                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();

                Stream resStream = response.GetResponseStream();

                string path = Path.GetTempFileName() + ".wav";

                using (FileStream fs = File.Create(path))
                {
                    resStream.CopyTo(fs);
                    fs.Close();
                    resStream.Close();
                }

                if (mediaPlayer == 1)
                    axWindowsMediaPlayer1.URL = path;
                else
                    axWindowsMediaPlayer2.URL = path;

                File.Delete(path);
            }
            catch (Exception)
            {
                MessageBox.Show(errorMessage);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            playAudio(textBox1.Text, comboBox1.Text, "Ingrese un Texto y Seleccione Voz", 1);
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            comboBox2.Text = "Seleccione Voz";
            textBox2.Text = "";
            string target = comboBox4.Text.Split('(', ')')[1];

            foreach (var voice in listVoices.Where(voice => voice.Language.StartsWith(target)))
            {
                comboBox2.Items.Add(voice.Name);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            playAudio(textBox2.Text, comboBox2.Text, "Traduzca el Texto y Seleccione Voz", 2);
        }
    }
}
