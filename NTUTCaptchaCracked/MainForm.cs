using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using mshtml;
using System.IO;
using System.Drawing.Imaging;

namespace NTUTCaptchaCracked
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            rb_gray.Click += new System.EventHandler(rb_CheckedChanged);
            rb_invert.Click += new System.EventHandler(rb_CheckedChanged);
            rb_bit.Click += new System.EventHandler(rb_CheckedChanged);
            comboBox_gray.SelectedIndexChanged += new System.EventHandler(rb_CheckedChanged);
            rb_bright.Click += new System.EventHandler(rb_CheckedChanged);
            //WB.ProgressChanged += new WebBrowserProgressChangedEventHandler(WB_ProgressChanged);
            //tb_URL.KeyDown += new KeyEventHandler(tb_URL_KeyDown);
            //tb_URL.Click += new System.EventHandler(tb_URL_Click);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            WB.ScriptErrorsSuppressed = true;
            this.MaximizeBox = false;
            LoadSampleList();
            //Navigate("https://nportal.ntut.edu.tw");
        }

        #region 介面控制項事件

        private void btn_StartDEMO_Click(object sender, EventArgs e)
        {
            StartDEMO();
        }

        private void btn_WBGo_Click(object sender, EventArgs e)
        {
            Navigate("https://nportal.ntut.edu.tw");
            if (!Loading(WB, Int32.Parse(tb_TimeOut.Text))) { MessageBox.Show("連線逾時!"); return; }
        }

        private void btn_WBStop_Click(object sender, EventArgs e)
        {
            WB.Stop();
            Application.DoEvents();
        }

        private void btn_WBReflash_Click(object sender, EventArgs e)
        {
            WB.Refresh();
            Application.DoEvents();
        }

        private void btn_ChangePic_Click(object sender, EventArgs e)
        {
            //<a href="javascript:changeAuthImage()"><img id="authImage" src="" border=0></a>
            //<a href="javascript:changeAuthImage()" title="重新產生驗證碼"><img src="images/refresh2.png" style="height:30px" border=0/></a>
            WB.Navigate("javascript:changeAuthImage()");
        }

        private void btn_GetPic_Click(object sender, EventArgs e)
        {
            Image img = GetRegCodePic(WB, "", "", "authImage");
            if (img == null) {
                list_debug.Items.Add("[" + DateTime.Now + "] 無法取得圖片");
                return; 
            }
            pic_Captcha_Original.Image = img;
            task_reflash();
            rb_reflash();
            pic_Captcha_Cut.Image = pic_Captcha_result.Image;
            Captcha obj = new Captcha(new Bitmap(pic_Captcha_result.Image));
            SplitCaptcha(obj,Int32.Parse(textBox_cut.Text));
        }

        private void btn_ChangeAndGetPic_Click(object sender, EventArgs e)
        {
            ChangCaptcha();
        }

        private void btn_ChangeAndGetPic__Click(object sender, EventArgs e)
        {
            ChangCaptcha();
        }

        private void btn_ChangePic__Click(object sender, EventArgs e)
        {
            WB.Navigate("javascript:changeAuthImage()");
        }

        private void btn_GetPic__Click(object sender, EventArgs e)
        {
            Image img = GetRegCodePic(WB, "", "", "authImage");
            pic_Captcha_Original.Image = img;
            task_reflash();
            rb_reflash();
            pic_Captcha_Cut.Image = pic_Captcha_result.Image;
            Captcha obj = new Captcha(new Bitmap(pic_Captcha_result.Image));
            SplitCaptcha(obj, Int32.Parse(textBox_cut.Text));
        }

        private void WB_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void WB_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            pbar.Visible = true;
            if ((e.CurrentProgress > 0) && (e.MaximumProgress > 0))
            {
                pbar.Maximum = Convert.ToInt32(e.MaximumProgress);//設置正在加載的文檔總字節數
                pbar.Step = Convert.ToInt32(e.CurrentProgress);////獲取已下載文檔的字節數
                pbar.PerformStep();
            }
            /*else if (WB.ReadyState == WebBrowserReadyState.Complete)//加載完成後隱藏進度條
            {
                pbar.Value = 0;
                pbar.Visible = false;
            }*/
        }

        private void WB_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            tb_URL.Text = WB.Url.ToString();
        }

        // Navigates to the URL in the address box when 
        // the ENTER key is pressed while the ToolStripTextBox has focus.
        private void tb_URL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Navigate(tb_URL.Text);
            }
        }

        // Navigates to the given URL if it is valid.
        private void Navigate(String address)
        {
            if (String.IsNullOrEmpty(address)) return;
            if (address.Equals("about:blank")) return;
            if (!address.StartsWith("http://") && !address.StartsWith("https://")){
                address = "http://" + address;
            }
            try{
                WB.Navigate(new Uri(address));
            }catch (System.UriFormatException){
                return;
            }
        }

        #endregion

        #region 動作副程式

        private void StartDEMO()
        {
            Navigate("https://nportal.ntut.edu.tw");
            if (!Loading(WB, Int32.Parse(tb_TimeOut.Text))) { MessageBox.Show("連線逾時!"); return; }
            Image img = GetRegCodePic(WB, "", "", "authImage");
            Captcha tmp = new Captcha(new Bitmap(img));
            tmp.ConvertGrayByPixels(0);
            tmp.ConvertGrayToBlackByPixels(160);
            tmp.InvertColorByPixels();
            SplitCaptcha(tmp, 4);
            WebFillIn(WB, text_id.Text, text_pw.Text, txt_p1.Text + txt_p2.Text + txt_p3.Text + txt_p4.Text);
        }

        private void CaptchaCracked(Bitmap pBmpImg)
        {
            CaptchaCrackedHelper.DecCodeList decCodeList =
                   new CaptchaCrackedHelper.DecCodeList();
            decCodeList.List.Add(new CaptchaCrackedHelper.DecCodes()    // 0
            {
                Code = new string[] { 
                    "001110001001000100110010011001001100100110010011001001000011000",
                    "0111100010010001001100100110010011001001100110010010010000111000110000",
                    "0111000100100010011001001100100010010011001101100100100001110100100000"}
            });
            decCodeList.List.Add(new CaptchaCrackedHelper.DecCodes()    // 1
            {
                Code = new string[] { 
                    "011100000010000001000000100000010000001000000100000010000111100",
                    "011100000010000011000001100000010000011000001100000110000111110",
                    "0001100000010000001000000100000011000011000000100000010000001100000000",
                    "0000000000110000001000000100000110000001000001100000010000001000001110"}
            });
            decCodeList.List.Add(new CaptchaCrackedHelper.DecCodes()    // 2
            {
                Code = new string[] { 
                    "001100001111000000100000010000000000001000001000000111100111100",
                    "0001100001111001000100000010000000000001000001000001111100111100000000",
                    "001110001111001001100000010000000000001000001001000111101111100"}
            });
            decCodeList.List.Add(new CaptchaCrackedHelper.DecCodes()    // 3
            {
                Code = new string[] { 
                    "001110001111000000100000110000111000000100000011011001000111000",
                    "0011110011110000001000001100001110000001100000100110010001110000000000",
                    ""}
            });
            decCodeList.List.Add(new CaptchaCrackedHelper.DecCodes()    // 4
            {
                Code = new string[] { 
                    "000010000011000001100000110000001000001100011111000001000000100",
                    "000010000001000001100001110000011000000100011111000011000000100",
                    "0001000000100000110000001000000101000110000111100000100000010000000000",
                    "0000000000001000001100000110000001000000100000110001111100000100000010"}
            });
            decCodeList.List.Add(new CaptchaCrackedHelper.DecCodes()    // 5
            {
                Code = new string[] { 
                    "001111000111100100000011100001111000000110000001001000000111100",
                    "0011110001101000000000111000011011000001100001010010000001111100001000",
                    ""}
            });
            decCodeList.List.Add(new CaptchaCrackedHelper.DecCodes()    // 6
            {
                Code = new string[] { 
                    "000011000110000010000011110001001100100110010011001101100011100",
                    "0000100001100000100000111100010011001001100100110011011000111000000000",
                    ""}
            });
            decCodeList.List.Add(new CaptchaCrackedHelper.DecCodes()    // 7
            {
                Code = new string[] { 
                    "011111001111100000000000010000001000000000000100000010000000000",
                    "1111110011011000000000000100000010000000000011000000100000000000000000",
                    ""}
            });
            decCodeList.List.Add(new CaptchaCrackedHelper.DecCodes()    // 8
            {
                Code = new string[] { 
                    "001110001000100110010011110000111000100110010011001001100111100",
                    "1111000100010011001001111000011000010001001001100100110001110000000000",
                    "0000000000111000100100011001001111000011100110011011000100100110001110",
                    "0001000000101000100010011101001111000011110010011001001000100110001010"}
            });
            decCodeList.List.Add(new CaptchaCrackedHelper.DecCodes()    // 9
            {
                Code = new string[] { 
                    "001110001001000100110010011001100100011100000010000110000110000",
                    "001110001101100100110110011001101100111110000010000111000110000",
                    "001110001001000100110010011001101100111110000010000000000000000",
                    "0000000101110011001010100110010011001100100011110000010000110000010000"}
            });

            CaptchaCrackedHelper cracked = new CaptchaCrackedHelper(pBmpImg, 128, 6, decCodeList);

            // Step1 灰階化
            cracked.ConvertGrayByPixels();
            
            
            // Step1 灰階化
            /*cracked.ConvertGrayByPixels();
            pic_Captcha_Gray.Image = cracked.BmpSource;

            cracked.ConvertGrayToBlackByPixels();
            pic_Captcha_bit.Image = cracked.BmpSource;

            cracked.InvertColorByPixels();
            pic_Captcha_Invert.Image = cracked.BmpSource;*/

            // Step2 其他處理
            /*if (comboBox1.SelectedIndex == 1)
                cracked.RemoteNoiseLineByPixels();
            else if (comboBox1.SelectedIndex == 2)
            {
                cracked.ClearPictureBorder(2);
                cracked.RemoteNoisePointByPixels();
            }
            cracked.ConvertGrayToBlackByPixels();*/
            

            // Step2 改變圖片範圍
            //cracked.ConvertBmpValidRange(5);
            

            // Step2 其他處理
            /*if (comboBox1.SelectedIndex == 1)
                cracked.RemoteNoiseLineByPixels();
            else if (comboBox1.SelectedIndex == 2)
            {
                cracked.ClearPictureBorder(2);
                cracked.RemoteNoisePointByPixels();
            }
            pic_Captcha_result.Image = cracked.BmpSource;*/

            // Step3 切割圖片範圍
            //Bitmap[] bitmap = cracked.GetSplitPicChars(5, 1);
            /*Bitmap[] bitmap = SampleOcr.SplitBitmaps(cracked.BmpSource);
            try {
                pic_p1.Image = bitmap[0];
                pic_p2.Image = bitmap[1];
                pic_p3.Image = bitmap[2];
                pic_p4.Image = bitmap[3];
            }
            catch { }*/

            /*txtCodeStr.Text = string.Empty;
            txtCode.Text = string.Empty;
            foreach (Bitmap bmp in bitmap)
            {
                string result = cracked.GetSingleBmpCode(bmp);
                txtCodeStr.Text += result + "@";
                txtCode.Text += cracked.GetDecChar(result);
            }*/

        }

        /*private string QueryCharDictionary(string pCharCode)
        {
            if (pCharCode != string.Empty)
            {
                string[] charDictionary = {"001110001001000100110010011001001100100110010011001001000011000",
                                       "011100000010000001000000100000010000001000000100000010000111100",
                                       "001100001111000000100000010000000000001000001000000111100111100",
                                       "001110001111000000100000110000111000000100000011011001000111000",
                                       "000010000011000001100000110000001000001100011111000001000000100",
                                       "001111000111100100000011100001111000000110000001001000000111100",
                                       "000011000110000010000011110001001100100110010011001101100011100",
                                       "011111001111100000000000010000001000000000000100000010000000000",
                                       "001110001000100110010011110000111000100110010011001001100111100",
                                       "001110001001000100110010011001100100011100000010000110000110000"};

                char[] codeChar = pCharCode.ToCharArray();
                int diffCharCount = 0;
                for (int i = 0; i < charDictionary.Length; i++)
                {
                    char[] dicChar = charDictionary[i].ToCharArray();
                    if (codeChar.Length == dicChar.Length)
                    {
                        for (int j = 0; j < codeChar.Length; j++)
                            if (codeChar[j] != dicChar[j])
                                diffCharCount++;
                        if (diffCharCount < 5)
                            return i.ToString();
                        else
                            diffCharCount = 0;
                    }
                    else
                        return "X";
                }
            }
            return "X";
        }*/

        private bool Loading(WebBrowser WB, Int32 TimeOutSec = 10)
        {
            DateTime WBTimeOut = DateTime.Now;
            while (!((WB.ReadyState == WebBrowserReadyState.Complete) || ((DateTime.Now - WBTimeOut).TotalSeconds) >= TimeOutSec)){
                Application.DoEvents();
            };
            if (((DateTime.Now - WBTimeOut).TotalSeconds) >= TimeOutSec){
                WB.Stop();
                return false;
            }else{
                return true;
            }
        }

        #endregion

        #region 圖形控制函數

        //取得驗證碼圖片
        public Image GetRegCodePic(WebBrowser WB,String ImgName,String Src,String Id){
            HTMLDocument doc = WB.Document.DomDocument as HTMLDocument;
            HTMLBody body = doc.body as HTMLBody;
            IHTMLControlRange rang = body.createControlRange() as IHTMLControlRange;
            IHTMLControlElement Img;
            if (ImgName == "") {
                //如果沒有圖片的名字,通過Src或Alt中的關鍵字來取
                Int32 ImgNum = GetPicIndex(WB, Src, Id);
                if(ImgNum == -1){return null;}
                Img = WB.Document.Images[ImgNum].DomElement as IHTMLControlElement;
            }else{
                Img = WB.Document.All[ImgName].DomElement as IHTMLControlElement;
            }
            rang.add(Img);
            rang.execCommand("copy", false, Type.Missing);
            Image RegImg = Clipboard.GetImage();
            Clipboard.Clear();
            return RegImg;
        }

        //從WB中抓取指定圖片Index
        public Int32 GetPicIndex(WebBrowser WB, String Src , String Id ){
            var items = WB.Document.Images;
            for(int i = 0;i<WB.Document.Images.Count - 1;i++){
                if (items[i].Id == Id){
                    return i;
                }
            }
            return -1;
        }

        public void WebFillIn(WebBrowser WB,String ID,String PW,String IMG){
            foreach (HtmlElement htmEle in WB.Document.GetElementsByTagName("input")){
                if (htmEle.Name == "muid") { htmEle.SetAttribute("value", ID); }
                if (htmEle.Name == "mpassword") { htmEle.SetAttribute("value", PW); }
                if (htmEle.Name == "authcode") { htmEle.SetAttribute("value", IMG); }
                if (htmEle.Name == "Submit2") { htmEle.InvokeMember("Click"); }
            }
        }

        #endregion

        private void ChangCaptcha()
        {
            WB.Navigate("javascript:changeAuthImage()");
            Loading(WB,2);
            Image img = GetRegCodePic(WB, "", "", "authImage");
            if (img == null)
            {
                list_debug.Items.Add("[" + DateTime.Now + "] 無法取得圖片");
                return;
            }
            pic_Captcha_Original.Image = img;
            task_reflash();
            rb_reflash();
            pic_Captcha_Cut.Image = pic_Captcha_result.Image;
            Captcha obj = new Captcha(new Bitmap(pic_Captcha_result.Image));
            SplitCaptcha(obj,Int32.Parse(textBox_cut.Text));
        }
       
        private void SplitCaptcha(Captcha obj,int want_char = 0,int mode = 0)
        {
            // Step2 其他處理
            /*if (comboBox1.SelectedIndex == 1)
                obj.RemoteNoiseLineByPixels();
            else if (comboBox1.SelectedIndex == 2)
            {
                obj.ClearPictureBorder(2);
                obj.RemoteNoisePointByPixels();
            }*/

            // Step2 改變圖片範圍
            /*obj.ConvertBmpValidRange(4, 128);
            pic_Captcha_Cut.Image = obj.BmpSource;*/

            // Step3 切割圖片範圍
            Bitmap[] bitmap = null;
            if (mode == 0) {
                bitmap = SampleOcr.SplitBitmaps(obj.BmpSource);
                if (bitmap.Length != want_char) {
                    bitmap = null;
                    bitmap = obj.GetSplitPicChars(want_char, 1);
                }
            }else if (mode == 1){
                bitmap = SampleOcr.SplitBitmaps(obj.BmpSource);
            }else if (mode == 2){
                bitmap = obj.GetSplitPicChars(want_char, 1);
            }
            CaptchaAlgorithm(bitmap, cb_algorithm.SelectedIndex);
        }

        private void CaptchaAlgorithm(Bitmap[] bitmap, int mode = 0)
        {
            try
            {
                for (int i = 0; i < bitmap.Length; i++)
                {
                    switch (i)
                    {
                        case 0:
                            pic_p1.Image = SampleOcr.Trim(bitmap[0]);
                            switch (mode)
                            {
                                case 0:
                                    txt_p1.Text = SampleOcr.OcrCharEx(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                                case 1:
                                    txt_p1.Text = SampleOcr.OcrHash(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                                case 2:
                                    txt_p1.Text = SampleOcr.OcrChar(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                            } 
                            txt_s1.Text = txt_p1.Text;
                            txt_n1.Text = (SampleOcr.LastSimilar * 100).ToString() + "%";
                            break;
                        case 1:
                            pic_p2.Image = SampleOcr.Trim(bitmap[1]);
                            switch (mode)
                            {
                                case 0:
                                    txt_p2.Text = SampleOcr.OcrCharEx(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                                case 1:
                                    txt_p2.Text = SampleOcr.OcrHash(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                                case 2:
                                    txt_p2.Text = SampleOcr.OcrChar(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                            } 
                            txt_s2.Text = txt_p2.Text;
                            txt_n2.Text = (SampleOcr.LastSimilar * 100).ToString() + "%";
                            break;
                        case 2:
                            pic_p3.Image = SampleOcr.Trim(bitmap[2]);
                            switch (mode)
                            {
                                case 0:
                                    txt_p3.Text = SampleOcr.OcrCharEx(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                                case 1:
                                    txt_p3.Text = SampleOcr.OcrHash(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                                case 2:
                                    txt_p3.Text = SampleOcr.OcrChar(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                            } 
                            txt_s3.Text = txt_p3.Text;
                            txt_n3.Text = (SampleOcr.LastSimilar * 100).ToString() + "%";
                            break;
                        case 3:
                            pic_p4.Image = SampleOcr.Trim(bitmap[3]);
                            switch (mode)
                            {
                                case 0:
                                    txt_p4.Text = SampleOcr.OcrCharEx(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                                case 1:
                                    txt_p4.Text = SampleOcr.OcrHash(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                                case 2:
                                    txt_p4.Text = SampleOcr.OcrChar(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                            } 
                            txt_s4.Text = txt_p4.Text;
                            txt_n4.Text = (SampleOcr.LastSimilar * 100).ToString() + "%";
                            break;
                        case 4:
                            pic_p5.Image = SampleOcr.Trim(bitmap[4]);
                            switch (mode)
                            {
                                case 0:
                                    txt_p5.Text = SampleOcr.OcrCharEx(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                                case 1:
                                    txt_p5.Text = SampleOcr.OcrHash(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                                case 2:
                                    txt_p5.Text = SampleOcr.OcrChar(SampleOcr.Trim(bitmap[i])).ToString();
                                    break;
                            } 
                            txt_s5.Text = txt_p5.Text;
                            txt_n5.Text = (SampleOcr.LastSimilar * 100).ToString() + "%";
                            break;
                    }
                }
            }
            catch { }
        }
        


        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            rb_reflash();
        }

        private void rb_reflash() {
            if (pic_Captcha_result.Image == null) { pic_Captcha_effect.Image = null; return; }
            Captcha obj = new Captcha(new Bitmap(pic_Captcha_result.Image));

            if (rb_gray.Checked == true)
            {
                obj.ConvertGrayByPixels(comboBox_gray.SelectedIndex);
            }
            else if (rb_invert.Checked == true)
            {
                obj.InvertColorByPixels();
            }
            else if (rb_bit.Checked == true)
            {
                obj.ConvertGrayToBlackByPixels(Int32.Parse(textBox_bit.Text));
            }
            else if (rb_bright.Checked == true)
            {
                obj.AdjustBrightness(Int32.Parse(textBox_bright.Text));
            }
            pic_Captcha_effect.Image = obj.BmpSource;
        }

        private void task_reflash() {
            try 
            {
                Captcha tmp = rb_taskEffect(new Bitmap(pic_Captcha_Original.Image));
                pic_Captcha_result.Image = tmp.BmpSource;
            }
            catch {
            }
        }

        private Captcha rb_taskEffect(Bitmap bmp)
        {
            Captcha tmp = new Captcha(bmp);
            for (int i = 0; i < listBox_task.Items.Count; i++) { 
                string[] cmd = listBox_task.Items[i].ToString().Split(' ');
                switch (cmd[0]) {
                    case "灰階化":
                        tmp.ConvertGrayByPixels(Int32.Parse(cmd[1]));
                        break;
                    case "反轉顏色":
                        tmp.InvertColorByPixels();
                        break;
                    case "顏色二值化":
                        tmp.ConvertGrayToBlackByPixels(Int32.Parse(cmd[1]));
                        break;
                    case "亮度調整":
                        tmp.AdjustBrightness(Int32.Parse(cmd[1]));
                        break;
                }
            }
            return tmp;
        }

        private void button_add_Click(object sender, EventArgs e)
        {
            if (rb_gray.Checked == true)
            {
                listBox_task.Items.Add("灰階化" + " " + comboBox_gray.SelectedIndex);
            }
            else if (rb_invert.Checked == true)
            {
                listBox_task.Items.Add("反轉顏色");
            }
            else if (rb_bit.Checked == true)
            {
                listBox_task.Items.Add("顏色二值化" + " " + textBox_bit.Text);
            }
            else if (rb_bright.Checked == true)
            {
                listBox_task.Items.Add("亮度調整" + " " + textBox_bright.Text);
            }
            task_reflash();
        }

        private void button_moveup_Click(object sender, EventArgs e)
        {
            if (listBox_task.SelectedIndex == -1 || listBox_task.SelectedIndex==0) { return; }
            string tmp = listBox_task.Items[listBox_task.SelectedIndex-1].ToString();
            listBox_task.Items[listBox_task.SelectedIndex-1] = listBox_task.Items[listBox_task.SelectedIndex];
            listBox_task.Items[listBox_task.SelectedIndex] = tmp;
            listBox_task.SelectedIndex -= 1;
            task_reflash();
        }

        private void button_movedown_Click(object sender, EventArgs e)
        {
            if (listBox_task.SelectedIndex == -1 || listBox_task.SelectedIndex == listBox_task.Items.Count-1) { return; }
            string tmp = listBox_task.Items[listBox_task.SelectedIndex+1].ToString();
            listBox_task.Items[listBox_task.SelectedIndex+1] = listBox_task.Items[listBox_task.SelectedIndex];
            listBox_task.Items[listBox_task.SelectedIndex] = tmp;
            listBox_task.SelectedIndex += 1;
            task_reflash();
        }

        private void button_remove_Click(object sender, EventArgs e)
        {
            if (listBox_task.SelectedIndex == -1) { return; }
            listBox_task.Items.RemoveAt(listBox_task.SelectedIndex);
            task_reflash();
        }

        private void btn_sample_save_Click(object sender, EventArgs e)
        {
            string sampleDir = System.Windows.Forms.Application.StartupPath + @"\samples";
            if (pic_p1.Image != null && txt_s1.Text != "") {
                pic_p1.Image.Save(sampleDir + "\\" + findFileName(txt_s1.Text) + ".bmp",ImageFormat.Bmp);
            }
            if (pic_p2.Image != null && txt_s2.Text != "")
            {
                pic_p2.Image.Save(sampleDir + "\\" + findFileName(txt_s2.Text) + ".bmp", ImageFormat.Bmp);
            }
            if (pic_p3.Image != null && txt_s3.Text != "")
            {
                pic_p3.Image.Save(sampleDir + "\\" + findFileName(txt_s3.Text) + ".bmp", ImageFormat.Bmp);
            }
            if (pic_p4.Image != null && txt_s4.Text != "")
            {
                pic_p4.Image.Save(sampleDir + "\\" + findFileName(txt_s4.Text) + ".bmp", ImageFormat.Bmp);
            }
            if (pic_p5.Image != null && txt_s5.Text != "")
            {
                pic_p5.Image.Save(sampleDir + "\\" + findFileName(txt_s5.Text) + ".bmp", ImageFormat.Bmp);
            }
            txt_s1.Text = "";
            txt_s2.Text = "";
            txt_s3.Text = "";
            txt_s4.Text = "";
            txt_s5.Text = "";
        }
        private string findFileName(string word) {
            string sampleDir = System.Windows.Forms.Application.StartupPath + @"\samples";
            int n = 0;
            while (File.Exists(sampleDir + "\\" + word + "_" + n.ToString() + ".bmp"))
            {
                n++;
            }
            return word + "_" + n.ToString();
        }

        private void LoadSampleList() {
            DirectoryInfo dir = new DirectoryInfo(System.Windows.Forms.Application.StartupPath + @"\samples");
            foreach (FileInfo file in dir.GetFiles())
            {
                try
                {
                    this.imageList_Sample.Images.Add(Image.FromFile(file.FullName));
                }
                catch
                {
                    Console.WriteLine("This is not an image file");
                }
            }
            this.listView_Sample.View = View.LargeIcon;
            this.imageList_Sample.ImageSize = new Size(20, 20);
            this.listView_Sample.LargeImageList = this.imageList_Sample;
            //or
            //this.listView1.View = View.SmallIcon;
            //this.listView1.SmallImageList = this.imageList1;

            for (int j = 0; j < this.imageList_Sample.Images.Count; j++)
            {
                ListViewItem item = new ListViewItem();
                item.ImageIndex = j;
                this.listView_Sample.Items.Add(item);
            }
        }

        private void btn_sample_reload_Click(object sender, EventArgs e)
        {
            listView_Sample.Clear();
            imageList_Sample.Dispose();
            LoadSampleList();
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            listBox_task.Items.Clear();
            task_reflash();
        }

        private void button_default_Click(object sender, EventArgs e)
        {
            listBox_task.Items.Clear();
            listBox_task.Items.Add("灰階化 0");
            listBox_task.Items.Add("顏色二值化 160");
            listBox_task.Items.Add("反轉顏色");
            task_reflash();
        }

        private void cb_algorithm_SelectedIndexChanged(object sender, EventArgs e)
        {
            Captcha obj = new Captcha(new Bitmap(pic_Captcha_result.Image));
            SplitCaptcha(obj, Int32.Parse(textBox_cut.Text));
        }

        private void btn_logout_Click(object sender, EventArgs e)
        {
            Navigate("http://nportal.ntut.edu.tw/logout.do");
            if (!Loading(WB, Int32.Parse(tb_TimeOut.Text))) { MessageBox.Show("連線逾時!"); return; }
        }
    }
}
