using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * Application created by: XKrowlerX http://leakforums.org/user-294351
 * Version: 1.0
 * Website: Krowler.tk
 **/


namespace Steam_Friend_Message_Broadcaster
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://leakforums.org");
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://krowler.tk");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://leakforums.org/user-294351");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SteamWorker worker = new SteamWorker();
            worker.ParseSteamCookies();
            if (worker.ParsedSteamCookies.Count > 0)
            {
                worker.getSessionID();
                worker.initChatSystem();
                worker.getFriends();
                worker.sendMessageToFriends(textBox1.Text);
            }
        }
       
    }
}
