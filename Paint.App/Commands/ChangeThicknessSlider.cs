using Paint.Core;


namespace Paint.App.Commands
{
    public class ChangeThicknessCommand : Paint.Core.ICommand
    {
        private List<(IShape shape, double oldThickness)> _oldStates = new();
        private double _newThickness;
        private Action _redraw;

        public ChangeThicknessCommand(List<IShape> shapes, double newThickness, Action redraw)
        {
            _newThickness = newThickness;
            _redraw = redraw;

            foreach (var s in shapes)
            {
                _oldStates.Add((s, s.StrokeThickness));
            }
        }

        public void Execute()
        {
            foreach (var state in _oldStates)
            {
                state.shape.StrokeThickness = _newThickness;
            }
            _redraw();
        }

        public void Unexecute()
        {
            foreach (var state in _oldStates)
            {
                state.shape.StrokeThickness = state.oldThickness;
            }
            _redraw();
        }
    }
}
