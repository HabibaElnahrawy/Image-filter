using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.InteropServices;

namespace ImageFilters
{
    public class ImageOperations
    {
        //private static Bitmap _currentBitmap;

        /// <summary>
        /// Open an image, convert it to gray scale and load it into 2D array of size (Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of gray values</returns>
        public static byte[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            byte[,] Buffer = new byte[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x] = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x] = (byte)((int)(p[0] + p[1] + p[2]) / 3);
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(byte[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(byte[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(byte[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[0] = p[1] = p[2] = ImageMatrix[i, j];
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }

       // public static byte[] window = Enumerable.Repeat(-1, 8).ToArray();
       // public static byte[] w;


        public static byte[,] applingFilter(byte[,] ImageMatrix,int window_size,int wmax,int sort,int filter1_Or_filter2)
        {
            byte[,] ImageMatrix2 = ImageMatrix;
           
            sort=1;
            for (int y = 0; y < GetHeight(ImageMatrix); y++)
            {
                for (int x = 0; x < GetWidth(ImageMatrix); x++)
                {
                   if (filter1_Or_filter2==1)
                    {
                        ImageMatrix2[y, x] = filter(ImageMatrix, y, x, false, window_size);
                    }
                   else
                     ImageMatrix2[y, x] = adaptive_median_filter_grouped(ImageMatrix, y, x, window_size, wmax, sort);

                }
            }

            return ImageMatrix2;
        }
        



        static int calAvg(byte[] SortedArr)
        {
            int sum = 0, avg = 0;

            for (int i = 1; i < SortedArr.Length - 1; i++)
            {
                sum += SortedArr[i];
            }
                avg = sum / (SortedArr.Length - 2);
            return avg;
        }

        public static byte filter(byte[,] image, int i, int j,bool counting, int window_size)
        {
            //for (int i = 0; i < image.GetLength(0); i++)
            //{
            //    for (int j = 0; j < image.GetLength(1); j++)
            //    {
            byte[] window;


            // Console.WriteLine("gjhgdhmj");
            // window = Enumerable.Repeat(-1, 8).ToArray();
            int start_row = i - window_size;
            int end_row = i + window_size;
            int start_col = j - window_size;
            int end_col = j + window_size;
            //int [] window= new int [(start_row-end_row+1)*(start_col-end_col+1)];
            if (start_row < 0)
                start_row=0;
            if (end_row > GetHeight(image) - 1)
                end_row=GetHeight(image)-1;
            if (start_col < 0)
                start_col=0;
            if (end_col > GetWidth(image) - 1)
                end_col = GetWidth(image) - 1;
            window = new byte[((end_row - start_row +1)*(end_col-start_col+1))-1];
            //window = new byte[100];
            //Console.WriteLine("i:{0},j:{1}", i, j);
            int hamada = 0;
            int avg;
            for (int k = start_row; k <= end_row; k++)
            {
                for (int l = start_col; l <= end_col; l++)
                {
                    if (!(k == i && l == j))
                    {
                        //Console.WriteLine("start row: {0}, startcol: {1},hamada:{2} ,endrow: {3}, endcol: {4}", k, l, hamada, end_row, end_col);
                        //Console.WriteLine("endrow: {0}, endcol: {1}", end_row, end_col);
                        window[hamada] = image[k, l];

                        hamada++;
                        //int a = arr[start_row, start_col];
                        // Console.WriteLine("hamada:{0}, arrvalue{1}:", hamada, arr[start_row, start_col]);

                    }

                    // window[start_row, start_col];
                }
            }

            if (counting)
            {
                countingsort(window);
                avg = calAvg(window);
                return (byte)avg;

            }
            else 
            {
                int sum=0 , avgK ;
                int k = 1;
                int kthSmallestIterativeSolution = KthSmallestIterativeSolution(window, k);
                int kthLargestIterativeSolution = kthlargestElement(window, window_size, k);
                for (int w = 0; w < window.Length; w++)
                {
                    if (w != kthSmallestIterativeSolution && w != kthLargestIterativeSolution) sum +=window[w];
                    
                }
                
                avgK = sum / (window.Length -2);
                return (byte)avgK;

            }
            


        }




        public static void countingsort(byte[] ar)
        {
            int n = ar.Length;
            int max = 0;
            //find largest element in the Array
            for (int i = 0; i < n; i++)
            {
                if (max < (int)ar[i])
                {
                    max = (int)ar[i];
                }
            }

            //Create a freq array to store number of occurrences of 
            //each unique elements in the given array 
            byte[] freq = new byte[max + 1];
            for (int i = 0; i < max + 1; i++)
            {
                freq[i] = 0;
            }
            for (int i = 0; i < n; i++)
            {
                freq[(int)ar[i]]++;
            }

            //sort the given array using freq array
            for (int i = 0, j = 0; i <= max; i++)
            {
                while (freq[i] > 0)
                {
                    ar[j] = (byte)i;
                    j++;
                    freq[i]--;
                }
            }

        }



        public static int kthlargestElement(byte[] arr, int size, int k)
        {
            int temp; 

            for (int i = 0; i < k; i++)
            {

                for (int j = 0; j < size - 1; j++)
                {
                    if (arr[j] > arr[j + 1])
                    {
                        temp = arr[j];
                        arr[j] = arr[j + 1];
                        arr[j + 1] = (byte)temp;
                    }
                }
            }

            int kthLargest = arr[size - k];
            return size - k;
            //cout << "K-th largest element is : " << arr[size - k] << endl;
        }

        public static int KthSmallestIterativeSolution(byte[] arr, int k)
        {
            int min = int.MaxValue;
            int minIndex=0;
            for (int i = 0; i < k; i++)
            {
                minIndex = i;
                min = int.MaxValue;
                for (int j = i; j < arr.Length; j++)
                {
                    if (arr[j] < min)
                    {
                        min = arr[j];
                        minIndex = j;
                    }
                }
                // Don't replace if min is already at its correct position
                if (minIndex != i)
                {
                    arr[minIndex] = arr[i];
                    arr[i] = (byte)min;
                }
            }
            return minIndex;
        }


        public static decimal CalMedian(byte[] sortedArray)
        {
            int count = sortedArray.Length;
            decimal medianValue = 0;
            if (count % 2 == 0)
                {
                    int middleElement1 = sortedArray[(count / 2) - 1];
                    int middleElement2 = sortedArray[(count / 2)];
                    medianValue = (middleElement1 + middleElement2) / 2;
                }
            else        
                {
                    medianValue = sortedArray[(count / 2)]; 
                }
            return medianValue;

        }

        private static void Quick_Sort(byte[] arr, int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(arr, left, right);

                if (pivot > 1)
                {
                    Quick_Sort(arr, left, pivot - 1);
                }
                if (pivot + 1 < right)
                {
                    Quick_Sort(arr, pivot + 1, right);
                }
            }

        }

        private static int Partition(byte[] arr, int left, int right)
        {
            int pivot = arr[left];
            while (true)
            {
                while (arr[left] < pivot)
                {
                    left++;
                }
                while (arr[right] > pivot)
                {
                    right--;
                }
                if (left < right)
                {
                    if (arr[left] == arr[right]) return right;

                    int temp = arr[left];
                    arr[left] = arr[right];
                    arr[right] = (byte)temp;
                }
                else
                {
                    return right;
                }
            }
        }
        public static byte adaptiveMedianFilter(byte[,] image,int i,int j,int window_size, int Wm,int sortAlgorithms,byte [] window)
        {

            int Zmed;
            if (sortAlgorithms == 1)
            {
                countingsort(window);
                Zmed = (int)CalMedian(window);
            }
            else
            {
                Quick_Sort(window, 0, window.Length - 1);
                Zmed = (int)CalMedian(window);
            }
            int Zmin = window[0];
            int Zmax = window[window.Length-1];
            int A1 = Zmed - Zmin;
            int A2 = Zmax - Zmed;
            //finding a true median
            if (A1 > 0 && A2 > 0)
                return (byte)Zmed;
            else {
                if (window_size + 2 <= Wm) return adaptiveMedianFilter(image, i, j, window_size + 2, Wm, sortAlgorithms, window);
                else return (byte)Zmed;
             }
        }



        public static byte apply_adaptive_median_filter(byte [,] image,byte[] sorted_window,byte Zmed,int i, int j)
        {
            byte Zxy = image[i, j];
            int Zmin = sorted_window[0];
            int Zmax = sorted_window[sorted_window.Length-1];
            int B1 = Zxy - Zmin;
            int B2 = Zmax - Zxy;
            if (B1 > 0 && B2 > 0) return  Zxy;
            else return Zmed;
        }
        public static byte adaptive_median_filter_grouped(byte [,] image, int i, int j, int window_size, int Wm, int sorting_algorithm)
        {
            byte[] window;
            if (window_size > Wm) window_size = Wm;
            if (window_size % 2 != 0) window_size++;
            int start_row = i - window_size;
            int end_row = i + window_size;
            int start_col = j - window_size;
            int end_col = j + window_size;
            if (start_row < 0)
                start_row = 0;
            if (end_row > GetHeight(image) - 1)
                end_row = GetHeight(image) - 1;
            if (start_col < 0)
                start_col = 0;
            if (end_col > GetWidth(image) - 1)
                end_col = GetWidth(image) - 1;
            window = new byte[((end_row - start_row + 1) * (end_col - start_col + 1)) - 1];

            int hamada = 0;

            for (int k = start_row; k <= end_row; k++) //filling the window
            {
                for (int l = start_col; l <= end_col; l++)
                {
                    if (!(k == i && l == j))
                    {
                        window[hamada] = image[k, l];
                        hamada++;
                    }
                }
            }

            byte Zmed = adaptiveMedianFilter(image, i, j, window_size, Wm, sorting_algorithm,window);
            return apply_adaptive_median_filter(image, window, Zmed,i,j);
        }
    }

}


