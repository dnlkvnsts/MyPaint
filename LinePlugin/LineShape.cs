

using Paint.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LinePlugin
{
    public  class LineShape : IShape
    {
        public List<Point> Points { get; set; } = new List<Point>();
        public Brush StrokeColor { get; set; } = Brushes.Black;
        public double StrokeThickness { get; set; } = 2.0;
        public Brush FillColor { get; set; } = Brushes.Transparent;
        public double Angle { get; set; } = 0;
        public bool IsSelected { get; set; } = false;
        public int OrderIndex { get; set; } = 0;


        public Point StartPoint
        {
            get => Points.Count > 0 ? Points[0] : new Point(0, 0);
            set
            {
                if (Points.Count == 0) Points.Add(new Point());
                Points[0] = value;
            }
        }

        public Point EndPoint
        {
            get => Points.Count > 1 ? Points[1] : new Point(0, 0);
            set
            {
                while (Points.Count < 2) Points.Add(new Point());
                Points[1] = value;
            }
        }


       
        public void Draw(Canvas canvas)
        {
            if (Points.Count < 2) return;

            var line = new System.Windows.Shapes.Line
            {
                X1 = Points[0].X,
                Y1 = Points[0].Y,
                X2 = Points[1].X,
                Y2 = Points[1].Y,
                Stroke = StrokeColor,
                StrokeThickness = StrokeThickness
               
            };

            double centerX = (Points[0].X + Points[1].X) / 2;
            double centerY = (Points[0].Y + Points[1].Y) / 2;

           
            RotateTransform rotateTransform = new RotateTransform(Angle, centerX, centerY);
            line.RenderTransform = rotateTransform;


            canvas.Children.Add(line);
        }

        public IShape Clone()
        {
            return new LineShape
            {
                Points = new List<Point>(this.Points.Select(p => new Point(p.X, p.Y))),
                StrokeColor = this.StrokeColor,
                StrokeThickness = this.StrokeThickness,
                FillColor = this.FillColor,
                Angle = this.Angle,
                IsSelected = false,
                OrderIndex = this.OrderIndex
            };
        }
    }
}

