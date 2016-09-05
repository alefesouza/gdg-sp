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

using DYMO.Label.Framework;
using GDGSPCheckIn.Properties;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
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
        bool useDymo;
        List<string> labelItems = new List<string>();
        ILabel _label;
        string printerName = "";

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

            useDymo = Settings.Default.UseDymo;

            if (useDymo)
            {
                SetupDymo();
            }
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
                        removeName.Stop();

                        int id = int.Parse(member[0].ToString());
                        string name = member[2].ToString();

                        ResultText.Text = name;
                        string now = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");

                        App.objConn.Prepare("UPDATE event_" + eventId + " SET checked=1, date='" + now + "' WHERE id=" + id).Step();

                        if (useDymo)
                        {
                            PrintCode(id, name, now);
                        }

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

        private void SetupDymo()
        {   
            if(!File.Exists(Settings.Default.LabelPath))
            {
                System.Windows.MessageBox.Show("Não foi possível abrir o arquivo .label");
                return;
            }

            _label = Framework.Open(Settings.Default.LabelPath);

            if (_label == null)
            {
                return;
            }

            foreach (string objName in _label.ObjectNames)
            {
                if (!string.IsNullOrEmpty(objName))
                {
                    labelItems.Add(objName);
                }
            }

            foreach (IPrinter printer in Framework.GetPrinters())
            {
                printerName = printer.Name;
            }

            if (printerName.Equals(""))
            {
                System.Windows.MessageBox.Show("Não foi possível encontrar uma impressora Dymo");
            }
        }

        /// <summary>
        /// Enviar dados para uma impressora Dymo imprimir a etiqueta
        /// </summary>
        /// <param name="id">ID do membro</param>
        /// <param name="name">Nome do membro</param>
        /// <param name="date">Data atual</param>
        private void PrintCode(int id, string name, string date)
        {
            if (printerName.Equals(""))
            {
                foreach (IPrinter printers in Framework.GetPrinters())
                {
                    printerName = printers.Name;
                }

                if (printerName.Equals(""))
                {
                    return;
                }
            }

            if (labelItems.Contains("name"))
            {
                _label.SetObjectText("name", name);
            }

            if (labelItems.Contains("barcode"))
            {
                _label.SetObjectText("barcode", id.ToString());
            }

            if (labelItems.Contains("qrcode"))
            {
                _label.SetObjectText("qrcode", App.QRCode + id);
            }

            if (labelItems.Contains("date"))
            {
                _label.SetObjectText("date", date);
            }

            IPrinter printer = Framework.GetPrinters()[printerName];
            _label.Print(printer);
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