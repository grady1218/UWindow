using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AutoResizer
{
    public static class Program
    {
        static string version = "v1.0";
        static GetWindow getWindow;
        static OperationWindow operationWindow;
        /// <summary>
        /// StartupAppで起動したプロセスのリスト
        /// </summary>
        public static List<Process> Processes;

        static void Main(string[] args)
        {
            CheckUpdate();
            Processes = new List<Process>();
            Task.Run(StartupApp);
            getWindow = new GetWindow("umamusume");
            operationWindow = new OperationWindow(getWindow.HWND);
        }

        static void CheckUpdate()
        {
            using (var client = new HttpClient())
            {
                var req = new HttpRequestMessage( HttpMethod.Get, "https://api.github.com/repos/grady1218/UWindow/releases");
                req.Headers.Add( "user-agent", "cs" );

                var res = client.SendAsync(req);
                var body = res.Result.Content.ReadAsStringAsync().Result.Split(',');
                var url = "";

                foreach( var text in body )
                {

                    url = text.Contains("html_url") && url == "" ? text.Substring(text.IndexOf(':') + 1) : url;
                    
                    if (text.Contains("tag_name"))
                    {
                        if (!text.Contains(version))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine( $"更新があります\n{url}" );
                            Console.ResetColor();

                            Process.Start(url);
                        }
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// StartupApp.txtに記述したpathを実行する
        /// </summary>
        static private async Task StartupApp()
        {
            await Task.Run( () =>
            {
                if ( File.Exists( "./StartupApp.txt" ) )
                {
                    Assembly assembly = Assembly.GetEntryAssembly();


                    Action<string> Error = ( string path ) =>
                    {
                        //  文字色を赤に変える
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{path}を起動できませんでした。");
                        Console.ResetColor();
                    };

                    foreach( var path in File.ReadAllLines("./StartupApp.txt") )
                    {
                        //  パスが自分自身だったら無視する
                        if (path.IndexOf(assembly.Location) != -1) continue;

                        //  パワーシェルか調べる
                        if( path.StartsWith( "-p " ) )
                        {
                            //  パワーシェルの設定をする
                            var info = new ProcessStartInfo
                            {
                                FileName = "PowerShell.exe",
                                Arguments = path.Remove(path.IndexOf("-p"), 3),
                                UseShellExecute = false
                            };

                            try
                            {
                                var p = Process.Start( info );
                                Console.WriteLine($"{info.Arguments}をパワーシェルで起動しました。");
                                Processes.Add(p);
                            }
                            catch
                            {
                                Error( path );
                            }
                        }
                        else
                        {
                            //  パスが自分自身だったら無視する
                            if (path == assembly.Location) continue;

                            try
                            {
                                var p = Process.Start(path);
                                Console.WriteLine($"{path}を起動しました。");
                                Processes.Add(p);
                            }
                            catch
                            {
                                Error(path);
                            }
                        }
                        
                    }
                }
                //  ファイルがなかったら生成する
                else
                {
                    File.Create( "./StartupApp.txt" );
                }

            });
        }
    }
}
