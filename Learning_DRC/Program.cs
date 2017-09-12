using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Diagnostics;


namespace Learning_DRC
{
    class Program
    {
        static void Main(string[] args)
        {
            var sw = new Stopwatch();

            sw.Start();
            var drcInstance = new LearningDRC(new DirectoryInfo("."));
            sw.Stop();
            System.Console.WriteLine($"Buildtime: {sw.ElapsedMilliseconds} ms");
            sw.Reset();

            var fileBatch = new FileInfo("batchwords.txt");
            var fileLog = new FileInfo("log.txt");

            // Ask user if they are running a batch or want to run individual stimuli manually.

            System.Console.WriteLine("Do you want to run a batch of stimuli or do a bulk run?");
            System.Console.Write("Press b for batch, k for bulk run, or any other key for individual stimuli: ");
            ConsoleKeyInfo batchChoice = System.Console.ReadKey();
            System.Console.WriteLine();
            System.Console.WriteLine();
            
            // Batch processing
            if ((batchChoice.KeyChar == 'b') || (batchChoice.KeyChar == 'B'))
            {
                StreamReader streamR = null;
                try
                {
                    streamR = fileBatch.OpenText();
                }
                catch
                {
                    System.Console.WriteLine("There was a problem opening the batch file.");
                    System.Console.Write("Press any key to exit. ");
                    System.Console.ReadKey();
                    Environment.Exit(0);
                }

                string line;
                string[] splitline;
                string[] output;
                do
                {
                    line = streamR.ReadLine();

                    if (line == null)
                        continue;

                    splitline = line.Split(new char[] { ' ' });

                    //code to handle stimuli presented either with or without context.
                    if (splitline.Length == 2)
                    {
                        sw.Start();
                        output = drcInstance.Simulate(splitline[0], splitline[1], new DirectoryInfo(Directory.GetCurrentDirectory()));
                        sw.Stop();

                        StreamWriter streamW = new StreamWriter(fileLog.FullName, true);
                        streamW.WriteLine($"Sim_time: {sw.ElapsedMilliseconds} ms");
                        streamW.Close();
                        System.Console.WriteLine($"Sim_time: {sw.ElapsedMilliseconds} ms");
                        sw.Reset();
                    }
                    else if (splitline.Length == 1)
                    {
                        sw.Start();
                        output = drcInstance.Simulate(splitline[0], "no_context", new DirectoryInfo(Directory.GetCurrentDirectory()));
                        sw.Stop();

                        StreamWriter streamW = new StreamWriter(fileLog.FullName, true);
                        streamW.WriteLine($"Sim_time: {sw.ElapsedMilliseconds} ms");
                        streamW.Close();
                        System.Console.WriteLine($"Sim_time: {sw.ElapsedMilliseconds} ms");
                        sw.Reset();
                    }
                    else
                    {
                        StreamWriter streamW = new StreamWriter(fileLog.FullName, true);
                        streamW.WriteLine("Bad input line skipped.");
                        streamW.Close();
                        System.Console.WriteLine("Bad input line skipped.");
                    }

                } while (line != null);
                streamR.Close();
            }

            else if ((batchChoice.KeyChar == 'k') || (batchChoice.KeyChar == 'K'))
            {
                var bulkRun = new BulkRun(drcInstance);
            }

            // Manual processing of individual stimuli
            else
            {
                var line = "";
                string[] output;

                do
                {
                    System.Console.WriteLine("Enter stimulus <Printed Word> <Context> :");
                    System.Console.WriteLine("Context is optional.");
                    System.Console.WriteLine("(Note: Context is represented with phoneme symbols)");
                    System.Console.WriteLine("Press ENTER to exit.");
                    line = System.Console.ReadLine();
                    string[] splitline = line.Split(new char[] { ' ' });
                    if (line == "")
                        continue;

                    //code to handle stimuli presented either with or without context.
                    if (splitline.Length == 2)
                    {
                        sw.Start();
                        output = drcInstance.Simulate(splitline[0], splitline[1], new DirectoryInfo(Directory.GetCurrentDirectory()));
                        sw.Stop();

                        StreamWriter streamW = new StreamWriter(fileLog.FullName, true);
                        streamW.WriteLine("Sim_time: {0} ms", sw.ElapsedMilliseconds);
                        streamW.Close();
                        System.Console.WriteLine("Sim_time: {0} ms", sw.ElapsedMilliseconds);
                        sw.Reset();
                    }
                    else if (splitline.Length == 1)
                    {
                        sw.Start();
                        output = drcInstance.Simulate(splitline[0], "no_context", new DirectoryInfo(Directory.GetCurrentDirectory()));
                        sw.Stop();

                        StreamWriter streamW = new StreamWriter(fileLog.FullName, true);
                        streamW.WriteLine("Sim_time: {0} ms", sw.ElapsedMilliseconds);
                        streamW.Close();
                        System.Console.WriteLine("Sim_time: {0} ms", sw.ElapsedMilliseconds);
                        sw.Reset();
                    }
                    else
                    {
                        System.Console.WriteLine("Bad input line.");
                    }
                } while (line != "");
            }
        }
    }
}
