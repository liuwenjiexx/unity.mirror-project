using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Unity.Project.Mirror.Editor
{
    internal static class CommandUtility
    {

        public const int DefaultTimeoutMS = 10 * 1000;

        public static string Run(string workDir, string file, params string[] args)
        {
            return Run(workDir, file, DefaultTimeoutMS, args);
        }

        public static string Run(string workDir, string file, int timeoutMS, params string[] args)
        {
            return Run(workDir, file, timeoutMS, (IEnumerable<string>)args);
        }

        public static string Run(string workDir, string file, int timeoutMS, IEnumerable<string> args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = file;
            startInfo.UseShellExecute = false;
            startInfo.WorkingDirectory = Path.GetFullPath(workDir);
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            if (args != null)
            {
                foreach (var arg in args)
                {
                    startInfo.ArgumentList.Add(arg);
                }
            }
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.StandardOutputEncoding = Encoding.GetEncoding("gb2312");
            startInfo.StandardErrorEncoding = Encoding.GetEncoding("gb2312");
            using (var proc = new Process())
            {
                proc.StartInfo = startInfo;
                StringBuilder errorBuilder = new StringBuilder();
                StringBuilder dataBuilder = new StringBuilder();
                proc.OutputDataReceived += (o, e) =>
                {
                    if (e.Data != null)
                    {
                        //string text = Encoding.UTF8.GetString(Encoding.Default.GetBytes(e.Data));
                        //string text = e.Data;
                        if (dataBuilder.Length > 0)
                            dataBuilder.Append("\n");
                        dataBuilder.Append(e.Data);

                    }
                };
                proc.ErrorDataReceived += (o, e) =>
                {
                    if (e.Data != null)
                    {
                        //string text = Encoding.UTF8.GetString(Encoding.Default.GetBytes(e.Data));
                        if (errorBuilder.Length > 0)
                            dataBuilder.Append("\n");
                        errorBuilder.Append(e.Data);

                    }
                };

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                if (timeoutMS > 0)
                {
                    proc.WaitForExit(timeoutMS);
                }
                else
                {
                    proc.WaitForExit();
                }

                if (!proc.HasExited)
                {
                    proc.Kill();
                }
                if (proc.ExitCode != 0)
                {
                    throw new CommandException(proc.ExitCode, errorBuilder.ToString());
                }

                return dataBuilder.ToString();
            }
        }

        public static string RunCmd(string workDir, string command, params string[] args)
        {
            return RunCmd(workDir, command, DefaultTimeoutMS, args);
        }

        public static string RunCmd(string workDir, string command, int timeoutMS, params string[] args)
        {
            return Run(workDir, "cmd", timeoutMS, new string[] { "/C", command }.Concat(args));
        }


        //public string[] Dir(string[] args)

        public static Dictionary<string, string> GetLinkDirOrFiles(string workDir)
        {
            if (string.IsNullOrEmpty(workDir))
                workDir = ".";

            string result = null;
            Dictionary<string, string> dic = new();
            try
            {
                result = RunCmd(workDir, "dir", "/AL");
            }
            catch (CommandException e)
            {
                //忽略，找不到文件
                if (e.ErrorCode == 1)
                {
                    return dic;
                }
                throw;
            }

            string pattern = "<(?<type>.+)>\\s+(?<from>.+)\\s+\\[(?<to>.+)\\]";

            foreach (Match m in Regex.Matches(result, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
            {
                string linkType = m.Groups["type"].Value;
                string fromPath = m.Groups["from"].Value.Trim();
                string toPath = m.Groups["to"].Value.Trim();
                fromPath = Path.Combine(workDir, fromPath);
                toPath = Path.Combine(workDir, toPath);
                switch (linkType)
                {
                    case "JUNCTION":
                        break;
                }
                dic[fromPath] = toPath;
            }
            return dic;
        }


        public static void CreateLinkDir(string path, string target)
        {
            path = path.NormalPathLocal();
            target = target.NormalPathLocal();

            string absPath = Path.GetFullPath(path).NormalPathLocal();

            string parent = Path.GetDirectoryName(absPath);
            bool exists = false;
            if (Directory.Exists(parent))
            {
                foreach (var item in GetLinkDirOrFiles(parent))
                {
                    if (item.Key.NormalPathLocal() == absPath)
                    {
                        if (item.Value.NormalPathLocal() == target)
                        {
                            exists = true;
                        }
                        break;
                    }

                }
            }
            else
            {
                Directory.CreateDirectory(parent);
            }

            if (exists)
                return;
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            string result = RunCmd(".", "mklink", "/J", path, target);
            if (!Directory.Exists(path))
                throw new Exception("Link dir error");
        }

        public static string NormalPath(this string path)
        {
            if (path == null)
                return null;
            path = path.Replace("\\", "/");
            return path;
        }


        public static string NormalPathLocal(this string path)
        {
            if (path == null)
                return null;
            if (Path.DirectorySeparatorChar == '/')
            {
                path = path.Replace('\\', Path.DirectorySeparatorChar);
            }
            else
            {
                path = path.Replace('/', Path.DirectorySeparatorChar);
            }
            return path;
        }

    }

    public class CommandException : Exception
    {
        public CommandException(int errorCode, string error)
        {
            ErrorCode = errorCode;
            Error = error;
        }

        public int ErrorCode { get; private set; }

        public string Error { get; private set; }

        public override string Message
        {
            get
            {
                if (!string.IsNullOrEmpty(Error))
                    return $"ErrorCode: {ErrorCode}, Error: {Error}";
                return $"ErrorCode: {ErrorCode}";
            }
        }

    }
}