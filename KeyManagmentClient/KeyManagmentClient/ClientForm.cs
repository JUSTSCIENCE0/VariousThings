using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Numerics;

namespace KeyManagmentClient
{
    public partial class ClientForm : Form
    {
        Socket socket;
        Thread Network;
        BigInteger N, S;
        DiffieHalman DH;
        ARCFOUR arc4 = new ARCFOUR();
        bool friendsChanged = false;
        bool messageChanged = false;
        bool needClear = false;
        string my_name;
        NumberGenerator numGen;
        Random rnd;

        List<TrustUser> friends = new List<TrustUser>();

        long Timestamp = 0;
        string P_key = "";
        string G_key = "";

        public ClientForm()
        {
            N = 0; S = 0;
            rnd = new Random();
            InitializeComponent();
        }

        private void Handler()
        {
            while (true)
            {
                byte[] data = new byte[1];

                try
                {
                    if (socket.Available <= 0)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    socket.Receive(data);
                }
                catch (SystemException)
                {
                    return;
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message);
                }

                if (data[0] == 100)
                {
                    try
                    {
                        UpdateTimestamp();
                    }
                    catch(Exception exp)
                    {
                        MessageBox.Show(exp.Message);
                    }
                }
                if (data[0] == 101)
                {
                    try
                    {
                        UpdateDH();
                    }
                    catch (Exception exp)
                    {
                        MessageBox.Show(exp.Message);
                    }
                }
                if (data[0] == 102)
                {
                    try
                    {
                        RecvPK();
                    }
                    catch (Exception exp)
                    {
                        MessageBox.Show(exp.Message);
                    }
                }
                if (data[0] == 120)
                {
                    try
                    {
                        RecvMsg();
                    }
                    catch (Exception exp)
                    {
                        MessageBox.Show(exp.Message);
                    }
                }
                if (data[0] == 220)
                {
                    try
                    {
                        DeleteFriend();
                    }
                    catch (Exception exp)
                    {
                        MessageBox.Show(exp.Message);
                    }
                }
            }
        }

        private void UpdateDH()
        {
            byte[] data = new byte[2];
            socket.Receive(data);
            short P_size = BitConverter.ToInt16(data, 0);

            byte[] p = new byte[P_size];
            socket.Receive(p, P_size, SocketFlags.None);

            socket.Receive(data, 1, SocketFlags.None);
            byte G_size = data[0];

            byte[] g = new byte[G_size];
            socket.Receive(g, G_size, SocketFlags.None);

            BigInteger pInt = new BigInteger(p);
            BigInteger gInt = new BigInteger(g);

            P_key = pInt.ToString();
            G_key = gInt.ToString();

            DH = new DiffieHalman(pInt, gInt);

            short A_size = (short)DH.A.ToByteArray().Length;
            data = BitConverter.GetBytes(A_size);
            socket.Send(data);

            byte[] A = DH.A.ToByteArray();
            socket.Send(A);
        }

        private void UpdateTimestamp()
        {
            byte[] data = new byte[8];
            socket.Receive(data);
            Timestamp = BitConverter.ToInt64(data, 0);
        }

        private void RecvPK()
        {
            byte[] data = new byte[2];
            socket.Receive(data, 2, SocketFlags.None);
            short size = BitConverter.ToInt16(data, 0);

            data = new byte[size];
            socket.Receive(data, size, SocketFlags.None);
            string name = Encoding.Unicode.GetString(data);

            data = new byte[2];
            socket.Receive(data, 2, SocketFlags.None);
            size = BitConverter.ToInt16(data, 0);

            data = new byte[size];
            socket.Receive(data, size, SocketFlags.None);
            BigInteger pk = new BigInteger(data);

            TrustUser user = new TrustUser(name, pk, DH.p, DH.a);

            friends.Add(user);
            friendsChanged = true;
        }

