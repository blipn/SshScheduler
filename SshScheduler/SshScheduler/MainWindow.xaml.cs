using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Renci.SshNet;

namespace SshScheduler
{
    /// <summary>
    /// SSH SCHEDULER
    /// </summary>
    public partial class MainWindow : Window
    {
        private string host = "";
        private int port = 22;
        private string user = "";
        private string pass = "";
        private string typeRepeat = "0";
        private int timeRep = 0;
        private string typeTime = "1";
        private SshClient client;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            textBox.Text = "Connecting...";

            //Set first line ( host - port - user - pass )
            host = textBoxHost.Text;
            Int32.TryParse(textBoxPort.Text, out port);
            user = textBoxUser.Text;
            pass = textBoxPass.Password;

            //Set de la deuxième ligne ( repeat type - time - time type )
            ComboBoxItem comboItemTypeRepeat = (ComboBoxItem)comboBoxTypeRepeat.SelectedItem;
            typeRepeat = comboItemTypeRepeat.Uid;

            Int32.TryParse(comboBoxTime.Text, out timeRep);

            ComboBoxItem comboItemTypeTime = (ComboBoxItem)comboBoxTypeTime.SelectedItem;
            typeTime = comboItemTypeTime.Uid;

            //translate time to millis
            if (timeRep > 0)
            {
                switch (typeTime)
                {
                    case "s":
                        timeRep *= 1000;
                        break;
                    case "mn":
                        timeRep *= 1000 * 60;
                        break;
                    case "h":
                        timeRep *= 1000 * 60 * 60;
                        break;
                }
            }

            if (typeRepeat == "repFrom")
            {
                // TODO: loop send from now
            }
            else if (typeRepeat == "repAfter")
            {
                // TODO: loop send after the first iter
            }
            else // Send Once
            {
                if (timeRep > 0)
                {
                    schedule(host, port, user, pass, comboBox.Text, timeRep);
                }
                else
                {
                    textBox.Text = send(host, port, user, pass, comboBox.Text);
                }
            }

            // Async send
            async void schedule(string host, int port, string user, string pass, string command, int msTime)
            {
                textBoxScheduled.Text = string.Format("{0:HH:mm:ss tt}", DateTime.Now.AddMilliseconds(msTime)) + " : " + user + "@" + host + ":" + port + " :\t" + command + "\n" + textBoxScheduled.Text;
                string res = await sendAsync(host, port, user, pass, command, msTime);
                textBox.Text = res;
            }
            Task<string> sendAsync(string host, int port, string user, string pass, string command, int msTime = 0)
            {
                return Task.Run<string>(() => send(host, port, user, pass, command, msTime));
            }


            // Sends a Command and return an Output
            string send(string host, int port, string user, string pass, string command, int msTime = 0)
            {
                Thread.Sleep(msTime);
                if (host.Length > 0 && port > 0 && user.Length > 0)
                {
                    using (client = new SshClient(host, port, user, pass))
                    {
                        try
                        {
                            client.Connect();
                            var result = "Successfully connected to " + client.ConnectionInfo.Host;
                            if (command.Length > 0)
                            {
                                var cmd = client.CreateCommand(command);
                                result = cmd.Execute();
                            }
                            return result;
                        }
                        catch (SocketException exception)
                        {
                            return "Unable to reach host " + client.ConnectionInfo.Host + "\n" + exception;
                        }
                        catch (Exception exception)
                        {
                            return "Error : " + client.ConnectionInfo.Host + "\n" + exception;
                        }
                        finally
                        {
                            client.Disconnect();
                        }
                    }
                }
                else
                {
                    return "Host or User Empty";
                }


            }


        }

    }
}
