using Paint.App.Models;
using Paint.Core;

using System.Windows.Input;

namespace Paint.App.Commands
{
    public class AddShapeCommand : Paint.Core.ICommand
    {
        private readonly Layer _layer;
        private readonly IShape _shape;
        private readonly Action _redraw;

        public AddShapeCommand(Layer layer, IShape shape, Action redraw)
        {
            _layer = layer;
            _shape = shape;
            _redraw = redraw;
        }

        public void Execute()
        {
            _layer.Shapes.Add(_shape);
            _redraw();
        }

        public void Unexecute()
        {
            _layer.Shapes.Remove(_shape);
            _redraw();
        }
    }
}
