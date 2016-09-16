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
using GDGSPCheckIn.Model;
using GDGSPCheckIn.Properties;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using WebEye.Controls.Wpf;
using ZXing;

namespace GDGSPCheckIn
{
    public partial class MainWindow : Window
    {
        public Timer removeName = new Timer(), checkQR = new Timer();
        bool useDymo;
        ILabel _label;
        string printerName = "", latestName = "";
        List<string> labelItems = new List<string>();
        List<int> printedIds = new List<int>();
        List<Event> events;

        public MainWindow(List<Event> events)
        {
            InitializeComponent();

            Title = App.AppName + " Check-in";

            this.events = events;

            removeName.Interval = 5000;
            removeName.Tick += (s, e) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ResultText.Text = "Bem-vindo!";
                    removeName.Stop();
                });
            };

            checkQR.Interval = 1000;
            checkQR.Tick += webCamTimer_Tick;
            checkQR.Start();

            LeftText.Text = "Abra o aplicativo do " + App.AppName + ", toque nos três pontinhos e escolha \"Fazer check-in\"";

            useDymo = Settings.Default.UseDymo;

            BoxName.Focus();

            BoxName.TextChanged += (s, e) =>
            {
                if (BoxName.Text.Length > 1)
                {
                    new System.Threading.Thread(() =>
                    {
                        System.Threading.Thread.CurrentThread.IsBackground = true;

                        List<Person> names = new List<Person>();

                        string query = "";
                        string text = "";

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            text = BoxName.Text.Replace("\"", "\"\"");
                        });

                        foreach (Event _event in events)
                        {
                            if (!query.Equals(""))
                            {
                                query += " UNION ALL ";
                            }

                            query += "SELECT * FROM event_" + _event.Id + " WHERE member_name LIKE '%" + text + "%'";
                        }

                        var member = App.objConn.Prepare(query);

                        while (member.Step() == SQLiteResult.ROW)
                        {
                            int id = int.Parse(member[1].ToString());

                            if (!names.Any(p => p.Id == id))
                            {
                                Person p = new Person() { Id = id, Name = member[2].ToString() };
                                names.Add(p);
                            }
                        }

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            ListName.ItemsSource = names;
                            ListName.Visibility = Visibility.Visible;
                        });
                    }).Start();
                }
                else
                {
                    ListName.Visibility = Visibility.Hidden;
                }
            };

            ListName.SelectionChanged += (s, e) =>
            {
                if (ListName.SelectedIndex != -1)
                {
                    Person p = e.AddedItems[0] as Person;
                    DoCheckin(p.Id);
                    BoxName.Text = "";
                    ListName.Visibility = Visibility.Hidden;
                    ListName.SelectedIndex = -1;
                }
            };

            ComboBox.ItemsSource = webCameraControl.GetVideoCaptureDevices();

            if (ComboBox.Items.Count > 0)
            {
                ComboBox.SelectedItem = ComboBox.Items[0];
            }

            if (useDymo)
            {
                SetupDymo();
            }
        }

        void webCamTimer_Tick(object sender, EventArgs e)
        {
            new System.Threading.Thread(() =>
            {
                Bitmap bitmap = null;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    bitmap = webCameraControl.GetCurrentImage();
                });

                if (bitmap == null)
                    return;
                var reader = new BarcodeReader();
                var result = reader.Decode(bitmap);
                if (result != null)
                {
                    string code = result.Text.ToString();
                    string[] codeSplit = code.Split('/');
                    int number = 0;
                    bool tryParse = int.TryParse(codeSplit[codeSplit.Length - 1], out number);

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (code.StartsWith(App.QRCode) && codeSplit.Length == 5 && tryParse)
                        {
                            DoCheckin(number);
                        }
                        else
                        {
                            ResultText.Text = "QR Code inválido";

                            removeName.Stop();
                            removeName.Start();
                        }
                    });
                }
            }).Start();
        }

        private void DoCheckin(int member_id)
        {
            if (!printedIds.Contains(member_id))
            {
                printedIds.Add(member_id);

                string query = "";

                foreach (Event _event in events)
                {
                    if (!query.Equals(""))
                    {
                        query += " UNION ALL ";
                    }

                    query += "SELECT * FROM event_" + _event.Id + " WHERE member_id=" + member_id;
                }

                var member = App.objConn.Prepare(query);

                if (member.ColumnCount != 0)
                {
                    ResultText.Text = "QR Code não encontrado neste evento";
                }

                string name = "";
                string now = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");

                while (member.Step() == SQLiteResult.ROW)
                {
                    int id = int.Parse(member[0].ToString());
                    name = member[2].ToString();
                    latestName = name;

                    ResultText.Text = name;

                    foreach (Event _event in events)
                    {
                        App.objConn.Prepare("UPDATE event_" + _event.Id + " SET checked=1, date='" + now + "' WHERE member_id=" + member_id).Step();
                    }
                }

                if (!name.Equals("") && useDymo)
                {
                    PrintCode(member_id, name, now);
                }
            }
            else
            {
                if (!BoxName.Text.Equals(latestName))
                {
                    ResultText.Text = "Check-in realizado anteriormente";
                }
            }

            BoxName.Text = "";
            ListName.Visibility = Visibility.Hidden;
            ListName.SelectedIndex = -1;

            BoxName.Focus();

            removeName.Stop();
            removeName.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var cameraId = (WebCameraId)ComboBox.SelectedItem;
            webCameraControl.StartCapture(cameraId);
        }

        private void SetupDymo()
        {
            if (!File.Exists(Settings.Default.LabelPath))
            {
                System.Windows.MessageBox.Show("Não foi possível abrir o arquivo .label");
                return;
            }

            try
            {
                _label = Framework.Open(Settings.Default.LabelPath);
            }
            catch
            {
                if (System.Windows.MessageBox.Show("Parece que o software da Dymo não está instalado, deseja baixar agora?", "GDG-SP", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Process.Start("http://www.dymo.com/en-US/dymo-user-guides");
                    useDymo = false;
                }
                return;
            }

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
                useDymo = false;
            }
        }

        /// <summary>
        /// Enviar dados para uma impressora Dymo imprimir a etiqueta
        /// </summary>
        /// <param name="member_id">ID do membro</param>
        /// <param name="name">Nome do membro</param>
        /// <param name="date">Data atual</param>
        private void PrintCode(int member_id, string name, string date_hour)
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

            if (labelItems.Contains("date"))
            {
                string date = Settings.Default.LabelDate.Replace("%s", date_hour.Split(' ')[1]);
                _label.SetObjectText("date", date);
            }

            if (labelItems.Contains("date_hour"))
            {
                string date_hour_format = Settings.Default.LabelDateHour.Replace("%s", date_hour);
                _label.SetObjectText("date_hour", date_hour_format);
            }

            if (labelItems.Contains("event"))
            {
                string eventName = "";

                if (events.Count == 1)
                {
                    eventName = Settings.Default.LabelEvent.Replace("%s", events[0].Name.Replace(" - ", "\n"));
                }
                else
                {
                    string allEvents = "";

                    foreach (Event _event in events)
                    {
                        if (!allEvents.Equals(""))
                        {
                            allEvents += "\n";
                        }

                        allEvents += _event.Name.Split('-')[0];
                    }

                    eventName = Settings.Default.LabelEvent.Replace("%s", allEvents);
                }
                _label.SetObjectText("event", eventName);
            }

            if (labelItems.Contains("meetup_id"))
            {
                _label.SetObjectText("meetup_id", member_id.ToString());
            }

            if (labelItems.Contains("qrcode"))
            {
                _label.SetObjectText("qrcode", App.QRCode + member_id.ToString());
            }

            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.CurrentThread.IsBackground = true;

                IPrinter printer = Framework.GetPrinters()[printerName];
                _label.Print(printer);
            }).Start();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            VideoDevice.Visibility = Visibility.Collapsed;
            BoxName.Focus();
        }
    }
}