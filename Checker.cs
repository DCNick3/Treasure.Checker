using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Treasure.GameLogic;
using Treasure.GameLogic.Message;
using Treasure.GameLogic.State;
using Treasure.GameLogic.State.Field;

namespace Treasure.Checker
{
    public class Checker
    {
        private readonly IProgramCommunicator _communicator;
        
        // single player only
        
        public Checker(GameFieldParams @params, IProgramCommunicator communicator) 
            : this(new GameFieldGenerator(@params, new Random()).Generate(), communicator)
        {
        }
        
        public Checker(GameField gameField, IProgramCommunicator communicator)
        {
            if (gameField.Parameters.PlayerCount != 1)
                throw new ArgumentException("Only one player games are supported");
            GameState = new GameState(gameField);
            _communicator = communicator;
        }
        
        public GameState GameState { get; private set; }

        private PlayerAction? TryParseAction(string action)
        {
            switch (action.ToLower())
            {
                case "выстрел вверх": return new PlayerAction(PlayerAction.ActionType.Shoot, Direction.Up);
                case "выстрел вправо": return new PlayerAction(PlayerAction.ActionType.Shoot, Direction.Right);
                case "выстрел вниз": return new PlayerAction(PlayerAction.ActionType.Shoot, Direction.Down);
                case "выстрел влево": return new PlayerAction(PlayerAction.ActionType.Shoot, Direction.Left);
                case "движение вверх": return new PlayerAction(PlayerAction.ActionType.Move, Direction.Up);
                case "движение вправо": return new PlayerAction(PlayerAction.ActionType.Move, Direction.Right);
                case "движение вниз": return new PlayerAction(PlayerAction.ActionType.Move, Direction.Down);
                case "движение влево": return new PlayerAction(PlayerAction.ActionType.Move, Direction.Left);
            }
            return null;
        }

        private string StringifyElement(PlayerMessageElement element)
        {
            var res = element.Type switch
            {
                PlayerMessageElement.ElementType.Field => "Поле",
                PlayerMessageElement.ElementType.River => "Река",
                PlayerMessageElement.ElementType.Swamp => "Болото",
                PlayerMessageElement.ElementType.Home => "Дом",
                PlayerMessageElement.ElementType.Portal => $"Портал {element.IntParam + 1}",
                PlayerMessageElement.ElementType.Wall => "Стена",
                PlayerMessageElement.ElementType.Grate => "Решётка",
                _ => throw new ArgumentOutOfRangeException()
            };

            if (element.WithTreasure)
                res += " с кладом";
            
            return res;
        }
        
        private string StringifyMessage(ImmutableArray<PlayerMessageElement> message)
        {
            return string.Join(" - ", message.Select(StringifyElement));
        }
        
        public async Task CheckAsync()
        {
            while (GameState.Winner == null)
            {
                var action = TryParseAction(await _communicator.ReadLineAsync());
                if (action == null)
                    throw new PresentationError();
                var (newGameState, message) = GameState.WithAppliedAction(action);
                await _communicator.WriteLineAsync(StringifyMessage(message));
                GameState = newGameState;
            }
        }

        public class PresentationError : Exception
        {
        }
    }
}