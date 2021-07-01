using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoResizer
{
    public class GetWindow
    {
        public IntPtr HWND;
        string windowName;
        public GetWindow( string WindowName )
        {
            windowName = WindowName;
            var task = Task.Run( WaitGetWindow );

            task.ConfigureAwait( false );
            task.Wait();
        }

        public async Task WaitGetWindow()
        {
            Console.WriteLine( "ウマ娘が開かれるまで待機します..." );
            while( true )
            {
                var handle = GetTargetWindow();
                if(IntPtr.Zero != handle)
                {
                    HWND = handle;
                    Console.WriteLine("開かれました！");
                    break;
                }
                await Task.Delay( 1000 );
            }
        }

        public IntPtr GetTargetWindow()
        {
            IntPtr pt = IntPtr.Zero;
            WAPI.EnumWindows((h, l) =>
            {

                int textLen = WAPI.GetWindowTextLength(h);
                if (0 < textLen)
                {
                    //ウィンドウのタイトルを取得する
                    StringBuilder tsb = new StringBuilder(textLen + 1);
                    WAPI.GetWindowText(h, tsb, tsb.Capacity);

                    if (tsb.ToString() == windowName)
                    {
                        pt = h;
                        return false;
                    }
                }
                return true;
            }, IntPtr.Zero);

            return pt;
        }
    }
}
