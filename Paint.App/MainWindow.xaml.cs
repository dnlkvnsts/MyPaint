using Paint.Core;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Paint.App
{
    public partial class MainWindow : Window
    {
        private List<IShape> _shapes = new List<IShape>();
        private IShape _currentShape;
        private bool _isDrawingActive = false;
        

        // Храним выбранный плагин
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
            // Путь к папке Plugins в папке с программой
            string pluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

            if (!Directory.Exists(pluginsDirectory))
                Directory.CreateDirectory(pluginsDirectory);

            // Ищем все файлы .dll
            string[] dllFiles = Directory.GetFiles(pluginsDirectory, "*.dll");

            foreach (string dllPath in dllFiles)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(dllPath);
                    // Ищем классы, которые реализуют IPlugin
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
                // Можно добавить визуальное выделение нажатой кнопки
            };

            PluginsPanel.Children.Add(btn);
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            if (_selectedPlugin == null)
            {
                MessageBox.Show("Сначала выберите фигуру на панели инструментов!");
                return;
            }

            Point mousePos = e.GetPosition(MainCanvas);
            _isDrawingActive = true;

            // ИСПОЛЬЗУЕМ ПЛАГИН вместо "new MyLine"
            _currentShape = _selectedPlugin.CreateInstance();
            _currentShape.StrokeColor = Brushes.Black; // Потом добавишь выбор цвета
            _currentShape.StrokeThickness = 2;
            _currentShape.Points = new List<Point> { mousePos, mousePos };

            MainCanvas.CaptureMouse();
            Redraw();
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawingActive && _currentShape != null)
            {
                _currentShape.Points[1] = e.GetPosition(MainCanvas);
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

            if (_isDrawingActive && _currentShape != null)
            {
                _currentShape.Points[1] = e.GetPosition(MainCanvas);

                _shapes.Add(_currentShape);
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