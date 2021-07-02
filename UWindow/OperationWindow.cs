using System;
using System.Drawing;
using System.Threading.Tasks;

namespace AutoResizer
{
	class OperationWindow
	{

		IntPtr HWND;

		int _time = 100;
		bool _isEdit = false;
		public OperationWindow(IntPtr hwnd)
		{
			HWND = hwnd;

			Task task1 = Task.Run(WaitKeyInput);
			Task mainLoop = Task.Run(SetWindowSize);

			mainLoop.Wait();

		}

		private async Task WaitKeyInput()
		{
			while (true)
			{
				string str = Console.ReadLine();
				if (str != "")
				{
					Operation(str)();
				}
				await Task.Delay(100);
			}
		}

		private Action Operation(string op)
		{
			switch (op)
			{
				case "s":
				case "save": return SavePosition;  //  エイリアス
				case "c":
				case "change": return ChangeWindowPosition;
				default: return () => { };
			}
		}

		private void SavePosition()
		{
			Rectangle rect = GetWindowSize(HWND);
			//  ゼロ除算回避
			string direction = rect.Width / (rect.Height + 1) < 1 ? "Vertical" : "Horizontal";
			_isEdit = false;

			if (direction == "Vertical") Program.WindowSize.Vertical = rect;
			else Program.WindowSize.Horizontal = rect;

			Program.WindowSize.WriteFile();

			Console.WriteLine("位置を保存しました");
		}

		private void ChangeWindowPosition()
		{
			_isEdit = true;
			Console.WriteLine( "変更を受け付けます・・・" );
		}

		private Rectangle GetWindowSize( IntPtr HWND )
		{
            bool flag = WAPI.GetWindowRect(HWND, out WAPI.RECT rect);

            int width = rect.right - rect.left;
			int height = rect.bottom - rect.top;

			return new Rectangle( rect.left, rect.top, width, height );
		}

		private async Task SetWindowSize()
		{
			while(true)
			{
				//  もしウィンドウが閉じられていたら関連のアプリを閉じる
				if (WAPI.IsWindow(HWND) == 0)
				{
					Program.WindowSize.Console = GetWindowSize( WAPI.GetConsoleWindow() );
					Program.WindowSize.WriteFile();

					foreach ( var p in Program.Processes )
                    {
						if( !p.HasExited ) p.Kill();
                    }

					Environment.Exit(0);
				}

				if (!_isEdit && WAPI.GetForegroundWindow() == HWND)
				{
					Rectangle rect = GetWindowSize( HWND );
					//  ゼロ除算回避
					string direction = rect.Width / ( rect.Height + 1 ) < 1 ? "Vertical" : "Horizontal";

					if (direction == "Vertical" && Program.WindowSize.Vertical != rect && Program.WindowSize.Vertical.Width != 0)
					{
						WAPI.SetWindowPos(HWND, IntPtr.Zero, Program.WindowSize.Vertical.X, Program.WindowSize.Vertical.Y, Program.WindowSize.Vertical.Width, Program.WindowSize.Vertical.Height, 0);

					}
					else if (direction == "Horizontal" && Program.WindowSize.Horizontal != rect && Program.WindowSize.Horizontal.Width != 0)
					{
						WAPI.SetWindowPos(HWND, IntPtr.Zero, Program.WindowSize.Horizontal.X, Program.WindowSize.Horizontal.Y, Program.WindowSize.Horizontal.Width, Program.WindowSize.Horizontal.Height, 0);
					}

				}
				await Task.Delay( _time );
			}
		}
	}
}
