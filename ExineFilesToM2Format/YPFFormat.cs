using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace NewYPF
{
    class YPFFormat
    {
        public YPFHeader ypfHeader = new YPFHeader();
        public YPFImageSet[] ypfImageSets =null;

        public YPFFormat(byte[] datas)
        {
            int idx = ypfHeader.FillData(datas);
            if(idx==-1) return; 

            ypfImageSets = new YPFImageSet[ypfHeader.ImageSetCount];

            for(int i=0;i<ypfImageSets.Length;i++)
            {
                ypfImageSets[i] = new YPFImageSet();
                idx=ypfImageSets[i].FillData(datas,idx,ypfHeader.Version,
                                            (int)ypfHeader.DataPotision,
                                            ypfHeader.HasPalette,
                                            ypfHeader.Palatte
                                            );
                //ypfImageSets[i].SaveFile(filename+"_ImageSet_"+i.ToString());
            }
        }
         
        public void SaveFile(string filename)
        {
            if(ypfImageSets==null)return;

            for (int i = 0; i < ypfImageSets.Length; i++)
            {
                ypfImageSets[i].SaveFile(filename + "_ImageSet_" + i.ToString());
            }
        }

        public void ConvertToLib(string filename)
        {
            DirectoryInfo di = new DirectoryInfo(".\\YPF_OUT");
            if (!di.Exists) di.Create();

            Console.WriteLine("ConvertToLib start");
            bool hasMaskImg = false;
            if (ypfImageSets == null) return;
            //if(ypfImageSets.count==2) //has mask
            List<ImagesWithPosition> imagesWithPosition = new List<ImagesWithPosition>();

            for (int i = 0; i < ypfImageSets.Length; i++)
            {
                Console.WriteLine("Convert to lib, ypfImageSetIdx:" + i);
                imagesWithPosition.Add(ypfImageSets[i].ConvertToLib(filename + "_ImageSet_" + i.ToString()));
            }
            if (imagesWithPosition.Count == 2) hasMaskImg = true;



            //.ypf.lib => .lib로 변경
            
            MLibraryV2 mLibraryV2 = new MLibraryV2(".\\YPF_OUT\\" + Path.GetFileNameWithoutExtension(filename) + ".lib");
            if (hasMaskImg)
            {
                Console.WriteLine("Has Mask Img");
                for(int i=0;i< imagesWithPosition[0].bitmaps.Count;i++)
                {
                    var img = imagesWithPosition[0].bitmaps[i];
                    var maskImg = imagesWithPosition[1].bitmaps[i];
                    var x = (short)imagesWithPosition[0].xVals[i];
                    var y = (short)imagesWithPosition[0].yVals[i];
                    mLibraryV2.AddImage(img, maskImg, x, y);
                    Console.Write(".");
                }
            }
            else
            {
                //파일 분리해서 저장하는게 맞을지?
                Console.WriteLine("Has No Mask Img");
                for (int i = 0; i < imagesWithPosition.Count; i++)
                {
                    for (int j = 0; j < imagesWithPosition[i].bitmaps.Count; j++)
                    {
                        var img = imagesWithPosition[i].bitmaps[j];
                        var x = (short)imagesWithPosition[i].xVals[j];
                        var y = (short)imagesWithPosition[i].yVals[j];
                        mLibraryV2.AddImage(img, x, y);
                        Console.Write(".");
                        //mLibraryV2.AddImage(bitmaps[i], maskBitmaps[i], (short)xVals[i], (short)yVals[i]);
                    }
                }
            }

            mLibraryV2.Save();
            /*
           MLibraryV2 mLibraryV2 = new MLibraryV2(filename+".lib");
           for(int i=0;i<bitmaps.Count;i++)
           {
               mLibraryV2.AddImage(bitmaps[i], (short)xVals[i], (short)yVals[i]);
               //mLibraryV2.AddImage(bitmaps[i], maskBitmaps[i], (short)xVals[i], (short)yVals[i]);
           }

           mLibraryV2.Save(); 
           Console.WriteLine(filename+" mLibraryV2.Count" + mLibraryV2.Count);
           */
        }
    }
    class YPFHeader
    {
         ushort version = 0;
         ushort width = 0;
         ushort height = 0;
         uint depth = 0;
         uint hasPalette = 0;// 0:false 1:true
         byte[] palatte = new byte[512];
         uint dataPotision = 0;
         ushort imageSetCount = 0;


        public int FillData(byte[] datas)
        {
            try
            {
                int idx = 0;

                version = BitConverter.ToUInt16(datas, idx);
                idx = idx + 2;

                width = BitConverter.ToUInt16(datas, idx);
                idx = idx + 2;

                height = BitConverter.ToUInt16(datas, idx);
                idx = idx + 2;

                depth = BitConverter.ToUInt32(datas, idx);
                idx = idx + 4;

                hasPalette = BitConverter.ToUInt32(datas, idx);
                idx = idx + 4;
                if (HasPalette)
                {
                    Buffer.BlockCopy(datas, idx, palatte, 0, 512);
                    idx = idx + 512;
                }
                dataPotision = BitConverter.ToUInt32(datas, idx);
                idx = idx + 4;

                imageSetCount = BitConverter.ToUInt16(datas, idx);
                idx = idx + 2;

                PrintVal();
               
                return idx;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return -1;
            }
        }
        private void PrintVal()
        { 
            Console.WriteLine("Version:"+version);
            Console.WriteLine("Width:"+width);
            Console.WriteLine("Height:"+height);
            Console.WriteLine("Depth:"+depth);
            Console.WriteLine("HasPalette:"+HasPalette);
            Console.WriteLine("Palatte[0]:"+Palatte[0]);
            Console.WriteLine("DataPotision:"+dataPotision);
            Console.WriteLine("ImageSetCount:"+imageSetCount); 
        }

        
        public ushort Version { get => version;  }
        public ushort Width { get => width;  }
        public ushort Height { get => height; }
        public uint Depth { get => depth;  }
        public bool HasPalette { get {return (hasPalette!=0);}  }
        public byte[] Palatte { get => palatte;  }
        public uint DataPotision { get => dataPotision; }
        public ushort ImageSetCount { get => imageSetCount; }
    }

    class YPFImageSet
    {
        Dummy1Info dummy1Info = new Dummy1Info(); 
        FrameInfo frameInfo  = new FrameInfo();
        ActionInfo actionInfo  = new ActionInfo();
        StateInfo stateInfo = new StateInfo();
        StateTransValue stateTransValue = new StateTransValue();


        public int FillData(byte[] datas, int idx,int version,int dataPotision,bool hasPalette, byte[] palette)
        { 
            Console.WriteLine("Start DummyInfo IDX:"+idx);
            //Console.ReadLine();

            idx = dummy1Info.FillData(datas,idx);

            Console.WriteLine("Start frameInfo IDX:"+idx);
            //Console.ReadLine();
            idx = frameInfo.FillData(datas,idx,version,dataPotision,hasPalette,palette);

            Console.WriteLine("Start actionInfo IDX:"+idx);
            //Console.ReadLine();

            idx = actionInfo.FillData(datas,idx);
            Console.WriteLine("Start stateInfo IDX:"+idx);
            //Console.ReadLine();
            
            idx = stateInfo.FillData(datas,idx);
            Console.WriteLine("Start stateTransValue IDX:"+idx);
            //Console.ReadLine();
             

            idx = stateTransValue.FillData(datas,idx);
            Console.WriteLine("END stateTransValue IDX:"+idx); 
            //Console.ReadLine();

            return idx;
        }
        public void SaveFile(string filename)
        {
            frameInfo.SaveFrame(filename);
        }

        public ImagesWithPosition ConvertToLib(string filename)
        {
            return frameInfo.ConvertToLib(filename);
        }


        public Dummy1Info Dummy1Info { get => dummy1Info; }
        public FrameInfo FrameInfo { get => frameInfo; }
        public ActionInfo ActionInfo { get => actionInfo; }
        public StateInfo StateInfo { get => stateInfo; }
        public StateTransValue StateTransValue { get => stateTransValue; }
    }


    class Dummy1Info
    {
        uint len = 0;
        uint position = 0;
        byte[] dummy1Datas = new byte[0];


        public int FillData(byte[] datas, int idx)
        { 
            len = BitConverter.ToUInt32(datas, idx);
            idx = idx + 4;
            Console.WriteLine("Dummy1Len:"+len);
            if (len == 0) return idx;

            position = BitConverter.ToUInt32(datas, idx);
            idx = idx + 4;

            //idx따로
            //실제로는 아무 의미가 없음
            /*
            int idx4 = (int)position; 
            dummy1Datas = new byte[len];  
            Buffer.BlockCopy(datas,idx4,dummy1Datas,0,(int)len);
            idx4 = idx4+(int)len;
            */

            //PrintVal();
            return idx;
        }

        private void PrintVal()
        {
            Console.WriteLine("len:" + len);
            Console.WriteLine("position:{0} ",position); 
        }

        public uint Len { get => len; }
        public uint Position { get => position;  } 
    }


    class FrameInfo
    {
        uint frameCount = 0; 
        Frame[] frames = null;

        public int FillData(byte[] datas,int idx,int version,int dataPotision,bool hasPalette,byte[] palette)
        {
            frameCount = BitConverter.ToUInt16(datas, idx);
            idx = idx + 2;
            if(frameCount!=0)
            {
                frames = new Frame[frameCount];
                for(int i=0;i<frames.Length;i++)//#임시로 일단 1개만 처리
                {
                    frames[i] = new Frame();
                    idx = frames[i].FillData(datas,idx,version,dataPotision,hasPalette,palette);

                    //Console.WriteLine("");
                    //Console.WriteLine("");
                    //frames[i].SaveImage(idx.ToString());
                }
            }
            //PrintVal();
            
            return idx;
        }

         private void PrintVal()
        {
            Console.WriteLine("frameCount:" + frameCount); 
        }

        public void SaveFrame(string filename)
        { 
            for(int i=0;i<frames.Length;i++)
            {
                Console.WriteLine("Save Frame:" + i + "/" + frames.Length);
                frames[i].SaveImage(filename+"_"+i);
            }
        }


        //imageSet단위로 돌려줌

        public ImagesWithPosition ConvertToLib(string filename)
        {
            //imageSetCount==2 => with mask
            /*
            List<Bitmap> bitmaps = new List<Bitmap>();
            List<int> xVals = new List<int>();
            List<int> yVals = new List<int>();
            */

            ImagesWithPosition imagesWithPosition = new ImagesWithPosition();


            for (int i = 0; i < frames.Length; i++)
            {
                Console.WriteLine("FrameIdx:" + i+"/"+frames.Length);
                imagesWithPosition.bitmaps.Add(frames[i].ConvertToLib(filename));
                imagesWithPosition.xVals.Add(frames[i].Top);
                imagesWithPosition.yVals.Add(frames[i].Left);
            }
            return imagesWithPosition;

           
        }
    }

    class ImagesWithPosition
    {
        public List<Bitmap> bitmaps = new List<Bitmap>();
        public List<int> xVals = new List<int>();
        public List<int> yVals = new List<int>();
    }

    class Frame
    {
         byte[] colorData = null;
         byte[] colorDataRGBA = null;

         int top = 0;
         int left = 0;
         int bottom = 0;
         int right = 0;
         uint flag = 0;

         uint alphaLen=0;
         uint alphaOffset=0;
         uint baseOffset=0;
         uint baseLen=0;
         byte depthType=0;
         ushort depthVal2=0;
         ushort depthNearestDist=0;
         uint depthOffset=0;
         uint depthSize=0;
         uint dummy2Len=0;
         uint dummy2Position=0;
         byte[] dummy2Data=new byte[0];
         uint[] alphaData = new uint[0];
         byte[] alphaMaskAndLen = new byte[0];
        

        public int FillData(byte[] datas, int idx,int version,int dataPotision,bool hasPalette, byte[] palette)
        {  
             //Console.WriteLine("####1####Frame FillData IDX:" +idx);
             //Console.ReadLine();
            
            top = BitConverter.ToInt16(datas, idx);
            idx = idx + 2;

            left = BitConverter.ToInt16(datas, idx);
            idx = idx + 2;

            bottom = BitConverter.ToInt16(datas, idx);
            idx = idx + 2;

            right = BitConverter.ToInt16(datas, idx);
            idx = idx + 2;
            
            flag = BitConverter.ToUInt32(datas, idx);
            idx = idx + 4;

            if(HasAlpha)
            {
                  alphaLen = BitConverter.ToUInt32(datas, idx);
                  idx = idx + 4;
                  
                  alphaOffset = BitConverter.ToUInt32(datas, idx);
                  idx = idx + 4;
            }
            if(HasBase)
            {
                  baseOffset = BitConverter.ToUInt32(datas, idx);
                  idx = idx + 4;
                  
                  if(version==14)
                  {
                      baseLen = (uint)(FrameHeight*FrameWidth);
                  }
                  else
                  {
                      baseLen = BitConverter.ToUInt32(datas, idx);
                      idx = idx + 4;
                  }
            }
            if(HasDepth)
            {
                if(version==14)
                {
                    uint ver14_depth_1 = 0;
                    uint ver14_depth_2 = 0;

                    ver14_depth_1 = BitConverter.ToUInt32(datas, idx);
                    idx = idx + 4;

                    ver14_depth_2 = BitConverter.ToUInt32(datas, idx);
                    idx = idx + 4;
                }
                else if(version==16)
                { 
                    ushort tempDepthSize; 
                    ushort tempDepthSize2;  

                    tempDepthSize = BitConverter.ToUInt16(datas, idx);
                    idx = idx + 2;

                    tempDepthSize2 = BitConverter.ToUInt16(datas, idx);
                    idx = idx + 2;

                    depthSize = (uint)tempDepthSize2; 
                    depthSize = BitConverter.ToUInt32(datas, idx);
                    idx = idx + 4; 
                }
                else
                {
                    depthType = datas[idx];
                    idx++;

                    depthVal2 = BitConverter.ToUInt16(datas, idx);
                    idx = idx + 2;

                    depthNearestDist = BitConverter.ToUInt16(datas, idx);
                    idx = idx + 2;

                    depthOffset = BitConverter.ToUInt32(datas, idx);
                    idx = idx + 4;

                    depthSize = BitConverter.ToUInt32(datas, idx);
                    idx = idx + 4;
                } 
            }
            //PrintVal();
            dummy2Len = BitConverter.ToUInt32(datas, idx);
            idx=idx+4;
            if(dummy2Len!=0)
            {
                dummy2Position = BitConverter.ToUInt32(datas, idx);
                idx=idx+4;
                int idx3=(int)dummy2Position;
 
                dummy2Data = new byte[dummy2Len];
                for(int i=0;i<dummy2Data.Length;i++)
                {
                    dummy2Data[i] = datas[idx3];
                    idx3++;
                }
            } 

            if(HasAlpha)
            {
                int idx2 = (int)(dataPotision + alphaOffset); //이부분에서 따로 돌림
                alphaData = new uint[FrameHeight];
                for (int i = 0; i < alphaData.Length; i++)
                {
                    alphaData[i] = BitConverter.ToUInt32(datas, idx2);
                    idx2 = idx2 + 4;
                } 
                 
                alphaMaskAndLen = new byte[FrameHeight*FrameHeight];

                int aMaskLenIdx = 0;
                while (true)
                {
                    if (idx2 >= dataPotision + alphaOffset + alphaLen) break;
                    if(aMaskLenIdx>=alphaMaskAndLen.Length)break;
                    
                    alphaMaskAndLen[aMaskLenIdx] = datas[idx2];
                    idx2++;
                    aMaskLenIdx++;
                }
 
                if(hasPalette)
                {
                    //idx=0;
                     colorData = new byte[FrameWidth * FrameHeight * 2];
                     uint colorIdx = (uint)(baseOffset+dataPotision);
                    
                     byte[] AlphaMask =  GetAlphaMask();
                     for(int i=0;i<AlphaMask.Length;i++)
                     { 
                            if (AlphaMask[i] == 0x00)
                            {
                                colorData[i * 2] = 0x1f;
                                colorData[i * 2 + 1] = 0xf8; 

                            }
                            else
                            {
                                if (baseOffset == 0)
                                { 
                                    colorData[i * 2] = 0x00;
                                    colorData[i * 2 + 1] = 0x00;
                                }
                                else
                                {
                                    colorData[i * 2] = palette[datas[colorIdx] * 2];
                                    colorData[i * 2 + 1] = palette[datas[colorIdx] * 2 + 1];
                                    colorIdx++;
                                }
                            }
                            idx2++; 
                     }

                    //PrintVal();
                }
                else
                {
                    colorData = new byte[FrameWidth * FrameHeight * 2];
                    uint colorIdx = (uint)(baseOffset + dataPotision);

                    byte[] AlphaMask = GetAlphaMask();
                    for (int i = 0; i < AlphaMask.Length; i++)
                    {
                        if (AlphaMask[i] == 0x00)
                        {
                            colorData[i * 2] = 0x1f;
                            colorData[i * 2 + 1] = 0xf8;

                        }
                        else
                        {
                            if (baseOffset == 0)
                            {
                                colorData[i * 2] = 0x00;
                                colorData[i * 2 + 1] = 0x00;
                            }
                            else
                            {
                                colorData[i * 2] = datas[colorIdx++];
                                colorData[i * 2 + 1] = datas[colorIdx++];
                            }
                        }
                    }
                }
            }
            else
            {
                if (hasPalette)
                { 
                    colorData = new byte[FrameWidth * FrameHeight * 2];  
                    uint colorIdx = (uint)(baseOffset+dataPotision);

                    for (int i = 0; i < baseLen; ++i)
                    { 
                        colorData[i * 2] = palette[datas[colorIdx] * 2];
                        colorData[i * 2 + 1] = palette[datas[colorIdx] * 2 + 1];
                        colorIdx++;
                    } 
                }
                else
                {
                    colorData = new byte[FrameWidth * FrameHeight * 2];
                    uint colorIdx = (uint)(baseOffset+dataPotision);

                    for (int i = 0; i < colorData.Length; i++)
                    {
                         colorData[i] = datas[colorIdx++]; 
                    }
                }
            }
            return idx;
        }

        public bool SaveImage(string filename)
        {
            if (colorData == null) return false;
            if(FrameHeight==0 || FrameWidth==0)
            {
                Console.WriteLine("Image Size Err. FrameHeight:"+FrameHeight+" FrameWidth:"+FrameWidth);
                return false;
            }

            colorDataRGBA = new byte[colorData.Length*2];
            int argbIdx=0;
            for(int i=0;i<colorData.Length;i=i+2)
            {
                ushort data = BitConverter.ToUInt16(colorData,i);
                byte r = (byte)(((data & 0xF800)>>11)<<3);
                byte g = (byte)(((data & 0x7E0)>>5)<<2);
                byte b = (byte)((data)<<3);

                colorDataRGBA[argbIdx] = b;//b
                colorDataRGBA[argbIdx+1] = g;//g
                colorDataRGBA[argbIdx+2] = r;//r
                colorDataRGBA[argbIdx+3] = 0xff;//a

                //transperent apply
                if(HasAlpha)
                {
                    byte[] AlphaMask =  GetAlphaMask();
                    /*
                    if (AlphaMask[i/2] == 0x00)
                    {
                        colorDataRGBA[argbIdx] = 0x00;//b
                        colorDataRGBA[argbIdx + 1] = 0x00;//g
                        colorDataRGBA[argbIdx + 2] = 0x00;//r
                        colorDataRGBA[argbIdx + 3] = 0x00;//a
                    }*/
                    colorDataRGBA[argbIdx + 3] = AlphaMask[i/2];//a 
                }

                argbIdx=argbIdx+4;
            }
             
            DirectoryInfo di = new DirectoryInfo(".\\YPF_OUT");
            if(!di.Exists) di.Create();
 
            
            Bitmap bitmap = new Bitmap(FrameWidth, FrameHeight, PixelFormat.Format32bppArgb);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            int rowSize = Math.Abs(bmpData.Stride);
            IntPtr bmpScan0 = bmpData.Scan0;
            for (int y = 0; y < FrameHeight; y++)
            {
                Marshal.Copy(colorDataRGBA, y * FrameWidth * 4, IntPtr.Add(bmpScan0, y * rowSize), FrameWidth * 4);
            }
            bitmap.UnlockBits(bmpData);
            bitmap.Save(".\\YPF_OUT\\" + filename+".png", ImageFormat.Png);
            //bitmap.Save(".\\out\\"+filename+".png", ImageFormat.Png);
            bitmap.Dispose();


            /*
            //Bitmap bitmap = new Bitmap(FrameWidth, FrameHeight, PixelFormat.Format32bppArgb);
            Bitmap bitmap = new Bitmap(FrameWidth, FrameHeight, PixelFormat.Format16bppRgb565);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            int rowSize = Math.Abs(bmpData.Stride);
            IntPtr bmpScan0 = bmpData.Scan0;
            for (int y = 0; y < FrameHeight; y++)
            {
                Marshal.Copy(colorData, y * FrameWidth * 2, IntPtr.Add(bmpScan0, y * rowSize), FrameWidth * 2);
            }
            bitmap.UnlockBits(bmpData);
            bitmap.Save(".\\out\\"+filename+".bmp", ImageFormat.Bmp);
            //bitmap.Save(".\\out\\"+filename+".png", ImageFormat.Png);
            bitmap.Dispose();
            */


            return true;
        }


        public Bitmap ConvertToLib(string filename)
        {
            if (colorData == null) return null;
            if (FrameHeight == 0 || FrameWidth == 0)
            {
                Console.WriteLine("Image Size Err. FrameHeight:" + FrameHeight + " FrameWidth:" + FrameWidth);
                return null;
            }

            colorDataRGBA = new byte[colorData.Length * 2];
            int argbIdx = 0;
            for (int i = 0; i < colorData.Length; i = i + 2)
            {
                ushort data = BitConverter.ToUInt16(colorData, i);
                byte r = (byte)(((data & 0xF800) >> 11) << 3);
                byte g = (byte)(((data & 0x7E0) >> 5) << 2);
                byte b = (byte)((data) << 3);

                colorDataRGBA[argbIdx] = b;//b
                colorDataRGBA[argbIdx + 1] = g;//g
                colorDataRGBA[argbIdx + 2] = r;//r
                colorDataRGBA[argbIdx + 3] = 0xff;//a

                //transperent apply
                if (HasAlpha)
                {
                    byte[] AlphaMask = GetAlphaMask(); 
                    colorDataRGBA[argbIdx + 3] = AlphaMask[i / 2];//a 
                }

                argbIdx = argbIdx + 4;
            }



            DirectoryInfo di = new DirectoryInfo(".\\out");
            if (!di.Exists) di.Create();

             
            Bitmap bitmap = new Bitmap(FrameWidth, FrameHeight, PixelFormat.Format32bppArgb);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            int rowSize = Math.Abs(bmpData.Stride);
            IntPtr bmpScan0 = bmpData.Scan0;
            for (int y = 0; y < FrameHeight; y++)
            {
                Marshal.Copy(colorDataRGBA, y * FrameWidth * 4, IntPtr.Add(bmpScan0, y * rowSize), FrameWidth * 4);
            }
            bitmap.UnlockBits(bmpData);
            
            //bitmap.Dispose();

            return bitmap;
        }

        public byte[] GetAlphaMask()
        {
            int index = 0; 
            byte[] alphaMask = new byte[FrameHeight * FrameWidth]; 
            int alphaMaskIdx = 0;
            while (true)
            {
                if (index + 1 >= alphaMaskAndLen.Length) break;

                var alphaMaskRowlen = ((alphaMaskAndLen[index] & 7) << 8 | alphaMaskAndLen[index + 1]);
                byte alphaMaskVal = (byte)(alphaMaskAndLen[index] & 0xf8);
                index = index + 2;

                for (int j = 0; j < (alphaMaskRowlen); j++)
                {
                    if (alphaMask.Length <= alphaMaskIdx) break; 
                    alphaMask[alphaMaskIdx] = alphaMaskVal; 
                    alphaMaskIdx++;
                }
            }
            return alphaMask;
        }

        //how to use rows?
        public uint[] ROWS
        {
            get 
            {
                uint[] rows = new uint[FrameHeight];
                rows[0] = alphaData[0]/2; 
                for(int i=1;i<rows.Length;i++)
                {
                    rows[i] = (alphaData[i] - alphaData[i-1])/2; 
                }
                return rows;
            }
        }

        
        public bool HasAlpha { get => (flag & 8) == 0x0; }//bits[3] == 0 => hasAlpha=true
        public bool HasBase { get =>  (flag & 4) != 0x0; }//bits[2] == 1 => hasBase=true
        public bool HasDepth { get =>  (flag & 1) != 0x0; }//bits[0] == 1 => hasDepth=true

        public int Top { get => top; }
        public int Left { get => left; }

        public int FrameHeight { get => bottom-top; } 
        public int FrameWidth { get => right-left; } 

         private void PrintVal()
        {
            Console.WriteLine("top :" +  top ); 
            Console.WriteLine("left :" +  left ); 
            Console.WriteLine("bottom :" +  bottom ); 
            Console.WriteLine("right :" +  right );  
            Console.WriteLine("FrameWidth :" +  FrameWidth );  
            Console.WriteLine("FrameWidth :" +  FrameHeight );  
            Console.WriteLine("HasAlpha :" +  HasAlpha );  
            Console.WriteLine("HasBase :" +  HasBase );  
            Console.WriteLine("HasDepth :" +  HasDepth );  
            Console.WriteLine("alphaLen :" +  alphaLen );  
            Console.WriteLine("alphaOffset :" +  alphaOffset );  
            Console.WriteLine("baseOffset :" +  baseOffset );  
            Console.WriteLine("baseLen :" +  baseLen );  
            Console.WriteLine("depthType :" +  depthType );  
            Console.WriteLine("depthVal2 :" +  depthVal2 );  
            Console.WriteLine("depthNearestDist :" +  depthNearestDist );  
            Console.WriteLine("depthOffset :" +  depthOffset );  
            Console.WriteLine("depthSize :" +  depthSize );  
            Console.WriteLine("dummy2Len :" +  dummy2Len );  
            Console.WriteLine("dummy2Position :" +  dummy2Position );  
            //Console.WriteLine("dummy2Data[0] :" +  dummy2Data[0] );  
            //Console.WriteLine("alphaData[0] :" +   alphaData[0] );  
            //Console.WriteLine("alphaMaskAndLen[0] :" +   alphaMaskAndLen[0] );  
            //Console.WriteLine("ROWS[0] :" +   ROWS[0] );   
            //Console.WriteLine("AlphaMask[0] :" +   GetAlphaMask()[0] );   
        }

    }


    class ActionInfo
    {
        ushort actionCount = 0;
        Action[] actions = new Action[0];
        
        public int FillData(byte[] datas, int idx)
        {   
            actionCount = BitConverter.ToUInt16(datas, idx);
            idx=idx+2;
            
            actions = new Action[actionCount];
            for(int i=0;i<actions.Length;i++)
            {
                actions[i] = new Action();
                idx = actions[i].FillData(datas,idx);
            } 
            return idx;
        }

         private void PrintVal()
        {
            Console.WriteLine("actionCount :" +  actionCount ); 
            Console.WriteLine("actions.Len :" +  actions.Length ); 
        }
    }

    
    class Action
    { 
        uint actionSize=0;
        uint actionOffset=0;
        byte[] time1 = new byte[6];
        byte[] time2 = new byte[6];
        ushort actionElementCount=0;
        ActionElement[] actionElements = new ActionElement[0];

        public int FillData(byte[] datas, int idx)
        {  
            actionSize = BitConverter.ToUInt32(datas, idx);
            idx=idx+4;

            if (actionSize != 0)
            {
                actionOffset = BitConverter.ToUInt32(datas, idx);
                idx = idx + 4;
            }
 
            //시간 부분 사실은 2바이트 단위로 반복해서 처리됨
            /*
              for (int timeIndex = 0; timeIndex <= 2; ++timeIndex)                                            //   for i=0 to 2
            {                                                                                                    
                br.Read(out time1[timeIndex * 2]);                                                          //     time1[i*2].b
                br.Read(out time1[timeIndex * 2 + 1]);                                                      //     time1[i*2+1].b
                br.Read(out time2[timeIndex * 2]);                                                          //     time2[i*2].b
                br.Read(out time2[timeIndex * 2 + 1]);                                                      //     time2[i*2+1].b
            }
            */
            Buffer.BlockCopy(datas,idx,time1,0,time1.Length);
            idx=idx+6;

            Buffer.BlockCopy(datas,idx,time2,0,time2.Length);
            idx=idx+6;
  
            actionElementCount = BitConverter.ToUInt16(datas, idx);
            idx=idx+2;

            
            actionElements = new ActionElement[actionElementCount];
            for(int i=0;i<actionElements.Length;i++)
            {
                actionElements[i] = new ActionElement();
                idx = actionElements[i].FillData(datas,idx);
            }
            return idx;
        }    
    }


    class ActionElement
    {
        ushort frameIdx=0;
        uint time = 0;
        uint actionElementLen = 0;
        uint actionElementOffset = 0;
        byte offsetX = 0;
        byte offsetY=0;
        ushort dummy = 0;

        public int FillData(byte[] datas, int idx)
        {  
           
            frameIdx = BitConverter.ToUInt16(datas, idx);
            idx=idx+2;

            time = BitConverter.ToUInt32(datas, idx);
            idx=idx+4;

            actionElementLen = BitConverter.ToUInt32(datas, idx);
            idx=idx+4;
             

            if (actionElementLen != 0)
            {
                actionElementOffset = BitConverter.ToUInt32(datas, idx);
                idx = idx + 4;
 
                uint idx5 = actionElementOffset; 
                for (int i = 0; i < actionElementLen; i++)
                {
                    offsetX = datas[idx5];
                    idx5 = idx5 + 1;

                    offsetY = datas[idx5];
                    idx5 = idx5 + 1;

                    dummy = BitConverter.ToUInt16(datas, (int)idx5);
                    idx5 = idx5 + 2;
                }
            }
 
            return idx;
        }   
    }

    class StateInfo
    {
        uint stateCount=0;
        State[] states = new State[0];
        public int FillData(byte[] datas, int idx)
        {
           
            stateCount = BitConverter.ToUInt16(datas, idx);
            idx=idx+2;

            
            states = new State[stateCount];
            for(int i=0;i<states.Length;i++)
            { 
                states[i] = new State();
                idx = states[i].FillData(datas, idx);
            }
            return idx;
        }
    }

    class State
    {
        uint stateSize;
        uint stateOffset = 0;
        ushort stateElemCount;
        StateElement[] stateElements = new StateElement[0];


         public int FillData(byte[] datas, int idx)
        {
            stateSize = BitConverter.ToUInt32(datas, idx);
            idx=idx+4;

            if(stateSize!=0)
            {
                stateOffset = BitConverter.ToUInt32(datas, idx);
                idx=idx+4;
            }

            stateElemCount = BitConverter.ToUInt16(datas, idx);
            idx=idx+2;
            
            

            stateElements = new StateElement[stateElemCount];
            for(int i=0;i<stateElemCount;i++)
            {
                stateElements[i] = new StateElement();
                idx=stateElements[i].FillData(datas,idx);
            }
            return idx;
        } 
    }


    class StateElement
    {
        byte isStateElementFrame;
        byte dummy1;
        byte dummy2;
        ushort dummy3;
        uint dummy4 = 0;
        uint stateElementSize;
        uint stateElementOffset = 0;
           public int FillData(byte[] datas, int idx)
        {
            
            isStateElementFrame = datas[idx];
            idx=idx+1;
            
            dummy1 = datas[idx];
            idx=idx+1;

            dummy2 = datas[idx];
            idx=idx+1;
            
            dummy3 = BitConverter.ToUInt16(datas, idx);
            idx=idx+2;

            if(isStateElementFrame!=0)
            { 
              
                dummy4 = BitConverter.ToUInt32(datas, idx);
                idx=idx+4;
            }

            stateElementSize = BitConverter.ToUInt32(datas, idx);
            idx=idx+4;

            if(stateElementSize!=0)
            {
                stateElementOffset = BitConverter.ToUInt32(datas, idx);
                idx=idx+4;
            }

            return idx;
        } 
    }

    class StateTransValue
    {
        ushort stateTransValueCount=0;
        StateTrans[] stateTranses = new StateTrans[0];
        public int FillData(byte[] datas, int idx)
        {
            stateTransValueCount = BitConverter.ToUInt16(datas, idx);
            idx=idx+2;

            stateTranses = new StateTrans[stateTransValueCount];

            for(int i=0;i<stateTranses.Length;i++)
            {
                stateTranses[i]=new StateTrans();
                idx = stateTranses[i].FillData(datas,idx);
            }

            return idx;
        }
    }

    class StateTrans
    {
        ushort key1;
        ushort key2;
        ushort value;
        uint stateTransValueSize=0;
        uint stateTransValueOffset = 0;

        public int FillData(byte[] datas, int idx)
        {
            key1 = BitConverter.ToUInt16(datas, idx);
            idx=idx+2;

            key2 = BitConverter.ToUInt16(datas, idx);
            idx=idx+2;
 
            value = BitConverter.ToUInt16(datas, idx);
            idx=idx+2;
 
            stateTransValueSize = BitConverter.ToUInt32(datas, idx);
            idx=idx+4;
            if(stateTransValueSize!=0)
            {
                stateTransValueOffset = BitConverter.ToUInt32(datas, idx);
                idx=idx+4;
            }

            idx+=(int)stateTransValueOffset;

            return idx;
        }  
    }
}