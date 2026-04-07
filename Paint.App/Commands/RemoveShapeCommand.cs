using Paint.App.Models;
using Paint.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Paint.App.Commands
{
    public class RemoveShapeCommand : Paint.Core.ICommand
    {

        private readonly List<(Layer layer, IShape shape)> _removedShapes;
        private readonly Action _redraw;

        public RemoveShapeCommand(List<(Layer, IShape)> shapes, Action redraw)
        {
            _removedShapes = shapes;
            _redraw = redraw;
        }

        public void Execute()
        {
            foreach (var item in _removedShapes)
                item.layer.Shapes.Remove(item.shape);
            _redraw();
        }

        public void Unexecute()
        {
            foreach (var item in _removedShapes)
                item.layer.Shapes.Add(item.shape);
            _redraw();
        }

    }
}
