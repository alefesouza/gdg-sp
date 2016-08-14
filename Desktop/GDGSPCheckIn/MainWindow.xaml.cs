/*
 * Copyright (C) 2016 Alefe Souza <http://alefesouza.com>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using SQLitePCL;
using System;
using System.Windows;
using System.Windows.Forms;
using ZXing;

namespace GDGSPCheckIn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PictureBox picWebCam = new PictureBox();
        WebCam wCam;
        Timer webCamTimer;
        int eventId;
        public Timer removeName = new Timer();

        public MainWindow(int eventId)
        {
            InitializeComponent();

            Title = App.AppName + " Check-in";

            this.eventId = eventId;

            removeName.Interval = 5000;
            removeName.Tick += (s, e) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ResultText.Text = "Bem-vindo!";
                    removeName.Stop();
                });
            };

            LeftText.Text = "Abra o aplicativo do " + App.AppName + ", toque nos três pontinhos e escolha \"Fazer check-in\"";
        }

        void webCamTimer_Tick(object sender, EventArgs e)
        {
            var bitmap = wCam.GetCurrentImage();
            if (bitmap == null)
                return;
            var reader = new BarcodeReader();
            var result = reader.Decode(bitmap);
            if (result != null)
            {
                string code = result.Text.ToString();
                string[] codeSplit = code.Split('/');

                if (code.StartsWith(App.QRCode) && codeSplit.Length == 5)
                {
                    var member = App.objConn.Prepare("SELECT * FROM event_" + eventId + " WHERE member_id=" + codeSplit[codeSplit.Length - 1]);

                    if (member.ColumnCount != 0)
                    {
                        ResultText.Text = "QR Code não encontrado neste evento";
                    }

                    while (member.Step() == SQLiteResult.ROW)
                    {
                        ResultText.Text = member[2].ToString();
                        string now = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
                        int id = int.Parse(member[0].ToString());

                        App.objConn.Prepare("UPDATE event_" + eventId + " SET checked=1, date='" + now + "' WHERE id=" + id).Step();

                        removeName.Stop();
                        removeName.Start();
                    }
                }
                else
                {
                    ResultText.Text = "QR Code inválido";

                    removeName.Stop();
                    removeName.Start();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitWebCam();
        }

        // Código de https://zxingnet.codeplex.com/SourceControl/latest#trunk/Clients/WindowsFormsDemo/WindowsFormsDemoForm.cs
        void InitWebCam()
        {
            picWebCam.Anchor = (((((AnchorStyles.Top | AnchorStyles.Bottom)
               | AnchorStyles.Left)
               | AnchorStyles.Right)));
            picWebCam.BorderStyle = BorderStyle.None;
            picWebCam.Location = new System.Drawing.Point(3, 57);
            picWebCam.Name = "picWebCam";
            picWebCam.Size = new System.Drawing.Size((int)SecondRow.ActualHeight, (int)SecondRow.ActualHeight);
            picWebCam.SizeMode = PictureBoxSizeMode.StretchImage;
            picWebCam.TabIndex = 8;
            picWebCam.BackColor = System.Drawing.Color.Blue;
            picWebCam.TabStop = false;

            WFHost.Child = picWebCam;

            if (wCam == null)
            {
                wCam = new WebCam
                {
                    Container = picWebCam
                };

                wCam.OpenConnection();

                webCamTimer = new Timer();
                webCamTimer.Tick += webCamTimer_Tick;
                webCamTimer.Interval = 50;
                webCamTimer.Start();
            }
            else
            {
                webCamTimer.Stop();
                webCamTimer = null;
                wCam.Dispose();
                wCam = null;
            }
        }
    }
}
