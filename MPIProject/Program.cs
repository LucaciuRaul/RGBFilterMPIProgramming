using C5;
using java.awt;
using java.awt.image;
using java.io;
using javax.imageio;
using javax.swing;
using MPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MPIProject
{
    class Program
    {
        private static void MPIWorker()
        {
            ArrayList<ImageIcon> imageChunksIcon = Communicator.world.Receive<ArrayList<ImageIcon>>(0, 0);
            int mask = Communicator.world.Receive<int>(0, 0);

            ArrayList<BufferedImage> imageChunks = new ArrayList<BufferedImage>();

            foreach (ImageIcon image in imageChunksIcon)
                imageChunks.Add(convertToBufferedImage(image.getImage()));

            List<BufferedImage> results = new List<BufferedImage>(20);

            int k = 0;

            foreach (BufferedImage imageChunk in imageChunks)
            {
                BufferedImage colorImage = new BufferedImage(imageChunk.getWidth(),
                        imageChunk.getHeight(), imageChunk.getType());

                for (int x = 0; x < imageChunk.getWidth(); x++)
                {
                    for (int y = 0; y < imageChunk.getHeight(); y++)
                    {
                        int pixel = imageChunk.getRGB(x, y) & mask;
                        colorImage.setRGB(x, y, pixel);
                    }
                }
                results.Add(colorImage);
            }

            ArrayList<ImageIcon> resultsIcons = new ArrayList<ImageIcon>(20);

            for (int i = 0; i < results.Count; i++)
            {
                resultsIcons.Add(new ImageIcon(results[i]));
            }
                Communicator.world.Send(resultsIcons, 0, 0);
        }

        public static BufferedImage convertToBufferedImage(java.awt.Image image)
        {
            BufferedImage newImage = new BufferedImage(
                image.getWidth(null), image.getHeight(null),
                BufferedImage.TYPE_INT_ARGB);
            Graphics2D g = newImage.createGraphics();
            g.drawImage(image, 0, 0, null);
            g.dispose();
            return newImage;
        }
        private static BufferedImage MPIMaster(RGBDisplayModel model, int mask)
        {
            int n = Communicator.world.Size;

            ArrayList<BufferedImage> chunks = model.splitImage(model.getOriginalImage(), 100);

            for (int i = 1; i < n; i++)
            {
                ArrayList<ImageIcon> imageList = new ArrayList<ImageIcon>();
                for (int j = 1; j < n; j++)
                {
                    try
                    {
                        imageList.Add(new ImageIcon(chunks[(i - 1) * (n - 1) + (j -1)]));
                    }
                    catch { Exception e; }
                    { }
                }
                Communicator.world.Send(imageList, i, 0);
                Communicator.world.Send(mask, i, 0);
            }

            ArrayList<ImageIcon> resultsIcon = new ArrayList<ImageIcon>();

            for (int i = 1; i < n; i++)
            {
                ArrayList<ImageIcon> aux = Communicator.world.Receive<ArrayList<ImageIcon>>(i, 0);
                resultsIcon.AddAll(aux);
            }

            ArrayList<BufferedImage> results = new ArrayList<BufferedImage>();

            //Convert imageIcon to BufferedImage
            foreach (ImageIcon image in resultsIcon)
                results.Add(convertToBufferedImage(image.getImage()));

            BufferedImage resultImage = new BufferedImage(model.getOriginalImage().getWidth(), model.getOriginalImage().getHeight(), model.getOriginalImage().getType());
            int howManyOnLine = (int)Math.Sqrt(results.ToArray().Length) ;


            for (int i = 0; i < howManyOnLine; i++)
            {
                for (int j = 0; j < howManyOnLine; j++)
                {
                    resultImage.createGraphics().drawImage(results.ElementAt(i * howManyOnLine + j), 100 * i, 100 * j, null);
                }
            }

            return resultImage;

        }
        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                if (Communicator.world.Rank == 0)
                {
                    System.Console.WriteLine("Hello from the 0-Rank computer!");
                    
                    RGBDisplayModel model = new RGBDisplayModel();

                    Bitmap originalImage = (Bitmap)System.Drawing.Image.FromFile("D:\\Anu3\\Sem1\\Programare paralela si distribuita\\Laborator\\MPIProject\\MPIProject\\img.bmp");
                    var originalImageCopy = new Bitmap(originalImage);
                    BufferedImage bufferedImage = new BufferedImage(originalImageCopy);

                    model.setOriginalImage(bufferedImage);

                    //TODO: we have 12 core, but one core is the main one => one core remaining -> currently the program is done to use 13 cores !!!! BUG
                    
                    int redMask = unchecked((int)0xFFFF0000);
                    int greenMask = unchecked((int)0xFF00FF00);
                    int blueMask = unchecked((int)0xFF0000FF);

                    DateTime start = DateTime.Now;

                    BufferedImage redResult = MPIMaster(model, redMask);
                    Bitmap redResultBitmap = redResult.getBitmap();
                    redResultBitmap.Save("D:\\Anu3\\Sem1\\Programare paralela si distribuita\\Laborator\\MPIProject\\MPIProject\\imgRedResult.bmp", ImageFormat.Bmp);
                    System.Console.WriteLine("Red filter DONE.");

                    BufferedImage greenResult = MPIMaster(model, greenMask);
                    Bitmap greenResultBitmap = greenResult.getBitmap();
                    greenResultBitmap.Save("D:\\Anu3\\Sem1\\Programare paralela si distribuita\\Laborator\\MPIProject\\MPIProject\\imgGreenResult.bmp", ImageFormat.Bmp);
                    System.Console.WriteLine("Green filter DONE.");

                    BufferedImage blueResult = MPIMaster(model, blueMask);
                    Bitmap blueResultBitmap = blueResult.getBitmap();
                    blueResultBitmap.Save("D:\\Anu3\\Sem1\\Programare paralela si distribuita\\Laborator\\MPIProject\\MPIProject\\imgBlueResult.bmp", ImageFormat.Bmp);
                    System.Console.WriteLine("Blue filter DONE.");

                    double time = (DateTime.Now - start).Milliseconds;
                    System.Console.WriteLine("MPI elapsed time: " + time.ToString() + " milliseconds.");

                    System.Console.WriteLine("DONE!");
                }
                else
                {
                    MPIWorker();
                    MPIWorker();
                    MPIWorker();
                    //System.Console.WriteLine($"Hello from the {Communicator.world.Rank + 1}-th child!");
                }
            }
        }
    }
}
