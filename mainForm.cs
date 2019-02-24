using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;

namespace CelestDebugger {
    public partial class mainForm : Form {
        public string mainPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\CelestDebugger\";
        public string sugestList = "game_end;game_restart;fork;fork pre;fork post;steam;steam peers";
        public List<string> macroCommands = new List<string>();
        public List<SolidBrush> messageTextColours = new List<SolidBrush> {
            new SolidBrush(Color.FromArgb(255, 41, 43, 44)), //Normal
            new SolidBrush(Color.FromArgb(255, 255, 255, 255)), // Warning
            new SolidBrush(Color.FromArgb(255, 255, 255, 255)), // Error
            new SolidBrush(Color.FromArgb(255, 255, 255, 255)), // Success

            new SolidBrush(Color.FromArgb(255, 35, 37, 38)), //Normal - Instance
            new SolidBrush(Color.FromArgb(255, 223, 223, 223)), // Warning - Instance
            new SolidBrush(Color.FromArgb(255, 223, 223, 223)), // Error - Instance
            new SolidBrush(Color.FromArgb(255, 223, 223, 223)) // Success - Instance
        };

        public List<SolidBrush> messageBackColours = new List<SolidBrush> {
            new SolidBrush(Color.FromArgb(255, 247, 247, 247)), //Normal
            new SolidBrush(Color.FromArgb(255, 240, 173, 78)), // Warning
            new SolidBrush(Color.FromArgb(255, 217, 83, 79)), // Error
            new SolidBrush(Color.FromArgb(255, 92, 184, 92)), // Success

            new SolidBrush(Color.FromArgb(255, 216, 216, 216)), //Normal - Instance
            new SolidBrush(Color.FromArgb(255, 209, 151, 68)), // Warning - Instance
            new SolidBrush(Color.FromArgb(255, 189, 72, 69)), // Error - Instance
            new SolidBrush(Color.FromArgb(255, 80, 160, 80)) // Success - Instance
        };

        public class celestMessage {
            public celestMessage(String text, SolidBrush textcol = null, SolidBrush backcol = null) {
                this.text = text;
                if (textcol == null) {
                    this.textcol = new SolidBrush(Color.FromArgb(255, 41, 43, 44));
                } else {
                    this.textcol = textcol;
                }
                if (backcol == null) {
                    this.backcol = new SolidBrush(Color.FromArgb(255, 247, 247, 247));
                } else {
                    this.backcol = backcol;
                }
            }

            public String text;
            public SolidBrush textcol;
            public SolidBrush backcol;
        }

        public mainForm() {
            InitializeComponent();

            // AppData
            if (Directory.Exists(mainPath) == false) {
                Directory.CreateDirectory(mainPath);
            }

            suggestReload();

            if (Directory.Exists(mainPath + "macros") == false) {
                Directory.CreateDirectory(mainPath + "macros");
            }

            macroReload();

            // Socket
            UdpClient celestSocket = new UdpClient(Celest.celestPort);
            celestSocket.BeginReceive(new AsyncCallback(Celest.celestRecieve), celestSocket);

            // NetThread
            Celest.celestForm = this;
            Celest.celestSocket = celestSocket;
        }

        public string addZero(int value) {
            return (value > 9) ? value.ToString() : "0" + value.ToString();
        }

