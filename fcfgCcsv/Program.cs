using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace fcfgCcsv
{
    class Program
    {
        static void WriteParameter(TextWriter writer, String parameterFile)
        {
            var iniFile = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddIniFile(parameterFile).Build();
            foreach (var section in iniFile.GetChildren()) {
                writer.WriteLine("{0}", section.Key);
                foreach (var item in section.GetChildren()) {
                    if (!String.IsNullOrEmpty(item.Value)) {
                        if (item.Value.IndexOf("@Variant") >= 0) {
                            // @Variantは単精度浮動小数点をByte->String変換して出力している
                            // 変換対象は()の内側の文字列
                            // 上のものをByte[]に変換しそれをリバースした結果をToSignleで値にする
                            var start = item.Value.IndexOf('(') + 1;
                            var end = item.Value.IndexOf(')');
                            var str = Regex.Unescape(item.Value.Substring(start, end - start));
                            var bytes = str.ToCharArray().Select(b => (byte)b).Reverse().ToArray();
                            var value = BitConverter.ToSingle(bytes);
                            writer.WriteLine("{0},{1}", item.Key, value);
                        }
                        else if (item.Value.IndexOf('[') >= 0) {
                            writer.WriteLine("{0},\"{1}\"", item.Key, item.Value);
                        }
                        else {
                            writer.WriteLine("{0},{1}", item.Key, item.Value);
                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 1) {
                WriteParameter(Console.Out, args[0]);
            }
            else if (args.Length == 2) {
                using (var fs = File.CreateText(args[1])) {
                    WriteParameter(fs, args[0]);
                }
            }
        }
    }
}
