using Paint.Core;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Paint.App
{
    public partial class MainWindow : Window
    {
        private List<IShape> _shapes = new List<IShape>();
        private IShape _currentShape;
        private IShape _selectedShape;
        private bool _isDrawingActive = false;
        private int? _currentSideCount = 5;
        
        private IPlugin _selectedPlugin;
      
        private bool _isSelectedMode = false;
        private IShape _clipboardShape;

        private bool _isDragging = false;
        private Point _lastMousePosition;

        //
        private bool _isResizing = false;
        private string _activeHandle = "";
        private Point _resizeAnchor;
        //


        private bool _isRotating = false;
        public MainWindow()
        {
            InitializeComponent();
            this.MouseUp += MainWindow_MouseUp;
            this.KeyDown += MainWindow_KeyDown;
            this.Focusable = true;
            this.Focus();
        }



        //click methods
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
           if(Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                if (_selectedShape != null)
                {
                    _clipboardShape = _selectedShape.Clone();
                }
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                if (_clipboardShape != null)
                {
                    IShape pastedShaped = _clipboardShape.Clone();

                    for(int i = 0; i < pastedShaped.Points.Count; i++)
                    {
                        pastedShaped.Points[i] = new Point(
                            pastedShaped.Points[i].X + 10,
                            pastedShaped.Points[i].Y + 10);
                    }

                    _shapes.Add(pastedShaped);
                    _selectedShape = pastedShaped;

                    _clipboardShape = pastedShaped.Clone();


                    Redraw();
                }
            }

            if (e.Key == Key.Delete || e.Key == Key.Z)
            {
                if (_selectedShape != null)
                {
                    _shapes.Remove(_selectedShape);
                    _selectedShape = null;
                    Redraw();
                }
            }


            if (_selectedShape != null)
            {
                double step = 5;
                double dx = 0, dy = 0;

                if (e.Key == Key.Left) dx = -step;
                if (e.Key == Key.Right) dx = step;
                if (e.Key == Key.Up) dy = -step;
                if (e.Key == Key.Down) dy = step;

                if (dx != 0 || dy != 0)
                {
                    for (int i = 0; i < _selectedShape.Points.Count; i++)
                    {
                        _selectedShape.Points[i] = new Point(
                            _selectedShape.Points[i].X + dx,
                            _selectedShape.Points[i].Y + dy);
                    }
                    Redraw();
                    e.Handled = true; 
                }
            }
        }

        //



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPlugins();
        }



        //work with plugins
        private void LoadPlugins()
        {
            string pluginsDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

            if (!Directory.Exists(pluginsDirectory))
                Directory.CreateDirectory(pluginsDirectory);

           
            string[] dllFiles = Directory.GetFiles(pluginsDirectory, "*.dll");

            foreach (string dllPath in dllFiles)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(dllPath);
                   
                    var pluginTypes = assembly.GetTypes()
                        .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface);

                    foreach (var type in pluginTypes)
                    {
                        IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                        AddPluginButton(plugin);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки плагина {dllPath}: {ex.Message}");
                }
            }
        }




        private void AddPluginButton(IPlugin plugin)
        {
            Button btn = new Button
            {
                Content = plugin.Name,
                Margin = new Thickness(5, 2, 5, 2),
                Padding = new Thickness(10, 0, 10, 0)
            };

            btn.Click += (s, e) =>
            {
                _selectedPlugin = plugin;
                _isSelectedMode = false;
                _selectedShape = null;

                if(plugin.Name == "Многоугольник")
                {
                    _currentSideCount = CountSideOfPolygon();
                }
            };

            PluginsPanel.Children.Add(btn);
        }

        //work with mouse


        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            Point mousePos = e.GetPosition(MainCanvas);

            //

            if (_isSelectedMode && _selectedShape != null)
            {
               
                var hitResult = VisualTreeHelper.HitTest(MainCanvas, mousePos);
                if (hitResult?.VisualHit is Rectangle handle && handle.Tag is string pos)
                {
                    if (pos == "ROT")
                    {
                        _isRotating = true;
                        _lastMousePosition = mousePos;
                        MainCanvas.CaptureMouse();
                        return;
                    }

                    _isResizing = true;
                    _activeHandle = pos;
                    _lastMousePosition = mousePos;

                    double minX = _selectedShape.Points.Min(p => p.X);
                    double maxX = _selectedShape.Points.Max(p => p.X);
                    double minY = _selectedShape.Points.Min(p => p.Y);
                    double maxY = _selectedShape.Points.Max(p => p.Y);
                    double midX = (minX + maxX) / 2;
                    double midY = (minY + maxY) / 2;

                   
                    if (pos == "SE") _resizeAnchor = new Point(minX, minY);
                    else if (pos == "NW") _resizeAnchor = new Point(maxX, maxY);
                    else if (pos == "NE") _resizeAnchor = new Point(minX, maxY);
                    else if (pos == "SW") _resizeAnchor = new Point(maxX, minY);
                    else if (pos == "N") _resizeAnchor = new Point(midX, maxY); 
                    else if (pos == "S") _resizeAnchor = new Point(midX, minY); 
                    else if (pos == "W") _resizeAnchor = new Point(maxX, midY); 
                    else if (pos == "E") _resizeAnchor = new Point(minX, midY); 

                    MainCanvas.CaptureMouse();
                    return;
                }
            }
            //


            if (_isSelectedMode)
            {
                _selectedShape = null;

                for (int i = _shapes.Count - 1; i >= 0; i--)
                {
                    if(IsPointInShape(mousePos, _shapes[i]))
                    {
                        _selectedShape =  _shapes[i];
                        _isDragging = true;
                        _lastMousePosition = mousePos;
                        ThicknessSlider.Value = _selectedShape.StrokeThickness;
                        MainCanvas.CaptureMouse();
                        break;
                    }
                }
                Redraw();
                return;
            }



            if (_selectedPlugin == null) return;
            bool isPolyline = _selectedPlugin.Name == "Ломаная";

            if (isPolyline)
            {
                if (!_isDrawingActive)
                {
                    MainCanvas.CaptureMouse();
                    _isDrawingActive = true;
                    _currentShape = _selectedPlugin.CreateInstance();
                    

                    if (StrokeColorPicker.SelectedItem is ComboBoxItem strokeColorItem)
                    {
                        _currentShape.StrokeColor = (Brush)strokeColorItem.Tag;
                    }


                    _currentShape.StrokeThickness = ThicknessSlider.Value;



                    _currentShape.Points = new List<Point> { mousePos, mousePos };
                }
                else
                {
                  
                    _currentShape.Points.Add(mousePos);
                }
            }
            else
            {
              
                _isDrawingActive = true;
                _currentShape = _selectedPlugin.CreateInstance();

                var sideCountProperty = _currentShape.GetType().GetProperty("SideCount");

                
                if (sideCountProperty != null)
                {
                   
                    sideCountProperty.SetValue(_currentShape, _currentSideCount);
                }

               


                if (FillColorPicker.SelectedItem is ComboBoxItem fillItem)
                {
                    _currentShape.FillColor = (Brush)fillItem.Tag;
                }


                if (StrokeColorPicker.SelectedItem is ComboBoxItem strokeColorItem)
                {
                    _currentShape.StrokeColor = (Brush)strokeColorItem.Tag;
                }

                _currentShape.StrokeThickness = ThicknessSlider.Value;



                _currentShape.Points = new List<Point> { mousePos, mousePos };
                MainCanvas.CaptureMouse();
            }
            Redraw();
        }




        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {

            Point currentPos = e.GetPosition(MainCanvas);

            //
            if (_isRotating && _selectedShape != null)
            {
               
                double minX = _selectedShape.Points.Min(p => p.X);
                double maxX = _selectedShape.Points.Max(p => p.X);
                double minY = _selectedShape.Points.Min(p => p.Y);
                double maxY = _selectedShape.Points.Max(p => p.Y);
                Point center = new Point((minX + maxX) / 2, (minY + maxY) / 2);

                
                double angleRad = Math.Atan2(currentPos.Y - center.Y, currentPos.X - center.X);
                double angleDeg = angleRad * (180 / Math.PI);

               
                _selectedShape.Angle = angleDeg + 90;

                Redraw();
                return;
            }
            else if (_isResizing && _selectedShape != null)
            {
                
                double oldDx = _lastMousePosition.X - _resizeAnchor.X;
                double oldDy = _lastMousePosition.Y - _resizeAnchor.Y;
                double newDx = currentPos.X - _resizeAnchor.X;
                double newDy = currentPos.Y - _resizeAnchor.Y;

               
                if (Math.Abs(oldDx) < 0.5) oldDx = 0.5;
                if (Math.Abs(oldDy) < 0.5) oldDy = 0.5;

               
                double scaleX = 1.0;
                double scaleY = 1.0;

                if (_activeHandle.Contains("E") || _activeHandle.Contains("W")) scaleX = newDx / oldDx;
                if (_activeHandle.Contains("N") || _activeHandle.Contains("S")) scaleY = newDy / oldDy;

                
                if (_selectedShape.GetType().Name.Contains("Polygon") && _selectedShape.Points.Count == 2)
                {
                   
                    double uniformScale = (_activeHandle.Length > 1)
                    ? (Math.Abs(scaleX) + Math.Abs(scaleY)) / 2 * Math.Sign(scaleX + scaleY)
                    : (scaleX != 1.0 ? scaleX : scaleY);
        
        
                        for (int i = 0; i < _selectedShape.Points.Count; i++)
                        {
                            Point p = _selectedShape.Points[i];
                            double nx = _resizeAnchor.X + (p.X - _resizeAnchor.X) * uniformScale;
                            double ny = _resizeAnchor.Y + (p.Y - _resizeAnchor.Y) * uniformScale;
                            _selectedShape.Points[i] = new Point(nx, ny);
                        }
                }
                else
                {
                   
                    for (int i = 0; i < _selectedShape.Points.Count; i++)
                    {
                        Point p = _selectedShape.Points[i];
                        double nx = _resizeAnchor.X + (p.X - _resizeAnchor.X) * scaleX;
                        double ny = _resizeAnchor.Y + (p.Y - _resizeAnchor.Y) * scaleY;
                        _selectedShape.Points[i] = new Point(nx, ny);
                    }
                }

                _lastMousePosition = currentPos;
                Redraw();
            }
            else if (_isDragging && _selectedShape != null)
            {
               
                double dx = currentPos.X - _lastMousePosition.X;
                double dy = currentPos.Y - _lastMousePosition.Y;

                
                for (int i = 0; i < _selectedShape.Points.Count; i++)
                {
                    _selectedShape.Points[i] = new Point(
                        _selectedShape.Points[i].X + dx,
                        _selectedShape.Points[i].Y + dy);
                }

                _lastMousePosition = currentPos; 
                Redraw();
            }
            else if (_isDrawingActive && _currentShape != null) 
            {
                int lastIndex = _currentShape.Points.Count - 1;
                _currentShape.Points[lastIndex] = currentPos;
                Redraw();
            }
   
        }


        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FinishDrawing(e);
        }

        private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FinishDrawing(e);
        }



        private void FinishDrawing(MouseButtonEventArgs e)
        {
            _isDragging = false;
            _isRotating = false;

            //
            _isResizing = false;
            _activeHandle = "";
            //


            MainCanvas.ReleaseMouseCapture();
            

            if (e.ChangedButton != MouseButton.Left) return;
            if (_selectedPlugin == null) return;

           
            if (_selectedPlugin.Name == "Ломаная") return;

            if (_isDrawingActive && _currentShape != null)
            {
                _shapes.Add(_currentShape);
                _currentShape = null;
                _isDrawingActive = false;
                MainCanvas.ReleaseMouseCapture();
                Redraw();
            }
        }




        private void MainCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectedPlugin == null) return;
            bool isPolyline = _selectedPlugin.Name == "Ломаная";

            if (isPolyline && _isDrawingActive && _currentShape != null)
            {
               
                
                if (_currentShape.Points.Count >= 2)
                {
                    _shapes.Add(_currentShape);
                }

               
                _currentShape = null;
                _isDrawingActive = false;

                MainCanvas.ReleaseMouseCapture();
                Redraw();
            }
        }
     
        private void Redraw()
        {
            MainCanvas.Children.Clear();
            foreach (var shape in _shapes)
            {
                shape.Draw(MainCanvas);
                
                if(shape == _selectedShape)
                {
                    DrawSelectionFrame(shape);
                }
            }

            if (_currentShape != null)
            {
                _currentShape.Draw(MainCanvas);
            }
        }



        //count polygon side

        private int? CountSideOfPolygon()
        {
            while (true)
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("Введите количество сторон многоугольника(3-8)", "Настройка фигуры", "5");

                if (string.IsNullOrEmpty(input)) return null;


                if (int.TryParse(input, out int result) && result >= 3 && result <= 8)
                {
                    return result;
                }

                MessageBox.Show("Неверный ввод!!!Введите корректное число сторон(от 3 до 8)");

            }
        }


        //Add selection mode
        private void SelectMode_Click(object sender, RoutedEventArgs e)
        {
            _isSelectedMode = true;
            _selectedPlugin = null;
            _selectedShape = null;
            Redraw();
        }

        private void StrokeColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedShape != null && StrokeColorPicker.SelectedItem is ComboBoxItem item)
            {
                _selectedShape.StrokeColor = (Brush)item.Tag;
                Redraw();
            }
        }

        private void FillColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(_selectedShape != null && FillColorPicker.SelectedItem is ComboBoxItem item )
            {
                _selectedShape.FillColor = (Brush)item.Tag;
                Redraw() ;
            }
        }


        private bool IsPointInShape(Point p, IShape shape)
        {
            if (shape.Points.Count < 2) return false;

            double minX, maxX, minY, maxY;

           
            if (shape.GetType().Name.Contains("Polygon") && shape.Points.Count == 2)
            {
                Point center = shape.Points[0];
                Point radiusPoint = shape.Points[1];
                double radius = Math.Sqrt(Math.Pow(radiusPoint.X - center.X, 2) + Math.Pow(radiusPoint.Y - center.Y, 2));

                minX = center.X - radius;
                maxX = center.X + radius;
                minY = center.Y - radius;
                maxY = center.Y + radius;
            }
            else 
            {
                minX = shape.Points.Min(pt => pt.X);
                maxX = shape.Points.Max(pt => pt.X);
                minY = shape.Points.Min(pt => pt.Y);
                maxY = shape.Points.Max(pt => pt.Y);
            }


            return p.X >= minX - 5 && p.X <= maxX + 5 && p.Y >= minY - 5 && p.Y <= maxY + 5;
        }


        private void DrawSelectionFrame(IShape shape)
        {
            double minX, maxX, minY, maxY;

           
            if (shape.GetType().Name.Contains("Polygon") && shape.Points.Count == 2)
            {
                Point center = shape.Points[0];
                Point radiusPoint = shape.Points[1];

               
                double radius = Math.Sqrt(Math.Pow(radiusPoint.X - center.X, 2) + Math.Pow(radiusPoint.Y - center.Y, 2));

                minX = center.X - radius;
                maxX = center.X + radius;
                minY = center.Y - radius;
                maxY = center.Y + radius;
            }
            else 
            {
                minX = shape.Points.Min(p => p.X);
                maxX = shape.Points.Max(p => p.X);
                minY = shape.Points.Min(p => p.Y);
                maxY = shape.Points.Max(p => p.Y);
            }

            //
            double midX = (minX + maxX) / 2;
            double midY = (minY + maxY) / 2;
            //

            Rectangle rect = new Rectangle
            {
                Width = maxX - minX + 20,
                Height = maxY - minY + 20,
                Stroke = Brushes.DeepSkyBlue,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection() { 4, 2 }
            };
            Canvas.SetLeft(rect, minX - 10);
            Canvas.SetTop(rect, minY - 10);
            MainCanvas.Children.Add(rect);

            //

            AddHandle(minX - 14, minY - 14, "NW"); 
            AddHandle(maxX + 6, minY - 14, "NE");
            AddHandle(minX - 14, maxY + 6, "SW");
            AddHandle(maxX + 6, maxY + 6, "SE");

            AddHandle(midX - 4, minY - 14, "N");  
            AddHandle(midX - 4, maxY + 6, "S");
            AddHandle(minX - 14, midY - 4, "W");
            AddHandle(maxX + 6, midY - 4, "E");
            //

            AddHandle(midX - 4, minY - 40, "ROT"); // "ROT" - метка для вращения, на 40 пикселей выше фигуры

            // Для красоты можно провести линию от фигуры к этой ручке
            Line connector = new Line
            {
                X1 = midX,
                Y1 = minY - 10,
                X2 = midX,
                Y2 = minY - 32,
                Stroke = Brushes.DeepSkyBlue,
                StrokeThickness = 1
            };
            MainCanvas.Children.Add(connector);
        }

        private void ThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_selectedShape != null )
            {
                _selectedShape.StrokeThickness = ThicknessSlider.Value;
                Redraw();
            }
        }


        //

        private void AddHandle(double x, double y, string position)
        {
            Rectangle handle = new Rectangle
            {
                Width = 8,
                Height = 8,
                Fill = Brushes.White,
                Stroke = Brushes.DeepSkyBlue,
                StrokeThickness = 1,
                Tag = position 
            };
            Canvas.SetLeft(handle, x);
            Canvas.SetTop(handle, y);
            MainCanvas.Children.Add(handle);
        }

        //

    }
}