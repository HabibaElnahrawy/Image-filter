using System;
using System.Drawing;
using System.Windows.Forms;
using ZGraphTools;

namespace ImageFilters
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        byte[,] ImageMatrix;
        //byte[,] newimage;
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);


                //newimage = ImageOperations.alphTrim(ImageMatrix);

                //  ImageOperations.DisplaynewImage(ImageMatrix, pictureBox);
            }
        }
        public static double execution_time, execution_time2;

        private void btnZGraph_Click(object sender, EventArgs e)
        {

            int Wm = 40;
            int N = 0;
            int W = Wm;
            while (W >= 3)
            {
                N++;
                W = W - 2;
            }
            double[] x_values = new double[N];
            double[] y_values_N = new double[N];
            double[] y_values_NLogN = new double[N];
            int j= 3;
            for(int i=3; i <=Wm; i=i+2)
            {
                x_values[i - j] = i;
                j++;
            }
            for (int i = 0; i < N; i++)
            {
                int tic = System.Environment.TickCount;
                ImageOperations.applingFilter(ImageMatrix, (int)x_values[i], Wm, 1, 2);
                int toc= System.Environment.TickCount;
                execution_time = (toc - tic);
                y_values_N[i] = execution_time;
                y_values_NLogN[i] = execution_time* Math.Log(execution_time);
            }

            //Create a graph and add two curves to it
            ZGraphForm ZGF = new ZGraphForm("Sample Graph", "N", "f(N)");
            ZGF.add_curve("f(N) = N", x_values, y_values_N, Color.Red);
            ZGF.add_curve("f(N) = N Log(N)", x_values, y_values_NLogN, Color.Blue);
            ZGF.Show();

        }



        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int Wm = 10;
            int window_size = 3;
            if(comboBox1.Text=="Alpha Trim")
            {

                ImageOperations.applingFilter(ImageMatrix,window_size, Wm, 1,1);
            }
            else
             ImageOperations.applingFilter(ImageMatrix, window_size, Wm, 1,2);

            ImageOperations.DisplayImage(ImageMatrix, pictureBox);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}