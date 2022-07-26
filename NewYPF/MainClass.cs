using System;
using System.IO;

namespace NewYPF
{  
   
    class MainClass
    {
        public void Main(string[] args)
        {
            string[] filenames = new string[] { "test.ypf" };
            if (args.Length > 0) filenames = args;
            for (int i = 0; i < filenames.Length; i++)
            {
                if (!File.Exists(filenames[i]))
                {
                    Console.WriteLine(filenames[i] + "Is Not Exist!");
                    continue;
                }
                var ypf = ConvertYpfToRGBA(filenames[i]);
                //ypf.SaveFile(filenames[i]);
                ypf.ConvertToLib(filenames[i]);
            }
            Console.WriteLine("FIN!");
        }

        YPFFormat ConvertYpfToRGBA(string filename)
        {
            Console.WriteLine("Start ConvertYpfToRGBA. Filename:" + filename);
            byte[] datas = ReadByteFile(filename);
            
            YPFFormat ypfFormat = new YPFFormat(datas);
            //ypfFormat.SaveFile(filename);//사용 안하면 제거
            //ypfFormat.ConvertToLib(filename);
            return ypfFormat;
        }


        public byte[] ReadByteFile(string filePath)
        {
            byte[] datas;
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                int length = (int)fileStream.Length;
                datas = new byte[length];
                int count;
                int sum = 0;

                while ((count = fileStream.Read(datas, sum, length - sum)) > 0)
                {
                    sum += count;
                }
            }
            finally
            {
                fileStream.Close();
            }
            return datas;
        }
    }
}
