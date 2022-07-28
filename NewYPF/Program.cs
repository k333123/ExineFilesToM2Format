using System;
using System.Collections;
using System.Drawing;
using System.IO;

namespace NewYPF
{
    class Program
    { 
        static void Main(string[] args)
        {
            
            MainClass mainClass = new MainClass();
            mainClass.Main(args);  
             
            //Save("Test111.map");
        }

        
        static void Save(string filename)
        {
            ushort width = 10;
            ushort height = 10;
            CellInfo[,] mapInfo = new CellInfo[width, height];
            for (int i = 0; i < height; i++)
            {
                for(int j=0;j<width;j++)
                {
                    mapInfo[i, j] = new CellInfo();
                    mapInfo[i, j].FrontImage = 1;
                }
            }
           // mapInfo[0, 0].FrontImage = 1;

            var fileStream = new FileStream(filename, FileMode.Create);
            var binaryWriter = new BinaryWriter(fileStream);
            short ver = 1;
            char[] tag = { 'C', '#' };
            binaryWriter.Write(ver);
            binaryWriter.Write(tag);

            binaryWriter.Write(Convert.ToInt16(width));
            binaryWriter.Write(Convert.ToInt16(height));
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    binaryWriter.Write(mapInfo[x, y].BackIndex); //lib index in libs
                    binaryWriter.Write(mapInfo[x, y].BackImage); //frame index in lib
                    binaryWriter.Write(mapInfo[x, y].MiddleIndex);
                    binaryWriter.Write(mapInfo[x, y].MiddleImage);
                    binaryWriter.Write(mapInfo[x, y].FrontIndex);
                    binaryWriter.Write(mapInfo[x, y].FrontImage);
                    binaryWriter.Write(mapInfo[x, y].DoorIndex);
                    binaryWriter.Write(mapInfo[x, y].DoorOffset);
                    binaryWriter.Write(mapInfo[x, y].FrontAnimationFrame);
                    binaryWriter.Write(mapInfo[x, y].FrontAnimationTick);
                    binaryWriter.Write(mapInfo[x, y].MiddleAnimationFrame);
                    binaryWriter.Write(mapInfo[x, y].MiddleAnimationTick);
                    binaryWriter.Write(mapInfo[x, y].TileAnimationImage);
                    binaryWriter.Write(mapInfo[x, y].TileAnimationOffset);
                    binaryWriter.Write(mapInfo[x, y].TileAnimationFrames);
                    binaryWriter.Write(mapInfo[x, y].Light);
                }
            }
            binaryWriter.Flush();
            binaryWriter.Dispose();

        }
    }
}
