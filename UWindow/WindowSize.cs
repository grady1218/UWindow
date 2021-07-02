using System;
using System.IO;
using System.Drawing;

namespace AutoResizer
{
    public class WindowSize
    {
        public Rectangle Console;
        public Rectangle Horizontal;
        public Rectangle Vertical;

        public static WindowSize LoadFile()
        {
            if( File.Exists( "./Size.xml" ) )
            {
                var s = new System.Xml.Serialization.XmlSerializer(typeof(WindowSize));
                using (var sr = new StreamReader("Size.xml", new System.Text.UTF8Encoding(false)))
                {
                    return (WindowSize)s.Deserialize(sr);
                }
            }
            return new WindowSize();
        }
    }

    static class Extensions
    {
        public static bool WriteFile(this WindowSize window)
        {
            var s = new System.Xml.Serialization.XmlSerializer(typeof(WindowSize));
            using (var sw = new StreamWriter("Size.xml", false, new System.Text.UTF8Encoding(false)))
            {
                s.Serialize(sw, window);
                return true;
            }
        }
    }
}
