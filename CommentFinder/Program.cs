using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CommentFinder
{
    class Program
    {
        private static string extension = ".cs";
        static void Main(string[] args)
        {
            string path;
            Console.WriteLine("Enter directory path");
            path = Console.ReadLine();
            Console.WriteLine();
            if (string.IsNullOrEmpty(path))
            {
                path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\..\\")); 
            }

            List<string> files = GetFiles(path, extension);
            if (File.Exists(path + "output.txt"))
            {
                File.Delete(path + "output.txt");
            }
            foreach (var file in files)
            {
                List<string> comments = GetComments(file);
                AppendFileComments(path + "output.txt", comments, file);
            }

            Console.WriteLine("Finished");
        }

        // Recursively get files of a given directory which have provided file extension
        static List<string> GetFiles(string directory, string extension)
        {
            List<string> filesFound = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(directory))
                {
                    if (f.EndsWith(extension))
                    {
                        filesFound.Add(f);
                    }
                }
                foreach (string dir in Directory.GetDirectories(directory))
                {
                    filesFound.AddRange(GetFiles(dir, extension));
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return filesFound;
        }

        private static int CountAppearances(string line, string of, int start)
        {
            int index = 0;
            int count = 0;
            int previous = 0;
            while ((index = line.IndexOf(of, index)) != -1 && index < start)
            {
                if(count != 0)
                {
                    if(index == previous)
                    {
                        break;
                    }
                }
                count++;
                previous = index++;
            }
            return count;
        }

        static List<string> GetComments(string path)
        {
            List<string> comments = new List<string>();
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        int start, end;
                        StringBuilder comment = new StringBuilder();
                        if ((start = line.IndexOf("/*")) != -1)
                        {
                            if (line.IndexOf("\"", 0) < start && line.IndexOf("\"", 0) > -1)
                            {
                                if(CountAppearances(line, "\"", start) % 2 != 0)
                                {
                                    continue;
                                }
                            }

                            while ((end = line.IndexOf("*/")) == -1 || line.IndexOf("*/") < start)
                            {
                                comment.Append(line.Substring(start, line.Length - start));
                                line = reader.ReadLine();
                                start = 0;
                            }

                            if (end != -1)
                            {
                                comment.Append(line.Substring(start, end - start + 2));
                            }

                            if(comment.ToString().Trim() != "")
                            {
                                comments.Add(comment.ToString());
                            }
                        }
                        else if((start = line.IndexOf("//")) != -1)
                        {
                            int indexQuotation;
                            if((indexQuotation = line.IndexOf("\"")) != -1)
                            {
                                if(indexQuotation < start)
                                {
                                    int weirdCombination;
                                    if((weirdCombination = line.IndexOf("\\\"")) != -1)
                                    {
                                        int count = CountAppearances(line, "\"", start);
                                        int count2 = CountAppearances(line, "\\\"", start);

                                        if ((count - count2) % 2 == 0)
                                        {
                                            comment.Append(line.Substring(start, line.Length - start).Trim());
                                        }
                                    }
                                    else
                                    {
                                        if (CountAppearances(line, "\"", start) % 2 == 0)
                                        {
                                            comment.Append(line.Substring(start, line.Length - start).Trim());
                                        }
                                    }
                                }
                                else
                                {
                                    if(!(start == line.IndexOf("///") && line[start + 3] != '/'))
                                    {
                                        comment.Append(line.Substring(start, line.Length - start).Trim());
                                    }
                                }
                            }
                            else
                            {
                                if (!(start == line.IndexOf("///") && line[start + 3] != '/'))
                                {
                                    comment.Append(line.Substring(start, line.Length - start).Trim());
                                }
                            }

                            if (comment.ToString().Trim() != "")
                            {
                                comments.Add(comment.ToString());
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return comments;
        }

        private static void AppendFileComments(string path, List<string> comments, string file)
        {
            using (StreamWriter writer = File.AppendText(path))
            {
                writer.WriteLine("===========" + file + "===========");
                int i = 1;
                foreach (var comment in comments)
                {
                    writer.Write("#" + i.ToString() + " " + comment + "\r\n");
                    i++;
                }
                writer.WriteLine();
            }
        }
    }
}
