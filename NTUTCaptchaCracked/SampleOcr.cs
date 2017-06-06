using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace NTUTCaptchaCracked
{
  /// <summary>
  /// 根據樣本做驗證碼破解
  /// 
  /// 需要在.config文件中的appSettings配置節中添加key為sampleOcr.sampleDir value設置為樣本圖片所在路徑
  /// 驗證碼：https://investorservice.cfmmc.com/https://investorservice.cfmmc.com/veriCode.do?t=1335521167762&ip=202.99.16.22
  /// 
  /// outofmemory.cn 20120427
  /// 100個樣例準確數為88個，錯誤主要發生在389這三個字符的混淆上
  /// </summary>
  public abstract class SampleOcr
  {
      /// <summary>
      /// 灰度中間值
      /// </summary>
      static int MiddleGrayValue =200;

      /// <summary>
      /// 分割圖片的差異容忍度
      /// </summary>
      static int ColorToleranceForSplit = 30;

      /// <summary>
      /// 樣本字典
      /// </summary>
      static Dictionary<string, Bitmap> _samples;

      /// <summary>
      /// 相似度輸出值
      /// </summary>
      public static float LastSimilar = 0;

      /// <summary>
      /// 破解驗證碼
      /// </summary>
      /// <param name="bm">驗證碼圖片</param>
      /// <returns>驗證碼文本</returns>
      public static string Ocr(Bitmap bm)
      {
           //做灰度處理
           GrayByPixels(bm);

           bm =RemoveVerticalSpaceRegion(bm);

           Bitmap[] splitBms =SplitBitmaps(bm);

           char[] result = new char[splitBms.Length];
           for (int i = 0; i <splitBms.Length; i++)
           {
               result[i] =OcrChar(splitBms[i]);
               splitBms[i].Dispose();
          }
           return new string(result);
      }

      /// <summary>
      /// 分割圖片
      /// </summary>
      /// <param name="bm">圖片</param>
      /// <returns>分割後的圖片對象</returns>
      public static Bitmap[] SplitBitmaps(Bitmap bm)
      {
           //找出垂直分割線
           List<int> removeXs = new List<int>();
           for (int x = 0; x <bm.Width; x++)
           {
               bool hasDiffPoint =false;
               Color color =Color.White;
               for (int y = 0; y <bm.Height; y++)
               {
                   if (y == 0)
                   {
                       color =bm.GetPixel(x, y);
                   }
                   else
                   {
                       Color currentColor = bm.GetPixel(x,y);
                       int diff =CalculateColorDifference(currentColor, color);
                       if (diff >ColorToleranceForSplit)
                       {
                           hasDiffPoint = true;
                           break;
                       }
                       // color =currentColor;
                   }
               }

               if (!hasDiffPoint)
               {
                   removeXs.Add(x);
               }
           }

           //根據空白區域，計算各個字符的位圖          
           List<Rectangle> charRects= new List<Rectangle>();
           for (int i = 1; i <removeXs.Count; i++)
           {
               int diff = removeXs[i] -removeXs[i - 1];
               if (diff > 5)
               {
                   if (diff >= 20)
                   {
                       Rectangle rect = new Rectangle(removeXs[i - 1], 0, diff / 2, bm.Height);
                      charRects.Add(rect);

                       rect = new Rectangle(removeXs[i - 1] + diff / 2, 0, diff / 2, bm.Height);
                      charRects.Add(rect);
                   }
                   else
                   {
                       Rectangle rect =new Rectangle(removeXs[i - 1], 0, diff, bm.Height);
                      charRects.Add(rect);
                   }
               }
           }

           int count =charRects.Count;
           Bitmap[] charBms = new Bitmap[count];
           int charBmIndex = 0;
           foreach (Rectangle item in charRects)
           {
               Bitmap bmChar =bm.Clone(item, bm.PixelFormat);
               charBms[charBmIndex] =bmChar;
               charBmIndex += 1;
          }
           return charBms;
      }

      /// <summary>
      /// 解析字元
      /// </summary>
      /// <param name="bm">分割後的小圖</param>
      /// <returns>字元</returns>
      public static char OcrChar(Bitmap bm)
      {
           Dictionary<string,Bitmap> samples = LoadSamples();

           double diff = .0;
           string mayBe = null;
           foreach (string key in samples.Keys)
           {
               double diffRate =CalcImageDiffRate(samples[key], bm);
               if (diffRate == 1)
               {
                   LastSimilar = 1;
                   return key[0];
               }

               if (diffRate >diff)
               {
                   mayBe = key;
                   diff = diffRate;
               }
           }

           if (mayBe == null) throw new ApplicationException();
           LastSimilar = (float)diff;
           return mayBe[0];
      }

      /// <summary>
      /// 解析文字加強
      /// </summary>
      /// <param name="bm">分割後的小圖</param>
      /// <returns>字元</returns>
      public static char OcrCharEx(Bitmap bm)
      {
          Dictionary<string, Bitmap> samples = LoadSamples();

          double diff = .0;
          string mayBe = null;
          foreach (string key in samples.Keys)
          {
              double diffRate = SampleOcr.HisogramComp(samples[key], bm);
              if (diffRate == 1)
              {
                  LastSimilar = 1;
                  return key[0];
              }

              if (diffRate > diff)
              {
                  mayBe = key;
                  diff = diffRate;
              }
          }
          if (mayBe == null) throw new ApplicationException();
          LastSimilar = (float)diff;
          return mayBe[0];
      }

      /// <summary>
      /// 感知哈希算法
      /// </summary>
      /// <param name="bm">分割後的小圖</param>
      /// <returns>字元</returns>
      public static char OcrHash(Bitmap bm)
      {
          Dictionary<string, Bitmap> samples = LoadSamples();

          double diff = .0;
          string mayBe = null;
          foreach (string key in samples.Keys)
          {
              double diffRate = SampleOcr.HashComp(samples[key], bm);
              if (diffRate == 1)
              {
                  LastSimilar = 1;
                  return key[0];
              }

              if (diffRate > diff)
              {
                  mayBe = key;
                  diff = diffRate;
              }
          }
          if (mayBe == null) throw new ApplicationException();
          LastSimilar = (float)diff;
          return mayBe[0];
      }

      /// <summary>
      /// 載入樣本字典
      /// </summary>
      /// <returns>樣本字典</returns>
      private static Dictionary<string, Bitmap> LoadSamples()
      {
           if (_samples == null)
           {
               _samples = new Dictionary<string, Bitmap>();
               string sampleDir = System.Windows.Forms.Application.StartupPath + @"\samples";
               DirectoryInfo dirInfo = new DirectoryInfo(sampleDir);
               FileInfo[] files =dirInfo.GetFiles("*.bmp");
               foreach (FileInfo item in files)
               {
                   Bitmap bm =(Bitmap)Bitmap.FromFile(item.FullName);
                   string key =Path.GetFileNameWithoutExtension(item.FullName);
                   _samples.Add(key,bm);
               }
           }

           return _samples;
      }

      /// <summary>
      /// 根據RGB，計算灰度值
      /// </summary>
      /// <param name="posClr">Color值</param>
      /// <returns>灰度值，整型</returns>
      static int GetGrayNumColor(System.Drawing.Color posClr)
      {
           return (posClr.R * 19595 +posClr.G * 38469 + posClr.B * 7472) >> 16;
      }

      /// <summary>
      /// 灰度轉換,逐點方式
      /// </summary>
      static void GrayByPixels(Bitmap bm)
      {
           for (int i = 0; i <bm.Height; i++)
           {
               for (int j = 0; j <bm.Width; j++)
               {
                   int tmpValue =GetGrayNumColor(bm.GetPixel(j, i));
                   bm.SetPixel(j, i,Color.FromArgb(tmpValue, tmpValue, tmpValue));
               }
          }
      }

      /// <summary>
      /// 刪除垂直方向上的空白區域
      /// </summary>
      /// <param name="bm">源圖片</param>
      /// <returns>刪除空白之後的圖片</returns>
      public static Bitmap RemoveVerticalSpaceRegion(Bitmap bm)
      {
           int topSpaceHeight = 0;
           for (int y = 0; y <bm.Height; y++)
           {
               bool hasDiffPoint =false;
               Color color =Color.White;
               for (int x = 0; x <bm.Width; x++)
               {
                   if (x == 0)
                   {
                       color =bm.GetPixel(x, y);
                   }
                   else
                   {
                       Color currentColor= bm.GetPixel(x, y);
                       int diff =CalculateColorDifference(currentColor, color);
                       if (diff >ColorToleranceForSplit)
                       {
                           hasDiffPoint =true;
                           break;
                       }
                   }
               }

               if (hasDiffPoint)
               {
                   break;
               }
               else
               {
                   topSpaceHeight +=1;
               }
           }

           int bottomSpaceHeight = 0;
           for (int y = bm.Height - 1; y> 0; y--)
           {
               bool hasDiffPoint =false;
               Color color =Color.White;
               for (int x = 0; x <bm.Width; x++)
               {
                   if (x == 0)
                   {
                       color =bm.GetPixel(x, y);
                   }
                   else
                   {
                       Color currentColor= bm.GetPixel(x, y);
                       int diff =CalculateColorDifference(currentColor, color);
                       if (diff >ColorToleranceForSplit)
                       {
                           hasDiffPoint =true;
                           break;
                       }
                       color =currentColor;
                   }
               }

               if (hasDiffPoint)
               {
                   break;
               }
               else
               {
                   bottomSpaceHeight +=1;
               }
           }

           Rectangle rectValid = new Rectangle(0, topSpaceHeight, bm.Width, bm.Height - topSpaceHeight -bottomSpaceHeight);
           Bitmap newBm =bm.Clone(rectValid, bm.PixelFormat);
           bm.Dispose();
           return newBm;
      }

      private static double CalcImageDiffRate(Bitmap bmSample, Bitmap bmCalc)
      {
           int[] eSample = new int[bmSample.Height];
           int[] eCalc = new int[bmSample.Height];
           for (int y = 0; y <bmSample.Height; y++)
           {
              eSample[y] =GetHorizontalValue(bmSample, y);
               eCalc[y] =GetHorizontalValue(bmCalc, y);
           }
           return GetCosine(eSample,eCalc);
      }

      /// <summary>
      /// 獲得向量的cos值
      /// </summary>
      /// <param name="e1"></param>
      /// <param name="e2"></param>
      /// <returns></returns>
      static double GetCosine(int[] e1, int[] e2)
      {
           double fenzi = 0;
           for (int i = 0; i <e1.Length; i++)
           {
               fenzi += e1[i] *e2[i];
           }

           double fenmuLeft = 0;
           double fenmuRight = 0;
          for (int i = 0; i <e1.Length; i++)
           {
               fenmuLeft += e1[i] *e1[i];
               fenmuRight += e2[i] *e2[i];
           }

           double fenmu =Math.Sqrt(fenmuLeft) * Math.Sqrt(fenmuRight);
           if (fenmu == 0.0) return 0;

           return fenzi / fenmu;
      }

      /// <summary>
      /// 計算水平方向上的差異點數
      /// </summary>
      /// <param name="bm">位圖</param>
      /// <param name="y">y坐標值</param>
      /// <returns>差異點數</returns>
      private static int GetHorizontalValue(Bitmap bm, int y)
      {
           if (y >= bm.Height) return 0;
           int val = 0;
           for (int x = 0; x <bm.Width; x++)
           {
               Color color =bm.GetPixel(x, y);

               int grayVal =GetColorGrayValue(color);
               if (grayVal >MiddleGrayValue)
               {
                   val |= (1 <<x);
               }
           }
           return val;
      }

      static int GetColorGrayValue(Color color)
      {
           return (int)(.299 * color.R +.587 * color.G + .114 * color.B);
      }

      /// <summary>
      /// 計算顏色之間的差值，這個只是一個簡單的計算，真正的色差計算很復雜
      /// </summary>
      /// <param name="colorA">A色</param>
      /// <param name="colorB">B色</param>
      /// <returns>差值</returns>
      static int CalculateColorDifference(Color colorA, Color colorB)
      {
           int diff =GetColorGrayValue(colorA) - GetColorGrayValue(colorB);
           return Math.Abs(diff);
      }

      #region 修剪圖片周圍白色區域
      public static Bitmap Trim(Bitmap bitmap)
      {
          Bitmap resultBmp = null;

          BitmapData bData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

          unsafe
          {
              int width = bitmap.Width;
              int height = bitmap.Height;

              int newx = 0, newy = 0, newHeight = 0, newWidth = 0;

              bool isbreak = false;

              //得到x坐標
              for (int x = 0; x < width; x++)
              {
                  for (int y = 0; y < height; y++)
                  {
                      byte* color = (byte*)bData.Scan0 + x * 3 + y * bData.Stride;
                      int R = *(color + 2);
                      int G = *(color + 1);
                      int B = *color;

                      if (R != 255 || G != 255 || B != 255)
                      {
                          newx = x;
                          isbreak = true;
                          break;
                      }
                  }

                  if (isbreak)
                  {
                      break;
                  }
              }

              isbreak = false;

              //得到y坐標
              for (int y = 0; y < height; y++)
              {
                  for (int x = 0; x < width; x++)
                  {
                      byte* color = (byte*)bData.Scan0 + x * 3 + y * bData.Stride;
                      int R = *(color + 2);
                      int G = *(color + 1);
                      int B = *color;

                      if (R != 255 || G != 255 || B != 255)
                      {
                          newy = y;
                          isbreak = true;
                          break;
                      }
                  }

                  if (isbreak)
                  {
                      break;
                  }
              }

              isbreak = false;

              int tmpy = 0;

              //得到height
              for (int y = height - 1; y >= 0; y--)
              {
                  for (int x = 0; x < width; x++)
                  {
                      byte* color = (byte*)bData.Scan0 + x * 3 + y * bData.Stride;
                      int R = *(color + 2);
                      int G = *(color + 1);
                      int B = *color;

                      if (R != 255 || G != 255 || B != 255)
                      {
                          tmpy = y;
                          isbreak = true;
                          break;
                      }
                  }

                  if (isbreak)
                  {
                      break;
                  }
              }

              isbreak = false;

              newHeight = tmpy - newy + 1;

              int tmpx = 0;

              //得到width
              //得到x坐標
              for (int x = width - 1; x >= 0; x--)
              {
                  for (int y = 0; y < height; y++)
                  {
                      byte* color = (byte*)bData.Scan0 + x * 3 + y * bData.Stride;
                      int R = *(color + 2);
                      int G = *(color + 1);
                      int B = *color;

                      if (R != 255 || G != 255 || B != 255)
                      {
                          Color newColor = Color.FromArgb(R, G, B);
                          tmpx = x;
                          isbreak = true;
                          break;
                      }
                  }

                  if (isbreak)
                  {
                      break;
                  }
              }

              newWidth = tmpx - newx + 1;

              Rectangle rect = new Rectangle(newx, newy, newWidth, newHeight);

              resultBmp = new Bitmap(newWidth, newHeight);

              resultBmp = bitmap.Clone(rect, bitmap.PixelFormat);
          }

          bitmap.UnlockBits(bData);

          return resultBmp;
      }
      #endregion

      #region 圖片相似度比對
      public static float HisogramComp(Bitmap SorceBmp, Bitmap DicBmp)
      {
          Bitmap SorceBmp_ = new Bitmap(SorceBmp, 20, 20);
          Bitmap DicBmp_ = new Bitmap(DicBmp, 20, 20);
          return GetResult(GetHisogram(SorceBmp_), GetHisogram(DicBmp_));
      }

      private static int[] GetHisogram(Bitmap img)
      {
          BitmapData data = img.LockBits(new System.Drawing.Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
          int[] histogram = new int[256];
          unsafe
          {
              byte* ptr = (byte*)data.Scan0;
              int remain = data.Stride - data.Width * 3;
              for (int i = 0; i < histogram.Length; i++)
                  histogram[i] = 0;
              for (int i = 0; i < data.Height; i++)
              {
                  for (int j = 0; j < data.Width; j++)
                  {
                      int mean = ptr[0] + ptr[1] + ptr[2];
                      mean /= 3;
                      histogram[mean]++;
                      ptr += 3;
                  }
                  ptr += remain;
              }
          }
          img.UnlockBits(data);
          return histogram;
      }

      //計算相減後的絕對值
      private static float GetAbs(int firstNum, int secondNum)
      {
          float abs = Math.Abs((float)firstNum - (float)secondNum);
          float result = Math.Max(firstNum, secondNum);
          if (result == 0)
              result = 1;
          return abs / result;
      }

      //最終計算結果
      private static float GetResult(int[] firstNum, int[] scondNum)
      {
          if (firstNum.Length != scondNum.Length)
          {
              return 0;
          }
          else
          {
              float result = 0;
              int j = firstNum.Length;
              for (int i = 0; i < j; i++)
              {
                  result += 1 - GetAbs(firstNum[i], scondNum[i]);
                  //Console.WriteLine(i + "----" + result);
              }
              return result / j;
          }
      }
      #endregion

      #region 感知哈希算法

      public static float HashComp(Bitmap SorceBmp, Bitmap DicBmp)
      {
          string SorceBmp_ = GetHash(SorceBmp);
          string DicBmp_ = GetHash(DicBmp);
          float i = (float)CalcSimilarDegree(SorceBmp_, DicBmp_) / SorceBmp_.Length;
          return i;
      }

      private static String GetHash(Image SourceImg)
      {
          Image image = ReduceSize(SourceImg);
          Byte[] grayValues = ReduceColor(image);
          Byte average = CalcAverage(grayValues);
          String reslut = ComputeBits(grayValues, average);
          return reslut;
      }

      // Step 1 : Reduce size to 20*20
      private static Image ReduceSize(Image SourceImg, int width = 20, int height = 20)
      {
          Image image = SourceImg.GetThumbnailImage(width, height, () => { return false; }, IntPtr.Zero);
          return image;
      }

      // Step 2 : Reduce Color
      private static Byte[] ReduceColor(Image image)
      {
          Bitmap bitMap = new Bitmap(image);
          Byte[] grayValues = new Byte[image.Width * image.Height];

          for (int x = 0; x < image.Width; x++)
              for (int y = 0; y < image.Height; y++)
              {
                  Color color = bitMap.GetPixel(x, y);
                  byte grayValue = (byte)((color.R * 30 + color.G * 59 + color.B * 11) / 100);
                  grayValues[x * image.Width + y] = grayValue;
              }
          return grayValues;
      }

      // Step 3 : Average the colors
      private static Byte CalcAverage(byte[] values)
      {
          int sum = 0;
          for (int i = 0; i < values.Length; i++)
              sum += (int)values[i];
          return Convert.ToByte(sum / values.Length);
      }

      // Step 4 : Compute the bits
      private static String ComputeBits(byte[] values, byte averageValue)
      {
          char[] result = new char[values.Length];
          for (int i = 0; i < values.Length; i++)
          {
              if (values[i] < averageValue)
                  result[i] = '0';
              else
                  result[i] = '1';
          }
          return new String(result);
      }

      // Compare hash
      public static Int32 CalcSimilarDegree(string a, string b)
      {
          if (a.Length != b.Length)
              throw new ArgumentException();
          int count = 0;
          for (int i = 0; i < a.Length; i++)
          {
              if (a[i] != b[i])
                  count++;
          }
          return count;
      }
      #endregion
  }
}
