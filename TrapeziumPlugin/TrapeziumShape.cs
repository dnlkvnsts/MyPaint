using Paint.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TrapeziumPlugin
{
    public class TrapeziumShape : IShape
    {
       
        public List<Point> Points { get; set; } = new List<Point>();
        public Brush StrokeColor { get; set; } = Brushes.Black;
        public double StrokeThickness { get; set; } = 2;
        public Brush FillColor { get; set; } = Brushes.Transparent; 
        public double Angle { get; set; } = 0;                   
        public bool IsSelected { get; set; } = false;              
        public int OrderIndex { get; set; } = 0;                  

        public void Draw(Canvas canvas)
        {
            if (Points.Count < 2) return;

            Point start = Points[0];
            Point end = Points[1];

           
            double left = Math.Min(start.X, end.X);
            double top = Math.Min(start.Y, end.Y);
            double right = Math.Max(start.X, end.X);
            double bottom = Math.Max(start.Y, end.Y);
            double width = right - left;
            double height = bottom - top;

            double centerX = left + width / 2;
            double centerY = top + height / 2;

            
            Point p1 = new Point(left + width * 0.2, top);   
            Point p2 = new Point(right - width * 0.2, top);   
            Point p3 = new Point(right, bottom);             
            Point p4 = new Point(left, bottom);              

            
            Polygon trapezium = new Polygon
            {
                Stroke = StrokeColor,
                StrokeThickness = StrokeThickness,
                Fill = FillColor, 
                Points = new PointCollection { p1, p2, p3, p4 },
                Uid = Guid.NewGuid().ToString()
                
            };

            RotateTransform rotateTransform = new RotateTransform(Angle, centerX, centerY);
            trapezium.RenderTransform = rotateTransform;

            canvas.Children.Add(trapezium);
        }

        
        public IShape Clone()
        {
            return new TrapeziumShape
            {
               
                Points = new List<Point>(this.Points.Select(p => new Point(p.X, p.Y))),
                StrokeColor = this.StrokeColor,
                StrokeThickness = this.StrokeThickness,
                FillColor = this.FillColor,
                Angle = this.Angle,
                IsSelected = this.IsSelected,
                OrderIndex = this.OrderIndex
            };
        }
    }

}
