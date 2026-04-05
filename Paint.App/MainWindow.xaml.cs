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

        public MainWindow()
        {
            InitializeComponent();
            this.MouseUp += MainWindow_MouseUp;
            
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPlugins();
        }




        //
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

        //

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            Point mousePos = e.GetPosition(MainCanvas);

            

            if (_isSelectedMode)
            {
                _selectedShape = null;

                for (int i = _shapes.Count - 1; i >= 0; i--)
                {
                    if(IsPointInShape(mousePos, _shapes[i]))
                    {
                        _selectedShape =  _shapes[i];
                        ThicknessSlider.Value = _selectedShape.StrokeThickness;

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
            if (_isDrawingActive && _currentShape != null)
            {
                int lastIndex = _currentShape.Points.Count - 1;
                _currentShape.Points[lastIndex] = e.GetPosition(MainCanvas);
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



        //

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

            // Логика для Многоугольника (Центр + Радиус)
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
            else // Для обычных фигур
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


            Rectangle rect = new Rectangle
            {
                Width = maxX - minX + 10,
                Height = maxY - minY + 10,
                Stroke = Brushes.DeepSkyBlue,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection() { 4, 2 }
            };
            Canvas.SetLeft(rect, minX - 5);
            Canvas.SetTop(rect, minY - 5);
            MainCanvas.Children.Add(rect);
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
    }
}