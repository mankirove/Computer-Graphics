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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LineList.Add(new Line());
            CircleList.Add(new Circle());
            PolygonList.Add(new Polygon());
            ArcList.Add(new Arc());
            RectangleList.Add(new RectangleLab());

        }
        Color curcolor;
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
                LineList.Clear();
                CircleList.Clear();
                PolygonList.Clear();
                ArcList.Clear();
                RectangleList.Clear();
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

       
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        [Serializable]
        public class AllFigures
        {
            public List<Line> LineList = new List<Line>();
            public List<Circle> CircleList = new List<Circle>();
            public List<Polygon> PolygonList = new List<Polygon>();
            public List<Arc> ArcList = new List<Arc>();
            public List<RectangleLab> RectangleList = new List<RectangleLab>();

        }


        List<Line> LineList = new List<Line>();
        [Serializable]
        public class Line
        {
            public Point p1;
            public Point p2;
            public (int R, int G, int B) savebordercolor;
            [JsonIgnore]
            public Color color;
            public int thickness;
        }
        [Serializable]
        public class RectangleLab
        {
            public Point p1;
            public Point p2;
            [JsonIgnore]
            public Color color;
            public List<Point> allpoints()
            {
                List<Point> points = new List<Point>{};
                points.Add(p1);
                Point p3 = new Point(p1.X, p2.Y);
                points.Add(p3);
                Point p4 = new Point(p2.X, p1.Y);
                points.Add(p2);
                points.Add(p4);

                return points;
            }
            [JsonIgnore]
            public Bitmap bit;
            public (int R, int G, int B) savebordercolor;
            public (int R, int G, int B) savefillcolor;
            [JsonIgnore]
            public Color fillcolor;
            public string location;


        }
        Line line = new Line();
        [Serializable]
        public class Circle
        {
            public Point p1;
            public (int R, int G, int B) savebordercolor;
            [JsonIgnore]
            public Color color;
            public int radius;

        }
        Circle circle = new Circle();
        List<Circle> CircleList = new List<Circle>();
        List<Polygon> PolygonList = new List<Polygon>();
        List<Arc> ArcList = new List<Arc>();
        List<RectangleLab> RectangleList = new List<RectangleLab>();

        [Serializable]
        public class Arc
        {
            public Point p1;
            public Point p2;
            public Point p3;
            [JsonIgnore]
            public Color color;
            public int radius;

           
        }
        Polygon polygon = new Polygon();
        [Serializable]
        public class Polygon
        {
            public List<Point> points = new List<Point>();
            [JsonIgnore]
            public Color color;
            [JsonIgnore]
            public Bitmap bit;
            public (int R,int G,int B) savebordercolor;
            public (int R, int G, int B) savefillcolor;
            [JsonIgnore]
            public Color fillcolor;
            public string location;
        }
        private void button11_Click(object sender, EventArgs e)
        {

          
            isLine = true;
            isCircle = false;
            isPolygon = false;
            isArc = false; isFilling = false;
            isImage = false;

        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        bool isLine;
        bool isCircle;
        bool isPolygon;
        bool isArc;
        bool isRectangle;
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (isLine) {
                if (LineList[LineList.Count - 1].p1.X == 0)
                {
                    LineList[LineList.Count - 1].p1.X = e.X;
                    LineList[LineList.Count - 1].p1.Y = e.Y;
                }
                else
                {
                    LineList[LineList.Count - 1].p2.X = e.X;
                    LineList[LineList.Count - 1].p2.Y = e.Y;

                    LineList[LineList.Count - 1].thickness = (int) numericUpDown1.Value;
                    LineList[LineList.Count - 1].color = curcolor;
                    Invalidate();
                    ddaline(LineList[LineList.Count - 1].p1, LineList[LineList.Count - 1].p2, LineList[LineList.Count - 1].color, (int)numericUpDown1.Value);
                    LineList.Add(new Line());
                }
            }
            else if (isCircle)
            {
                if (CircleList[CircleList.Count - 1].p1.X == 0)
                {
                    CircleList[CircleList.Count - 1].p1.X = e.X;
                    CircleList[CircleList.Count - 1].p1.Y = e.Y;
                }
                else
                {
                    CircleList[CircleList.Count - 1].color = curcolor;
                    CircleList[CircleList.Count - 1].radius = (int)Math.Sqrt((CircleList[CircleList.Count - 1].p1.X - e.X) * (CircleList[CircleList.Count - 1].p1.X - e.X) + (CircleList[CircleList.Count - 1].p1.Y - e.Y) * (CircleList[CircleList.Count - 1].p1.Y - e.Y));

                    Bitmap Bit = new Bitmap(pictureBox1.Image);
                    Invalidate();
                    circleMidPoint(CircleList[CircleList.Count - 1].p1, CircleList[CircleList.Count - 1].radius, Bit,curcolor);
                    CircleList.Add(new Circle());
                }

            }
            else if (isPolygon)
            {
                
                PolygonList[PolygonList.Count - 1].color = curcolor;
                if (PolygonList[PolygonList.Count-1].points.Count>0 && (int)Math.Sqrt((PolygonList[PolygonList.Count - 1].points[0].X - e.X) * (PolygonList[PolygonList.Count - 1].points[0].X - e.X) + (PolygonList[PolygonList.Count - 1].points[0].Y - e.Y) * (PolygonList[PolygonList.Count - 1].points[0].Y - e.Y)) < 10)        
                {

                    foreach (var (p1, p2) in PolygonList[PolygonList.Count - 1].points.Zip(PolygonList[PolygonList.Count - 1].points.Skip(1)))
                    { 
                        ddaline(p1, p2, PolygonList[PolygonList.Count - 1].color, (int)numericUpDown1.Value);

                    }
                    ddaline( PolygonList[PolygonList.Count - 1].points[0],PolygonList[PolygonList.Count - 1].points[PolygonList[PolygonList.Count - 1].points.Count - 1], PolygonList[PolygonList.Count - 1].color, (int)numericUpDown1.Value);
                    PolygonList.Add(new Polygon());
                }
                else
                {
                    PolygonList[PolygonList.Count - 1].points.Add(e.Location);

                }
               
            }
            else if (isArc)
            {
                if (ArcList[ArcList.Count - 1].p1.X == 0)
                {
                    ArcList[ArcList.Count - 1].p1.X = e.X;
                    ArcList[ArcList.Count - 1].p1.Y = e.Y;
                }
                else if(ArcList[ArcList.Count - 1].p2.X == 0)
                {
                    ArcList[ArcList.Count - 1].p2.X = e.X;
                    ArcList[ArcList.Count - 1].p2.Y = e.Y;
                }
                else
                {
                    ArcList[ArcList.Count - 1].p3 = e.Location;
                    ArcList[ArcList.Count - 1].radius = (int)Math.Sqrt((ArcList[ArcList.Count - 1].p1.X - ArcList[ArcList.Count - 1].p2.X) * (ArcList[ArcList.Count - 1].p1.X - ArcList[ArcList.Count - 1].p2.X) + (ArcList[ArcList.Count - 1].p1.Y - ArcList[ArcList.Count - 1].p2.Y) * (ArcList[ArcList.Count - 1].p1.Y - ArcList[ArcList.Count - 1].p2.Y));

                    Bitmap Bit = new Bitmap(pictureBox1.Image);
                    Invalidate();
                    arcMidPoint(ArcList[ArcList.Count - 1].p1, ArcList[ArcList.Count - 1].radius, Bit);
                    ddaline(ArcList[ArcList.Count - 1].p1, ArcList[ArcList.Count - 1].p2,Color.Black, 1);

                   Point D = arcddaline(ArcList[ArcList.Count - 1].p1, ArcList[ArcList.Count - 1].p3,true);

                  //  if(det(ArcList[ArcList.Count - 1].p1, ArcList[ArcList.Count - 1].p2,D)>0 && det(ArcList[ArcList.Count - 1].p1, ArcList[ArcList.Count - 1].p3, D)<0)
                  //  {

                      //  arcMidPoint(ArcList[ArcList.Count - 1].p1, ArcList[ArcList.Count - 1].radius, Bit);
                    //}
                    //else if(det(ArcList[ArcList.Count - 1].p1, ArcList[ArcList.Count - 1].p2, D) > 0 || det(ArcList[ArcList.Count - 1].p1, ArcList[ArcList.Count - 1].p3, D) < 0)
                   // {
                       // semicircleMidPoint(ArcList[ArcList.Count - 1].p1, ArcList[ArcList.Count - 1].radius, Bit);

                   // }
                    ArcList.Add(new Arc());
                }

            }


            else if (isRectangle)
            {
                if (RectangleList[RectangleList.Count - 1].p1.X == 0)
                {
                    RectangleList[RectangleList.Count - 1].p1.X = e.X;
                    RectangleList[RectangleList.Count - 1].p1.Y = e.Y;
                }
                else
                {
                    RectangleList[RectangleList.Count - 1].p2.X = e.X;
                    RectangleList[RectangleList.Count - 1].p2.Y = e.Y;

                    RectangleList[RectangleList.Count - 1].color = curcolor;
                    Point p3= new Point(e.X, RectangleList[RectangleList.Count - 1].p1.Y);
                    Point p4= new Point(RectangleList[RectangleList.Count - 1].p1.X, e.Y);
                    Invalidate();
                    ddaline(RectangleList[RectangleList.Count - 1].p1, p3, RectangleList[RectangleList.Count - 1].color, (int)numericUpDown1.Value);
                    ddaline(p3, RectangleList[RectangleList.Count - 1].p2, RectangleList[RectangleList.Count - 1].color, (int)numericUpDown1.Value);

                    ddaline(RectangleList[RectangleList.Count - 1].p2, p4, RectangleList[RectangleList.Count - 1].color, (int)numericUpDown1.Value);

                    ddaline(RectangleList[RectangleList.Count - 1].p1, p4, RectangleList[RectangleList.Count - 1].color, (int)numericUpDown1.Value);

                    RectangleList.Add(new RectangleLab());
                }
            }

            else if (isFilling)
            {

                foreach(var p in PolygonList)
                {
                    if(inside( e.Location, p.points))
                    {
                        activePolygon = p;
                        return;
                    }
                }
                foreach (var r in RectangleList) 
                {
                    if (inside(e.Location, r.allpoints()))
                    {
                        activeRectangle = r;
                        return;
                    }
                }
            }
            else if (isImage)
            {
                foreach (var p in PolygonList)
                {
                    if (inside(e.Location, p.points))
                    {
                        activePolygon = p;
                        return;
                    }
                }
                foreach (var r in RectangleList)
                {
                    if (inside(e.Location, r.allpoints()))
                    {
                        activeRectangle = r;
                        return;
                    }
                }
            }
            else if (isClipping)
            {
                //CyrusBeck(PolygonList[PolygonList.Count - 2].points, LineList[LineList.Count - 2].p1, LineList[LineList.Count - 2].p2);


                Point t = PolygonList[PolygonList.Count - 2].points[PolygonList[PolygonList.Count - 2].points.Count - 1];
                foreach( Point p in PolygonList[PolygonList.Count-2].points)
                {

                    CyrusBeck(PolygonList[PolygonList.Count - 3].points, p, t);
                    t = p;
                }

            }
        }

        bool isClipping;
        Polygon activePolygon;
        RectangleLab activeRectangle;
        private void button5_Click(object sender, EventArgs e)
        {


            isImage = false;
            isCircle = true;
            isLine= false;
            isPolygon = false;
            isArc = false;
            isFilling = false;


        }
        private void circleMidPoint(Point p1, int radius, Bitmap Bit,Color c,bool arc=false)
        {
            int x = 0;
            int y = radius;
            int p = 1 - radius;

            circlePlotPoints(p1.X, p1.Y, x, y, Bit,c);
            while (y > x)
            {
                x++;
                if (p < 0) 
                {
                    p += 2 * x + 1;
                }
                else
                {  
                    y--;
                    p += 2 * (x - y) + 1;
                }

                circlePlotPoints(p1.X, p1.Y, x, y, Bit,c);

            }
;
        }
        private void arcMidPoint(Point p1, int radius, Bitmap Bit, bool arc = false)
        {
            int x = 0;
            int y = radius;
            int p = 1 - radius;

           ArcPlotPoints(p1.X, p1.Y, x, y, Bit);
            while (y > x)
            {
                x++;
                if (p < 0)
                {
                    p += 2 * x + 1;
                }
                else
                {
                    y--;
                    p += 2 * (x - y) + 1;
                }

                ArcPlotPoints(p1.X, p1.Y, x, y, Bit);

            }
;
        }
        private void semicircleMidPoint(Point p1, int radius, Bitmap Bit, bool arc = false)
        {
            int x = 0;
            int y = radius;
            int p = 1 - radius;

            semicirclePlotPoints(p1.X, p1.Y, x, y, Bit);
            while (y > x)
            {
                x++;
                if (p < 0)
                {
                    p += 2 * x + 1;
                }
                else
                {
                    y--;
                    p += 2 * (x - y) + 1;
                }

                semicirclePlotPoints(p1.X, p1.Y, x, y, Bit);

            }
;
        }
        private void circlePlotPoints(int xCenter, int yCenter, int x, int y, Bitmap Bit,Color c)
        {
            Bit.SetPixel(xCenter + x, yCenter + y, c);
            Bit.SetPixel(xCenter - x, yCenter + y, c);
            Bit.SetPixel(xCenter + x, yCenter - y, c);
            Bit.SetPixel(xCenter - x, yCenter - y, c);
            Bit.SetPixel(xCenter + y, yCenter + x, c);
            Bit.SetPixel(xCenter - y, yCenter + x, c);
            Bit.SetPixel(xCenter + y, yCenter - x, c);
            Bit.SetPixel(xCenter - y, yCenter - x, c);
            pictureBox1.Image = Bit;
        }
        private double det(Point A,Point B,Point C)
        {
            return  A.X * B.Y - A.X * C.Y - A.Y * B.X + A.Y * C.X + B.X * C.Y - B.Y*C.Y;

        }
        private void semicirclePlotPoints(int xCenter, int yCenter, int x, int y, Bitmap Bit)
        {
            Bit.SetPixel(xCenter + x, yCenter + y, Color.Blue);
            Bit.SetPixel(xCenter + y, yCenter + x, Color.Blue);
            Bit.SetPixel(xCenter + x, yCenter - y, Color.Blue);
            Bit.SetPixel(xCenter + y, yCenter - x, Color.Blue);
            Bit.SetPixel(xCenter - x, yCenter - y, Color.Blue);
            Bit.SetPixel(xCenter - y, yCenter - x, Color.Blue);

            pictureBox1.Image = Bit;

        }
        private void ArcPlotPoints(int xCenter, int yCenter, int x, int y, Bitmap Bit)
        {
            Bit.SetPixel(xCenter + x, yCenter + y, Color.White);
            Bit.SetPixel(xCenter - x, yCenter + y, Color.White);
            Bit.SetPixel(xCenter + x, yCenter - y, Color.White);
            Bit.SetPixel(xCenter - x, yCenter - y, Color.White);
            Bit.SetPixel(xCenter + y, yCenter + x, Color.White);
            Bit.SetPixel(xCenter - y, yCenter + x, Color.White);
            Bit.SetPixel(xCenter + y, yCenter - x, Color.White);
            Bit.SetPixel(xCenter - y, yCenter - x, Color.White);
            pictureBox1.Image = Bit;

        }
        public string bitmaplocation;
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {

            if (activeRectangle != null)
            {
                if (tmp is not null && isImage)
                {
                    fillImageRect(activeRectangle.allpoints(), tmp);
                    activeRectangle.bit = tmp;
                    activeRectangle.location = bitmaplocation;
                    activeRectangle = null;
                }
                else if (isFilling)
                {
                    fillPolygon(activeRectangle.allpoints(), curcolor);
                    activeRectangle.fillcolor = curcolor;
                    activeRectangle = null;

                }

            }
            if (activePolygon != null)
            {
                if (tmp is not null && isImage)
                {
                    fillImage(activePolygon.points, tmp);
                    activePolygon.bit = tmp;
                    activePolygon.location = bitmaplocation;

                    activePolygon = null;
                }
                else if (isFilling)
                {
                    fillPolygon(activePolygon.points, curcolor);
                    activePolygon.fillcolor = curcolor;

                    activePolygon = null;

                }

            }
            
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            
        }

        private void button12_Click(object sender, EventArgs e)
        {
            isCircle = false;
            isLine = false;
            isPolygon = true;
            isArc = false;
            isRectangle = false;
            isFilling = false;
            isImage = false;

        }
        bool isImage;
        private void ddaline(Point p1, Point p2,Color c,int thickness)
        {
            int k = 0;
            double xinc, yinc, x, x1, y, y1, steps;
            Bitmap p = new Bitmap(pictureBox1.Image);
            double deltaX = p2.X - p1.X;
            double deltaY = p2.Y - p1.Y;
            if (Math.Abs(deltaX) > Math.Abs(deltaY)) steps = Math.Abs(deltaX);
            else steps = Math.Abs(deltaY);
            xinc = deltaX / steps;
            yinc = deltaY / steps;
            x = x1 = p1.X;
            y = y1 = p1.Y;
            p.SetPixel((int)(Math.Round(x)), (int)Math.Round(y), c);
            CopyPixel((int)Math.Round(x), (int)Math.Round(y),(Math.Abs(deltaX) > Math.Abs(deltaY)),thickness,p,c);
            for (k = 1; k <= steps; k++)
            {

                //if (drawarc==true && p.GetPixel((int)(Math.Round(x)), (int)Math.Round(y)).ToArgb() == Color.White.ToArgb()) return;
                p.SetPixel((int)(Math.Round(x)), (int)Math.Round(y), c);
                CopyPixel((int)Math.Round(x), (int)Math.Round(y), (Math.Abs(deltaX) > Math.Abs(deltaY)), thickness, p,c);
                x = x + xinc;
                y = y + yinc;
            }
            pictureBox1.Image = p;
        }
        private void CopyPixel(int x, int y, bool Gorizontal, int thickness,Bitmap p,Color c)
        {
            int distance = thickness / 2;
            for (var d = -distance; d <= distance; ++d)
            {
                if(Gorizontal==true)
                p.SetPixel(x, y + d,c);
                    else
               p.SetPixel(x +d, y,c);
        }
    }
        private Point arcddaline(Point p1, Point p2, bool drawarc = false)
        {
           // int k = 0;
            double xinc, yinc, x, x1, y, y1, steps;
            Bitmap p = new Bitmap(pictureBox1.Image);
            double deltaX = p2.X - p1.X;
            double deltaY = p2.Y - p1.Y;
            if (Math.Abs(deltaX) > Math.Abs(deltaY)) steps = Math.Abs(deltaX);
            else steps = Math.Abs(deltaY);
            xinc = deltaX / steps;
            yinc = deltaY / steps;
            x = x1 = p1.X;
            y = y1 = p1.Y;
            p.SetPixel((int)(Math.Round(x)), (int)Math.Round(y), Color.Black);
            if (drawarc == true)
            {
                while(p.GetPixel((int)(Math.Round(x)), (int)Math.Round(y)).ToArgb() != Color.White.ToArgb())
                {

                    p.SetPixel((int)(Math.Round(x)), (int)Math.Round(y), Color.Black);

                    x = x + xinc;
                    y = y + yinc;
                }
            }
            
            pictureBox1.Image = p;
            return new Point((int)Math.Round(x), (int)Math.Round(y));
        }
       
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            isCircle = false;
            isLine = false;
            isPolygon = false;
            isArc = true;
            isFilling = false;
            isImage= false;

        }


        private void ddalineforload(Point p1, Point p2, int thickness,Bitmap b,Color c)
        {
            int k = 0;
            double xinc, yinc, x, x1, y, y1, steps;
            //Bitmap p = new Bitmap(pictureBox1.Image);
            double deltaX = p2.X - p1.X;
            double deltaY = p2.Y - p1.Y;
            if (Math.Abs(deltaX) > Math.Abs(deltaY)) steps = Math.Abs(deltaX);
            else steps = Math.Abs(deltaY);
            xinc = deltaX / steps;
            yinc = deltaY / steps;
            x = x1 = p1.X;
            y = y1 = p1.Y;
            b.SetPixel((int)(Math.Round(x)), (int)Math.Round(y), c);
            CopyPixel((int)Math.Round(x), (int)Math.Round(y), (Math.Abs(deltaX) > Math.Abs(deltaY)), thickness, b,c);
            for (k = 1; k <= steps; k++)
            {

                //if (drawarc==true && p.GetPixel((int)(Math.Round(x)), (int)Math.Round(y)).ToArgb() == Color.White.ToArgb()) return;
                b.SetPixel((int)(Math.Round(x)), (int)Math.Round(y), c);
                CopyPixel((int)Math.Round(x), (int)Math.Round(y), (Math.Abs(deltaX) > Math.Abs(deltaY)), thickness, b,c);
                x = x + xinc;
                y = y + yinc;
            }
           // pictureBox1.Image = p;
        }
       
        private void button3_Click(object sender, EventArgs e)
        {

            AllFigures allFigures = new AllFigures() ;
            allFigures.LineList = LineList;
            allFigures.CircleList = CircleList;
            allFigures.PolygonList = PolygonList;
            allFigures.ArcList = ArcList;
            allFigures.RectangleList = RectangleList;

            foreach(Polygon p in PolygonList)
            {
                p.savebordercolor.R = p.color.R;
                p.savebordercolor.G = p.color.G;
                p.savebordercolor.B = p.color.B;
                p.savefillcolor.R = p.fillcolor.R;
                p.savefillcolor.G = p.fillcolor.G;

                p.savefillcolor.B = p.fillcolor.B;

            }
            foreach (Circle p in CircleList)
            {
                p.savebordercolor.R = p.color.R;
                p.savebordercolor.G = p.color.G;
                p.savebordercolor.B = p.color.B;
            }
            foreach (Line p in LineList)
            {
                p.savebordercolor.R = p.color.R;
                p.savebordercolor.G = p.color.G;
                p.savebordercolor.B = p.color.B;
            }
            foreach (RectangleLab p in RectangleList)
            {
                p.savebordercolor.R = p.color.R;
                p.savebordercolor.G = p.color.G;
                p.savebordercolor.B = p.color.B;
                p.savefillcolor.R = p.fillcolor.R;
                p.savefillcolor.G = p.fillcolor.G;
                p.savefillcolor.B = p.fillcolor.B;
               
            }
            var dialog = new SaveFileDialog();
            dialog.Filter = "JSON Files|*.json";
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                var options = new JsonSerializerOptions();
                options.IncludeFields = true;
                string jasonstring = JsonSerializer.Serialize(allFigures, options);
                File.WriteAllText(dialog.FileName, jasonstring);
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "JSON Files|*.json";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string jasonstring = File.ReadAllText(dialog.FileName);
                var options = new JsonSerializerOptions();
                options.IncludeFields = true;
                AllFigures allFigures = JsonSerializer.Deserialize<AllFigures>(jasonstring,options);
               LineList= allFigures.LineList;
                CircleList= allFigures.CircleList ;
                PolygonList= allFigures.PolygonList ;
                 ArcList=allFigures.ArcList;
                RectangleList = allFigures.RectangleList;

                foreach (RectangleLab p in RectangleList)
                {

                    if (File.Exists(p.location))
                    {
                        p.bit = new Bitmap(p.location);
                    }
                    else
                    { p.fillcolor = Color.FromArgb(p.savefillcolor.R, p.savefillcolor.G, p.savefillcolor.B); }

                    p.color = Color.FromArgb(p.savebordercolor.R, p.savebordercolor.G, p.savebordercolor.B);

                }
                foreach (Polygon p in PolygonList)
                {

                    if (File.Exists(p.location))
                    {
                        p.bit = new Bitmap(p.location);
                    }
                    else
                    {
                        p.fillcolor = Color.FromArgb(p.savefillcolor.R, p.savefillcolor.G, p.savefillcolor.B);
                    }
                    p.color = Color.FromArgb(p.savebordercolor.R, p.savebordercolor.G, p.savebordercolor.B);


                }
                foreach (Circle p in CircleList)
                {
                    p.color = Color.FromArgb(p.savebordercolor.R, p.savebordercolor.G, p.savebordercolor.B);

                }
                foreach (Line p in LineList)
                {
                    p.color = Color.FromArgb(p.savebordercolor.R, p.savebordercolor.G, p.savebordercolor.B);

                }
                Redraw();

            }

        }
        private void Redraw()
        {
            Bitmap Bit = new Bitmap(pictureBox1.Image);

            for ( int i =0; i<LineList.Count-1;i++)
            {
                ddalineforload(LineList[i].p1,LineList[i].p2,LineList[i].thickness,Bit,LineList[i].color);
            }
            for (int i = 0; i < CircleList.Count-1; i++)
            {
                circleMidPoint(CircleList[i].p1, CircleList[i].radius,Bit, CircleList[i].color);
            }
            for (int i = 0; i < PolygonList.Count-1; i++)
            {
                foreach (var (p1, p2) in PolygonList[i].points.Zip(PolygonList[i].points.Skip(1)))
                {
                    ddalineforload(p1, p2, LineList[i].thickness,Bit, LineList[i].color);

                }
                ddalineforload(PolygonList[i].points[0], PolygonList[i].points[PolygonList[i].points.Count - 1], LineList[i].thickness,Bit, LineList[i].color);
            } 
            for (int i = 0; i < ArcList.Count-1; i++)
            {
            
                arcMidPoint(ArcList[i].p1, ArcList[i].radius, Bit);
                ddaline(ArcList[i].p1, ArcList[i].p2, LineList[i].color,(int)numericUpDown1.Value);
                Point D = arcddaline(ArcList[i].p1, ArcList[i].p3, true);
            }

        }

        private void button14_Click(object sender, EventArgs e)
        {
            var dialog = new ColorDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                curcolor = dialog.Color;
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            isLine = false;
            isCircle = false;
            isPolygon = false;
            isArc = false;
            isRectangle = true;
            isFilling = false;
            isImage = false;
        }

        public class ETentry
        {
            public int ymin { get; set; }
            public int ymax { get; set; }
            public float xmin { get; set; }
            public float oneoverm { get; set; }
        }

        public List<ETentry> getEgdeTable(List<Point> polygon)
        {
            List<ETentry> edgeTable = new List<ETentry>();
            Point temp = polygon.Last();

            foreach (var v in polygon)
            {
                ETentry et = new ETentry();
                et.ymin = Math.Min(v.Y, temp.Y);
                et.ymax = Math.Max(v.Y, temp.Y);
                if (v.Y < temp.Y)
                    et.xmin = v.X;
                else
                    et.xmin = temp.X;
                int dy = (v.Y - temp.Y);
                int dx = (v.X - temp.X);
              
                if (dy == 0) et.oneoverm = 0;
               
                if (dy != 0) et.oneoverm = (float)dx / (float)dy;
               
                edgeTable.Add(et);

                temp = v;
            }

            edgeTable.Sort((p, q) => p.ymin.CompareTo(q.ymin));

            return edgeTable;
        }

        public void fillPolygon(List<Point> polygon, Color color)
        {
            List<ETentry> edgeTable = getEgdeTable(polygon);
            ETentry ETmin = edgeTable[0];
            int y = ETmin.ymin; 
            Bitmap img = new Bitmap(pictureBox1.Image);

            List<ETentry> activeEdgeTable = new List<ETentry>();
            while (edgeTable.Count != 0 || activeEdgeTable.Count != 0)
            {
                List<ETentry> toRemove = new List<ETentry>();
                foreach (var et in edgeTable)
                {
                    if (et.ymin == y)
                    {
                        activeEdgeTable.Add(et);
                        toRemove.Add(et);
                    }
                    else
                    {
                        break;
                    }
                }
                foreach (var et in toRemove)
                {
                    edgeTable.Remove(et);
                }
                toRemove.Clear();
                activeEdgeTable.Sort((p, q) => p.xmin.CompareTo(q.xmin));

                for (int n = 0; n < activeEdgeTable.Count; n += 2)
                {
                    
                    for (int a = (int)activeEdgeTable[n].xmin; (int)a <= activeEdgeTable[n + 1].xmin; a++)
                    {
                        img.SetPixel(a, y,color);                    
                    }
                }

                ++y;

                foreach (var e in activeEdgeTable.ToList())
                {
                    if (e.ymax == y)
                        activeEdgeTable.Remove(e);
                }

                foreach (var e in activeEdgeTable.ToList())
                {
                    e.xmin += e.oneoverm;
                }
            }
            pictureBox1.Image = img;
        }
        public void fillRectangle(List<Point> polygon, Color color)
        {
            List<ETentry> edgeTable = getEgdeTable(polygon);
            ETentry ETmin = edgeTable[0];
            int y = ETmin.ymin;
            Bitmap img = new Bitmap(pictureBox1.Image);

            List<ETentry> activeEdgeTable = new List<ETentry>();
            while (edgeTable.Count != 0 || activeEdgeTable.Count != 0)
            {
                List<ETentry> toRemove = new List<ETentry>();
                foreach (var et in edgeTable)
                {
                    if (et.ymin == y )
                    {
                        activeEdgeTable.Add(et);
                        toRemove.Add(et);
                    }
                    else
                    {
                        break;                        
                    }
                }
                foreach (var et in toRemove)
                {
                    edgeTable.Remove(et);
                }
                toRemove.Clear();
                activeEdgeTable.RemoveAll(entry=>entry.ymin==entry.ymax);
                
                activeEdgeTable.Sort((p, q) => p.xmin.CompareTo(q.xmin));

                for (int n = 0; n < activeEdgeTable.Count; n += 2)
                {

                    for (int a = (int)activeEdgeTable[n].xmin; (int)a <= activeEdgeTable[n + 1].xmin; a++)
                    {
                        img.SetPixel(a, y, color);
                    }
                }

                ++y;

                foreach (var e in activeEdgeTable.ToList())
                {
                    if (e.ymax == y)
                        activeEdgeTable.Remove(e);
                }

                foreach (var e in activeEdgeTable.ToList())
                {
                    e.xmin += e.oneoverm;
                }
            }
            pictureBox1.Image = img;
        }
        private void button16_Click(object sender, EventArgs e)
        {
            isLine = false;
            isCircle = false;
            isPolygon = false;
            isArc = false;
            isRectangle = false;
            isFilling = true;
            isImage = false;
        }
        bool isFilling;
        private bool inside(Point point, List<Point> vs)
        {


            var x = point.X;
            var y = point.Y;

            bool inside = false;
            for (int i = 0, j = vs.Count - 1; i < vs.Count; j = i++)
            {
                var xi = vs[i].X; 
                var yi = vs[i].Y;
                var xj = vs[j].X;
                var yj = vs[j].Y;

                var intersect = ((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi);
                if (intersect) inside = !inside;
            }

            return inside;
        }
        public void fillImage(List<Point> polygon,Bitmap image)
        {
            List<ETentry> edgeTable = getEgdeTable(polygon);
            ETentry ETmin = edgeTable[0];
            int y = ETmin.ymin;
            Bitmap bmp = new Bitmap(pictureBox1.Image);

            List<ETentry> activeEdgeTable = new List<ETentry>();
            while (edgeTable.Count != 0 || activeEdgeTable.Count != 0)
            {
                List<ETentry> toRemove = new List<ETentry>();
                foreach (var et in edgeTable)
                {
                    if (et.ymin == y)
                    {
                        activeEdgeTable.Add(et);
                        toRemove.Add(et);
                    }
                    else
                    {
                        break;
                    }
                }
                foreach (var et in toRemove)
                {
                    edgeTable.Remove(et);
                }
                toRemove.Clear();
                activeEdgeTable.Sort((p, q) => p.xmin.CompareTo(q.xmin));

                for (int n = 0; n < activeEdgeTable.Count; n += 2)
                {

                    for (int a = (int)activeEdgeTable[n].xmin; (int)a <= activeEdgeTable[n + 1].xmin; a++)
                    {
                        Color color = image.GetPixel(a%image.Width, y%image.Height);
                        bmp.SetPixel(a, y,color);
                    }
                }

                ++y;

                foreach (var e in activeEdgeTable.ToList())
                {
                    if (e.ymax == y)
                        activeEdgeTable.Remove(e);
                }

                foreach (var e in activeEdgeTable.ToList())
                {
                    e.xmin += e.oneoverm;
                }
            }
            pictureBox1.Image = bmp;
        }

        public void fillImageRect(List<Point> polygon, Bitmap image)
        {
            List<ETentry> edgeTable = getEgdeTable(polygon);
            ETentry ETmin = edgeTable[0];
            int y = ETmin.ymin;
            Bitmap bmp = new Bitmap(pictureBox1.Image);

            List<ETentry> activeEdgeTable = new List<ETentry>();
            while (edgeTable.Count != 0 || activeEdgeTable.Count != 0)
            {
                List<ETentry> toRemove = new List<ETentry>();
                foreach (var et in edgeTable)
                {
                    if (et.ymin == y)
                    {
                        activeEdgeTable.Add(et);
                        toRemove.Add(et);
                    }
                    else
                    {
                        break;
                    }
                }
                foreach (var et in toRemove)
                {
                    edgeTable.Remove(et);
                }
                toRemove.Clear();
                activeEdgeTable.RemoveAll(entry => entry.ymin == entry.ymax);

                activeEdgeTable.Sort((p, q) => p.xmin.CompareTo(q.xmin));

                for (int n = 0; n < activeEdgeTable.Count; n += 2)
                {

                    for (int a = (int)activeEdgeTable[n].xmin; (int)a <= activeEdgeTable[n + 1].xmin; a++)
                    {
                         Color color = image.GetPixel(a % image.Width, y % image.Height);
                        bmp.SetPixel(a, y, color);
                    }
                }

                ++y;

                foreach (var e in activeEdgeTable.ToList())
                {
                    if (e.ymax == y)
                        activeEdgeTable.Remove(e);
                }

                foreach (var e in activeEdgeTable.ToList())
                {
                    e.xmin += e.oneoverm;
                }
            }
            pictureBox1.Image = bmp;
        }
        Bitmap tmp;
        private void button17_Click(object sender, EventArgs e)
        {

            isLine = false;
            isCircle = false;
            isPolygon = false;
            isArc = false;
            isRectangle = false;
            isFilling = false;
            isImage = true;
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "Bitmaps|*.bmp|jpeps|*.jpg";
            
            if (op.ShowDialog() == DialogResult.OK)
            {
                tmp = new Bitmap(op.FileName);
                bitmaplocation = op.FileName;
            }

        }

        public bool Clockwisecheck(List<Point> polygon)
        {
            bool clockwise = false;
            Point p1 = polygon[0];
            Point p2 = polygon[1];
            Point p3 = polygon[2];
            int delX1 = p2.X - p1.X;
            int delX2 = p3.X - p2.X;
            int delY1 = p2.Y - p1.Y;
            int delY2 = p3.Y - p2.Y;

            if ((delY1 > 0 && delX2 < 0) || (delX1 < 0 && delY2 < 0) || (delY1 < 0 && delX2 > 0) || (delX1 > 0 && delY2 > 0))
                clockwise = true;

            return clockwise;

        }

        void CyrusBeck(List<Point> polygon, Point p1, Point p2)
        {
           
            int delX = p2.X - p1.X;
            int delY = p2.Y - p1.Y;
            Point D = new Point(delX, delY);
            Point temp1 = polygon.ElementAt(polygon.Count() - 1);
            double tEnter = 0;
            double tLeave = 1;

            bool clockwise = Clockwisecheck(polygon);

            foreach (var p in polygon)
            {
                Point n = getInsideNormal(temp1, p, clockwise);
                temp1 = p;
                Point w = new Point(p1.X - p.X, p1.Y - p.Y);
                double num = dotProduct(w, n);
                double den = dotProduct(D, n);
                if (den == 0) 
                {
                    if (num < 0) 
                    {
                        return;
                    }
                    else
                    {
                        continue;
                    }
                }

                double t = -num / den;
                if (den > 0)
                {
                    tEnter = Math.Max(tEnter, t);
                }
                else
                {
                    tLeave = Math.Min(tLeave, t);
                }
            }
            if (tEnter > tLeave)
            {
                return;
            }
            double x1 = p1.X + delX * tEnter;
            double y1 = p1.Y + delY * tEnter;
            double x2 = p1.X + delX * tLeave;
            double y2 = p1.Y + delY * tLeave;
            Point ddap1 = new Point((int)(p1.X + delX * tEnter), (int)(p1.Y + delY * tEnter));
            Point ddap2 = new Point((int)(p1.X + delX * tLeave), (int)(p1.Y + delY * tLeave));

            ddaline(ddap1,ddap2,Color.Blue,(int)(numericUpDown1.Value));
            pictureBox1.Invalidate();
        }
        double dotProduct(Point p1, Point p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y;
        }

        Point getInsideNormal(Point p1, Point p2/*, Point z*/, bool clockwise)
        {
            int delX = p2.X - p1.X;
            int delY = p2.Y - p1.Y;
            Point n = new Point();
            if (clockwise)
                n = new Point(-delY, delX);
            else
                n = new Point(delY, -delX);
   
            return n;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            isLine = false;
            isCircle = false;
            isPolygon = false;
            isArc = false;
            isRectangle = false;
            isFilling = false;
            isImage = false;
            isClipping = true;
        }
    }
}
