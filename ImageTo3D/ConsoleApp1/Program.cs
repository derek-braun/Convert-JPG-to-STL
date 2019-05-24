using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections;

namespace ConsoleApp1
{
    class Program
    {
        public ArrayList outlinePoints;
        public Bitmap img;
        public char[,] pixelArray;

        static void Main(string[] args)
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine("JPG Outline to STL Conversion Tool");
            Console.WriteLine("----------------------------------\n");

            Program program = new Program();

            Console.WriteLine("Enter a path to a '.jpg' file:");
            string userFile = Console.ReadLine();
            Console.WriteLine("Enter a destination path for the '.stl' file:");
            string destFile = Console.ReadLine();
            Console.WriteLine();

            //Get image from filepath
            program.img = new Bitmap(userFile);
            program.pixelArray = new char[program.img.Height, program.img.Width];
            program.outlinePoints = new ArrayList();

            //Read in all pixels from image, store in array
            program.readInPixels();

            Point startCoords = program.searchForStart();
            
            if(startCoords == null)
            {
                Console.Write("No graphic found");
                Console.ReadKey();
                System.Environment.Exit(1);
            }

            Point endCoords = new Point(startCoords.getX(), startCoords.getY());

            program.createOutline(startCoords, endCoords);

            //program.debugDraw();

            program.createSTL(destFile);
            Console.Write("File Creation Complete");

            Console.ReadKey();
        }

