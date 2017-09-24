using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace AsciiConverter
{
    public partial class MainWindow : Window
    {
        //Массив символов с соответствующими им значениями яркости. 
        //Используется для дальнейшего отображения пикселей в символы соответственно значению поля Value
        List<CharMap> charMapList = new List<CharMap>();
        string paramError = "Заданы некорректные параметры!";
        string converterSuccess = "Конвертация прошла успешно! Открыть полученный файл?";
        string success = "Успешно";
        Bitmap img = new Bitmap(1, 1);
        public MainWindow()
        {
            //Символы, не используемые при составлении картинки.
            string badChars = "_`[](){}\\~|/!?\"><1";
            InitializeComponent();
            Calibrate(badChars);
            button2.IsEnabled = true;
        }


        private int getCharCount(String text, Font font)
        {
            //создаем исходный битмап
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //Получаем размер текста в заданном шрифте
            SizeF textSize = drawing.MeasureString(text, font);

            //Очищаем исходный битмап
            img.Dispose();
            drawing.Dispose();

            //Изменяем размер под нужный для заданной буквы
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);
            
            drawing.Clear(Color.White);
            
            Brush textBrush = new SolidBrush(Color.Black);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            Bitmap mybitmap = new Bitmap(img);
            int counter = 0;
            for (int y = 0; y < mybitmap.Height; y++)
                for (int x = 0; x < mybitmap.Width; x++)
                    //Если значение ниже 0.5, т.е. близко к чёрному, повышаем счетчик
                    if (mybitmap.GetPixel(x, y).GetBrightness() < 0.5)
                        ++counter;
            return counter;

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            textBlock2.Text = ChooseImg();
            if (textBlock2.Text != "")
            {
                img = new Bitmap(textBlock2.Text);
                WidthTexthBox.IsEnabled = true;
                WidthTexthBox.Text = img.Width.ToString();
                saveProportionsCheckBox.IsEnabled = true;
                saveProportionsCheckBox.IsChecked = true;
                button3.IsEnabled = true;
                refreshScale();
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            string filename;
            int width;
            int height;
            if (Int32.TryParse(WidthTexthBox.Text, out width) && Int32.TryParse(HeightTextBox.Text, out height))
            {
                filename = ImgToAscii(img, textBlock2.Text, width, height);
                textBlock3.Text = filename;
                if (MessageBox.Show(converterSuccess, success, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Process.Start(filename);
                }
            }
            else
            {
                MessageBox.Show(paramError);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (!refreshScale())
                HeightTextBox.IsEnabled = true;
            else
                HeightTextBox.IsEnabled = false;
        }

        private void WidthTexthBox_KeyUp(object sender, KeyEventArgs e)
        {
            refreshScale();
        }
        private bool refreshScale()
        {
            int result;
            /*17 к 8 - отношение высоты к ширине моноширных символов. 
             * Необходимо реализовать поправку для корректного отображения. 
             * В ином случае изображение будет сплюснуто.
             */
            double ratio = (double)img.Width / (double)img.Height * 17 / 8;
            if (saveProportionsCheckBox.IsChecked.Value)
            {
                if (Int32.TryParse(WidthTexthBox.Text, out result))
                {
                    HeightTextBox.Text = Math.Round((Int32.Parse(WidthTexthBox.Text) / ratio)).ToString();
                }
                return true;

            }
            return false;
        }
        /// <summary>
        /// 
        /// Функция устанавливает коллекцию символов, используемых для отображения, и 
        /// ставит им в соответствие значение яркости для текущей системы
        /// </summary>
        /// <param name="badChars"> Коллекция символов, которые игнорируются при отображении </param>
        /// <returns>0 при успешном выполнении</returns>
        private int Calibrate(string badChars)
        {
            Font font = new Font("Consolas", System.Drawing.SystemFonts.DefaultFont.Size);
            List<CharMap> buffCharMapList = new List<CharMap>();
            int j = 0;
            //Минимальный шаг разности яркости соседних символов
            //Наилучшее значение, полученное эмпирическим путём
            int minDiff = 3;

            for (char c = ' '; c <= '~'; c++)
            {
                if (badChars.IndexOf(c) == -1)
                    buffCharMapList.Add(new CharMap(c, getCharCount(c.ToString(), font)));
            }
            buffCharMapList.Sort();

            charMapList.Add(buffCharMapList[j]);
            
            for (int i = 1; i < buffCharMapList.Count; i++)
            {
                //Отбрасываем лишние символы, обеспечивающие излишнюю передачу яркости
                if ((charMapList[j].Value - buffCharMapList[i].Value >= minDiff) || (buffCharMapList[i].Symbol == ' '))
                {
                    charMapList.Add(buffCharMapList[i]);
                    ++j;
                }
            }
            
            return 0;
        }

        private string ChooseImg()
        {
            //Сохраняем предыдущее имя файла на случай неудачного открытия
            string oldFileName = textBlock2.Text;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "img";
            dlg.DefaultExt = ".png";
            //Допустимые расширения файлов
            dlg.Filter = "Images|*.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.bmp";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                return dlg.FileName;
            }
            else return oldFileName;
        }

        private string ImgToAscii(Bitmap sourceimg, string imagePath, int width, int height)
        {
            //Корректируем размер
            Bitmap img = Resize(sourceimg, width, height);
            //Имя файла-результата
            string textPath = System.IO.Path.GetDirectoryName(imagePath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(imagePath) + ".txt";
            using (StreamWriter wrtr = new StreamWriter(textPath))
            {
                for (int y = 0; y < img.Height; y++)
                {
                    for (int x = 0; x < img.Width; x++)
                    {
                        Color color = img.GetPixel(x, y);
                        //Получаем значение яркости пикселя
                        double brightness = getBrightness(color);                        
                        double index = brightness / 255 * (charMapList.Count - 1);
                        //Получаем индекс из коллекции символов, ближайший к необходимому значению яркости
                        char c = charMapList[(int)Math.Round(index)].Symbol;
                        wrtr.Write(c);
                    }
                    wrtr.WriteLine();
                }
            }

            return textPath;
        }
        /// <summary>
        /// Функция изменения размера изображения с сохранением качества.
        /// </summary>
        /// <param name="image">Исходный Bitmap</param>
        /// <param name="width">Новое значение длины</param>
        /// <param name="height">Новое значение высоты</param>
        /// <returns>Измененный Bitmap</returns>
        private Bitmap Resize(Bitmap image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            //Сохраняем DPI для повышения качества при уменьшении размера
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    //Убирает размытие за пределами изображения
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    //Рисуем изображение
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        //Получение яркости из значений RGB
        private static double getBrightness(Color c)
        {
            return Math.Sqrt(0.299 * c.R * c.R + 0.587 * c.G * c.G + 0.114 * c.B * c.B);
        }

    }
}
