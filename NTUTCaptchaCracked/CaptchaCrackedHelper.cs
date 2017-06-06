using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace NTUTCaptchaCracked
{
    public class CaptchaCrackedHelper
    {
        /// <summary>
        /// 存放來源圖檔
        /// </summary>
        public Bitmap BmpSource { get; set; }
        /// <summary>
        /// 區分背景與數字的灰階值
        /// </summary>
        private int GrayValue { get; set; }
        /// <summary>
        /// 可容忍的錯誤噪點數
        /// </summary>
        private int AllowDiffCount { get; set; }
        /// <summary>
        /// 對照樣本字典
        /// </summary>
        private DecCodeList DecCodeDictionary { get; set; }

        public CaptchaCrackedHelper() { }
        public CaptchaCrackedHelper(
            Bitmap pBmpSource, int pGrayValue, int pAllowDiffCount, DecCodeList pDecCodeDictionary)
        {
            BmpSource = pBmpSource;
            GrayValue = pGrayValue;
            AllowDiffCount = pAllowDiffCount;
            //DecCodeDictionary = pDecCodeDictionary;
        }

        #region 灰階值處理

        /// <summary>
        /// 將每點像素色彩轉換成灰階值
        /// </summary>
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

        #region 反轉顏色處理

        /// <summary>
        /// 將每點像素色彩轉換成相反顏色
        /// </summary>
        public void InvertColorByPixels()
        {
            for (int x = 0; x < BmpSource.Width; x++){
                for (int y = 0; y < BmpSource.Height; y++){
                    Color color = BmpSource.GetPixel(x, y);
                    BmpSource.SetPixel(x, y, Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B));
                }
            }
        }

        #endregion

        #region 對比顏色處理

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
                    if (color.R < value || color.G < value || color.B < value) {
                        BmpSource.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                    }else{
                        BmpSource.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                    }
                }
            }
        }

        #endregion

        #region 其他處理

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

        #region 圖片均分處理

        /// <summary>
        /// 轉換圖片有效範圍
        /// </summary>
        /// <param name="pCharsCount">int-字元數量</param>
        public void ConvertBmpValidRange(int pCharsCount)
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

        #region 圖片編碼轉換

        /// <summary>
        /// 取得圖片轉換後的01編碼，0為背景像素1為灰階像素
        /// </summary>
        /// <param name="pBmp">bitmap-單一圖片</param>
        /// <returns></returns>
        public string GetSingleBmpCode(Bitmap pBmp)
        {
            Color color;
            string code = string.Empty;
            for (int i = 0; i < pBmp.Height; i++)
                for (int j = 0; j < pBmp.Width; j++)
                {
                    color = pBmp.GetPixel(j, i);
                    if (color.R < GrayValue)
                        code += "1";
                    else
                        code += "0";
                }
            return code;
        }

        /// <summary>
        /// 取得解碼後的驗證碼字元
        /// </summary>
        /// <param name="pSourceCode">string-圖片編碼</param>
        /// <returns></returns>
        public string GetDecChar(string pSourceCode)
        {
            string tmpResult = "X";
            for (int i = 0; i < DecCodeDictionary.List.Count; i++)
            {
                foreach (string code in DecCodeDictionary.List[i].Code.ToArray())
                {
                    int diffCharCount = 0;
                    char[] decChar = code.ToCharArray();
                    char[] sourceChar = pSourceCode.ToCharArray();
                    if (decChar.Length == sourceChar.Length)
                    {
                        for (int j = 0; j < decChar.Length; j++)
                            if (decChar[j] != sourceChar[j])
                                diffCharCount++;
                        if (diffCharCount <= AllowDiffCount)
                            tmpResult = i.ToString();
                    }
                }
            }
            return tmpResult;
        }

        #endregion

        public class DecCodeList
        {
            public DecCodeList() { List = new List<DecCodes>(); }
            public List<DecCodes> List { get; set; }
        }

        public class DecCodes
        {
            public string[] Code { get; set; }
        }
    }
}
