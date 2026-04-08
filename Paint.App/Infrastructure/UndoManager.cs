using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint.App.Infrastructure
{
    public class UndoManager
    {
       
        private readonly Stack<Paint.Core.ICommand> _undoStack = new Stack<Paint.Core.ICommand>(); // ВЫПОЛНЕННЫЙ КОМАНДЫ

        
        private readonly Stack<Paint.Core.ICommand> _redoStack = new Stack<Paint.Core.ICommand>(); //ОТМЕНЕНННЫЕ КОМАНДЫ

        
        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        
        public void Execute(Paint.Core.ICommand command) // выполнение какого-то действия (удаляем redostack, потому что создаем новое действие и если мы перед этим что-то отменили,а потом создали то эти отмененные действия стираются)) 
        {
           
            command.Execute();

           
            _undoStack.Push(command);

           
            _redoStack.Clear();
        }

       
        public void Undo() // отменяем действие возвращаем все как было 
        {
            if (_undoStack.Count > 0)
            {
              
                var command = _undoStack.Pop();

               
                command.Unexecute();

               
                _redoStack.Push(command);
            }
        }

        
        public void Redo() //идем вперед 
        {
            if (_redoStack.Count > 0)
            {
                
                var command = _redoStack.Pop();

               
                command.Execute();

               
                _undoStack.Push(command);
            }
        }

       
        public void ClearHistory()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
