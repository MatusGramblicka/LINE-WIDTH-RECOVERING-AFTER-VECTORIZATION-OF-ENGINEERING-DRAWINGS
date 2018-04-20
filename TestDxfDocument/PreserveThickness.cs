using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDxfDocument
{
    class PreserveThickness
    {

        string r = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string s = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

         static Bitmap drawing = new Bitmap(@"img-X06065050-0002uprava.bmp");
      
        public static List<double> prepareToComputeThickness(List<Coordinates> coordinates)
        {
            List<Coordinates> coordinatesForThicknessLocal = new List<Coordinates>(coordinates.Select(x => x.Clone())); 
            List<double> lineThicknessesLocal = new List<double>(); 

            int imageWidth = drawing.Width;
            int imageHeight = drawing.Height;

            int numberOfPoints = coordinatesForThicknessLocal.Count;    // počet bodov v polyline

            coordinatesForThicknessLocal = changeYCoordinates(imageHeight, coordinatesForThicknessLocal); // prepočítavam Yovú súradnicu

            for (int i = 0; i < numberOfPoints - 1; i++) // postupujem cez jednotlivé body
            {                
                int length = lineLength(i, coordinatesForThicknessLocal); // zisťujem dĺžku úsečky       

                PointF S = new PointF();   // stred úsečky
                S.X = (int)(coordinatesForThicknessLocal[i].X + coordinatesForThicknessLocal[i + 1].X) / 2;
                S.Y = (int)(coordinatesForThicknessLocal[i].Y + coordinatesForThicknessLocal[i + 1].Y) / 2;

                double thickness = pointsToCompute(i, length, coordinatesForThicknessLocal, S); // zisťujem hrúbku čiary na základe bodov

           //     double thickness = pointsToComputeDependingOnLineLenght(i, length, coordinatesForThicknessLocal); // zisťujem hrúbku čiary na základe bodov
                

                lineThicknessesLocal.Add(thickness);
            }

            return lineThicknessesLocal;
        }

        private static List<Coordinates> changeYCoordinates(int imageHeight, List<Coordinates> coordinatesForThicknessLocal)
        {
            int numberOfPoints = coordinatesForThicknessLocal.Count;

            for (int i = 0; i < numberOfPoints; i++)
            {
                coordinatesForThicknessLocal[i].Y = imageHeight - coordinatesForThicknessLocal[i].Y;
            }

            return coordinatesForThicknessLocal;
        }

        private static int lineLength(int i, List<Coordinates> coordinatesForThicknessLocal)
        {
            double lX = Math.Pow(coordinatesForThicknessLocal[i + 1].X - coordinatesForThicknessLocal[i].X, 2);
            double lY = Math.Pow(coordinatesForThicknessLocal[i + 1].Y - coordinatesForThicknessLocal[i].Y, 2);

            return (int)Math.Sqrt(lX + lY);
        }

        private static double pointsToComputeDependingOnLineLenght(int i, int length, List<Coordinates> coordinatesForThicknessLocal)
        {
            Color pixelColor = Color.Red;

            int particularThickness = 0; 
            int a;
            int nextPoint = 0;
            int inc = 0;
            double thickness = 0;

            PointF precisePoint = new PointF(); 

            if (length % 2 == 0)        // the length has to be even
                a = length / 2;
            else
            {
                length = length - 1;
                a = length / 2;
            }

            while (nextPoint < a)
            {
                precisePoint.X = coordinatesForThicknessLocal[i].X + inc;
                precisePoint.Y = coordinatesForThicknessLocal[i].Y + inc;

                pixelColor = drawing.GetPixel((int)precisePoint.X, (int)precisePoint.Y + 2); 

                if (pixelColor.Name == "ff000000") //Čierna farba, som na úsečke
                    {
                        //numberOfBlack++;
                        particularThickness += computeThickness(precisePoint, coordinatesForThicknessLocal[i]); // hrúbka na konkrétnom bode
                    }

                    nextPoint++;
                    inc = inc + 2;
            }
            if (particularThickness != 0)
            {
                thickness = particularThickness / nextPoint;
            }
            else if (particularThickness == 0)
            {
                thickness = 0;
            }

            return thickness;
        }   
        
        private static double pointsToCompute(int i, int length, List<Coordinates> coordinatesForThicknessLocal, PointF S)
        {
            Color pixelColor = Color.Red;

            int particularThickness = 0; 
            double thickness = 0;

            int nextPoint = 0; 

            if (length >= 4)   
            {
                PointF precisePoint = new PointF(); 

                PointF quarter = new PointF();  
                quarter.X = (int)(coordinatesForThicknessLocal[i].X + S.X) / 2;
                quarter.Y = (int)(coordinatesForThicknessLocal[i].Y + S.Y) / 2;

                PointF threeQuarter = new PointF(); 
                threeQuarter.X = (int)(S.X + coordinatesForThicknessLocal[i + 1].X) / 2;
                threeQuarter.Y = (int)(S.Y + coordinatesForThicknessLocal[i + 1].Y) / 2;

                while (nextPoint < 3)   // zisťovanie na strede, štvrtine a 3štvrtine
                {
                    if (nextPoint == 0)
                    { pixelColor = drawing.GetPixel((int)S.X, (int)S.Y); precisePoint = S; }
                    if (nextPoint == 1)
                    { pixelColor = drawing.GetPixel((int)quarter.X, (int)quarter.Y); precisePoint = quarter; }
                    if (nextPoint == 2)
                    { pixelColor = drawing.GetPixel((int)threeQuarter.X, (int)threeQuarter.Y); precisePoint = threeQuarter; }

                    if (pixelColor.Name == "ff000000") //Čierna farba, som na úsečke
                    {
                        //numberOfBlack++;
                        particularThickness += computeThickness(precisePoint, coordinatesForThicknessLocal[i]); // hrúbka na konkrétnom bode
                    }

                    nextPoint++;
                }
                if (particularThickness != 0)  
                {
                    thickness = particularThickness / nextPoint;
                }
                else if (particularThickness == 0) 
                {
                    thickness = 0;
                }
            }

            return thickness;
        }

        private static int computeThickness(PointF precisePoint, Coordinates preciseCoordForThicknessLocal)
        {           
            Color pixelColor = Color.Red;

            PointF particularPoint = new PointF(); 

            int trueThickness = 0; 

            float u1 = 0, u2 = 0; 

            float n1, n2; 
            float n3, n4;

            float cn; 

            u1 = precisePoint.X - preciseCoordForThicknessLocal.X; // smerový vektor
            u2 = precisePoint.Y - preciseCoordForThicknessLocal.Y;

            n1 = -u2; 
            n2 = u1;

            n3 = -n2; 
            n4 = n1;

            cn = -n3 * precisePoint.X - n4 * precisePoint.Y;

            for (int nextP = 1; ; nextP++) // začínam krokovanie pripočítavaním pri práci s normálovým vektorom
            {
                particularPoint = computeParticularPoint(precisePoint, nextP, n3, n4, cn);// získavam súr., na zákl. kt. zisťujem hrúbku 

                pixelColor = drawing.GetPixel((int)particularPoint.X, (int)particularPoint.Y);

                if (pixelColor.Name == "ff000000")
                {
                    trueThickness++;
                }
                else break; // ukončujem pripočítavanie, lebo som narazil na miesto mimo úsečky
            }
            for (int nextP = -1; ; nextP--) // pokračujem krokovanie odčítavaním pri práci s normálovým vektorom
            {
                particularPoint = computeParticularPoint(precisePoint, nextP, n3, n4, cn);// získavam súr., na zákl. kt. zisťujem hrúbku. 

                pixelColor = drawing.GetPixel((int)particularPoint.X, (int)particularPoint.Y);

                if (pixelColor.Name == "ff000000")
                {
                    trueThickness++;
                }
                else break; // ukončujem odčítavanie, lebo som narazil na miesto mimo úsečky
            }

            trueThickness++; // pripočítavam 1, pretože započítavam bod ležiaci na pôvodnej úsečke, podľa ktorého určujem hrúbku, teda stred, štvrtinu a 3štvrtinu

            return trueThickness;
        }

        private static PointF computeParticularPoint(PointF precisePoint, int nextP, float n3, float n4, float cn)
        {
            PointF tempPoint = new PointF(); 

            if ((n4 == 0) & (n3 != 0))// ide zvislo,
            {
                tempPoint.Y = (int)precisePoint.Y + nextP; // zisťujem Xovú súr. na základe Yovej
                tempPoint.X = (int)(-n4 * tempPoint.Y - cn) / (int)n3;
            }
            if ((n3 == 0) & (n4 != 0))// ide vodorovne,
            {
                tempPoint.X = (int)precisePoint.X + nextP; // zisťujem Yovú súr. na základe Xovej
                tempPoint.Y = (int)(-n3 * tempPoint.X - cn) / (int)n4;
            }
            else if ((n3 != 0) & (n4 != 0))//ked ide normalne (povedzme diagonálne), n3 a  n4 nie sú nuly         
            {
                tempPoint.X = (int)precisePoint.X + nextP; // zisťujem Yovú súr. na základe Xovej
                tempPoint.Y = (int)(-n3 * tempPoint.X - cn) / (int)n4;

                if (tempPoint.Y > drawing.Height) // stával sa prípad, keď vypočítané Y bolo väčšie ako veľkosť obrázka, vtedy treba vypočítať X
                {
                    tempPoint.Y = (int)precisePoint.Y + nextP; // zisťujem Xovú súr. na základe Yovej
                    tempPoint.X = (int)(-n4 * tempPoint.Y - cn) / (int)n3;
                }
            }

            return tempPoint;
        }
    }
}
