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


        public MainWindow()
        {
            InitializeComponent();
            this.MouseUp += MainWindow_MouseUp;
            
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPlugins();
        }

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
               

                if(plugin.Name == "Многоугольник")
                {
                    _currentSideCount = CountSideOfPolygon();
                }
            };

            PluginsPanel.Children.Add(btn);
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            if (_selectedPlugin == null) return;

            Point mousePos = e.GetPosition(MainCanvas);
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
            }

            if (_currentShape != null)
            {
                _currentShape.Draw(MainCanvas);
            }
        }

    
    }
}