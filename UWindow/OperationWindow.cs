using System;
using System.Drawing;
using System.Threading.Tasks;

namespace AutoResizer
{
	class OperationWindow
	{

		IntPtr HWND;
		Rectangle _currentVerticalWindowSize;
		Rectangle _currentHorizontalWindowSize;

		int _time = 100;
		bool _isEdit = false;
		public OperationWindow( IntPtr hwnd )
		{
			HWND = hwnd;

			LoadPosition();

			Task task1 = Task.Run( WaitKeyInput );
			Task mainLoop = Task.Run( SetWindowSize );

			mainLoop.Wait();

		}

		private async Task WaitKeyInput()
		{
			while(true)
			{
				string str = Console.ReadLine();
				if (str != "")
				{
					Operation(str)();
				}
				await Task.Delay( 100 );
			}
		}

		private Action Operation(string op)
		{
			switch(op)
			{
				case "s":
				case "save": return SavePosition;  //  エイリアス
				case "c":
				case "change": return ChangeWindowPosition;
				default: return () => { };
			}
		}

		private void LoadPosition()
		{
			if(System.IO.File.Exists("Vertical.xml"))
			{
				var s = new System.Xml.Serialization.XmlSerializer(typeof(Rectangle));
				using (var sr = new System.IO.StreamReader("Vertical.xml", new System.Text.UTF8Encoding(false)))
				{
					_currentVerticalWindowSize = (Rectangle)s.Deserialize( sr );
				}
			}
			else
			{
				SavePosition();
			}

			if (System.IO.File.Exists("Horizontal.xml"))
			{
				var s = new System.Xml.Serialization.XmlSerializer(typeof(Rectangle));
				using (var sr = new System.IO.StreamReader("Horizontal.xml", new System.Text.UTF8Encoding(false)))
				{
					_currentHorizontalWindowSize = (Rectangle)s.Deserialize(sr);
				}
			}
		}

		private void SavePosition()
		{
			Rectangle rect = GetWindowSize();
			//  ゼロ除算回避
			string direction = rect.Width / (rect.Height + 1) < 1 ? "Vertical" : "Horizontal";
			_isEdit = false;

			if (direction == "Vertical") _currentVerticalWindowSize = rect;
			else _currentHorizontalWindowSize = rect;

			var s = new System.Xml.Serialization.XmlSerializer( typeof( Rectangle ) );
			using (var sw = new System.IO.StreamWriter( $"{direction}.xml", false, new System.Text.UTF8Encoding(false) ))
			{
				s.Serialize( sw, rect );
			}

			Console.WriteLine("現在位置を保存しました");
		}

		private void ChangeWindowPosition() => _isEdit = true;

		private Rectangle GetWindowSize()
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
					foreach( var p in Program.Processes )
                    {
						if( !p.HasExited ) p.Kill();
                    }
					Environment.Exit(0);
				}

				if (!_isEdit && WAPI.GetForegroundWindow() == HWND)
				{
					Rectangle rect = GetWindowSize();
					//  ゼロ除算回避
					string direction = rect.Width / ( rect.Height + 1 ) < 1 ? "Vertical" : "Horizontal";

					if (direction == "Vertical" && _currentVerticalWindowSize != rect && _currentVerticalWindowSize.Width != 0)
					{
						WAPI.SetWindowPos(HWND, IntPtr.Zero, _currentVerticalWindowSize.X, _currentVerticalWindowSize.Y, _currentVerticalWindowSize.Width, _currentVerticalWindowSize.Height, 0);

					}
					else if (direction == "Horizontal" && _currentHorizontalWindowSize != rect && _currentHorizontalWindowSize.Width != 0)
					{
						WAPI.SetWindowPos(HWND, IntPtr.Zero, _currentHorizontalWindowSize.X, _currentHorizontalWindowSize.Y, _currentHorizontalWindowSize.Width, _currentHorizontalWindowSize.Height, 0);
					}

				}
				await Task.Delay( _time );
			}
		}
	}
}
