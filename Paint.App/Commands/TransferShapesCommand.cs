using Paint.App.Models;
using Paint.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint.App.Commands
{
    public class TransferShapesCommand : Paint.Core.ICommand
{
    private readonly List<IShape> _shapes;
    private readonly Layer _targetLayer;
    private readonly Dictionary<IShape, Layer> _sourceMap = new Dictionary<IShape, Layer>();
    private readonly Action _redraw;

    public TransferShapesCommand(List<IShape> shapes, Layer target, IEnumerable<Layer> allLayers, Action redraw)
    {
        _shapes = shapes;
        _targetLayer = target;
        _redraw = redraw;

        // Находим настоящий "дом" для каждой фигуры
        foreach (var s in _shapes)
        {
            foreach (var layer in allLayers)
            {
                if (layer.Shapes.Contains(s))
                {
                    _sourceMap[s] = layer;
                    break;
                }
            }
        }
    }

    public void Execute()
    {
        foreach (var s in _shapes)
        {
            if (_sourceMap.TryGetValue(s, out var originalLayer))
                originalLayer.Shapes.Remove(s);
            
            _targetLayer.Shapes.Add(s);
        }
        _redraw();
    }

    public void Unexecute()
    {
        foreach (var s in _shapes)
        {
            _targetLayer.Shapes.Remove(s);
            if (_sourceMap.TryGetValue(s, out var originalLayer))
                originalLayer.Shapes.Add(s);
        }
        _redraw();
    }
}
}
