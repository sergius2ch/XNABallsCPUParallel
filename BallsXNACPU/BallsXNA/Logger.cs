using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace BallsXNA
{
    class Logger
    {
        #region поля
        private string path;
        private string bpath;
        private FileStream fs;
        private StreamWriter sw;
        private int counter;
        #endregion
        public Logger(string path)
        {
            this.path = path;
            DateTime start = DateTime.Now;
            string fname = string
                .Format(
                    "{0}_{1}.log",
                    start.ToShortDateString(),
                    start.ToLongTimeString().Replace(':','-')
                );
            fname = Path.Combine(path, fname);
            fs = new FileStream(fname, FileMode.Create);
            sw = new StreamWriter(fs);
            counter = 0;
            AddLine("start");
            bpath = Path.Combine(path, "Balls");
            DirectoryInfo di = new DirectoryInfo(bpath);
            if (!di.Exists) di.Create();
            FileInfo[] files = di.GetFiles();
            foreach (FileInfo fi in files)
            {
                fi.Delete();
            }
        }

        public void Save(Ball[] balls)
        {
            counter++;
            string fname = string.Format("{0:D5}_balls.log", counter);
            fname = Path.Combine(bpath, fname);
            var fsc = new FileStream(fname, FileMode.Create);
            XmlSerializer xs =
                new XmlSerializer(typeof(Ball[]));
            xs.Serialize(fsc, balls);
            fsc.Close();
        }
        public void Add(string format, params object[] args)
        {
            sw.Write(format, args);
            sw.Flush();
        }
        public void AddLine(string format, params object[] args)
        {
            sw.WriteLine(format, args);
            sw.Flush();
        }
        public void Add(string msg)
        {
            sw.Write(msg);
            sw.Flush();
        }
        public void AddLine(string msg)
        {
            sw.WriteLine(msg);
            sw.Flush();
        }
        ~Logger()
        {
            /*sw.WriteLine();
            sw.WriteLine("Done.");
            sw.Close();*/
            fs.Close();
        }
    }
}
