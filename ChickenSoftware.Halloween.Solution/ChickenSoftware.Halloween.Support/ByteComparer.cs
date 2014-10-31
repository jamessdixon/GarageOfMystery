using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChickenSoftware.Halloween.Support
{
    public static class ByteComparer
    {
        public unsafe static int differenceCount(byte[] source, byte[] target, int threshold)
        {
            if(source == null)
            {
                return 0;
            }

            if(target == null)
            {
                return 0;
            }

            int differences = 0;
            byte green, blue, red;
            byte previousGreen, previousBlue, previousRed;
            int greenDiff, blueDiff, redDiff;
            fixed(byte* imageBase=target, previousBase=source)
            {
                byte* imagePosition = imageBase;
                byte* previousPosition = previousBase;
                byte* redPos, greenPos, bluePos;

                byte* imageEnd = imageBase + source.Length;

                while(imagePosition != imageEnd)
                {
                    green = *imagePosition;
                    greenPos = imagePosition;
                    imagePosition++;

                    blue = * imagePosition;
                    bluePos = imagePosition;
                    imagePosition++;
                    
                    red = *imagePosition;
                    redPos = imagePosition;
                    imagePosition++;

                    imagePosition++;

                    previousGreen = *previousPosition;
                    *previousPosition = green;
                    previousPosition++;

                    previousBlue = *previousPosition;
                    *previousPosition = blue;
                    previousPosition++;

                    previousRed = *previousPosition;
                    *previousPosition = red;
                    previousPosition++;

                    previousPosition++;

                    if(green > previousGreen)   
                        greenDiff = green - previousGreen;
                    else
                        greenDiff = previousGreen - green;

                    if(blue > previousBlue)   
                        blueDiff = blue - previousBlue;
                    else
                        blueDiff = previousBlue - blue;

                    if(red > previousRed)   
                        redDiff = red - previousRed;
                    else
                        redDiff = previousRed - red;

                    if (greenDiff > threshold || blueDiff > threshold || redDiff > threshold)
                    {
                        *greenPos = 255;
                        *bluePos = 255;
                        *redPos = 255;
                        differences++;
                    }

                }

            }

            return differences;
        }
    }
}