        public void outputWrite(String input, int type = 0) {
            saveToolStripMenuItem.Enabled = true;
            listOutput.Items.Add(
                new celestMessage(
                    "[" + addZero(DateTime.Now.Hour) + ":" + addZero(DateTime.Now.Minute) + ":" + addZero(DateTime.Now.Second) + "] - " + input,
                    messageTextColours[type],
                    messageBackColours[type]
                )
            );
            /*
            if (type == 0) {
                listOutput.Items.Add(
                    new celestMessage(
                        "[" + addZero(DateTime.Now.Hour) + ":" + addZero(DateTime.Now.Minute) + ":" + addZero(DateTime.Now.Second) + "] - " + input,
                        new SolidBrush(Color.FromArgb(255, 41, 43, 44)),
                        new SolidBrush(Color.FromArgb(255, 247, 247, 247))
                    )
                );
            } else {
                listOutput.Items.Add(
                    new celestMessage(
                        "[" + addZero(DateTime.Now.Hour) + ":" + addZero(DateTime.Now.Minute) + ":" + addZero(DateTime.Now.Second) + "] - " + input,
                        messageTextColours[type],
                        messageBackColours[type]
                    )
                );
            }*/

            /*
            switch (type) {
                default:
                    // Normal
                    listOutput.Items.Add(new celestMessage("[" + addZero(DateTime.Now.Hour) + ":" + addZero(DateTime.Now.Minute) + ":" + addZero(DateTime.Now.Second) + "] - " + input));
                    break;
                case 1:
                    // Warning
                    listOutput.Items.Add(new celestMessage("[" + addZero(DateTime.Now.Hour) + ":" + addZero(DateTime.Now.Minute) + ":" + addZero(DateTime.Now.Second) + "] - " + input, ));
                    break;
                case 2:
                    // Error
                    listOutput.Items.Add(new celestMessage("[" + addZero(DateTime.Now.Hour) + ":" + addZero(DateTime.Now.Minute) + ":" + addZero(DateTime.Now.Second) + "] - " + input, new SolidBrush(Color.FromArgb(255, 217, 83, 79))));
                    break;
                case 3:
                    // Instance - Normal
                    listOutput.Items.Add(new celestMessage("[" + addZero(DateTime.Now.Hour) + ":" + addZero(DateTime.Now.Minute) + ":" + addZero(DateTime.Now.Second) + "] - " + input, new SolidBrush(Color.FromArgb(255, 38, 38, 38))));
                    break;
                case 4:
                    // Instance - Warning
                    listOutput.Items.Add(new celestMessage("[" + addZero(DateTime.Now.Hour) + ":" + addZero(DateTime.Now.Minute) + ":" + addZero(DateTime.Now.Second) + "] - " + input, new SolidBrush(Color.FromArgb(255, 201, 145, 66))));
                    break;
                case 5:
                    // Instance - Error
                    listOutput.Items.Add(new celestMessage("[" + addZero(DateTime.Now.Hour) + ":" + addZero(DateTime.Now.Minute) + ":" + addZero(DateTime.Now.Second) + "] - " + input, new SolidBrush(Color.FromArgb(255, 178, 67, 66))));
                    break;
            }
            */
            /* Hacky Code! */
            listOutput.TopIndex = listOutput.Items.Count - 1;
        }

        public void inputSend(string command, List<string> parameters = null) {
            if (command != "") {
                switch (textboxCommand.Text) {
                    case "memory": {
                            outputWrite("Command Executed: \"" + command + "\"");
                            Process[] processList = Process.GetProcessesByName("Runner");
                            if (processList.Length > 0) {
                                outputWrite("Runner.exe Memory Usage: " + ((processList[0].WorkingSet64 / 1024) / 1024).ToString() + " MB");
                            } else {
                                outputWrite("Wonder Wickets is not running.");
                            }
                            break;
                        }
                    default: {
                            outputWrite("Command Sent: \"" + command + "\"");
                            if (parameters != null) {
                                Celest.celestSend(command, parameters);
                            } else {
                                Celest.celestSend(command);
                            }
                            break;
                        }
                }
            }
        }

        private void macroReload() {
            macroCommands.Clear();
            int i = 0;
            foreach (string macroFile in Directory.GetFiles(mainPath + "macros")) {
                using (StreamReader macroRead = new StreamReader(macroFile)) {
                    ToolStripMenuItem macroButton = new ToolStripMenuItem();
                    macroButton.Text = macroRead.ReadLine();
                    macroButton.ToolTipText = macroRead.ReadLine();
                    macroButton.Click += macroItem_Click;
                    macrosToolStripMenuItem.DropDownItems.Add(macroButton);
                    macroCommands.Add(macroRead.ReadLine());
                }
            }
        }

        private void suggestCreate() {
            using (StreamWriter suggestWrite = new StreamWriter(mainPath + "suggest.txt")) {
                Array suggestListArray = sugestList.Split(';');
                foreach (String suggestGet in suggestListArray) {
                    suggestWrite.Write(suggestGet + ";");
                    textboxCommand.AutoCompleteCustomSource.Add(suggestGet);
                }
            }
        }

        private void suggestReload() {
            if (File.Exists(mainPath + "suggest.txt") == true) {
                using (StreamReader suggestRead = new StreamReader(mainPath + "suggest.txt")) {
                    Array suggestListArray = suggestRead.ReadToEnd().Split(';');
                    foreach (String suggestGet in suggestListArray) {
                        textboxCommand.AutoCompleteCustomSource.Add(suggestGet);
                    }
                }
            } else {
                suggestCreate();
            }
        }

        private void buttonSend_Click(object sender, EventArgs e) {
            inputSend(textboxCommand.Text);
            textboxCommand.Text = "";
        }

