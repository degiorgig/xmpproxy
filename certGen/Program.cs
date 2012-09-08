using System;
using System.IO;

namespace certGen
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] c = Certificate.CreateSelfSignCertificatePfx(
                "CN=ChrisD-desktop", //host name
                DateTime.Parse("2000-01-01"), //not valid before
                DateTime.Parse("2015-01-01"), //not valid after
                "mypassword"); //password to encrypt key file


            using (BinaryWriter binWriter = new BinaryWriter(
                File.Open(@"testcert.pfx", FileMode.Create)))
            {
                binWriter.Write(c);
            }
        }
    }
}
