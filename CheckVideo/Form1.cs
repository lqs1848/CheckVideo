using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AvCoverDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dilog.Description = "请选择识别路径";
            DialogResult res = dilog.ShowDialog();
            if (res == DialogResult.OK || res == DialogResult.Yes)
            {
                textBox1.Text = dilog.SelectedPath;
            }
        }


        List<Av> avs;

        private void button2_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox1.Text))
            {
                MessageBox.Show("请先选择路径");
                return;
            }
            avs = new List<Av>();
            textBox2.AppendText("迭代目录中\r\n");
            Director(textBox1.Text);
            textBox2.AppendText("开始校验\r\n");
            Collector c = new Collector(SynchronizationContext.Current, avs);
            c.CollectorLog += (o, text) =>
            {
                textBox2.AppendText(text + "\r\n");
            };
            c.ErrorLog += (o, text) =>
            {
                textBox3.AppendText(text + "\r\n");
            };
            c.Start();
        }//method

        string extNames = ".ts.tp.m2ts.tod.m2t.mts.avi.mov.mpg.mpeg.divx.RA.RM.RMVB.WMV.mkv.mp4.asf.m4v.vob.flv";

        void Director(string dir)
        {
            DirectoryInfo fdir = new DirectoryInfo(dir);
            FileSystemInfo[] fsinfos = fdir.GetFileSystemInfos();
            foreach (FileSystemInfo fsinfo in fsinfos)
            {
                if (fsinfo is DirectoryInfo)     //判断是否为文件夹
                {
                    Director(fsinfo.FullName);//递归调用
                }
                else
                {
                    if (extNames.ToUpper().IndexOf(fsinfo.Extension.ToUpper()) != -1)
                    {//视频文件
                        FileInfo ff =  new System.IO.FileInfo(fsinfo.FullName);
                        //太小的视频文件 判断为损坏跳过
                        if (ff.Length < 1024) {
                            textBox3.AppendText(fsinfo.Name + ":"+ff.Length+"\r\n");
                            ExeLog.WriteLog("nolist.txt", fsinfo.Name + "\r\n");
                            continue;
                        }
                        Av av = new Av();
                        av.Path = fsinfo.FullName;
                        avs.Add(av);
                        textBox2.AppendText(fsinfo.Name + "\r\n");
                    }//if 视频文件
                }//if
            }//foreach
        }//method



    }
}