        private void listOutputClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                if (listOutput.SelectedIndices[0] != -1) {
                    for (var i = 4; i < listOutputContext.Items.Count; i++) {
                        listOutputContext.Items[i].Enabled = true;
                    }
                } else {
                    for (var i = 4; i < listOutputContext.Items.Count; i++) {
                        listOutputContext.Items[i].Enabled = false;
                    }
                }
                listOutputContext.Show(Cursor.Position);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            // Save Output
            if (listOutput.Items.Count > 0) {
                SaveFileDialog fileLocation = new SaveFileDialog();
                fileLocation.FileName = "CelestOutput.txt";
                fileLocation.Filter = "Text File (*.txt)|*.txt";
                if (fileLocation.ShowDialog() == DialogResult.OK) {
                    using (StreamWriter fileSave = new StreamWriter(fileLocation.FileName)) {
                        for (var i = 0; i < listOutput.Items.Count; i++) {
                            fileSave.WriteLineAsync(listOutput.Items[i].ToString());
                        }
                    }
                }
            }

        }

        private void clearToolStripMenuItem2_Click(object sender, EventArgs e) {
            // Clear Output
            listOutput.Items.Clear();
            saveToolStripMenuItem.Enabled = false;
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e) {
            // Copy
            if (this.Enabled == true) {
                Clipboard.SetText((listOutput.Items[listOutput.SelectedIndices[0]] as celestMessage).text.ToString());
            }
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e) {
            // Delete
            if (this.Enabled == true) {
                listOutput.Items.RemoveAt(listOutput.SelectedIndices[0]);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e) {
            // Cut
            if (this.Enabled == true) {
                Clipboard.SetText((listOutput.Items[listOutput.SelectedIndices[0]] as celestMessage).text.ToString());
                listOutput.Items.RemoveAt(listOutput.SelectedIndices[0]);
            }
        }

        private void textboxSend(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                inputSend(textboxCommand.Text);
                textboxCommand.Text = "";
            }
        }

        private void listOutputRightclick(object sender, MouseEventArgs e) {
            //listOutput.SelectedIndices[0] = listOutput(e.X, e.Y);
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e) {
            ToolStripItemCollection macroMenuItems = macrosToolStripMenuItem.DropDownItems;
            while (macroMenuItems.Count > 2) {
                macroMenuItems.RemoveAt(macroMenuItems.Count - 1);
            }
            macroReload();
        }

        private void macroItem_Click(object sender, EventArgs e) {
            int macroIndex = macrosToolStripMenuItem.DropDownItems.IndexOf(sender as ToolStripItem);
            if (macroIndex != -1) {
                inputSend(macroCommands[macrosToolStripMenuItem.DropDownItems.IndexOf(sender as ToolStripItem) - 2]);
            }
        }

        private void listOutput_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index > -1) {
                celestMessage celestMessageGet = ((sender as ListBox).Items[e.Index] as celestMessage);
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 91, 192, 222)), e.Bounds);
                    e.Graphics.DrawString(celestMessageGet.text, (sender as ListBox).Font, Brushes.White, new Point(e.Bounds.X, e.Bounds.Y));
                } else {
                    e.Graphics.FillRectangle(celestMessageGet.backcol, e.Bounds);
                    e.Graphics.DrawString(celestMessageGet.text, (sender as ListBox).Font, celestMessageGet.textcol, new Point(e.Bounds.X, e.Bounds.Y));
                }
                //e.Graphics.DrawString((listOutput.Items[e.Index] as celestMessage).text.ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds);
                //e.DrawFocusRectangle();
                /*
                celestMessage celestMessageGet = ((sender as ListBox).Items[e.Index] as celestMessage);
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
                    e.Graphics.FillRectangle(Brushes.SkyBlue, e.Bounds);
                } else {
                    e.Graphics.FillRectangle((e.Index % 2 == 0) ? new SolidBrush(Color.FromArgb(255, 255, 255, 255)) : new SolidBrush(Color.FromArgb(255, 239, 240, 241)), e.Bounds);
                }
                if (celestMessageGet.color.Color != Color.Black) {
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(96, celestMessageGet.color.Color)), e.Bounds);
                }
                e.Graphics.DrawString(celestMessageGet.text, (sender as ListBox).Font, new SolidBrush(Color.Black), new Point(e.Bounds.X, e.Bounds.Y));
                e.DrawFocusRectangle();
                */
            }
        }

        private void mainForm_Load(object sender, EventArgs e) {

        }
    }
}
