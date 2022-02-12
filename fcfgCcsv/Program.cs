using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace fcfgCcsv
{
    class Program
    {
        static byte[] unescapeStringAndReverse(string str)
        {
            List<Byte> result = new List<byte>();
            for (var i = 0; i < str.Length; i++) {
                if (str[i] == '\\') {
                    i++;
                    if (str[i] == '0') {
                        result.Add(0x00);
                    }
                    else if (str[i] == 'f') {
                        result.Add(0x0C);
                    }
                    else if (str[i] == 'a') {
                        result.Add(0x07);
                    }
                    else if (str[i] == 'r') {
                        result.Add(0x0D);
                    }
                    else if (str[i] == 'x') {
                        i++;
                        // 後の2文字を調べる
                        if (Byte.TryParse(str.Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out byte ret)) {
                            i++;
                            result.Add(ret);
                        }
                        else if (Byte.TryParse(str.Substring(i, 1), System.Globalization.NumberStyles.HexNumber, null, out byte ret1)) {
                            result.Add(ret1);
                        }
                    }
                }
                else {
                    result.Add((byte)str[i]);
                }
            }
            result.Reverse();
            return result.ToArray();
        }

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
                            var bytes = unescapeStringAndReverse(item.Value.Substring(start, end - start));
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
