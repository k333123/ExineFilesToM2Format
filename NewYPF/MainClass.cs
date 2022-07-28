using System;
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


            //Path.GetFileNameWithoutExtension


            if (args.Length > 0) filenames = args;

            foreach (var filename in filenames)
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
                    case ".png": ConvertPngToLib(filename,-48,+24); break;//지도 통째로 올릴때 사용됨


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

        void ConvertPngToLib(string filename, short offsetX = 0, short offsetY = 0)
        {
            MLibraryV2 mLibraryV2 = new MLibraryV2(filename + ".lib");
            Bitmap bitmap = new Bitmap(filename);
            mLibraryV2.AddImage(bitmap, offsetX, offsetY);
            mLibraryV2.Save();
            //이걸 정상적으로 적용시키려면 지도의 크기를 1씩 증가시키고 위치를 x, y를 +1씩 옮겨야한다
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
