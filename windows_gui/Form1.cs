using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Http;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // On checking add
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked) {
                lengthInput.ReadOnly = false;
                lengthInput.Enabled = true;
            } else {
                lengthInput.ReadOnly = true;
                lengthInput.Enabled = false;
            }
        }

        // On submit button click
        private void button1_Click(object sender, EventArgs e)
        {
            //   string args = userInput.Text + ' ' + passwordInput.Text + ' ' + accountInput.Text + ' ' + lengthInput.Text;
            //    DialogResult r1 = MessageBox.Show(args, "Debug", MessageBoxButtons.OK, 0);
                if (checkBox1.Checked) { // Add Mode
                    try {
                        addProcedure(userInput.Text, accountInput.Text, passwordInput.Text, Int32.Parse(lengthInput.Text));
                    } catch (FormatException) {
                        ResultLabel.Text = "Password Length Must be an\nInteger";
                    }
                } else { // Get Mode
                    getProcedure(userInput.Text, accountInput.Text, passwordInput.Text);
                }
        }

        // GET procedure
        private async void getProcedure(string userText, string accountText, string passwordText)
        {
            // Hash user, send HTTP request and store response in httpresult
            string userhash = BackendClass.hashuserhex(userText);
            FormUrlEncodedContent toSend = BackendClass.generateFirstPost(userhash, accountText);
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Config.serverURL);
            client.DefaultRequestHeaders.Add("Referer", Config.serverURI + "/getpass.html");
            string httpresult = "";
            try {
                HttpResponseMessage httpresultraw = await client.PostAsync(Config.serverURI + "/getpass.php", toSend);
                httpresult = await httpresultraw.Content.ReadAsStringAsync();
            } catch (HttpRequestException ex) {
                ResultLabel.Text = "HTTP error. Are you connected\nto the Internet?"; return;
            }
            // Run parseGetResult
            string getResult = BackendClass.parseGetResult(userhash, httpresult, passwordText);
            // Check success and act accordingly
            if (getResult.StartsWith("Password: ")) {
                Clipboard.SetText(getResult.Substring(10)); // Cut off the "password" header before copying
                ResultLabel.Text = "Result: Password Copied";
            } else {
                ResultLabel.Text = getResult.Trim();
            }
        }

        // ADD procedure
        private async void addProcedure(string userText, string accountText, string passwordText, int passLength)
        {
            // Hash user, send HTTP request and store response in httpresult. Cookies stored in handler.
            string userhash = BackendClass.hashuserhex(userText);
            FormUrlEncodedContent toSend = BackendClass.generateFirstPost(userhash, accountText);
            HttpClientHandler handler = new HttpClientHandler(); handler.UseCookies = true;
            HttpClient client = new HttpClient(handler);
            client.BaseAddress = new Uri(Config.serverURL);
            client.DefaultRequestHeaders.Add("Referer", Config.serverURI + "/addpass.html");
            string httpresult = "";
            try {
                HttpResponseMessage httpresultraw = await client.PostAsync(Config.serverURI + "/addpass_challenge.php", toSend);
                httpresult = await httpresultraw.Content.ReadAsStringAsync();
            } catch (HttpRequestException ex) {
                ResultLabel.Text = "HTTP error. Are you connected\nto the Internet?"; return;
            }
            // Run respondToAdd
            string[] responseFields;
            try {
                responseFields = BackendClass.respondToAdd(userhash, httpresult, passwordText, accountText, passLength).Split('$');
            } catch (Exception ex) {
                ResultLabel.Text = ex.Message.Trim();
                return;
            }
            // Send verification
            toSend = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("userhash", userhash),
                new KeyValuePair<string, string>("passwordcrypt", responseFields[0]),
                new KeyValuePair<string, string>("signature", responseFields[1])
            });
            HttpResponseMessage httpresultfinalraw = await client.PostAsync(Config.serverURI + "/addpass_verify.php", toSend);
            string httpresultfinal = await httpresultfinalraw.Content.ReadAsStringAsync();
            // Show result and return
            ResultLabel.Text = httpresultfinal;
        }
    }
}
