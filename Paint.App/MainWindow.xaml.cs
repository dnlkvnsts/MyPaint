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


        private List<IShape> _selectedShapes = new List<IShape>();
        private bool _isSelectingWithBox = false; // Флаг выделения рамкой
        private Point _selectionBoxStart; // Начало рамки
        private Rectangle _selectionBoxVisual; // Визуальный прямоугольник рамки
        private List<IShape> _clipboardShapes = new List<IShape>();

        private bool _isDrawingActive = false;
        private int? _currentSideCount = 5;
        
        private IPlugin _selectedPlugin;
      
        private bool _isSelectedMode = false;
        

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
            // --- КОПИРОВАНИЕ (Ctrl + C) ---
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                if (_selectedShapes.Count > 0)
                {
                    _clipboardShapes.Clear();
                    foreach (var shape in _selectedShapes)
                    {
                        _clipboardShapes.Add(shape.Clone());
                    }
                }
            }

            // --- ВСТАВКА (Ctrl + V) ---
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                if (_clipboardShapes.Count > 0)
                {
                    _selectedShapes.Clear(); // Выделяем только новые вставленные фигуры

                    List<IShape> newClonesForClipboard = new List<IShape>();

                    foreach (var shapeToPaste in _clipboardShapes)
                    {
                        IShape pastedShape = shapeToPaste.Clone();

                        // Смещаем точки, чтобы вставка не была точно поверх оригинала
                        for (int i = 0; i < pastedShape.Points.Count; i++)
                        {
                            pastedShape.Points[i] = new Point(
                                pastedShape.Points[i].X + 15,
                                pastedShape.Points[i].Y + 15);
                        }

                        _shapes.Add(pastedShape);
                        _selectedShapes.Add(pastedShape);

                        // Подготавливаем копию для следующей возможной вставки (с накопленным смещением)
                        newClonesForClipboard.Add(pastedShape.Clone());
                    }

                    _clipboardShapes = newClonesForClipboard;
                    Redraw();
                }
            }

            // --- УДАЛЕНИЕ (Delete) ---
            if (e.Key == Key.Delete)
            {
                if (_selectedShapes.Count > 0)
                {
                    foreach (var shape in _selectedShapes)
                    {
                        _shapes.Remove(shape);
                    }
                    _selectedShapes.Clear();
                    Redraw();
                }
            }

            // --- ПЕРЕМЕЩЕНИЕ СТРЕЛКАМИ ---
            if (_selectedShapes.Count > 0)
            {
                double step = Keyboard.IsKeyDown(Key.LeftShift) ? 1 : 5; // С шифтом ходим по 1 пикселю
                double dx = 0, dy = 0;

                if (e.Key == Key.Left) dx = -step;
                if (e.Key == Key.Right) dx = step;
                if (e.Key == Key.Up) dy = -step;
                if (e.Key == Key.Down) dy = step;

                if (dx != 0 || dy != 0)
                {
                    foreach (var shape in _selectedShapes)
                    {
                        for (int i = 0; i < shape.Points.Count; i++)
                        {
                            shape.Points[i] = new Point(
                                shape.Points[i].X + dx,
                                shape.Points[i].Y + dy);
                        }
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
                // 1. Устанавливаем выбранный плагин
                _selectedPlugin = plugin;

                // 2. Выходим из режима выделения
                _isSelectedMode = false;

                // 3. ОЧИЩАЕМ СПИСОК ВЫДЕЛЕННЫХ ФИГУР
                _selectedShapes.Clear();

                // 4. Специфическая логика для многоугольника
                if (plugin.Name == "Многоугольник")
                {
                    _currentSideCount = CountSideOfPolygon();
                }

                // 5. Перерисовываем канвас, чтобы убрать рамки выделения
                Redraw();
            };

            PluginsPanel.Children.Add(btn);
        }


        //work with mouse


        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            Point mousePos = e.GetPosition(MainCanvas);

            if (_isSelectedMode)
            {
                // 1. ПРОВЕРКА ХЭНДЛОВ (Изменение размера / Поворот)
                var primaryShape = _selectedShapes.LastOrDefault();
                if (primaryShape != null)
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

                        double minX = primaryShape.Points.Min(p => p.X);
                        double maxX = primaryShape.Points.Max(p => p.X);
                        double minY = primaryShape.Points.Min(p => p.Y);
                        double maxY = primaryShape.Points.Max(p => p.Y);
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

                // 2. ПОИСК ФИГУРЫ ПОД МЫШЬЮ
                IShape clickedShape = null;
                for (int i = _shapes.Count - 1; i >= 0; i--)
                {
                    if (IsPointInShape(mousePos, _shapes[i]))
                    {
                        clickedShape = _shapes[i];
                        break;
                    }
                }

                bool isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

                if (clickedShape != null)
                {
                    if (isShiftPressed)
                    {
                        if (_selectedShapes.Contains(clickedShape))
                            _selectedShapes.Remove(clickedShape);
                        else
                            _selectedShapes.Add(clickedShape);
                    }
                    else
                    {
                        if (!_selectedShapes.Contains(clickedShape))
                        {
                            _selectedShapes.Clear();
                            _selectedShapes.Add(clickedShape);
                        }
                    }

                    _isDragging = true;
                    _lastMousePosition = mousePos;
                    MainCanvas.CaptureMouse();
                }
                else
                {
                    // 3. ВЫДЕЛЕНИЕ РАМКОЙ
                    if (!isShiftPressed) _selectedShapes.Clear();

                    _isSelectingWithBox = true;
                    _selectionBoxStart = mousePos;

                    if (_selectionBoxVisual == null)
                    {
                        _selectionBoxVisual = new Rectangle
                        {
                            Stroke = Brushes.DeepSkyBlue,
                            StrokeDashArray = new DoubleCollection { 2, 2 },
                            Fill = new SolidColorBrush(Color.FromArgb(30, 0, 191, 255)),
                            IsHitTestVisible = false
                        };
                    }
                    MainCanvas.Children.Add(_selectionBoxVisual);
                    MainCanvas.CaptureMouse();
                }
                Redraw();
                return;
            }

            // 4. ЛОГИКА РИСОВАНИЯ
            if (_selectedPlugin == null) return;

            // --- НОВОЕ: Обработка Ломаной ---
            if (_selectedPlugin.Name == "Ломаная")
            {
                if (!_isDrawingActive)
                {
                    // Начинаем новую ломаную
                    _isDrawingActive = true;
                    _currentShape = _selectedPlugin.CreateInstance();
                    ApplyCurrentSettings(_currentShape); // Применяем цвет/толщину
                    _currentShape.Points = new List<Point> { mousePos, mousePos };
                }
                else
                {
                    // Добавляем следующую точку в существующую ломаную
                    _currentShape.Points.Add(mousePos);
                }
                // Мы НЕ вызываем CaptureMouse для ломаной, чтобы MouseUp не прерывал процесс
            }
            // --- Обычные фигуры (Прямоугольник, Эллипс и т.д.) ---
            else
            {
                _isDrawingActive = true;
                _currentShape = _selectedPlugin.CreateInstance();
                ApplyCurrentSettings(_currentShape);
                _currentShape.Points = new List<Point> { mousePos, mousePos };

                MainCanvas.CaptureMouse();
            }

            Redraw();
        }

        // Вспомогательный метод, чтобы не дублировать код настройки новой фигуры
        private void ApplyCurrentSettings(IShape shape)
        {
            var sideCountProp = shape.GetType().GetProperty("SideCount");
            if (sideCountProp != null) sideCountProp.SetValue(shape, _currentSideCount);

            if (FillColorPicker.SelectedItem is ComboBoxItem fill) shape.FillColor = (Brush)fill.Tag;
            if (StrokeColorPicker.SelectedItem is ComboBoxItem stroke) shape.StrokeColor = (Brush)stroke.Tag;
            shape.StrokeThickness = ThicknessSlider.Value;
        }





        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPos = e.GetPosition(MainCanvas);

            // 1. ВРАЩЕНИЕ ГРУППЫ
            if (_isRotating && _selectedShapes.Count > 0)
            {
                // Берем последнюю выбранную фигуру как эталон для центра вращения 
                // или вычисляем общий центр (здесь для простоты - по последней)
                var primary = _selectedShapes.Last();
                double minX = primary.Points.Min(p => p.X);
                double maxX = primary.Points.Max(p => p.X);
                double minY = primary.Points.Min(p => p.Y);
                double maxY = primary.Points.Max(p => p.Y);
                Point center = new Point((minX + maxX) / 2, (minY + maxY) / 2);

                double angleRad = Math.Atan2(currentPos.Y - center.Y, currentPos.X - center.X);
                double angleDeg = angleRad * (180 / Math.PI);

                foreach (var shape in _selectedShapes)
                {
                    shape.Angle = angleDeg + 90;
                }

                Redraw();
                return;
            }

            // 2. ИЗМЕНЕНИЕ РАЗМЕРА ГРУППЫ
            else if (_isResizing && _selectedShapes.Count > 0)
            {
                double oldDx = _lastMousePosition.X - _resizeAnchor.X;
                double oldDy = _lastMousePosition.Y - _resizeAnchor.Y;
                double newDx = currentPos.X - _resizeAnchor.X;
                double newDy = currentPos.Y - _resizeAnchor.Y;

                if (Math.Abs(oldDx) < 0.5) oldDx = 0.5;
                if (Math.Abs(oldDy) < 0.5) oldDy = 0.5;

                double scaleX = (_activeHandle.Contains("E") || _activeHandle.Contains("W")) ? newDx / oldDx : 1.0;
                double scaleY = (_activeHandle.Contains("N") || _activeHandle.Contains("S")) ? newDy / oldDy : 1.0;

                foreach (var shape in _selectedShapes)
                {
                    // Если это правильный многоугольник (2 точки: центр и радиус), используем унифицированное масштабирование
                    if (shape.GetType().Name.Contains("Polygon") && shape.Points.Count == 2)
                    {
                        double uniformScale = (_activeHandle.Length > 1)
                            ? (Math.Abs(scaleX) + Math.Abs(scaleY)) / 2 * Math.Sign(scaleX + scaleY)
                            : (scaleX != 1.0 ? scaleX : scaleY);

                        for (int i = 0; i < shape.Points.Count; i++)
                        {
                            Point p = shape.Points[i];
                            shape.Points[i] = new Point(
                                _resizeAnchor.X + (p.X - _resizeAnchor.X) * uniformScale,
                                _resizeAnchor.Y + (p.Y - _resizeAnchor.Y) * uniformScale);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < shape.Points.Count; i++)
                        {
                            Point p = shape.Points[i];
                            shape.Points[i] = new Point(
                                _resizeAnchor.X + (p.X - _resizeAnchor.X) * scaleX,
                                _resizeAnchor.Y + (p.Y - _resizeAnchor.Y) * scaleY);
                        }
                    }
                }

                _lastMousePosition = currentPos;
                Redraw();
            }

            // 3. ПЕРЕМЕЩЕНИЕ ГРУППЫ
            else if (_isDragging && _selectedShapes.Count > 0)
            {
                double dx = currentPos.X - _lastMousePosition.X;
                double dy = currentPos.Y - _lastMousePosition.Y;

                foreach (var shape in _selectedShapes)
                {
                    for (int i = 0; i < shape.Points.Count; i++)
                    {
                        shape.Points[i] = new Point(
                            shape.Points[i].X + dx,
                            shape.Points[i].Y + dy);
                    }
                }

                _lastMousePosition = currentPos;
                Redraw();
            }

            // 4. ВИЗУАЛИЗАЦИЯ РАМКИ ВЫДЕЛЕНИЯ
            else if (_isSelectingWithBox && _selectionBoxVisual != null)
            {
                double x = Math.Min(_selectionBoxStart.X, currentPos.X);
                double y = Math.Min(_selectionBoxStart.Y, currentPos.Y);
                double w = Math.Abs(_selectionBoxStart.X - currentPos.X);
                double h = Math.Abs(_selectionBoxStart.Y - currentPos.Y);

                Canvas.SetLeft(_selectionBoxVisual, x);
                Canvas.SetTop(_selectionBoxVisual, y);
                _selectionBoxVisual.Width = w;
                _selectionBoxVisual.Height = h;
            }

            // 5. РИСОВАНИЕ НОВОЙ ФИГУРЫ
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
            // 1. ОБРАБОТКА ЗАВЕРШЕНИЯ ВЫДЕЛЕНИЯ РАМКОЙ
            if (_isSelectingWithBox)
            {
                Point endPos = e.GetPosition(MainCanvas);

                // Создаем прямоугольник Rect на основе начальной и конечной точек
                double x = Math.Min(_selectionBoxStart.X, endPos.X);
                double y = Math.Min(_selectionBoxStart.Y, endPos.Y);
                double width = Math.Abs(_selectionBoxStart.X - endPos.X);
                double height = Math.Abs(_selectionBoxStart.Y - endPos.Y);
                Rect selectionRect = new Rect(x, y, width, height);

                // Проверяем каждую фигуру: если хоть одна точка попала в рамку - добавляем в список
                foreach (var shape in _shapes)
                {
                    if (shape.Points.Any(p => selectionRect.Contains(p)))
                    {
                        if (!_selectedShapes.Contains(shape))
                        {
                            _selectedShapes.Add(shape);
                        }
                    }
                }

                // Удаляем визуальную рамку с холста
                if (_selectionBoxVisual != null)
                {
                    MainCanvas.Children.Remove(_selectionBoxVisual);
                }
                _isSelectingWithBox = false;
            }

            // 2. СБРОС ФЛАГОВ ТРАНСФОРМАЦИИ
            _isDragging = false;
            _isRotating = false;
            _isResizing = false;
            _activeHandle = "";

            MainCanvas.ReleaseMouseCapture();

            // 3. ЗАВЕРШЕНИЕ РИСОВАНИЯ НОВОЙ ФИГУРЫ
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_isDrawingActive && _currentShape != null)
                {
                    // Если мы рисовали не ломаную, то добавляем фигуру в список
                    if (_selectedPlugin != null && _selectedPlugin.Name != "Ломаная")
                    {
                        _shapes.Add(_currentShape);

                        // Автоматически выделяем только что созданную фигуру
                        _selectedShapes.Clear();
                        _selectedShapes.Add(_currentShape);

                        _currentShape = null;
                        _isDrawingActive = false;
                    }
                }
            }

            Redraw();
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

                if (_selectedShapes.Contains(shape))
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
            _selectedShapes.Clear(); // Очищаем весь список выделенных фигур
            Redraw();
        }

        private void StrokeColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Если есть выделенные фигуры и выбран элемент в комбобоксе
            if (_selectedShapes.Count > 0 && StrokeColorPicker.SelectedItem is ComboBoxItem item)
            {
                foreach (var shape in _selectedShapes)
                {
                    shape.StrokeColor = (Brush)item.Tag;
                }
                Redraw();
            }
        }

        private void FillColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedShapes.Count > 0 && FillColorPicker.SelectedItem is ComboBoxItem item)
            {
                foreach (var shape in _selectedShapes)
                {
                    shape.FillColor = (Brush)item.Tag;
                }
                Redraw();
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

            AddHandle(midX - 4, minY - 40, "ROT"); 

           
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
            if (_selectedShapes != null && _selectedShapes.Count > 0)
            {
                foreach (var shape in _selectedShapes)
                {
                    shape.StrokeThickness = ThicknessSlider.Value;
                }
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