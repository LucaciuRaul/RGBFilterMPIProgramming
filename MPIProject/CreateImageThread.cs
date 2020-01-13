using C5;
using java.awt.image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MPIProject
{
    class CreateImageThread
    {

        public void run(BufferedImage originalImage, int mask, ArrayList<BufferedImage> resultList, int index)
        {
            createColorImageThreadTask(originalImage, mask, resultList, index);
        }

        private void createColorImageThreadTask(BufferedImage imageChunk, int mask, ArrayList<BufferedImage> resultList, int index)
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
            resultList.Add(colorImage);
        }
    }
}