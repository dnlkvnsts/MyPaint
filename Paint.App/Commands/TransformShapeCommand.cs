using Paint.Core;

using System.Windows;


namespace Paint.App.Commands
{
    public class TransformShapeCommand : Paint.Core.ICommand
    {
        private readonly List<(IShape shape, List<Point> oldPts, double oldAng, List<Point> newPts, double newAng)> _changes;
        private readonly Action _redraw;

        public TransformShapeCommand(List<(IShape, List<Point>, double, List<Point>, double)> changes, Action redraw)
        {
            _changes = changes;
            _redraw = redraw;
        }

        public void Execute()
        {
            foreach (var c in _changes)
            {
                c.shape.Points = new List<Point>(c.newPts);
                c.shape.Angle = c.newAng;
            }
            _redraw();
        }

        public void Unexecute()
        {
            foreach (var c in _changes)
            {
                c.shape.Points = new List<Point>(c.oldPts);
                c.shape.Angle = c.oldAng;
            }
            _redraw();
        }
    }
}
