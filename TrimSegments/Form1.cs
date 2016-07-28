using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TrimSegments
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            listBox1.Items.Clear();
            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string filename in FileList)
            {
                listBox1.Items.Add(filename);
                break; //the break here will force array to stop filling after one file from the drag and drop //TODO??
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox3.Text = "";  // reset any text in the displayed ffmpeg arguments box
            string fname = listBox1.Items[0].ToString(); // get the file name dragged into the program
            FileInfo fileInfo = new FileInfo(fname);  // get info about the file
            
            string ext = fileInfo.Extension;
            string dirpath = fileInfo.DirectoryName;
            var dir = new DirectoryInfo(dirpath);
            string path = dirpath.ToString() + @"\list.txt";


            /* 
             * ffmpeg concat muxer takes an input file of list of videos to concatenate
             *      delete that file if it exists and initialize a blank one
             */
            if (File.Exists(path))
            {
                File.Delete(path);
                string[] createText = { };
                File.WriteAllLines(path, createText, Encoding.Default);
            }

            /*
             * we are using the -report argment in ffmpeg, which generates *(timestamp)*.log
             *     files within the dir containing ffmpeg, which should also contain this program
             */
            var appdir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            foreach (var file in appdir.EnumerateFiles("*.log"))
            {
                file.Delete();
            }

            /*
             *  Now that we have the video file to process, begin main loop:
             *     - gather and parse all the time segments listed
             *     - trim each segment out of the main video
             *     - append each trimmed segment filename to list.txt
             */
            int count = 0;
            List<string> segments = textBox2.Text.Split('\n').ToList();
            foreach (var segment in segments)
            {
                
                string start = segment.Split('-')[0].Trim();
                string end   = segment.Split('-')[1].Trim();
                count++;

                string appendText = "";

                // string trim(): returns the generated filename
                appendText = trim(start, end, count, ext);
                textBox3.Text = textBox3.Text + "Created: " + appendText + Environment.NewLine + Environment.NewLine;

                // ffmpeg expects format: file 'filename.ext' - per line
                appendText = "file '" + appendText + "'";
                appendText = appendText.Trim();           
                appendText = appendText+Environment.NewLine;

                // encoding = default - ffmpeg had problems? with utf8 7/28/2016
                File.AppendAllText(path, appendText, Encoding.Default);
                
            }

            if (checkBox3.Checked == true)  // concatenate segment files into one
            {
                string newfile = concat(path,ext);
                textBox3.Text = textBox3.Text + "Created: " + newfile + Environment.NewLine + Environment.NewLine; 
            }

            if (checkBox1.Checked == true)  // delete source file
            {
                File.Delete(fname);
                textBox3.Text = textBox3.Text + "Deleted: " + fname + Environment.NewLine + Environment.NewLine;
            }
            if (checkBox2.Checked == true)  // delete segment (part) files
            {
                foreach (var file in dir.EnumerateFiles("*._part*"))
                {
                    file.Delete();
                    textBox3.Text = textBox3.Text + "Deleted: " + file.ToString() + Environment.NewLine;
                }
            }

        }

        /*
         *  Main file segmenting function
         *  Returns: filename of generated file
         *  
         *    - generate args:
         *      -- get input file name and use it to build first part of argument string, arg1
         *      -- get seekstart (start) and timeoutput (out) times
         *      -- get input file name and add a part and count identifier for new filename
         *    - print arguments to program arg box (textbox3)
         *    - run ffmpeg
         */
        private string trim(string start, string end, int count, string ext)
        {
            string fname = listBox1.Items[0].ToString();

            string arg1 = "-report -y -avoid_negative_ts 1 -i \"" + fname + "\" -vcodec copy -acodec copy ";

            string arg2 = "-ss " + start + " -to " + end;

            fname = fname + "._part" + count.ToString().PadLeft(2, '0') + ext;
            string arg3 = " \"" + fname + "\"";

            string args = arg1+arg2+arg3;

            textBox3.Text = textBox3.Text + "ffmpeg.exe " + args + Environment.NewLine; 
            
            var proc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    FileName = @"ffmpeg.exe",
                    Arguments = args,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();
           
            return fname;
        }


        /*
         *  Concatenation function
         *  Returns: filename of generated file
         * 
         *   - set concat mux to use list.txt, arg1
         *   - generate new filename based on old filename, arg2
         *   - add ffmpeg arg to argbox
         *   - run ffmpeg
         */
        private string concat(string path, string ext)
        {
            string arg1 = "-report -y -f concat -i \"" + path + "\" -codec copy";

            string fname = listBox1.Items[0].ToString();
            string newext = "new"+ext;
            fname = fname.Replace(ext,newext);
            string arg2 = " \"" + fname + "\"";

            string args = arg1 + arg2;

            textBox3.Text = textBox3.Text + "ffmpeg.exe " + args + Environment.NewLine; 

            var proc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    FileName = @"ffmpeg.exe",
                    Arguments = args,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();

            return fname;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            listBox1.Items.Clear();
            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string filename in FileList)
            {
                listBox1.Items.Add(filename);
                break; //the break here will force array to stop filling after one file from the drag and drop //TODO??
            }
        }

    }
}
