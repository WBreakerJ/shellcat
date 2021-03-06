﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShellCat
{
    public partial class ShellTab : UserControl
    {
        public RichTextBox RtbShell;
        private readonly RemoteClient _client;
        private int _oldLength = 0;
        public bool ConnectionLost = false;

        public ShellTab(RemoteClient client)
        {
            InitializeComponent();
            _client = client;
            RtbShell = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen,
            };
            try
            {
                RtbShell.Font = new Font("Consolas", 10.5f);
            } 
            catch
            {

            }
            
            RtbShell.SelectionFont = new Font(RtbShell.Font.FontFamily, 10.5f);
            this.Controls.Add(RtbShell);
            RtbShell.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RtbShell_KeyUp);
            //RtbShell.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RtbShell_KeyUp);
            RtbShell.TextChanged += new System.EventHandler(this.RtbShell_TextChanged);
        }


        public void AppendText(string content)
        {
            // 对于该控件的请求来自于创建该控件所在线程以外的线程
            if (RtbShell.InvokeRequired)
            {
                var rtbSet = new DelegateRichTextBox(delegate(RichTextBox tb, string cnt)
                {
                    // 设置插入点的字体
                    tb.SelectionFont = new Font(tb.Font.FontFamily, 10.5f);
                    tb.AppendText(cnt);
                    // 控制最大行数
                    Utils.RichTextBoxMaxLineControl(tb);
                    tb.ScrollToCaret(); //让滚动条拉到最底处   
                    _oldLength = tb.TextLength; // 获取richtextbox中已有内容长度
                });
                RtbShell.Invoke(rtbSet, RtbShell, content);
            }
            else
            {
                // 设置插入点的字体
                RtbShell.SelectionFont = new Font(RtbShell.Font.FontFamily, 10.5f);
                RtbShell.AppendText(content);
                // 控制最大行数
                Utils.RichTextBoxMaxLineControl(RtbShell);
                RtbShell.ScrollToCaret(); //让滚动条拉到最底处  
                _oldLength = RtbShell.TextLength; // 获取richtextbox中已有内容长度 
            }
        }

        private delegate void DelegateRichTextBox(RichTextBox textBox, string content);

        /// <summary>
        /// 往回删除的时候，如果发现少于上次接收完成的文本长度，就禁止再继续删除，自动撤销删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RtbShell_TextChanged(object sender, EventArgs e)
        {
            if (RtbShell.TextLength < _oldLength || RtbShell.SelectionStart < RtbShell.TextLength)
            {
                RtbShell.Undo();
            }
        }

        private void RtbShell_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!ConnectionLost)
                {
                    var cmd = RtbShell.Text.Substring(_oldLength);
                    if (cmd.Trim().Equals(""))
                    {
                        RtbShell.AppendText("\n$ ");
                        _oldLength = RtbShell.TextLength; // 获取richtextbox中已有内容长度 
                    } else
                    {
                        _oldLength = RtbShell.TextLength;
                        _client.SendMessage(cmd);
                    }

                }
            }
        }
    }
}