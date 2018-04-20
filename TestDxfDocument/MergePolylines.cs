using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Group = netDxf.Objects.Group;
using Point = netDxf.Entities.Point;
using Attribute = netDxf.Entities.Attribute;
using Image = netDxf.Entities.Image;
using netDxf.Entities;

namespace TestDxfDocument
{
    class MergePolylines
    {
        string r = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string s = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

      static string[] lines = System.IO.File.ReadAllLines(@"img-X06065050-0002uprava.txt");
       
        public static void mergePolylines(Polyline[] polyline, List<double> polyThickness)
        {
            List<Coordinates> coordinates = new List<Coordinates>();
            List<double> lineThicknesses = new List<double>();

            int count = 0;
            int countPoly = 0;

            polyline[countPoly] = new Polyline(); 
           
            foreach (string eachLine in lines)
            {
                if (eachLine != "POLYLINE")
                {
                    if (count < 4) 
                    {
                        count++;
                        count++;
                    }

                    coordinates = fillCoordinates(eachLine, coordinates);                     

                    removeRedundacy(coordinates); 
                }
                if ((eachLine == "POLYLINE") & (count > 2))
                {
                    coordinates = removeRedundacy3InCircle(coordinates);  // if only 3 points, 1. and 3. are identical, the last has to be removed                 

                    //---------------------                  
                    lineThicknesses = PreserveThickness.prepareToComputeThickness(coordinates); //   polyline s thickness
                    
                    polyThickness.Add(lineThicknesses.Sum() / lineThicknesses.Count);// average polyline s thickness
                    //---------------------                   

                    coordinates = computeCoordinates(coordinates);                   

                    if (coordinates.Count > 2)      
                    {
                        if (insertCoordinatesIfCircle(coordinates)) // are in loop?
                        {
                            coordinates = computeCoordinatesInCircle(coordinates);                       
                        }
                    }

                    writeDxf(polyline, coordinates, countPoly); 

                    countPoly++; 
                    polyline[countPoly] = new Polyline();

                    coordinates.Clear(); 
                }
            }
        }

        private static List<Coordinates> fillCoordinates(string eachLine, List<Coordinates> coordinates)
        {
            float cooX, cooY;          

            string[] words = eachLine.Split(' ');

            words[0] = words[0].Replace(".", ","); 
            words[1] = words[1].Replace(".", ",");

            float.TryParse(words[0], out cooX);
            float.TryParse(words[1], out cooY);

            coordinates.Add(new Coordinates(cooX, cooY));

            return coordinates;
        }

        private static void removeRedundacy(List<Coordinates> coordinates)
        {
            if (coordinates.Count >= 2) 
            {
                if ((coordinates[coordinates.Count - 2].X == coordinates[coordinates.Count - 1].X) & (coordinates[coordinates.Count - 2].Y == coordinates[coordinates.Count - 1].Y))
                {
                    coordinates.RemoveRange(coordinates.Count - 1, 1);
                }
            }
        }

        private static List<Coordinates> removeRedundacy3InCircle(List<Coordinates> coordinates) // compares 1. and 3., if identical, last is removed
        {
            if (coordinates.Count == 3) 
            {
                if ((coordinates[0].X == coordinates[2].X) & (coordinates[0].Y == coordinates[2].Y))
                {
                    coordinates.RemoveRange(coordinates.Count - 1, 1);
                }
            }

            return coordinates;
        }

        private static List<Coordinates> computeCoordinates(List<Coordinates> coordinates) //computes directional and normal vector and coeficients of lines and whether they are identical
        {
            List<Coordinates> mergeCoordinates = new List<Coordinates>();

            float u1, u2; 
            float u3, u4; 

            float n1, n2; 
            float n3, n4; 

            float c1, c2; 

            float result1, result2, result3; 

            int count = coordinates.Count;
            
            while (coordinates.Count > 2)
            {
                u1 = coordinates[1].X - coordinates[0].X;
                u2 = coordinates[1].Y - coordinates[0].Y;

                u3 = coordinates[2].X - coordinates[1].X;
                u4 = coordinates[2].Y - coordinates[1].Y;

                n1 = -u2;
                n2 = u1;

                n3 = -u4;
                n4 = u3;

                c1 = n1 * coordinates[0].X + n2 * coordinates[0].Y;
                c2 = n3 * coordinates[2].X + n4 * coordinates[2].Y;

                result1 = n1 / n3;
                result2 = n2 / n4;
                result3 = c1 / c2;

                if ((n1 == 0.0) & (n3 == 0.0)) 
                {
                    if (result2 == result3)
                    {
                        coordinates.RemoveRange(1, 1);  
                        continue;
                    }
                }
                else if ((n2 == 0.0) & (n4 == 0.0)) 
                {
                    if (result1 == result3)
                    {
                        coordinates.RemoveRange(1, 1);  
                        continue;
                    }
                }
                else if ((c1 == 0.0) & (c2 == 0.0))       
                {
                    if (result1 == result2)
                    {
                        coordinates.RemoveRange(1, 1);  
                        continue;
                    }
                }
                else if ((result1 == result2) & (result1 == result3) & (result2 == result3))
                {
                    coordinates.RemoveRange(1, 1);
                    continue;
                }
                else
                {
                    mergeCoordinates.Add(new Coordinates(coordinates[0].X, coordinates[0].Y)); 

                    coordinates.RemoveRange(0, 1); 

                    continue;
                }
            }
            mergeCoordinates.Add(coordinates[0]);
            mergeCoordinates.Add(coordinates[1]);

            return mergeCoordinates;
        }

        private static List<Coordinates> computeCoordinatesInCircle(List<Coordinates> coordinates)
        {
            List<Coordinates> first3Points = new List<Coordinates>();

            coordinates.Insert(0, new Coordinates(coordinates[coordinates.Count - 2].X, coordinates[coordinates.Count - 2].Y));

            first3Points = addFirst3Points(first3Points, coordinates); 

            first3Points = computeCoordinates(first3Points);   

            if (first3Points.Count == 2) 
            {
                coordinates.RemoveRange(coordinates.Count - 1, 1); 

                coordinates.RemoveRange(1, 1);              
            }
            if (first3Points.Count == 3) 
            {
                coordinates.RemoveRange(0, 1);
            }

            return coordinates;
        }

        private static List<Coordinates> addFirst3Points(List<Coordinates> first3Points, List<Coordinates> coordinates)
        {
            for (int i = 0; i < 3; i++)
            {
                first3Points.Add(new Coordinates(coordinates[i].X, coordinates[i].Y));
            }

            return first3Points;
        }

        private static bool insertCoordinatesIfCircle(List<Coordinates> coordinates)
        {
            bool isCircle = false; 
            
            if ((coordinates[0].X == coordinates[coordinates.Count - 1].X) & (coordinates[0].Y == coordinates[coordinates.Count - 1].Y))// are in loop? 
            {
                isCircle = true; // sú v kruhu                
            }

            return isCircle;
        }

        private static void writeDxf(Polyline[] poly, List<Coordinates> coordinates, int counPoly)
        {
            for (int j = 0; j < coordinates.Count; j++)
            {
                poly[counPoly].Vertexes.Add(new PolylineVertex(coordinates[j].X, coordinates[j].Y, 0));
            }
        }  
    }
}
