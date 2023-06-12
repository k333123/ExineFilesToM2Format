using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace NewYPF
{
    class MainClass
    {
        public void Main(string[] args)
        {
            //string[] filenames = new string[] { "test.ypf" };
            string[] filenames = new string[] { "00000.map" };
            List<String> inputFileNames = new List<string>();

            if (args.Length > 0) filenames = args;
            

            if (!File.Exists(filenames[0]))
            {
                if (Path.GetFileNameWithoutExtension(filenames[0]).Equals("*"))
                {
                    Console.WriteLine("Scan File List");
                    //scan *.dat files
                    inputFileNames.AddRange(System.IO.Directory.GetFiles(".", filenames[0]));
                    foreach(var fileName in inputFileNames)
                    {
                        Console.WriteLine(fileName);
                    }
                } 
                else
                {
                    foreach (var filename in filenames)
                    {
                        inputFileNames.Add(filename);
                    }
                } 
            }

            foreach (var filename in inputFileNames)
            {
                if (!File.Exists(filename))
                {
                     
                    Console.WriteLine(filename + "Is Not Exist!");
                    continue;
                }

                switch (GetExt(filename))
                {
                    case ".ypf": ConvertYpfToRGBA(filename).ConvertToLib(filename); break;
                    case ".map": ConvertMapToM2Map(filename); break;
                    case ".png": ConvertPngToLib(filename,-48,+24); break;//To One Lib
                    case ".dat": ExtractDat(filename); break; 
                }
            }
            Console.WriteLine("FIN!");

        }

        string GetExt(string filename)
        {
            string ext = Path.GetFileName(filename);
            ext = ext.Replace(Path.GetFileNameWithoutExtension(filename), "");
            return ext;
        }

        void ExtractDat(string filename)
        {
            byte[] datas = ReadByteFile(filename);
            DATFormat datFormat = new DATFormat(datas);
            datFormat.Save();
        }

        void ConvertPngToLib(string filename, short offsetX = 0, short offsetY = 0)
        {
            DirectoryInfo di = new DirectoryInfo(".\\PNG_OUT");
            if (!di.Exists) di.Create();

            MLibraryV2 mLibraryV2 = new MLibraryV2(".\\PNG_OUT\\"+filename + ".lib");
            Bitmap bitmap = new Bitmap(filename);
            mLibraryV2.AddImage(bitmap, offsetX, offsetY);
            mLibraryV2.Save(); 
        }

        void ConvertMapToM2Map(string filename)
        {
            Console.WriteLine("Start ConvertMapToM2Map. Filename:" + filename);
            byte[] datas = ReadByteFile(filename);
            MAPFormat mapFormat = new MAPFormat(datas,filename);
            mapFormat.ConvertToM2MAP("M2_"+filename);

            Console.WriteLine(datas.Length);
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
