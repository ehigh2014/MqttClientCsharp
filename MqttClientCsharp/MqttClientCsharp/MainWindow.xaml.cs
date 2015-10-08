using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Threading;

namespace MqttClientCsharp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        MqttClient client;
        StringBuilder sb = new StringBuilder(4096);
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            //handle messge received
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                (ThreadStart)delegate()
                {
                    if (sb.Length > 10000)
                    {
                        sb.Clear();
                    }
                    sb.Append("Topic:" + e.Topic.ToString() + " Len:" + e.Message.Length.ToString() + " Msg:");
                    if (hexCheckBox.IsChecked == true)
                    {
                        foreach (byte b in e.Message)
                        {
                            sb.Append(b.ToString("x2").ToUpper() + " ");
                        }
                    }
                    else
                    {
                        sb.Append(Encoding.Default.GetString(e.Message));
                    }
                    sb.AppendLine();
                    recvInfoTB.Text = sb.ToString();
                });
        }

        private void publishBtn_Click(object sender, RoutedEventArgs e)
        {
            if (client != null && client.IsConnected)
            {
                try
                {
                    if (pubTopicTB.Text.Length != 0)
                    {
                        client.Publish(pubTopicTB.Text, Encoding.Default.GetBytes(pubInfoTB.Text), (byte)subQosCB.SelectedIndex, false);
                    }
                    else
                    {
                        MessageBox.Show("请输入有效的topic");
                    }

                }
                catch (Exception ex)
                {
                    statusTB.Text = ex.Message;
                }
            }
            else
            {
                MessageBox.Show("未连接Broker");
            }
        }

        private void subBtn_Click(object sender, RoutedEventArgs e)
        {
            if (client != null && client.IsConnected)
            {
                if (topicTB.Text.Length != 0)
                {
                    MqttSubscribeTopic(topicTB.Text, (byte)qosCB.SelectedIndex);
                }
                else
                {
                    MessageBox.Show("请输入有效的topic");
                }
            }
            else
            {
                MessageBox.Show("未连接Broker");
            }
        }

        /// <summary>
        /// 对于订阅一个topic进行封装
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="qoslevel"></param>
        private void MqttSubscribeTopic(string topic, byte qoslevel)
        {

            string[] topics = { topic };
            byte[] qoss = { qoslevel };
            try
            { client.Subscribe(topics, qoss); }
            catch (Exception ex)
            { statusTB.Text = ex.Message; }

        }

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            string clientId = Guid.NewGuid().ToString();
            try
            {
                client = new MqttClient(brokerTB.Text);
                client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
                client.Connect(clientId);
                statusTB.Text = "成功连接到Broker";
            }
            catch (Exception ex)
            {
                statusTB.Text = ex.Message;
            }
        }

        private void disconnectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (client != null && client.IsConnected)
            {
                client.Disconnect();
                statusTB.Text = "Broker未连接";
            }
        }
    }
}
