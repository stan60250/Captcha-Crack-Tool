using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace NTUTCaptchaCracked
{
    public class Captcha
    {
        /// <summary>
        /// 存放來源圖檔
        /// </summary>
        public Bitmap BmpSource { get; set; }

        public Captcha() { }
        public Captcha(Bitmap pBmpSource)
        {
            BmpSource = pBmpSource;
        }

        #region 灰階化

        /// <summary>
        /// 將每點像素色彩轉換成灰階值
        /// </summary>
        /// <param name="pColor">模式(0-1)</param>
        /// <returns></returns>
        public void ConvertGrayByPixels(int mode = 0)
        {
            if (mode == 0)
            {
                for (int i = 0; i < BmpSource.Height; i++)
                    for (int j = 0; j < BmpSource.Width; j++)
                    {
                        int grayValue = GetGrayValue(BmpSource.GetPixel(j, i));
                        BmpSource.SetPixel(j, i, Color.FromArgb(grayValue, grayValue, grayValue));
                    }
            }
            else if (mode == 1)
            {
                for (int x = 0; x < BmpSource.Width; x++)
                {
                    for (int y = 0; y < BmpSource.Height; y++)
                    {
                        Color color = BmpSource.GetPixel(x, y);
                        int gray = (color.R + color.G + color.B) / 3;
                        BmpSource.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                    }
                }
            }
        }

        /// <summary>
        /// 計算灰階值
        /// </summary>
        /// <param name="pColor">color-像素色彩</param>
        /// <returns></returns>
        private int GetGrayValue(Color pColor)
        {
            return Convert.ToInt32(pColor.R * 0.299 + pColor.G * 0.587 + pColor.B * 0.114); // 灰階公式
        }

        #endregion
        #region 反轉顏色

        /// <summary>
        /// 將每點像素色彩轉換成相反顏色
        /// </summary>
        public void InvertColorByPixels()
        {
            for (int x = 0; x < BmpSource.Width; x++)
            {
                for (int y = 0; y < BmpSource.Height; y++)
                {
                    Color color = BmpSource.GetPixel(x, y);
                    BmpSource.SetPixel(x, y, Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B));
                }
            }
        }

        #endregion
        #region 顏色二值化

        /// <summary>
        /// 將每點像素色彩對比調整
        /// </summary>
        public void ConvertGrayToBlackByPixels(int value = 160)
        {
            for (int x = 0; x < BmpSource.Width; x++)
            {
                for (int y = 0; y < BmpSource.Height; y++)
                {
                    Color color = BmpSource.GetPixel(x, y);
                    if (color.R < value || color.G < value || color.B < value)
                    {
                        BmpSource.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }
                    else
                    {
                        BmpSource.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                }
            }
        }

        #endregion
        #region 亮度調整
        /// <summary>
        /// 增加或減少亮度
        /// </summary>
        /// <param name="valBrightness">0~255</param>
        /// <returns></returns>
        public void AdjustBrightness(int valBrightness)
        {
            // 讀入欲轉換的圖片並轉成為 Bitmap
            for (int y = 0; y < BmpSource.Height; y++)
            {
                for (int x = 0; x < BmpSource.Width; x++)
                {
                    // 取得每一個 pixel
                    var pixel = BmpSource.GetPixel(x, y);

                    // 判斷 如果處理過後 255 就設定為 255 如果小於則設定為 0
                    var pR = ((pixel.R + valBrightness > 255) ? 255 : pixel.R + valBrightness) < 0 ? 0 : ((pixel.R + valBrightness > 255) ? 255 : pixel.R + valBrightness);
                    var pG = ((pixel.G + valBrightness > 255) ? 255 : pixel.G + valBrightness) < 0 ? 0 : ((pixel.G + valBrightness > 255) ? 255 : pixel.G + valBrightness);
                    var pB = ((pixel.B + valBrightness > 255) ? 255 : pixel.B + valBrightness) < 0 ? 0 : ((pixel.B + valBrightness > 255) ? 255 : pixel.B + valBrightness);

                    // 將改過的 RGB 寫回
                    Color newColor = Color.FromArgb(pixel.A, pR, pG, pB);
                    BmpSource.SetPixel(x, y, newColor);
                }
            }
        }
        #endregion
        

        #region 清除灰階值邊框

        /// <summary>
        /// 轉換圖片有效範圍
        /// </summary>
        /// <param name="pCharsCount">int-字元數量</param>
        public void ConvertBmpValidRange(int pCharsCount, int GrayValue = 128)
        {
            // 圖片最大 X, Y，處理後變成起始 X, Y
            int posX1 = BmpSource.Width, posY1 = BmpSource.Height;
            // 圖片起始 X, Y，處理後變成最大 X, Y
            int posX2 = 0, posY2 = 0;

            // 取得有效範圍區域
            for (int i = 0; i < BmpSource.Height; i++)
            {
                for (int j = 0; j < BmpSource.Width; j++)
                {
                    int pixelVal = BmpSource.GetPixel(j, i).R;
                    if (pixelVal < GrayValue) // 如像該素值低於指定灰階值則進行縮小區域
                    {
                        if (posX1 > j) posX1 = j; // 如 X2 像素位置大於圖片寬度則縮小寬度
                        if (posY1 > i) posY1 = i; // 如 Y2 像素位置大於圖片高度則縮小高度
                        if (posX2 < j) posX2 = j; // 如 X1 像素位置小於圖片寬度則縮小寬度
                        if (posY2 < i) posY2 = i; // 如 Y1 像素位置小於圖片寬度則縮小寬度
                    }
                }
            }

            // 確保圖片可以平均切割圖片
            int span = pCharsCount - (posX2 - posX1 + 1) % pCharsCount;
            if (span < pCharsCount)
            {
                int leftSpan = span / 2;
                if (posX1 > leftSpan)
                    posX1 = posX1 - leftSpan;
                if (posX2 + span - leftSpan < BmpSource.Width)
                    posX2 = posX2 + span - leftSpan;
            }
            // 產生變更後的圖片
            Rectangle cloneRect = new Rectangle(posX1, posY1, posX2 - posX1 + 1, posY2 - posY1 + 1);
            BmpSource = BmpSource.Clone(cloneRect, BmpSource.PixelFormat);
        }

        #endregion
        #region 清除指定邊框

        /// <summary>
        /// 清除邊框處理
        /// </summary>
        /// <param name="borderWidth"></param>
        public void ClearPictureBorder(int pBorderWidth)
        {
            for (int i = 0; i < BmpSource.Height; i++)
            {
                for (int j = 0; j < BmpSource.Width; j++)
                {
                    if (i < pBorderWidth || j < pBorderWidth || j > BmpSource.Width - 1 - pBorderWidth || i > BmpSource.Height - 1 - pBorderWidth)
                        BmpSource.SetPixel(j, i, Color.FromArgb(255, 255, 255));
                }
            }
        }

        #endregion
        #region 去除噪音線
        /// <summary>
        /// 噪音線處理
        /// </summary>
        public void RemoteNoiseLineByPixels()
        {
            for (int i = 0; i < BmpSource.Height; i++)
                for (int j = 0; j < BmpSource.Width; j++)
                {
                    int grayValue = BmpSource.GetPixel(j, i).R;
                    if (grayValue <= 255 && grayValue >= 160)
                        BmpSource.SetPixel(j, i, Color.FromArgb(255, 255, 255));
                }
        }
        #endregion
        #region 去除噪音點

        /// <summary>
        /// 噪音點處理
        /// </summary>
        public void RemoteNoisePointByPixels()
        {
            List<NoisePoint> points = new List<NoisePoint>();

            for (int k = 0; k < 5; k++)
            {
                for (int i = 0; i < BmpSource.Height; i++)
                    for (int j = 0; j < BmpSource.Width; j++)
                    {
                        int flag = 0;
                        int garyVal = 255;
                        // 檢查上相鄰像素
                        if (i - 1 > 0 && BmpSource.GetPixel(j, i - 1).R != garyVal) flag++;
                        if (i + 1 < BmpSource.Height && BmpSource.GetPixel(j, i + 1).R != garyVal) flag++;
                        if (j - 1 > 0 && BmpSource.GetPixel(j - 1, i).R != garyVal) flag++;
                        if (j + 1 < BmpSource.Width && BmpSource.GetPixel(j + 1, i).R != garyVal) flag++;
                        if (i - 1 > 0 && j - 1 > 0 && BmpSource.GetPixel(j - 1, i - 1).R != garyVal) flag++;
                        if (i + 1 < BmpSource.Height && j - 1 > 0 && BmpSource.GetPixel(j - 1, i + 1).R != garyVal) flag++;
                        if (i - 1 > 0 && j + 1 < BmpSource.Width && BmpSource.GetPixel(j + 1, i - 1).R != garyVal) flag++;
                        if (i + 1 < BmpSource.Height && j + 1 < BmpSource.Width && BmpSource.GetPixel(j + 1, i + 1).R != garyVal) flag++;

                        if (flag < 3)
                            points.Add(new NoisePoint() { X = j, Y = i });
                    }
                foreach (NoisePoint point in points)
                    BmpSource.SetPixel(point.X, point.Y, Color.FromArgb(255, 255, 255));

            }
        }

        public class NoisePoint
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        #endregion

        #region 圖片均分切割

        /// <summary>
        /// 切割圖片成指定目標數
        /// </summary>
        /// <param name="pHorizontalColNumber">int-水平切割數</param>
        /// <param name="pVerticalRowNumber">int-垂直切割數</param>
        /// <returns></returns>
        public Bitmap[] GetSplitPicChars(int pHorizontalColNumber, int pVerticalRowNumber)
        {
            if (pHorizontalColNumber == 0 || pVerticalRowNumber == 0)
                return null;
            int avgWidth = BmpSource.Width / pHorizontalColNumber;
            int avgHeight = BmpSource.Height / pVerticalRowNumber;
            // 產生存放圖片容器陣列
            Bitmap[] bmpAry = new Bitmap[pHorizontalColNumber * pVerticalRowNumber];
            // 重新取得數字區域
            Rectangle cloneRect;
            for (int i = 0; i < pVerticalRowNumber; i++)
            {
                for (int j = 0; j < pHorizontalColNumber; j++)
                {
                    cloneRect = new Rectangle(j * avgWidth, i * avgHeight, avgWidth, avgHeight);
                    bmpAry[i * pHorizontalColNumber + j] = BmpSource.Clone(cloneRect, BmpSource.PixelFormat);
                }
            }
            return bmpAry;
        }

        #endregion
    }
        

}
