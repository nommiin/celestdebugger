using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace CelestDebugger {
    public class Celest {
        public enum celestHeader {
            PRINT
        };

        public static mainForm celestForm;
        public static UdpClient celestSocket;
        public static int celestPort = 15000;
        public static int celestTargetPort = 15001;

        public static void celestWrite(String text, int type = 0) {
            celestForm.BeginInvoke((Action<string, int>)celestForm.outputWrite, text, type);
        }

        public static void celestSend(String command, List<string> parameters = null) {
            if (celestTargetPort != -1) {
                MemoryStream packetData = new MemoryStream();
                using (BinaryWriter packetWrite = new BinaryWriter(packetData)) {
                    packetWrite.Write(command.ToArray<char>());
                    packetWrite.Write(0x00);
                    if (parameters != null) {
                        foreach(string parameter in parameters) {
                            packetWrite.Write(parameter);
                            packetWrite.Write(0x00);
                        }
                    }
                    celestSocket.Send(packetData.ToArray(), (int)packetWrite.BaseStream.Position, new IPEndPoint(IPAddress.Parse("127.0.0.1"), celestTargetPort));
                }
            } else {
                celestWrite("CelestDebugger module has not been initalized.");
            }
        }

        public static void celestRecieve(IAsyncResult result) {
            UdpClient celestSocket = result.AsyncState as UdpClient;
            IPEndPoint celestSource = new IPEndPoint(0, 0);
            try {
                using (BinaryReader packetRead = new BinaryReader(new MemoryStream(celestSocket.EndReceive(result, ref celestSource)))) {
                    switch (packetRead.ReadByte()) {
                        case (byte)celestHeader.PRINT: {
                                String celestPrint = "";
                                int celestType = packetRead.ReadByte();
                                while (packetRead.PeekChar() != 0x00) {
                                    celestPrint += packetRead.ReadChar();
                                }
                                celestWrite(celestPrint, celestType);
                                break;
                            }

                        default: {
                                packetRead.BaseStream.Seek(0, SeekOrigin.Begin);
                                celestWrite("An unknown packet has been recieved [Header: " + packetRead.ReadByte().ToString() + "]");
                                break;
                            }
                    }
                }
            } catch (System.Net.Sockets.SocketException socketError) {
                // Ignore!!
                //celestWrite("Socket Exception: " + socketError.Message);
            }
            
            celestSocket.BeginReceive(new AsyncCallback(Celest.celestRecieve), celestSocket);
        }
    }
}
