using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using Emgu.CV.CvEnum;
using Emgu.CV;
using Emgu.CV.Structure;
using MySql.Data.MySqlClient;

namespace imageProcessing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        Graphics g;
        Bitmap video;
        int trafficId = 0;
        int mode;
        int red, green, blue;
        int degisken = 5;
        private FilterInfoCollection captureDevice;
        private VideoCaptureDevice finalFrame;
        public Size size;
        public string ofdFileName;
        public string chainCode = "";
        double[,] freemanVector = new double[8, 1];
        public byte[] byteChainCode;
        public int baslagicKonumX, baslangickonumY;
        public Bitmap imageFreeman, imageFreeman2, imageFreemanCopy;
        Color siyah = Color.FromArgb(0, 0, 0),
                kirmizi = Color.FromArgb(255, 0, 0),
                beyaz = Color.FromArgb(255, 255, 255);
        System.Drawing.Point beyazPixel, baslangicPixeli, bufferBeyazPixel;
        MySqlConnection connection;
        string connectionString = "server=localhost;database=trafficsignhistnorms;uid=root;pwd=1234";

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (finalFrame.IsRunning)
                finalFrame.Stop();

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ofdFileName = openFileDialog1.FileName;
                pictureBox1.Image = (Bitmap)System.Drawing.Image.FromFile(ofdFileName);
            }
        }

        private void grayscaleImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GrayscaleBT709 gray = new GrayscaleBT709();

            pictureBox2.Image = gray.Apply((Bitmap)pictureBox1.Image);
        }

        private void cameraCaptureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            finalFrame = new VideoCaptureDevice(captureDevice[comboBox1.SelectedIndex].MonikerString);
            finalFrame.NewFrame += FinalFrame_NewFrame;
            finalFrame.Start();
        }

        private void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            video = (Bitmap)eventArgs.Frame.Clone();
            Bitmap video2 = (Bitmap)eventArgs.Frame.Clone();

            switch (mode)
            {
                case 2:
                    g = Graphics.FromImage(video2);
                    g.DrawString(degisken.ToString(), new Font("Arial", 20), Brushes.Cyan, new PointF(2, 2));
                    g.Dispose();

                    break;

                case 1:

                    ColorFiltering colorFilter = new ColorFiltering();
                    colorFilter.Red = new IntRange(red, (int)numericUpDownRed.Value);
                    colorFilter.Green = new IntRange(green, (int)numericUpDownGreen.Value);
                    colorFilter.Blue = new IntRange(blue, (int)numericUpDownBlue.Value);
                    colorFilter.ApplyInPlace(video2);

                    BlobCounter blobCounter = new BlobCounter();
                    blobCounter.MinHeight = 20;
                    blobCounter.MinWidth = 20;
                    blobCounter.ObjectsOrder = ObjectsOrder.Size;
                    blobCounter.ProcessImage(video2);
                    Rectangle[] rect = blobCounter.GetObjectsRectangles();
                    if (rect.Length > 0)
                    {
                        Rectangle objectt = rect[0];
                        Graphics graph = Graphics.FromImage(video2);
                        using (Pen pen = new Pen(Color.White, 3))
                        {
                            graph.DrawRectangle(pen, objectt);
                        }
                        graph.Dispose();
                    }
                    pictureBox2.Image = video2;

                    break;
            }
            pictureBox1.Image = video;
            //pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            captureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in captureDevice)
            {
                comboBox1.Items.Add(device.Name);
            }

            comboBox1.SelectedIndex = 0;
            finalFrame = new VideoCaptureDevice();

            string queryReadString = "select id from trafficsignhistnorms.traffic_signs;";
            connection = new MySqlConnection(connectionString);
            MySqlCommand readCmd = new MySqlCommand(queryReadString, connection);

            try
            {
                connection.Open();
                MySqlDataReader dataReader = readCmd.ExecuteReader();
                while (dataReader.Read())
                {
                    trafficId = Convert.ToInt32(dataReader["id"].ToString());
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString() + " Connection Unsuccessfull");
            }


        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (finalFrame.IsRunning)
                finalFrame.Stop();

            connection.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = (Bitmap)pictureBox1.Image.Clone();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
                pictureBox2.Image.Save(@"../../../resim1.bmp", ImageFormat.Bmp);
            else
                MessageBox.Show("Kaydedilecek Bir Resim Bulunamadı !!!");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            degisken--;
            if (degisken == 0)
            {
                timer1.Enabled = false;
                degisken = 5;
            }
            pictureBox2.Image = video;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            mode = 2;
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Sepia sp = new Sepia();
            pictureBox2.Image = sp.Apply((Bitmap)pictureBox1.Image);
        }

        private void hueModifierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HueModifier hue = new HueModifier();
            pictureBox2.Image = hue.Apply((Bitmap)pictureBox1.Image);
        }

        private void rotateChannelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RotateChannels rotateChannels = new RotateChannels();
            pictureBox2.Image = rotateChannels.Apply((Bitmap)pictureBox1.Image);
        }

        private void ınvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Invert invert = new Invert();
            pictureBox2.Image = invert.Apply((Bitmap)pictureBox1.Image);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            lbDeger.Text = trackBar1.Value.ToString();

            if (pictureBox1.Image != null)
            {
                BrightnessCorrection brgCrr = new BrightnessCorrection(trackBar1.Value);
                try
                {
                    pictureBox2.Image = brgCrr.Apply((Bitmap)pictureBox1.Image.Clone());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }
        }

        private void trackBarRed_Scroll(object sender, EventArgs e)
        {
            red = (int)trackBarRed.Value;
        }

        private void trackBarGreen_Scroll(object sender, EventArgs e)
        {
            green = (int)trackBarGreen.Value;
        }

        private void trackBarBlue_Scroll(object sender, EventArgs e)
        {
            blue = (int)trackBarBlue.Value;
        }

        private void trackBarRotate_Scroll(object sender, EventArgs e)
        {
            RotateBilinear rotateImg = new RotateBilinear((double)trackBarRotate.Value);
            pictureBox2.Image = rotateImg.Apply((Bitmap)pictureBox1.Image.Clone());

        }

        private void makeGrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox2.Image = makeGray((Bitmap)pictureBox1.Image);
            }
            else
                MessageBox.Show("Önce bir resim seçiniz !!!");
        }

        private void makeBinaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox2.Image = makeBinary((Bitmap)pictureBox1.Image);
            }
            else
                MessageBox.Show("Önce bir resim seçiniz !!!");
        }

        private void brightnessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
                trackBar1.Visible = true;
            else
                MessageBox.Show("PictureBox Can Not Be Null !!!");

        }

        private void button4_Click(object sender, EventArgs e)
        {
            mode = 1;
        }

        private void sobelAlgorithmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap image = new Bitmap(pictureBox1.Image);
            Bitmap sobel = sobelEdgeDetection(image);
            pictureBox2.Image = sobel;
        }

        private Bitmap sobelEdgeDetection(Bitmap image)
        {
            Bitmap gri = makeGray(image);
            Bitmap buffer = new Bitmap(gri.Width, gri.Height);
            Color renk;

            int valX, valY, gradient;

            int[,] GX = new int[,] { {-1, 0, 1 },
                                     { 0, 0, 0 },  // yatay yönde kenar bulma
                                     {-1, 0, 1 } };

            int[,] GY = new int[,] { { -1, -2, -1 },
                                     {  0,  0,  0 },  // DÜşey yönde kenar bulma
                                     {  1,  2,  1 } };



            for (int i = 0; i < gri.Width; i++)
            {
                for (int j = 0; j < gri.Height; j++)
                {
                    if (i == 0 || i == gri.Width - 1 || j == 0 || j == gri.Height - 1)
                    {
                        renk = Color.FromArgb(255, 255, 255);
                        buffer.SetPixel(i, j, renk);

                        valX = 0;
                        valY = 0;
                    }
                    else
                    {
                        valX = gri.GetPixel(i - 1, j - 1).R * GX[0, 0] +
                               gri.GetPixel(i - 1, j).R * GX[0, 1] +
                               gri.GetPixel(i - 1, j + 1).R * GX[0, 2] +
                               gri.GetPixel(i, j - 1).R * GX[1, 0] +
                               gri.GetPixel(i, j).R * GX[1, 1] +
                               gri.GetPixel(i, j + 1).R * GX[1, 2] +
                               gri.GetPixel(i + 1, j - 1).R * GX[2, 0] +
                               gri.GetPixel(i + 1, j).R * GX[2, 1] +
                               gri.GetPixel(i + 1, j + 1).R * GX[2, 2];

                        valY = gri.GetPixel(i - 1, j - 1).R * GY[0, 0] +
                               gri.GetPixel(i - 1, j).R * GY[0, 1] +
                               gri.GetPixel(i - 1, j + 1).R * GY[0, 2] +
                               gri.GetPixel(i, j - 1).R * GY[1, 0] +
                               gri.GetPixel(i, j).R * GY[1, 1] +
                               gri.GetPixel(i, j + 1).R * GY[1, 2] +
                               gri.GetPixel(i + 1, j - 1).R * GY[2, 0] +
                               gri.GetPixel(i + 1, j).R * GY[2, 1] +
                               gri.GetPixel(i + 1, j + 1).R * GY[2, 2];

                        gradient = (int)(Math.Abs(valX) + Math.Abs(valY));

                        // gradient = gradient < 0 ? 0 : gradient;
                        gradient = gradient > 255 ? 255 : gradient;

                        renk = Color.FromArgb(gradient, gradient, gradient);
                        buffer.SetPixel(i, j, renk);
                    }
                }
            }

            return buffer;
        }

        private void medianFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap image = new Bitmap(pictureBox1.Image);
            Bitmap median = medianFilter(image);
            pictureBox2.Image = median;
        }

        private Bitmap medianFilter(Bitmap image)
        {
            Bitmap buffer = new Bitmap(image.Width, image.Height);
            Color renk;

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    if ((j == 0) || (j == image.Height - 1) || (i == 0) || (i == image.Width - 1))
                        continue;
                    else
                    {
                        int ortanca = ortancayiBul(image, i, j);
                        renk = Color.FromArgb(ortanca, ortanca, ortanca);
                        buffer.SetPixel(i, j, renk);
                    }
                }
            }
            return buffer;
        }

        private int ortancayiBul(Bitmap image, int i, int j)
        {

            Color renk;

            int sagKomsu, sagUstCaprazKomsu, ustKomsu, solUstCaprazKomsu, solKomsu, solAltCaprazKomsu, altkomsu, sagAltCaprazKomsu;
            sagKomsu = image.GetPixel(i + 1, j).R;
            solKomsu = image.GetPixel(i - 1, j).R;
            sagUstCaprazKomsu = image.GetPixel(i + 1, j - 1).R;
            solUstCaprazKomsu = image.GetPixel(i - 1, j - 1).R;
            sagAltCaprazKomsu = image.GetPixel(i + 1, j + 1).R;
            solAltCaprazKomsu = image.GetPixel(i - 1, j + 1).R;
            ustKomsu = image.GetPixel(i, j - 1).R;
            altkomsu = image.GetPixel(i, j + 1).R;

            int[] dizi = new int[] { image.GetPixel(i, j).R, sagKomsu, sagUstCaprazKomsu, ustKomsu, solUstCaprazKomsu, solKomsu, solAltCaprazKomsu, altkomsu, sagAltCaprazKomsu };

            for (int x = 0; x < 8; x++)
            {
                for (int y = x + 1; y < 9; y++)
                {
                    if (dizi[x] < dizi[y])
                        continue;
                    else
                    {
                        int temp = dizi[y];
                        dizi[y] = dizi[x];
                        dizi[x] = temp;
                    }
                }
            }
            return dizi[4];
        }


        private void button5_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = pictureBox1.Image;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = pictureBox2.Image;
        }

        private void karsilastirToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void thresoldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Threshold thresold = new Threshold((int)trackBartThresold.Value);
                Bitmap org = new Bitmap(pictureBox1.Image);
                Bitmap clone = org.Clone(new Rectangle(0, 0, org.Width, org.Height), PixelFormat.Format24bppRgb);
                pictureBox2.Image = thresold.Apply(clone);
            }
        }

        private void trackBartThresold_Scroll(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Threshold thresold = new Threshold((int)trackBartThresold.Value);
                Bitmap org = new Bitmap(pictureBox1.Image);
                Bitmap clone = org.Clone(new Rectangle(0, 0, org.Width, org.Height), PixelFormat.Format8bppIndexed);
                pictureBox2.Image = thresold.Apply(clone);
            }
        }

        private void Button_HSV_Filter_Click(object sender, EventArgs e)
        {
            Bitmap image = new Bitmap(pictureBox1.Image);
            HSLFiltering filterHsl = new HSLFiltering();
            Mean filterMean = new Mean();


            filterHsl.Luminance = new AForge.Range(0.1f, 1);
            filterHsl.UpdateHue = false;
            filterHsl.UpdateSaturation = false;
            filterHsl.UpdateLuminance = true;
            filterHsl.ApplyInPlace(image);

            filterMean.ApplyInPlace(image);

            SISThreshold filterThresold = new SISThreshold();
            GrayscaleBT709 filterGray = new GrayscaleBT709();
            image = filterGray.Apply(image);
            Bitmap clone = image.Clone(new Rectangle(0, 0, image.Width, image.Height), PixelFormat.Format8bppIndexed);
            filterThresold.ApplyInPlace(clone);
            image = clone;

            Bitmap clone2normal = image.Clone(new Rectangle(0, 0, image.Width, image.Height), PixelFormat.Format32bppRgb);
            image = clone2normal;
            BlobCounter bc = new BlobCounter();
            bc.FilterBlobs = true;
            bc.MinWidth = 5;
            bc.MinHeight = 5;

            bc.ProcessImage(image);
            Blob[] blobs = bc.GetObjects(image, false);

            var rectanglesToClear = from blob in blobs select blob.Rectangle;
            using (var gfx = Graphics.FromImage(image))
            {
                foreach (var rect in rectanglesToClear)
                {
                    if (rect.Height < 20 && rect.Width < 20)
                        gfx.FillRectangle(Brushes.White, rect);
                }
                gfx.Flush();
            }

            Dilatation filterDilation = new Dilatation();

            image = image.Clone(new Rectangle(0, 0, image.Width, image.Height), PixelFormat.Format48bppRgb);
            filterDilation.ApplyInPlace(image);
            filterDilation.ApplyInPlace(image);

            Erosion filterErosion = new Erosion();
            filterErosion.ApplyInPlace(image);


            pictureBox2.Image = image;

        }

        private Bitmap makeGray(Bitmap image)
        {
            int griDeger;
            Color renk;
            for (int i = 0; i < image.Width - 1; i++)
            {
                for (int j = 0; j < image.Height - 1; j++)
                {
                    griDeger = ((image.GetPixel(i, j).R + image.GetPixel(i, j).G + image.GetPixel(i, j).B) / 3);
                    renk = Color.FromArgb(griDeger, griDeger, griDeger);
                    image.SetPixel(i, j, renk);
                }
            }

            return image;

        }


        private void Button_Belirt_Click(object sender, EventArgs e)
        {
            Bitmap image = (Bitmap)pictureBox1.Image.Clone();

            ColorFiltering colorFilter = new ColorFiltering();
            colorFilter.Red = new IntRange(red, (int)numericUpDownRed.Value);
            colorFilter.Green = new IntRange(green, (int)numericUpDownGreen.Value);
            colorFilter.Blue = new IntRange(blue, (int)numericUpDownBlue.Value);
            colorFilter.ApplyInPlace(image);

            BlobCounter blobCounter = new BlobCounter();
            blobCounter.MinHeight = 100;
            blobCounter.MinWidth = 300;

            blobCounter.ObjectsOrder = ObjectsOrder.Size;
            blobCounter.ProcessImage(image);

            Rectangle[] rectangle = blobCounter.GetObjectsRectangles(); // bundan bir daha oluştur 
            if (rectangle.Length > 0)                                   //içerde beyazları tespit etmek için
            {
                size = rectangle[0].Size;
                //MessageBox.Show(size.ToString());
                Rectangle objectt = rectangle[0];
                Graphics graphic = Graphics.FromImage(image);
                using (Pen pen = new Pen(Color.Turquoise, 10))
                {
                    graphic.DrawRectangle(pen, objectt);
                }
                graphic.Dispose();
            }
            pictureBox2.Image = image;

        }

        private void cannyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap image = (Bitmap)pictureBox1.Image.Clone();

            pictureBox2.Image = CannyEdge(image);

        }

        private Bitmap CannyEdge(Bitmap image)
        {
            Mat matImage = GetMatFromSDImage(image);//System.Drawing.Image to Emgu.CV.Mat.Image
                                                    // Mat matImage = new Mat(ofdFileName, ImreadModes.Color);

            Mat imgGrayScale = new Mat(matImage.Size, DepthType.Cv8U, 1);
            Mat imgBlurred = new Mat(matImage.Size, DepthType.Cv8U, 1);
            Mat imgCanny = new Mat(matImage.Size, DepthType.Cv8U, 1);

            CvInvoke.CvtColor(matImage, imgGrayScale, ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(imgGrayScale, imgBlurred, new Size(5, 5), 1.5);
            CvInvoke.Canny(imgBlurred, imgCanny, 100, 200);

            image = imgCanny.Bitmap;
            return image;

        }

        private void button7_Click(object sender, EventArgs e)
        {
            HistogramCikar();
        }
        public void HistogramCikar()
        {
            double[] q = new double[8];
            
            ChainCodeHistogram(textBoxFreeMan.Text, q);
            chart1.Series["Series1"].LegendText = "Chain Norm";
            chart1.Series["Series1"].Points.Clear();

            for (int i = 0; i < 8; i++)
            {
                chart1.Series["Series1"].Points.AddXY(i, q[i]);

            }
        }

        public void isaretSorgula()
        {
            connection = new MySqlConnection(connectionString);
            string queryGetDataBase = "select * from trafficsignhistnorms.traffic_signs;";
            MySqlCommand selectCommand = new MySqlCommand(queryGetDataBase, connection);
            double[,] freemanVectorFromDatabase = new double[8, 1];
            double similarity = 0, similarityPercent = 0;
            string signResult = "";
            try
            {
                connection.Open();
                MySqlDataReader dataReader = selectCommand.ExecuteReader();
                while (dataReader.Read())
                {
                    for (int i = 0; i < 8; i++)
                    {
                        freemanVectorFromDatabase[i, 0] = Convert.ToDouble(dataReader[i + 2]);
                    }

                    similarity = computeSimilarity(freemanVector, freemanVectorFromDatabase);

                    if (similarity > similarityPercent)
                    {
                        similarityPercent = similarity;
                        signResult = dataReader["sign"].ToString();
                    }

                }

                textBoxTrfkisareti.Text = signResult;
                textBoxBenzerlik.Text = similarityPercent.ToString();
                connection.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Mat GetMatFromSDImage(System.Drawing.Image image)
        {
            int stride = 0;
            Bitmap bmp = new Bitmap(image);

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            PixelFormat pf = bmp.PixelFormat;
            if (pf == PixelFormat.Format32bppArgb)
            {
                stride = bmp.Width * 4;
            }
            else
            {
                stride = bmp.Width * 3;
            }

            Image<Bgra, byte> cvImage = new Image<Bgra, byte>(bmp.Width, bmp.Height, stride, (IntPtr)bmpData.Scan0);

            bmp.UnlockBits(bmpData);

            return cvImage.Mat;
        }

        private void button_Sorgula_Click(object sender, EventArgs e)
        {
            isaretSorgula();
        }
        private double computeSimilarity(double[,] vector, double[,] vectorDatabase)
        {
            double vectorCarpim = 0, squareA = 0, squareB = 0, similarity = 0;


            for (int i = 0; i < 8; i++)
            {
                vectorCarpim += vector[i, 0] * vectorDatabase[i, 0];
                squareA += Math.Pow(vector[i, 0], 2);
                squareB += Math.Pow(vectorDatabase[i, 0], 2);
            }

            similarity = vectorCarpim / (Math.Sqrt(squareA) * Math.Sqrt(squareB));
            return similarity;
        }
        private void button_AddToDB_Click(object sender, EventArgs e)
        {

            string queryReadString = "select * from trafficsignhistnorms.traffic_signs;";
            connection = new MySqlConnection(connectionString);
            string query =
                "insert into trafficsignhistnorms.traffic_signs(id,sign,chain_norm_1,chain_norm_2,chain_norm_3,chain_norm_4,chain_norm_5,chain_norm_6,chain_norm_7,chain_norm_8) values('" + (trafficId + 1).ToString() + "','" +
                textBoxAddToDB.Text + "','" + freemanVector[0, 0].ToString() + "','" +
                freemanVector[1, 0].ToString() + "','" + freemanVector[2, 0].ToString() + "','" +
                freemanVector[3, 0].ToString() + "','" + freemanVector[4, 0].ToString() + "','" +
                freemanVector[5, 0].ToString() + "','" + freemanVector[6, 0].ToString() + "','" +
                freemanVector[7, 0].ToString() + "');";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlCommand readCmd = new MySqlCommand(queryReadString, connection);//okuma

            try
            {
                connection.Open();
                command.ExecuteNonQuery();

                MessageBox.Show("Connection Succesful");
                connection.Close();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Connection Unsuccesful");
                MessageBox.Show(exc.ToString());
            }
            trafficId++;
        }

        private void freemanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stopwatch kronoMetre = new Stopwatch();
            kronoMetre.Start();
            textBoxFreeMan.Clear();
            chainCode = "";
            int freemanKoduSayisi = 0;
            imageFreeman = new Bitmap(pictureBox1.Image);
            imageFreeman2 = new Bitmap(imageFreeman.Width, imageFreeman.Height);            
            Color renk;


            int x, y, xm, ym, xb, yb;

            imageFreeman = CannyEdge(imageFreeman);
            imageFreemanCopy = new Bitmap(imageFreeman);
            //bool[,] beyazMi = new bool[imageFreeman.Width, imageFreeman.Height];
            //for (int i = 0; i < imageFreeman.Width; i++)
            //{
            //    for (int j = 0; j < imageFreeman.Height; j++)
            //    {
            //        renk = imageFreeman.GetPixel(i, j);
            //        if (renk.R == 255)
            //            beyazMi[i, j] = true;
            //        else
            //            beyazMi[i, j] = false;
            //    }
            //}

            //for (int i = 0; i < imageFreeman.Width; i++)
            //{
            //    for (int j = 0; j < imageFreeman.Height; j++)
            //    {
            //        if (beyazMi[i, j])
            //            imageFreemanCopy.SetPixel(i, j, beyaz);
            //        else
            //            imageFreemanCopy.SetPixel(i, j, siyah);
            //    }
            //}

            System.Drawing.Point AgirlikMerkezi = AgirlikMerkeziBul(imageFreeman);
            xm = AgirlikMerkezi.X;
            ym = AgirlikMerkezi.Y;



            //2. Resmi kapkara yap
            for (int i = 0; i < imageFreeman.Width; i++)
            {
                for (int j = 0; j < imageFreeman.Height; j++)
                {
                    imageFreeman2.SetPixel(i, j, siyah);
                }
            }

            // İlk beyazı bulduk
            x = xm;
            do
            {
                x++;
                xb = x;
            } while (imageFreeman.GetPixel(x, ym).R != 255 && x < imageFreeman.Width);

            imageFreeman2.SetPixel(xm, ym, Color.Turquoise);

            byte freemanDegeri = 0;
            x = xb;
            y = ym;

            baslangicPixeli = new System.Drawing.Point();
            beyazPixel = new System.Drawing.Point();
            bufferBeyazPixel = new System.Drawing.Point();

            baslangicPixeli.X = x;
            baslangicPixeli.Y = y;
            beyazPixel.X = x;
            beyazPixel.Y = y;
            textBoxBasX.Text = x.ToString();
            textBoxBasY.Text = y.ToString();
            baslagicKonumX = x;
            baslangickonumY = y;
            /* for (int i = 0; i < 10; i++)
             {                
                 beyazMi = pixelKaydir(0, x, y);
                 x = beyazMi.X;                     //pixel hesaplama sağlaması
                 y = beyazMi.Y;
                 image2.SetPixel(x, y, Color.Red);
             }
             for (int i = 0; i < 10; i++)
             {
                 beyazMi = pixelKaydir(1, x, y);
                 x = beyazMi.X;                     //pixel hesaplama sağlaması
                 y = beyazMi.Y;
                 image2.SetPixel(x, y, Color.Red);
             }
             for (int i = 0; i < 10; i++)
             {
                 beyazMi = pixelKaydir(2, x, y);
                 x = beyazMi.X;                     //pixel hesaplama sağlaması
                 y = beyazMi.Y;
                 image2.SetPixel(x, y, Color.Red);
             }
             for (int i = 0; i < 10; i++)
             {
                 beyazMi = pixelKaydir(3, x, y);
                 x = beyazMi.X;                     //pixel hesaplama sağlaması
                 y = beyazMi.Y;
                 image2.SetPixel(x, y, Color.Red);
             }
             for (int i = 0; i < 10; i++)
             {
                 beyazMi = pixelKaydir(4, x, y);
                 x = beyazMi.X;                     //pixel hesaplama sağlaması
                 y = beyazMi.Y;
                 image2.SetPixel(x, y, Color.Red);
             }
             for (int i = 0; i < 10; i++)
             {
                 beyazMi = pixelKaydir(5, x, y);
                 x = beyazMi.X;                     //pixel hesaplama sağlaması
                 y = beyazMi.Y;
                 image2.SetPixel(x, y, Color.Red);
             }
             for (int i = 0; i < 10; i++)
             {
                 beyazMi = pixelKaydir(6, x, y);
                 x = beyazMi.X;                     //pixel hesaplama sağlaması
                 y = beyazMi.Y;
                 image2.SetPixel(x, y, Color.Red);
             }
             for (int i = 0; i < 10; i++)
             {
                 beyazMi = pixelKaydir(7, x, y);
                 x = beyazMi.X;                     //pixel hesaplama sağlaması
                 y = beyazMi.Y;
                 image2.SetPixel(x, y, Color.Red);
             }*/

            imageFreeman2.SetPixel(x, y, kirmizi);
            do
            {
                do
                {
                    x = beyazPixel.X;
                    y = beyazPixel.Y;
                    bufferBeyazPixel = pixelKaydir(freemanDegeri, x, y);
                    freemanDegeri++;
                    x = bufferBeyazPixel.X;
                    y = bufferBeyazPixel.Y;
                } while (imageFreemanCopy.GetPixel(x, y).R != 255 && x < imageFreemanCopy.Width && y < imageFreemanCopy.Height);

                imageFreemanCopy.SetPixel(bufferBeyazPixel.X, bufferBeyazPixel.Y, siyah);
                imageFreeman2.SetPixel(x, y, beyaz);

                beyazPixel.X = x;
                beyazPixel.Y = y;
                chainCodeHesapla(--freemanDegeri);
                freemanDegeri = 0;
                freemanKoduSayisi++;
            } while (!(baslangicPixeli.X == beyazPixel.X && baslangicPixeli.Y == beyazPixel.Y));

            byteChainCode = new byte[chainCode.Length];
            byte chainCodeDegeri;
            for (int i = 0; i < chainCode.Length; i++)
            {
                chainCodeDegeri = Convert.ToByte(chainCode.Substring(i, 1));
                byteChainCode[i] = chainCodeDegeri;
            }
            textBoxFreeMan.Text = chainCode;

            labelFreemanAdet.Text = freemanKoduSayisi.ToString();
            pictureBox2.Image = imageFreeman2;

            HistogramCikar();
            isaretSorgula();
            kronoMetre.Stop();
            textBoxTespitSuresi.Text = kronoMetre.ElapsedMilliseconds.ToString()+ " ms";
            
        }

        public void ChainCodeHistogram(string chaincode, double[] h)
        {
            int i;

            char[] chars = chaincode.ToCharArray();
            for (i = 0; i < 8; i++)
            { h[i] = 0; }

            for (i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '0') h[0] = h[0] + 1;
                if (chars[i] == '1') h[1]++;
                if (chars[i] == '2') h[2]++;
                if (chars[i] == '3') h[3]++;
                if (chars[i] == '4') h[4]++;
                if (chars[i] == '5') h[5]++;
                if (chars[i] == '6') h[6]++;
                if (chars[i] == '7') h[7]++;
            }


            for (i = 0; i < h.Length; i++)
            {
                h[i] = h[i] / chars.Length;
                freemanVector[i, 0] = h[i];
            }

        }
        private System.Drawing.Point pixelKaydir(byte freemanDegeri, int x, int y)
        {
            System.Drawing.Point currentWhitePixel = new System.Drawing.Point();
            currentWhitePixel.X = x;
            currentWhitePixel.Y = y;
            switch (freemanDegeri)
            {
                case 0:
                    currentWhitePixel.X += 1;
                    currentWhitePixel.Y = currentWhitePixel.Y;
                    break;
                case 1:
                    currentWhitePixel.X += 1;
                    currentWhitePixel.Y -= 1;
                    break;
                case 2:
                    currentWhitePixel.X = currentWhitePixel.X;
                    currentWhitePixel.Y -= 1;
                    break;
                case 3:
                    currentWhitePixel.X -= 1;
                    currentWhitePixel.Y -= 1;
                    break;
                case 4:
                    currentWhitePixel.X -= 1;
                    currentWhitePixel.Y = currentWhitePixel.Y;
                    break;
                case 5:
                    currentWhitePixel.X -= 1;
                    currentWhitePixel.Y += 1;
                    break;
                case 6:
                    currentWhitePixel.X = currentWhitePixel.X;
                    currentWhitePixel.Y += 1;
                    break;
                case 7:
                    currentWhitePixel.X += 1;
                    currentWhitePixel.Y += 1;
                    break;
                //Bir tur tamamlandı
                case 8:
                    currentWhitePixel.X += 2;
                    currentWhitePixel.Y += 1;
                    break;
                case 9:
                    currentWhitePixel.X += 2;
                    currentWhitePixel.Y = currentWhitePixel.Y;
                    break;
                case 10:
                    currentWhitePixel.X += 2;
                    currentWhitePixel.Y -= 1;
                    break;
                case 11:
                    currentWhitePixel.X += 2;
                    currentWhitePixel.Y -= 2;
                    break;
                case 12:
                    currentWhitePixel.X += 1;
                    currentWhitePixel.Y -= 2;
                    break;
                case 13:
                    currentWhitePixel.X = currentWhitePixel.X;
                    currentWhitePixel.Y -= 2;
                    break;
                case 14:
                    currentWhitePixel.X -= 1;
                    currentWhitePixel.Y -= 2;
                    break;
                case 15:
                    currentWhitePixel.X -= 2;
                    currentWhitePixel.Y -= 2;
                    break;
                case 16:
                    currentWhitePixel.X -= 2;
                    currentWhitePixel.Y -= 1;
                    break;
                case 17:
                    currentWhitePixel.X -= 2;
                    currentWhitePixel.Y = currentWhitePixel.Y;
                    break;
                case 18:
                    currentWhitePixel.X -= 2;
                    currentWhitePixel.Y += 1;
                    break;
                case 19:
                    currentWhitePixel.X -= 2;
                    currentWhitePixel.Y += 2;
                    break;
                case 20:
                    currentWhitePixel.X -= 1;
                    currentWhitePixel.Y += 2;
                    break;
                case 21:
                    currentWhitePixel.X = currentWhitePixel.X;
                    currentWhitePixel.Y += 2;
                    break;
                case 22:
                    currentWhitePixel.X += 1;
                    currentWhitePixel.Y += 2;
                    break;
                case 23:
                    currentWhitePixel.X += 2;
                    currentWhitePixel.Y += 2;
                    break;
            }
            return currentWhitePixel;

        }
        public void chainCodeHesapla(byte freemanDegeri)
        {
            switch (freemanDegeri)
            {
                case 0:
                    chainCode += "0";
                    break;
                case 1:
                    chainCode += "1";
                    break;
                case 2:
                    chainCode += "2";
                    break;
                case 3:
                    chainCode += "3";
                    break;
                case 4:
                    chainCode += "4";
                    break;
                case 5:
                    chainCode += "5";
                    break;
                case 6:
                    chainCode += "6";
                    break;
                case 7:
                    chainCode += "7";
                    break;
                //Bir tur tamamlandı
                case 8:
                    chainCode += "70";
                    break;
                case 9:
                    chainCode += "00";
                    break;
                case 10:
                    chainCode += "10";
                    break;
                case 11:
                    chainCode += "11";
                    break;
                case 12:
                    chainCode += "12";
                    break;
                case 13:
                    chainCode += "22";
                    break;
                case 14:
                    chainCode += "32";
                    break;
                case 15:
                    chainCode += "33";
                    break;
                case 16:
                    chainCode += "43";
                    break;
                case 17:
                    chainCode += "44";
                    break;
                case 18:
                    chainCode += "45";
                    break;
                case 19:
                    chainCode += "55";
                    break;
                case 20:
                    chainCode += "65";
                    break;
                case 21:
                    chainCode += "66";
                    break;
                case 22:
                    chainCode += "76";
                    break;
                case 23:
                    chainCode += "77";
                    break;
            }
        }
        public System.Drawing.Point AgirlikMerkeziBul(Bitmap img)
        {
            System.Drawing.Point merkez = new System.Drawing.Point();
            Color p9; int x, y, xt, yt, xc, yc, n;

            xt = 0; yt = 0; n = 0;
            for (x = 0; x < img.Width; x++)
            {
                for (y = 0; y < img.Height; y++)
                {
                    p9 = img.GetPixel(x, y);
                    if (p9.R == 255)
                    {
                        xt = xt + x * 255; yt = yt + y * 255; n++;
                    }
                }
            }

            xc = xt / (255 * n);
            yc = yt / (255 * n);
            merkez.X = xc; merkez.Y = yc;
            return merkez;
        }

        private void buttonChaindenCizdir_Click(object sender, EventArgs e)
        {
            int x, y;
            Bitmap image = new Bitmap(imageFreeman.Width, imageFreeman.Height);
            System.Drawing.Point freemandanDonenPixel = new System.Drawing.Point();
            for (int i = 0; i < imageFreeman.Width; i++)
            {
                for (int j = 0; j < imageFreeman.Height; j++)
                {
                    image.SetPixel(i, j, siyah);
                }
            }

            x = baslagicKonumX;
            y = baslangickonumY;

            image.SetPixel(x, y, Color.Green);
            for (int i = 0; i < byteChainCode.Length; i++)
            {
                freemandanDonenPixel = pixelKaydir(byteChainCode[i], x, y);
                x = freemandanDonenPixel.X;
                y = freemandanDonenPixel.Y;
                image.SetPixel(x, y, Color.Green);
            }
            pictureBox1.Image = image;
        }

        private Bitmap makeBinary(Bitmap image)
        {
            Bitmap binary = makeGray(image);

            int temp = 0;
            int esik = 100;
            Color renk;
            for (int i = 0; i < image.Width - 1; i++)
            {
                for (int j = 0; j < image.Height - 1; j++)
                {
                    temp = binary.GetPixel(i, j).R; // burada herhangi bir R,G veya B yi alabiliriz çünkü makeGray fonksiyonunda bütün hepsini aynı değer yapmıştık o yüzden hepsi aynı değer döndürecektir.
                    if (temp < esik)
                    {
                        renk = Color.FromArgb(0, 0, 0);
                        binary.SetPixel(i, j, renk);
                    }
                    else
                    {
                        renk = Color.FromArgb(255, 255, 255);
                        binary.SetPixel(i, j, renk);
                    }
                }
            }

            return binary;
        }

        private Bitmap makeBinary(Bitmap image, int esik)
        {
            Bitmap binary = makeGray(image);

            int temp = 0;
            Color renk;
            for (int i = 0; i < image.Width - 1; i++)
            {
                for (int j = 0; j < image.Height - 1; j++)
                {
                    temp = binary.GetPixel(i, j).R; // burada herhangi bir R,G veya B yi alabiliriz çünkü makeGray fonksiyonunda bütün hepsini aynı değer yapmıştık o yüzden hepsi aynı değer döndürecektir.
                    if (temp < esik)
                    {
                        renk = Color.FromArgb(0, 0, 0);
                        binary.SetPixel(i, j, renk);
                    }
                    else
                    {
                        renk = Color.FromArgb(255, 255, 255);
                        binary.SetPixel(i, j, renk);
                    }
                }
            }

            return binary;
        }
    }
}
