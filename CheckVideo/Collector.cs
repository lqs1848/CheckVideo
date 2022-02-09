using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AvCoverDownloader
{
    class Collector
    {
        public EventHandler<String> CollectorLog;
        public EventHandler<String> ErrorLog;
        SynchronizationContext _syncContext;
        //HttpClient client;

        //线程数
        const int cycleNum = 1;

        private Queue<Av> _avs;
        
        public Collector(SynchronizationContext formContext, List<Av> avs)
        {
            _avs = new Queue<Av>(avs);
            _syncContext = formContext;
        }

        public void Start()
        {
            //client = new HttpClient();
            new Thread(Collect).Start();
        }

        public void Collect()
        {
            ThreadPool.SetMinThreads(cycleNum, cycleNum);
            ThreadPool.SetMaxThreads(cycleNum, cycleNum);
            for (int i = 1; i <= cycleNum; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadFun));
            }
            
        }//method 

        public void DownloadFun(object o)
        {
            while(_avs.Count > 0)
            {
                Av a;
                try
                {
                    a = _avs.Dequeue();
                }
                catch (InvalidOperationException e) {
                    return;
                }
                if(a!=null) Ffmpeg(a.Path);
                
            }//while
            _syncContext.Post(OutLog, "线程退出");
        }//method

        public void Ffmpeg(string filePath)
        {
            //string path = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "temp";
            string name = Path.GetFileName(filePath);
            //if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string tempFile = filePath + ".checkVideo";
            string strArg = "-ss 00:00:01 -i \"" + filePath + "\" -y -f mjpeg -t 0.001 -s 1x1 \"" + tempFile +"\"";
            //string strArg = "-ss 00:00:01 -i \"S:\\test\\1.mkv\" -y \"S:\\test\\1.jpg\"";
            strArg = strArg.Replace("\\\\", "\\");
            ffmpeg(strArg);

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
                _syncContext.Post(OutLog, name);
                return;
            }
            //ffmpeg -ss 00:50:00  -i RevolutionOS.rmvb sample.jpg  -r 1 -vframes 1 -an -vcodec mjpeg
            //strArg = "-ss 00:00:01 -i \"" + filePath + "\" -y -f mjpeg -s 1x1 \"" + path + "\\" + name + ".jpg\"";
            //strArg = "-ss 00:00:30 -i \"" + filePath + "\" \"" + path + "\\" + name + ".jpg\" -s 1x1 -r 1 -vframes 1 -an -vcodec mjpeg";
            //"-ss 00:00:01 - i S:\test\1.mkv - f image2 - y S:\test\1.jpg"
            //strArg = "-ss 1 -i \"" + filePath + "\" -y -f image2 -t 0.001 -s 1x1 \"" + path + "\\" + name + ".jpg\"";
            //ffmpeg(strArg);

            Thread.Sleep(10000);

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
                _syncContext.Post(OutLog, name);
                return;
            }

            ExeLog.WriteLog("nolist.txt",name+"\r\n");
            _syncContext.Post(OutError, name);
            
        }

        private void ffmpeg(string strArg) {
            /*
            Process p = new Process();//建立外部调用线程
            p.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\DLL\\ffmpeg.exe";//要调用外部程序的绝对路径
            p.StartInfo.Arguments = strArg;
            p.StartInfo.UseShellExecute = false;//不使用操作系统外壳程序启动线程(一定为FALSE,详细的请看MSDN)
            p.StartInfo.RedirectStandardError = false;//把外部程序错误输出写到StandardError流中(这个一定要注意,FFMPEG的所有输出信息,都为错误输出流,用StandardOutput是捕获不到任何消息的...这是我耗费了2个多月得出来的经验...mencoder就是用standardOutput来捕获的)
            p.StartInfo.CreateNoWindow = true;//不创建进程窗口
            //p.ErrorDataReceived += new DataReceivedEventHandler(Output);//外部程序(这里是FFMPEG)输出流时候产生的事件,这里是把流的处理过程转移到下面的方法中,详细请查阅MSDN
            p.Start();//启动线程
            //p.BeginErrorReadLine();//开始异步读取
            p.WaitForExit(5000);//阻塞等待进程结束
            p.Close();//关闭进程
            p.Dispose();//释放资源
            */
            
            try
            {
                using (Process p = new Process())
                {
                    p.StartInfo.Arguments = strArg;
                    p.StartInfo.RedirectStandardError = false;//把外部程序错误输出写到StandardError流中(这个一定要注意,FFMPEG的所有输出信息,都为错误输出流,用StandardOutput是捕获不到任何消息的...这是我耗费了2个多月得出来的经验...mencoder就是用standardOutput来捕获的)
                    p.StartInfo.CreateNoWindow = true;//不创建进程窗口
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\DLL\\ffmpeg.exe";//要调用外部程序的绝对路径
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    p.WaitForExit(5 * 1000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        private void Output(object sendProcess, DataReceivedEventArgs output)
        {
            if (!String.IsNullOrEmpty(output.Data))
            {
                //处理方法...
                Console.WriteLine(output.Data);
            }
        }

        private void OutLog(object state)
        {
            CollectorLog?.Invoke(this, state.ToString());
        }

        private void OutError(object state)
        {
            ErrorLog?.Invoke(this, state.ToString());
        }

    }//class
}
