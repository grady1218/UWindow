using System.Drawing;
using System.IO;

namespace AutoResizer
{
    class WindowSize
    {
        public Rectangle Console;
        public Rectangle Hotizontal;
        public Rectangle Vertical;

        public static WindowSize LoadFile()
        {
            if( File.Exists( "./Size.xml" ) )
            {
                var s = new System.Xml.Serialization.XmlSerializer(typeof(Rectangle));
                using (var sr = new StreamReader("Size.xml", new System.Text.UTF8Encoding(false)))
                {
                    return (WindowSize)s.Deserialize(sr);
                }
            }
            return null;
        }
    }

    static class Extensions
    {
        public static bool CreateFile(this WindowSize window)
        {
            if (!File.Exists("./Size.xml"))
            {
                var s = new System.Xml.Serialization.XmlSerializer(typeof(Rectangle));
                using (var sw = new System.IO.StreamWriter("Size.xml", false, new System.Text.UTF8Encoding(false)))
                {
                    s.Serialize(sw, window);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
