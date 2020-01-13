using C5;
using java.awt.image;
using javax.imageio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;


namespace MPIProject
{
    class RGBDisplayModel
    {
        private BufferedImage originalImage;
        private BufferedImage redImage;
        private BufferedImage greenImage;
        private BufferedImage blueImage;

        public void setOriginalImage(BufferedImage originalImage)  
        {
        this.originalImage = originalImage;
        this.makeRGBImages();
    }

        public BufferedImage getOriginalImage()
        {
            return this.originalImage;
        }

    private void makeRGBImages()  
    {
        this.redImage = createColorImage(originalImage, unchecked((int) 0xdFFFF0000));
        //this.greenImage = createColorImage(originalImage, 0xFF00FF00);
        //this.blueImage = createColorImage(originalImage, 0xFF0000FF);
    }

    public BufferedImage getRedImage()
    {
        return redImage;
    }

    public BufferedImage getGreenImage()
    {
        return greenImage;
    }

    public BufferedImage getBlueImage()
    {
        return blueImage;
    }

    public ArrayList<BufferedImage> splitImage(BufferedImage originalImage, int size)
    {
        ArrayList<BufferedImage> imgArray = new ArrayList<BufferedImage>();
        for (int x = 0; x < originalImage.getWidth(); x += size)
            for (int y = 0; y < originalImage.getHeight(); y += size)
            {
                imgArray.Add(originalImage.getSubimage(x, y, size, size));
            }
        return imgArray;
    }

    private BufferedImage createColorImage(BufferedImage originalImage, int mask)  
    {
            //BufferedImage colorImage = new BufferedImage(originalImage.getWidth(),
            //    originalImage.getHeight(), originalImage.getType());

            //for (int x = 0; x < originalImage.getWidth(); x++)
            //{
            //    for (int y = 0; y < originalImage.getHeight(); y++)
            //    {
            //        int pixel = originalImage.getRGB(x, y) & mask;
            //        colorImage.setRGB(x, y, pixel);
            //    }
            //}

            //return colorImage;


            ArrayList<BufferedImage> resultList = new ArrayList<BufferedImage>();
            List<CreateImageThread> threads = new List<CreateImageThread>();

            int imgChunkSize = 100;
            ArrayList<BufferedImage> images = this.splitImage(originalImage, imgChunkSize);

            for (int i = 0; i < images.ToList().Count; i++)
            {
                CreateImageThread thread = new CreateImageThread();
                thread.run(images.ElementAt(i), mask, resultList, i);
                threads.Add(thread);
            }
            foreach (CreateImageThread thread in threads)
            {
                thread.ToString();
            }

            BufferedImage resultImage = new BufferedImage(originalImage.getWidth(), originalImage.getHeight(), originalImage.getType());
            int howManyOnLine = (int)Math.Sqrt(resultList.ToList().Count);
            for (int i = 0; i < howManyOnLine; i++)
            {
                for (int j = 0; j < howManyOnLine; j++)
                    resultImage.createGraphics().drawImage(resultList.ElementAt(i * howManyOnLine + j), imgChunkSize * i, imgChunkSize * j, null);
            }
            return resultImage;
        }
    }
}
