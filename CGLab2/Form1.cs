using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "Bitmaps|*.bmp|jpeps|*.jpg";

            if (op.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.ImageLocation = op.FileName;
                pictureBox3.ImageLocation = op.FileName;
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (pictureBox1.Image == null)
                return;
            Bitmap image = new Bitmap(pictureBox1.Image);
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    Color newColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B);
                    image.SetPixel(x, y, newColor);
                }
            }

            pictureBox1.Image = image;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
                return;
            if (comboBox1.Text == "Brightness correction")
            {
                Bitmap image = new Bitmap(pictureBox1.Image);

                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        Color pixelColor = image.GetPixel(x, y);
                        Color newColor = Color.FromArgb(pixelColor.R - Convert.ToByte(pixelColor.R * 0.1), pixelColor.G - Convert.ToByte(pixelColor.G * 0.1), pixelColor.B - Convert.ToByte(pixelColor.B * 0.1));
                        image.SetPixel(x, y, newColor);
                    }
                }

                pictureBox1.Image = image;
            }
            else if (comboBox1.Text == "Contrast enhacement")
            {
                Bitmap image = new Bitmap(pictureBox1.Image);
                int red, green, blue;
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        Color pixelColor = image.GetPixel(x, y);
                        red = Convert.ToInt32((pixelColor.R - 128) * 2) + 128;
                        green = Convert.ToInt32((pixelColor.G - 128) * 2) + 128;
                        blue = Convert.ToInt32((pixelColor.B - 128) * 2) + 128;
                        if (red > 255)
                            red = 255;
                        else if (red < 0)
                            red = 0;
                        if (green > 255)
                            green = 255;
                        else if (green < 0)
                            green = 0;
                        if (blue > 255)
                            blue = 255;
                        else if (blue < 0)
                            blue = 0;
                        Color newColor = Color.FromArgb(red, green, blue);
                        image.SetPixel(x, y, newColor);
                    }
                }

                pictureBox1.Image = image;
            }
            else if (comboBox1.Text == "Gamma correction")
            {
                Bitmap image = new Bitmap(pictureBox1.Image);

                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        Color pixelColor = image.GetPixel(x, y);
                        Color newColor = Color.FromArgb(Convert.ToInt32(255 * Math.Pow(((double)pixelColor.R / 255), 0.85)), Convert.ToInt32(255 * Math.Pow(((double)pixelColor.G / 255), 0.85)), Convert.ToInt32(255 * Math.Pow(((double)pixelColor.B / 255), 0.85)));
                        image.SetPixel(x, y, newColor);
                    }
                }

                pictureBox1.Image = image;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
                return;
            if (comboBox2.Text == "Blur")
            {
                Bitmap image = new Bitmap(pictureBox1.Image);
                double[,] kernel = {
                  {0.11, 0.11, 0.11},
                  {0.11, 0.11, 0.11},
                  {0.11, 0.11, 0.11}
            };
                Convolution(image, kernel);
                pictureBox1.Image = image;
            }
            else if (comboBox2.Text == "Gaussian Blur")
            {
                Bitmap image = new Bitmap(pictureBox1.Image);
                double[,] kernel = {
                  { 1/16f, 1/8f, 1/16f },
    { 1/8f, 1/4f, 1/8f },
    { 1/16f, 1/8f, 1/16f }
            };
                Convolution(image, kernel);
                pictureBox1.Image = image;
            }
            else if (comboBox2.Text == "Sharpen filter")
            {
                Bitmap image = new Bitmap(pictureBox1.Image);
                double[,] kernel = {
                  {0, -1, 0},
                  {-1, 5, -1},
                  {0, -1, 0}
            };
                Convolution(image, kernel);
                pictureBox1.Image = image;
            }
            else if (comboBox2.Text == "Edge detection")
            {
                Bitmap image = new Bitmap(pictureBox1.Image);
                double[,] kernel = {
                  {0, -1, 0},
                 {0, 1, 0},
                  {0, 0, 0}
            };
                Convolution(image, kernel);
                pictureBox1.Image = image;
            }
            else if (comboBox2.Text == "East Emboss")
            {
                Bitmap image = new Bitmap(pictureBox1.Image);
                double[,] kernel = {
                  {-1, 0, 1},
                 {-1, 1, 1},
                  {-1, 0, 1}
            };
                Convolution(image, kernel);
                pictureBox1.Image = image;
            }
        }
        //https://www.cyberforum.ru/blogs/529033/blog3507.html
        public class ImageWrapper : IDisposable, IEnumerable<Point>
        {

            public int Width { get; private set; }
            public int Height { get; private set; }
            public Color DefaultColor { get; set; }
            private byte[] data;
            private byte[] outData;
            private int stride;
            private BitmapData bmpData;
            private Bitmap bmp;
            public ImageWrapper(Bitmap bmp, bool copySourceToOutput = false)
            {
                Width = bmp.Width;
                Height = bmp.Height;
                this.bmp = bmp;
                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                stride = bmpData.Stride;
                data = new byte[stride * Height];
                System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, data, 0, data.Length);
                outData = copySourceToOutput ? (byte[])data.Clone() : new byte[stride * Height];
            }

            public Color this[int x, int y]
            {
                get
                {
                    var i = GetIndex(x, y);
                    return i < 0 ? DefaultColor : Color.FromArgb(data[i + 3], data[i + 2], data[i + 1], data[i]);
                }

                set
                {
                    var i = GetIndex(x, y);
                    if (i >= 0)
                    {
                        outData[i] = value.B;
                        outData[i + 1] = value.G;
                        outData[i + 2] = value.R;
                        outData[i + 3] = value.A;
                    };
                }
            }


            public Color this[Point p]
            {
                get { return this[p.X, p.Y]; }
                set { this[p.X, p.Y] = value; }
            }


            public void SetPixel(Point p, double r, double g, double b)
            {
                if (r < 0) r = 0;
                if (r >= 256) r = 255;
                if (g < 0) g = 0;
                if (g >= 256) g = 255;
                if (b < 0) b = 0;
                if (b >= 256) b = 255;

                this[p.X, p.Y] = Color.FromArgb((int)r, (int)g, (int)b);
            }

            int GetIndex(int x, int y)
            {
                return (x < 0 || x >= Width || y < 0 || y >= Height) ? -1 : x * 4 + y * stride;
            }
            public void Dispose()
            {
                System.Runtime.InteropServices.Marshal.Copy(outData, 0, bmpData.Scan0, outData.Length);
                bmp.UnlockBits(bmpData);
            }
            public IEnumerator<Point> GetEnumerator()
            {
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        yield return new Point(x, y);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        private void Convolution(Bitmap img, double[,] matrix)
        {
            var w = matrix.GetLength(0);
            var h = matrix.GetLength(1);

            using (var wr = new ImageWrapper(img) { DefaultColor = Color.Silver })
                foreach (var p in wr)
                {
                    double r = 0d, g = 0d, b = 0d;

                    for (int i = 0; i < w; i++)
                        for (int j = 0; j < h; j++)
                        {
                            var pixel = wr[p.X + i - 1, p.Y + j - 1];
                            r += matrix[j, i] * pixel.R;
                            g += matrix[j, i] * pixel.G;
                            b += matrix[j, i] * pixel.B;
                        }
                    wr.SetPixel(p, r, g, b);
                }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        //https://vscode.ru/prog-lessons/sohranit-izobrazhenie-iz-picturebox.html
        private void button4_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
                return;
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF)|*.GIF|Image Files(*.PNG)|*.PNG|All files (*.*)|*.*";

            if (sf.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pictureBox1.Image.Save(sf.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                catch
                {
                    MessageBox.Show("Impossible to save image", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


       

        private void button6_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
                return;
            else
            {
                pictureBox1.Image = pictureBox3.Image;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Bitmap img = new Bitmap(pictureBox1.Image);
            using (var wr = new ImageWrapper(img) { DefaultColor = Color.Silver })
                foreach (var p in wr)
                {

                    float r = 0f, g = 0f, b = 0f;
                    var pixel = wr[p.X, p.Y];
                    float[] arr = { pixel.R, pixel.G, pixel.B };
                    float min = arr.Min();
                    float max = arr.Max();
                    if (min == max)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                    }
                    else if (max == pixel.R)
                    {
                       
                        r = ((pixel.G - pixel.B) / (max - min)) % 6;
                      
                        if(r<0)
                        {
                            r = ((pixel.G - pixel.B) / (max - min)) + 6;
                        }
                        g =((pixel.G - pixel.B) / (max - min)) % 6;
                        if (g < 0)
                        {
                            g = ((pixel.G - pixel.B) / (max - min)) + 6;
                        }
                        b = ((pixel.G - pixel.B) / (max - min)) % 6;
                        if (b < 0)
                        {
                            b = ((pixel.G - pixel.B) / (max - min)) + 6;
                        }
                    }
                    else if (max == pixel.G)
                    {
                        r =(pixel.B - pixel.R) / (max - min) + 2;
                        g =(pixel.B - pixel.R) / (max - min) + 2;
                        b = (pixel.B - pixel.R) / (max - min) + 2;
                    }
                    else if (max == pixel.B)
                    {
                        r = (pixel.R - pixel.G) / (max - min) + 4;
                        g = (pixel.R - pixel.G) / (max - min) + 4;
                        b = (pixel.R - pixel.G) / (max - min) + 4;
                    }
                    wr.SetPixel(p, (r/6)*255, (g / 6) * 255, (b / 6) * 255);
                }
            pictureBox1.Image = img;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Bitmap img = new Bitmap(pictureBox1.Image);
            using (var wr = new ImageWrapper(img) { DefaultColor = Color.Silver })
                foreach (var p in wr)
                {
                    float r = 0f, g = 0f, b = 0f;
                    var pixel = wr[p.X  , p.Y];
                    float[] arr = { pixel.R, pixel.G, pixel.B };
                    float min = arr.Min();
                    float max = arr.Max();
                           if(max==0)
                            {
                                r = 0;
                                g = 0;
                                b = 0;
                            }
                            else
                            {
                                r = (max - min) / max;
                                g = (max - min) / max;
                                b = (max - min) / max;
                    }

                    wr.SetPixel(p, r *255, g  * 255, b  * 255);
                }
            pictureBox1.Image = img;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Bitmap img = new Bitmap(pictureBox1.Image);
            using (var wr = new ImageWrapper(img) { DefaultColor = Color.Silver })
                foreach (var p in wr)
                {

                    double r = 0d, g = 0d, b = 0d;
                    var pixel = wr[p.X, p.Y];
                    int[] arr = { pixel.R, pixel.G, pixel.B };
                    int min = arr.Min();
                    int max = arr.Max();
                    r = max;
                    g = max;
                    b = max;
                    wr.SetPixel(p, r, g, b);
                }
            pictureBox1.Image = img;

        }

        private void button10_Click(object sender, EventArgs e)
        {
            Bitmap img = new Bitmap(pictureBox1.Image);
            List<decimal> list = new List<decimal>(); ;
            int K = (int)numericUpDown1.Value;
            List<decimal> listthresh = new List<decimal>(); ;

            for (int i = 0; i < K; i++)
            {

                list.Add(i * Math.Ceiling((decimal)255 / (K - 1)));
            }
            listthresh = CalcualteTreshold(img,K,list);

            using (var wr = new ImageWrapper(img) { DefaultColor = Color.Silver })
                foreach (var p in wr)
                {


                    if (K == 2)
                    {
                        var pixel = wr[p.X, p.Y];
                        decimal intensity = (decimal)(0.2126 * pixel.R + 0.7152 * pixel.G + 0.0722 * pixel.B);
                        if (intensity > 127)
                        {
                            wr.SetPixel(p, 255, 255, 255);
                        }
                        else wr.SetPixel(p, 0, 0, 0);
                    }
                    else
                    {
                        var pixel = wr[p.X, p.Y];
                        decimal intensity = (decimal)(0.2126 * pixel.R + 0.7152 * pixel.G + 0.0722 * pixel.B);
                        //int k = (int)Math.Floor((intensity * (K - 1) / 255));
                        for (int k = 1; k < K - 1; k++)
                        {
                            if (intensity >= listthresh[k - 1] && intensity <= listthresh[k])
                            {
                                wr.SetPixel(p, (double)list[k + 1], (double)list[k + 1], (double)list[k + 1]);
                            }
                            else if (intensity < listthresh[0] && intensity >= 0)
                                wr.SetPixel(p, 0, 0, 0);
                            else if (intensity >= listthresh[listthresh.Count - 1] && intensity < 255)
                                wr.SetPixel(p, 255, 255, 255);
                            else continue;
                        }
                    }


                }
            pictureBox1.Image = img;

        }
       
        private List<decimal> CalcualteTreshold(Bitmap bitmap, int K, List<decimal> list)
        {
            List<decimal> listhresh = new List<decimal>();
            decimal sum = 0;
            int n=0;
            List<decimal> avg = new List<decimal>();
            for (int i=0; i < K-1; i++) {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        var pixel = bitmap.GetPixel(x, y);
                        decimal intensity = (decimal)(0.2126 * pixel.R + 0.7152 * pixel.G + 0.0722 * pixel.B);
                         if(intensity >= list[i] && intensity <= list[i+1])
                        {
                            listhresh.Add(intensity);
                            n++;
                            sum += intensity;
                        }
                    }
                }
                if (n == 0) n = 1;
                avg.Add((decimal)(sum / n));
                n = 0;
                sum = 0;
                listhresh.Clear();
            }


            return avg;
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            var value = 0;
            int numberOfColors = Int32.TryParse(textBox1.Text, out value) ? isPowerOfTwo(value) ? value : ShowMessageBox("Input has to be power of two") : ShowMessageBox("Invalid input");
            if (numberOfColors > 0) MedianCutQuantization(numberOfColors);

        }
        private int ShowMessageBox(String message) { MessageBox.Show(message, "Error"); return 0; }
        private bool isPowerOfTwo(int value)
        {
            return (Math.Ceiling(Math.Log(value, 2.0)) == Math.Floor(Math.Log(value, 2.0)));
        }
        private List<Color> RecursivelyDivideListintoParts(int depth, int maxDepth, List<Color> listOfPixels)
        {
            if (depth == maxDepth)
            {
                return listOfPixels;
            }
            else if (depth < maxDepth)
            {
                List<Color> returnList = new List<Color>();
                var priority = GetChannelWithBiggestRange(listOfPixels);
                var sortedList = SortWithChannelPriority(listOfPixels, priority);
                var leftList = sortedList.Take(sortedList.Count / 2).ToList();
                var rightList = sortedList.Skip(sortedList.Count / 2).ToList();
                returnList.AddRange(RecursivelyDivideListintoParts(depth + 1, maxDepth, leftList));
                returnList.AddRange(RecursivelyDivideListintoParts(depth + 1, maxDepth, rightList));
                return returnList;
            }
            else
                return null;

        }
        private void MedianCutQuantization(int numberOfColors)
        {
            Bitmap bitmap = new Bitmap(pictureBox1.Image);
            var listOfPixels = GetListOfPixels(bitmap);
            int depth = (int)Math.Sqrt(numberOfColors);
            var pixels = RecursivelyDivideListintoParts(0, depth, listOfPixels);
            var sizeOfCuboid = pixels.Count / numberOfColors;
            List<Color> listOfColorsForGivenCuboid = new List<Color>();
            int counter = 0;
            int meanR = 0; int meanG = 0; int meanB = 0;
            foreach (var pixel in pixels)
            {
                if (counter < sizeOfCuboid - 1)
                {
                    var temp = pixel;
                    meanR += temp.R;
                    meanG += temp.G;
                    meanB += temp.B;
                    counter++;
                }
                else
                {
                    listOfColorsForGivenCuboid.Add(Color.FromArgb(255, meanR / sizeOfCuboid, meanG / sizeOfCuboid, meanB / sizeOfCuboid));
                    meanR = 0; meanG = 0; meanB = 0; counter = 0;
                }
            }
            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    Color color = SetColorForGivenPixelAccordingToProperCuboid(pixel, listOfColorsForGivenCuboid);
                    bitmap.SetPixel(x, y, color);
                }
            }
            pictureBox1.Image = bitmap;
        }

        private Color SetColorForGivenPixelAccordingToProperCuboid(Color pixel, List<Color> listOfColorsForGivenCuboid)
        {
            double smallesDist = Math.Pow(255, 3);
            Color colorToSet = Color.FromArgb(255, 255, 255);
            for (int i = 0; i < listOfColorsForGivenCuboid.Count; i++)
            {
                if (Distance(pixel, listOfColorsForGivenCuboid[i]) < smallesDist)
                {
                    smallesDist = Distance(pixel, listOfColorsForGivenCuboid[i]);
                    colorToSet = listOfColorsForGivenCuboid[i];
                }
            }
            return colorToSet;
        }

        private double Distance(Color pixel, Color color)
        {
            return Math.Sqrt(Math.Pow(pixel.R - color.R, 2) + Math.Pow(pixel.G - color.G, 2) + Math.Pow(pixel.B - color.B, 2));
        }

        private List<Color> SortWithChannelPriority(List<Color> listOfPixels, int priority)
        {
           
            switch (priority)
            {
                case 0:
                    var sortedByR = listOfPixels.OrderBy(pixel => pixel.R).ToList();
                    return sortedByR;
                case 1:
                    var sortedByG = listOfPixels.OrderBy(pixel => pixel.G).ToList();
                    return sortedByG;
                case 2:
                    var sortedByB = listOfPixels.OrderBy(pixel => pixel.B).ToList();
                    return sortedByB;
            }
            return null;
        }

        private List<Color> GetListOfPixels(Bitmap bitmap)
        {
            List<Color> list = new List<Color>();
            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    list.Add(bitmap.GetPixel(x, y));
                }
            }
            return list;
        }

        private int GetChannelWithBiggestRange(List<Color> list)
        {
            int channelCode;
            int minR = 255;
            int minG = 255;
            int minB = 255;
            int maxR = 0;
            int maxG = 0;
            int maxB = 0;
            foreach (var pixel in list)
            {
                if (pixel.R < minR) minR = pixel.R;
                if (pixel.R > maxR) maxR = pixel.R;
                if (pixel.G < minG) minG = pixel.G;
                if (pixel.G > maxG) maxG = pixel.G;
                if (pixel.B > maxB) maxB = pixel.B;
                if (pixel.B < minB) minB = pixel.B;
            }
            channelCode = CompareChannels(maxR, maxG, maxB, minR, minG, minB);
            
            return channelCode;
        }

        private int CompareChannels(int maxR, int maxG, int maxB, int minR, int minG, int minB)
        {
            int code = 0;
            int rRange = maxR - minR;
            int gRange = maxG - minG;
            int bRange = maxB - minB;
            if (rRange > gRange && rRange > bRange) code = 0;
            else if (gRange > rRange && gRange > bRange) code = 1;
            else if (bRange > rRange && bRange > gRange) code = 2;
            return code;
        }
    }
}