        //Convert jpg image to pixel array filter by brighness
        private void readInPixels()
        {
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    Color pixel = img.GetPixel(j, i);
                    if (pixel.GetBrightness() < 0.95)
                    {
                        pixelArray[i, j] = '0';
                    }
                    else
                    {
                        pixelArray[i, j] = ' ';
                    }
                    //Console.Write(pixelArray[i, j] + " ");
                }
                //Console.WriteLine();
            }
        }

        //Draw image to console, only useful for debugging with images < 50x50
        private void debugDraw()
        {
            for (int i = 0; i < pixelArray.GetLength(0); i++)
            {
                for (int j = 0; j < pixelArray.GetLength(1); j++)
                {
                    if (pixelArray[i, j] == '0')
                    {
                        Console.Write(' ' + " ");
                    }
                    else
                    {
                        Console.Write(pixelArray[i, j] + " ");
                    }
                }
                Console.WriteLine();
            }
        }

        //Mark outline points of shape
        private void createOutline(Point startCoords, Point endCoords)
        {
            Console.WriteLine("Processing...");
            do
            {
                //Define surrounding 8 pixels
                char[] kernel = new char[8];
                kernel[0] = pixelArray[(int)startCoords.getX() - 1, (int)startCoords.getY() - 1];
                kernel[1] = pixelArray[(int)startCoords.getX() - 1, (int)startCoords.getY() + 0];
                kernel[2] = pixelArray[(int)startCoords.getX() - 1, (int)startCoords.getY() + 1];
                kernel[3] = pixelArray[(int)startCoords.getX() + 0, (int)startCoords.getY() + 1];
                kernel[4] = pixelArray[(int)startCoords.getX() + 1, (int)startCoords.getY() + 1];
                kernel[5] = pixelArray[(int)startCoords.getX() + 1, (int)startCoords.getY() + 0];
                kernel[6] = pixelArray[(int)startCoords.getX() + 1, (int)startCoords.getY() - 1];
                kernel[7] = pixelArray[(int)startCoords.getX() + 0, (int)startCoords.getY() - 1];

                //Define locations of surrounding 8 pixels
                Point[] kernelLocations = new Point[8];
                kernelLocations[0] = new Point(startCoords.getX() - 1, startCoords.getY() - 1);
                kernelLocations[1] = new Point(startCoords.getX() - 1, startCoords.getY() + 0);
                kernelLocations[2] = new Point(startCoords.getX() - 1, startCoords.getY() + 1);
                kernelLocations[3] = new Point(startCoords.getX() + 0, startCoords.getY() + 1);
                kernelLocations[4] = new Point(startCoords.getX() + 1, startCoords.getY() + 1);
                kernelLocations[5] = new Point(startCoords.getX() + 1, startCoords.getY() + 0);
                kernelLocations[6] = new Point(startCoords.getX() + 1, startCoords.getY() - 1);
                kernelLocations[7] = new Point(startCoords.getX() + 0, startCoords.getY() - 1);

                //Search for barrier
                int i = 0;
                while (kernel[i] != '0')
                {
                    i++;
                    if (i == 8)
                    {
                        i = 0;
                    }
                }

                //Search for clockwise exit edge of barrier
                while (kernel[i] != ' ')
                {
                    i++;
                    if (i == 8)
                    {
                        i = 0;
                    }
                }

                //Search for clockwise entrance edge of barrier
                while (kernel[i] != '0')
                {
                    i++;
                    if (i == 8)
                    {
                        i = 0;
                    }
                }
                i--;
                if (i == -1)
                {
                    i = 7;
                }

                //Mark location for debugging
                pixelArray[(int)kernelLocations[i].getX(), (int)kernelLocations[i].getY()] = '*';

                //Set new start coordinates to found location
                startCoords = kernelLocations[i];

                outlinePoints.Add(new Point(startCoords.getX(), startCoords.getY()));
            } while (!(startCoords.getX() == endCoords.getX() && startCoords.getY() == endCoords.getY()));

            //if (startCoords.getX() == endCoords.getX() && startCoords.getY() == endCoords.getY())
            //{
            //    return;
            //}
            return;

            //createOutline(startCoords, endCoords);
        }

        //Search for starting non-white pixel
        private Point searchForStart()
        {
            for(int i = 0; i < pixelArray.GetLength(0); i++)
            {
                for(int j = 0; j < pixelArray.GetLength(1); j++)
                {
                    if(pixelArray[i, j] == '0')
                    {
                        Point startCoords = new Point(i, j-1);
                        return startCoords;
                    }
                }
            }

            return null;
        }

        //Create the STL file from the point data
        private void createSTL(string destFile)
        {
            string[] lines = new string[outlinePoints.Count*2*7*2 + 2];
            lines[0] = "solid model";
            int lineCount = 1;
            double resizeFactor = 150.0/(double)img.Height;
            Console.WriteLine("Resizing by factor of: " + resizeFactor);
            float depth = 30;

            for(int i = 0; i < outlinePoints.Count - 1; i++)
            {
                lines[lineCount] = "outer loop";
                lines[lineCount + 1] = "facet normal 0.0 0.0 0.0";
                lines[lineCount + 2] = "vertex " + ((Point)outlinePoints[i]).getX() * resizeFactor + " " + ((Point)outlinePoints[i]).getY() * resizeFactor + " " + 0.0;
                lines[lineCount + 3] = "vertex " + ((Point)outlinePoints[i + 1]).getX() * resizeFactor + " " + ((Point)outlinePoints[i + 1]).getY() * resizeFactor + " " + 0.0;
                lines[lineCount + 4] = "vertex " + ((Point)outlinePoints[i]).getX() * resizeFactor + " " + ((Point)outlinePoints[i]).getY() * resizeFactor + " " + depth;
                lines[lineCount + 5] = "endloop";
                lines[lineCount + 6] = "endfacet";
                lineCount += 7;
            }

            lines[lineCount] = "outer loop";
            lines[lineCount + 1] = "facet normal 0.0 0.0 0.0";
            lines[lineCount + 2] = "vertex " + ((Point)outlinePoints[outlinePoints.Count - 1]).getX() * resizeFactor + " " + ((Point)outlinePoints[outlinePoints.Count - 1]).getY() * resizeFactor + " " + 0.0;
            lines[lineCount + 3] = "vertex " + ((Point)outlinePoints[0]).getX() * resizeFactor + " " + ((Point)outlinePoints[0]).getY() * resizeFactor + " " + 0.0;
            lines[lineCount + 4] = "vertex " + ((Point)outlinePoints[outlinePoints.Count - 1]).getX() * resizeFactor + " " + ((Point)outlinePoints[outlinePoints.Count - 1]).getY() * resizeFactor + " " + depth;
            lines[lineCount + 5] = "endloop";
            lines[lineCount + 6] = "endfacet";
            lineCount += 7;

            for (int i = 0; i < outlinePoints.Count - 1; i++)
            {
                lines[lineCount] = "outer loop";
                lines[lineCount + 1] = "facet normal 0.0 0.0 0.0";
                lines[lineCount + 2] = "vertex " + ((Point)outlinePoints[i]).getX() * resizeFactor + " " + ((Point)outlinePoints[i]).getY() * resizeFactor + " " + depth;
                lines[lineCount + 3] = "vertex " + ((Point)outlinePoints[i + 1]).getX() * resizeFactor + " " + ((Point)outlinePoints[i + 1]).getY() * resizeFactor + " " + depth;
                lines[lineCount + 4] = "vertex " + ((Point)outlinePoints[i + 1]).getX() * resizeFactor + " " + ((Point)outlinePoints[i + 1]).getY() * resizeFactor + " " + 0.0;
                lines[lineCount + 5] = "endloop";
                lines[lineCount + 6] = "endfacet";
                lineCount += 7;
            }

            lines[lineCount] = "outer loop";
            lines[lineCount + 1] = "facet normal 0.0 0.0 0.0";
            lines[lineCount + 2] = "vertex " + ((Point)outlinePoints[outlinePoints.Count - 1]).getX() * resizeFactor + " " + ((Point)outlinePoints[outlinePoints.Count - 1]).getY() * resizeFactor + " " + depth;
            lines[lineCount + 3] = "vertex " + ((Point)outlinePoints[0]).getX() * resizeFactor + " " + ((Point)outlinePoints[0]).getY() * resizeFactor + " " + depth;
            lines[lineCount + 4] = "vertex " + ((Point)outlinePoints[0]).getX() * resizeFactor + " " + ((Point)outlinePoints[0]).getY() * resizeFactor + " " + 0.0;
            lines[lineCount + 5] = "endloop";
            lines[lineCount + 6] = "endfacet";
            lineCount += 7;
            
            lines[lineCount] = "endsolid model";
            System.IO.File.WriteAllLines(@destFile, lines);
        }
    }
}
