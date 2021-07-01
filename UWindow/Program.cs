using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace AutoResizer
{
    public static class Program
    {
        static GetWindow getWindow;
        static OperationWindow operationWindow;
        /// <summary>
        /// StartupAppで起動したプロセスのリスト
        /// </summary>
        public static List<Process> Processes;

        static void Main(string[] args)
        {
            Processes = new List<Process>();
            Task.Run( StartupApp );
            getWindow = new GetWindow( "umamusume" );
            operationWindow = new OperationWindow( getWindow.HWND );
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
