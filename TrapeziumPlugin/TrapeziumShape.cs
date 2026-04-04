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
        // Реализация свойств интерфейса IShape
        public List<Point> Points { get; set; } = new List<Point>();
        public Brush StrokeColor { get; set; } = Brushes.Black;
        public double StrokeThickness { get; set; } = 2;
        public Brush FillColor { get; set; } = Brushes.Transparent; // Заливка
        public double Angle { get; set; } = 0;                     // Угол поворота
        public bool IsSelected { get; set; } = false;              // Выделена ли фигура
        public int OrderIndex { get; set; } = 0;                   // Порядок (для слоев)

        public void Draw(Canvas canvas)
        {
            if (Points.Count < 2) return;

            Point start = Points[0];
            Point end = Points[1];

            // 1. Расчет геометрии (границы)
            double left = Math.Min(start.X, end.X);
            double top = Math.Min(start.Y, end.Y);
            double right = Math.Max(start.X, end.X);
            double bottom = Math.Max(start.Y, end.Y);
            double width = right - left;
            double height = bottom - top;

            // 2. Создаем 4 точки трапеции
            Point p1 = new Point(left + width * 0.2, top);    // Верх лево
            Point p2 = new Point(right - width * 0.2, top);   // Верх право
            Point p3 = new Point(right, bottom);              // Низ право
            Point p4 = new Point(left, bottom);               // Низ лево

            // 3. Создаем объект Polygon (фигура WPF)
            Polygon trapezium = new Polygon
            {
                Stroke = StrokeColor,
                StrokeThickness = StrokeThickness,
                Fill = FillColor, // Применяем заливку из интерфейса
                Points = new PointCollection { p1, p2, p3, p4 },
                Uid = Guid.NewGuid().ToString() // Уникальный ID (полезно для поиска)
            };

            // 4. Реализация вращения (Angle)
            // Вращаем относительно центра фигуры
            //RotateTransform rotateTransform = new RotateTransform(
            //    Angle,
            //    left + width / 2,
            //    top + height / 2
            //);
            //trapezium.RenderTransform = rotateTransform;

            // 5. Визуализация выделения (IsSelected)
            //if (IsSelected)
            //{
                // Если фигура выделена, добавим ей эффект свечения или пунктирную обводку
            //    trapezium.StrokeDashArray = new DoubleCollection() { 2, 2 };
            //    trapezium.Stroke = Brushes.Red; // Для наглядности при выделении
            //}

            // 6. Учет Z-Index (порядок отрисовки)
            //Canvas.SetZIndex(trapezium, OrderIndex);

            canvas.Children.Add(trapezium);
        }

        // Реализация метода Clone для копирования и Undo/Redo
        public IShape Clone()
        {
            return new TrapeziumShape
            {
                // Создаем новый список точек, чтобы не было ссылки на старый
                Points = new List<Point>(this.Points),
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