        private void RecvMsg()
        {
            byte[] row_size = new byte[4];
            socket.Receive(row_size, 4, SocketFlags.None);
            int size = BitConverter.ToInt32(row_size, 0);

            byte[] row_sender = new byte[size];
            socket.Receive(row_sender, size, SocketFlags.None);
            string sender = Encoding.Unicode.GetString(row_sender);

            row_size = new byte[4];
            socket.Receive(row_size, 4, SocketFlags.None);
            size = BitConverter.ToInt32(row_size, 0);

            byte[] msg = new byte[size];
            socket.Receive(msg, size, SocketFlags.None);

            TrustUser user = friends.Find(x => x.login == sender);
            arc4.NewKey(user.Key.ToByteArray(), Timestamp);

            byte[] decr_msg = arc4.ConjunctionWithRC(msg);
            TrustMessage new_msg = new TrustMessage();
            new_msg.Out = true;
            new_msg.Text = Encoding.Unicode.GetString(decr_msg);

            int id_user = friends.FindIndex(x => x.login == sender);
            friends[id_user].messages.Add(new_msg);

            messageChanged = true;
        }

        private bool Registration()
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(Host.Text), Convert.ToInt32(Port.Text));

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipPoint);

            byte[] data = { 100 };
            socket.Send(data);

            data = BitConverter.GetBytes(Encoding.Unicode.GetBytes(Login.Text).Length);
            socket.Send(data);
            data = Encoding.Unicode.GetBytes(Login.Text);
            socket.Send(data);

            data = new byte[1];
            socket.Receive(data);

            if (data[0] == 255)
            {
                socket.Close();
                Login.Text = "";
                throw new Exception("Клиент с таким логином уже активен в системе!");
            } else if (data[0] == 254)
            {
                socket.Close();
                Login.Text = "";
                throw new Exception("Клиент с таким логином уже зарегистрирован в системе!");
            } else if (data[0] == 200)
            {
                LogConsole.Text += "Received server answer: Ready for registration" + Environment.NewLine;
                LogConsole.Text += "Start Generate 1024-bit p and q" + Environment.NewLine;
                LogConsole.Text += "It will take a while..." + Environment.NewLine;
                LogConsole.Update();
                numGen = new NumberGenerator(ref LogConsole);

                //save private key in file
                LogConsole.Text += "Save private key in file" + Environment.NewLine;
                LogConsole.Update();
                StreamWriter sw = new StreamWriter(File.OpenWrite("D:/keys/" + Login.Text));
                sw.WriteLine(numGen.S.ToString());
                sw.WriteLine(numGen.N.ToString());
                sw.Flush();
                sw.Close();

                //send public key to server
                LogConsole.Text += "Send public key to server" + Environment.NewLine;
                LogConsole.Update();
                //send announce
                data[0] = 200;
                socket.Send(data);
                //send size V
                byte[] row_size = BitConverter.GetBytes(numGen.V.ToByteArray().Length);
                socket.Send(row_size);
                //send V
                byte[] num_data = numGen.V.ToByteArray();
                socket.Send(num_data);
                //send size N
                row_size = BitConverter.GetBytes(numGen.N.ToByteArray().Length);
                socket.Send(row_size);
                //send N
                num_data = numGen.N.ToByteArray();
                socket.Send(num_data);

                //recv server answer
                data[0] = 0;
                socket.Receive(data, 1, SocketFlags.None);
                if (data[0] != 200)
                {
                    LogConsole.Text += "Server send wrong answer!" + Environment.NewLine;
                    return false;
                }

                return true;
            }

            return false;
        }

        private bool AuthRound()
        {
            byte[] data = new byte[1];
            socket.Receive(data, 1, SocketFlags.None);
            if (data[0] == 200)
            {
                int size = N.ToByteArray().Length;
                byte[] rand = new byte[size - 1];
                rnd.NextBytes(rand);
                BigInteger r = new BigInteger(rand);
                if (r < 0) r = -r;
                BigInteger z = BigInteger.ModPow(r, 2, N);

                LogConsole.Text += "Random r = " + r.ToString() + Environment.NewLine;
                LogConsole.Text += "z = r*r % n = " + z.ToString() + Environment.NewLine;

                byte[] row_size = BitConverter.GetBytes(z.ToByteArray().Length);
                socket.Send(row_size);

                byte[] row_z = z.ToByteArray();
                socket.Send(row_z);

                byte[] bit = new byte[1];
                socket.Receive(bit, 1, SocketFlags.None);

                if (bit[0] == 0)
                {
                    LogConsole.Text += "Server bit = 0, server want r" + Environment.NewLine;
                    row_size = BitConverter.GetBytes(r.ToByteArray().Length);
                    socket.Send(row_size);

                    byte[] row_r = r.ToByteArray();
                    socket.Send(row_r);
                }
                else
                {
                    LogConsole.Text += "Server bit = 1, server want y" + Environment.NewLine;
                    BigInteger y = (r * S) % N;
                    LogConsole.Text += "y = (r * S) % N = " + y.ToString() + Environment.NewLine;

                    row_size = BitConverter.GetBytes(y.ToByteArray().Length);
                    socket.Send(row_size);

                    byte[] row_y = y.ToByteArray();
                    socket.Send(row_y);
                }

                bit = new byte[1];
                socket.Receive(bit, 1, SocketFlags.None);
                if (bit[0] == 200)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        private bool Authorization()
        {
            StreamReader keys = new StreamReader(File.OpenRead("D:/keys/" + Login.Text));
            string Stxt = keys.ReadLine();
            string Ntxt = keys.ReadLine();

            S = BigInteger.Parse(Stxt);
            N = BigInteger.Parse(Ntxt);

            byte[] data = new byte[1];
            socket.Receive(data, 1, SocketFlags.None);

            if(data[0] == 254)
            {
                socket.Close();
                Login.Text = "";
                throw new Exception("На сервере не найдено ключей для данного пользователя!");
            }
            if(data[0] == 200)
            {
                for(int i=0; i<16; i++)
                {
                    LogConsole.Text += "Round of authorization №" + (i + 1).ToString() + Environment.NewLine;
                    if (AuthRound())
                    {
                        LogConsole.Text += "Round of authorization №" + (i + 1).ToString() + " - Success!" + Environment.NewLine;
                        LogConsole.Update();
                    }
                    else
                    {
                        LogConsole.Text += "Round of authorization №" + (i + 1).ToString() + " - Failed!" + Environment.NewLine;
                        return false;
                    }
                }
            }

            return true;
        }

        private void butConnect_Click(object sender, EventArgs e)
        {
            try
            {
                /*IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(Host.Text), Convert.ToInt32(Port.Text));

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipPoint);
                */

                if (File.Exists("D:/keys/" + Login.Text))
                {
                    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(Host.Text), Convert.ToInt32(Port.Text));

                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(ipPoint);

                    byte[] data = { 200 };
                    socket.Send(data);

                    data = BitConverter.GetBytes(Encoding.Unicode.GetBytes(Login.Text).Length);
                    socket.Send(data);
                    data = Encoding.Unicode.GetBytes(Login.Text);
                    socket.Send(data);

                    data = new byte[1];
                    socket.Receive(data);

                    if (data[0] == 255)
                    {
                        socket.Close();
                        Login.Text = "";
                        throw new Exception("Клиент с таким логином уже активен в системе!");
                    }
                    else if (data[0] == 200)
                    {
                        if (Authorization())
                        {
                            panAuth.Visible = false;
                            panWork.Visible = true;

                            Network = new Thread(Handler);
                            Network.Start();

                            this.Text = "Клиент " + Login.Text;
                            my_name = Login.Text;
                        }
                        else
                        {
                            throw new Exception("Авторизация провалена!");
                        }
                    }
                    return;
                }
                else
                {
                    DialogResult dialogResult = MessageBox.Show(
                        "Зарегистрироваться от имени введенного пользователя?",
                        "Ключ авторизации для данного пользователя не найден!", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        if (Registration())
                        {
                            if (Authorization())
                            {
                                panAuth.Visible = false;
                                panWork.Visible = true;

                                Network = new Thread(Handler);
                                Network.Start();

                                this.Text = "Клиент " + Login.Text;
                                my_name = Login.Text;
                            }
                            else
                            {
                                throw new Exception("Авторизация провалена!");
                            }
                        }
                        else
                        {
                            throw new Exception("Не удалось зарегистрировать пользователя");
                        }
                        return;
                    }
                    else
                    {
                        Login.Text = "";
                        return;
                    }
                }

                /*
                byte[] data = BitConverter.GetBytes(Encoding.Unicode.GetBytes(Login.Text).Length);
                socket.Send(data);
                data = Encoding.Unicode.GetBytes(Login.Text);
                socket.Send(data);

                data = new byte[1];
                socket.Receive(data);

                if (data[0] == 200)
                {
                    panAuth.Visible = false;
                    panWork.Visible = true;

                    Network = new Thread(Handler);
                    Network.Start();

                    this.Text = "Клиент " + Login.Text;
                    my_name = Login.Text;
                }
                else if (data[0] == 255)
                {
                    socket.Close();
                    throw new Exception("Логин занят, выберите другой");
                }
                */
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
                return;
            }
        }

        private void Redraw_Tick(object sender, EventArgs e)
        {
            DateTime date = new DateTime(Timestamp);
            labTimestamp.Text = "Время сервера = " + date.ToString("HH:mm   dd/MM/yyyy") +
                " = " + Timestamp.ToString();
            labPG.Text = "p = " + P_key + Environment.NewLine + "g = " + G_key;

            if (friendsChanged)
            {
                listUsers.Items.Clear();

                for(int i=0; i<friends.Count; i++)
                {
                    listUsers.Items.Add(friends[i].login);
                }

                friendsChanged = false;
            }

            if (messageChanged)
            {
                RedrawMessages();
            }

            if (needClear)
            {
                listUsers.Items.Clear();

                for (int i = 0; i < friends.Count; i++)
                {
                    listUsers.Items.Add(friends[i].login);
                }

                needClear = false;

                MessageHistory.Text = "";
                MessageBox.Show("Пользователь покинул систему!");
            }
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Network != null)
                Network.Abort();
        }

        private void listUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listUsers.SelectedItem == null)
                return;
            string cur_name = listUsers.SelectedItem.ToString();

            TrustUser user = friends.Find(x => x.login == cur_name);
            if (user == null)
                return;

            labKey.Text = user.Key.ToString();
            RedrawMessages();
        }

        private void SendMessage_Click(object sender, EventArgs e)
        {
            if (listUsers.SelectedItem == null)
                return;

            string cur_name = listUsers.SelectedItem.ToString();
            TrustUser user = friends.Find(x => x.login == cur_name);
            if (user == null)
                return;

            byte[] text = Encoding.Unicode.GetBytes(NewMessage.Text);
            byte[] address = Encoding.Unicode.GetBytes(cur_name);

            arc4.NewKey(user.Key.ToByteArray(), Timestamp);
            byte[] encrypted = arc4.ConjunctionWithRC(text);

            byte[] data = { 110 };
            socket.Send(data);

            data = BitConverter.GetBytes(address.Length);
            socket.Send(data);
            socket.Send(address);

            data = BitConverter.GetBytes(encrypted.Length);
            socket.Send(data);
            socket.Send(encrypted);

            TrustMessage msg = new TrustMessage();
            msg.Out = false;
            msg.Text = NewMessage.Text;

            int id_user = friends.FindIndex(x => x.login == cur_name);
            friends[id_user].messages.Add(msg);

            RedrawMessages();
        }

        private void RedrawMessages()
        {
            if (listUsers.SelectedItem == null)
            {
                messageChanged = false;
                return;
            }
            string cur_name = listUsers.SelectedItem.ToString();
            TrustUser user = friends.Find(x => x.login == cur_name);
            if (user == null)
                return;

            MessageHistory.Text = "";

            foreach(TrustMessage msg in user.messages)
            {
                if (msg.Out)
                {
                    MessageHistory.Text += user.login + ": ";
                }
                else
                {
                    MessageHistory.Text += my_name + ": ";
                }
                MessageHistory.Text += msg.Text + Environment.NewLine;
            }

            messageChanged = false;
        }

        private void DeleteFriend()
        {
            byte[] row_size = new byte[4];
            socket.Receive(row_size, 4, SocketFlags.None);
            int size = BitConverter.ToInt32(row_size, 0);

            byte[] row_friend = new byte[size];
            socket.Receive(row_friend, size, SocketFlags.None);
            string friend = Encoding.Unicode.GetString(row_friend);

            friends.RemoveAll(x => x.login == friend);

            needClear = true;

        }
    }
}
