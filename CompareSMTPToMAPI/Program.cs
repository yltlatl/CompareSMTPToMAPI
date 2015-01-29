using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CompareSMTPToMAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputPath = args[0];
            var df = new DelimitedFile(inputPath, "utf-8");
            Console.WriteLine("Header fields:");
            foreach (var fieldName in df.HeaderRecord)
            {
                Console.WriteLine(fieldName);
            }
            df.GetNextRecord();
            var outputPath = args[1];
            var _str = new StreamWriter(outputPath, false, Encoding.UTF8) {AutoFlush = true};
            while (!df.EndOfFile)
            {
                var controlId = df.GetFieldByName("Control ID");
                var from = df.GetFieldByName("Email From");
                var to = df.GetFieldByName("Email To");
                var cc = df.GetFieldByName("Email CC");
                var bcc = df.GetFieldByName("Email Bcc");
                var mapiFieldsCharacterCount = to.Length + cc.Length + bcc.Length;
                var header = df.GetFieldByName("Headers");
                var smtpHeader = new SmtpHeader(header, "®");
                var xZantazRecip = smtpHeader.GetCustomField("X-ZANTAZ-RECIP:");
                if (xZantazRecip != null)
                {
                    var xzrString = string.Join(string.Empty, xZantazRecip);
                    if (xzrString.Length - mapiFieldsCharacterCount > 1000)
                    {
                        _str.WriteLine(controlId);
                    }
                }
                df.GetNextRecord();
            }
            _str.Dispose();
        }
    }
}
