namespace ForestQuest.Input.Commands
{
    public sealed class PlayerInteractCommand : ICommand
    {
        private readonly GameInputContext _ctx;
        private readonly int _playerIndex;
        private readonly bool _pressed;

        public PlayerInteractCommand(GameInputContext ctx, int playerIndex, bool pressed)
        {
            _ctx = ctx;
            _playerIndex = playerIndex;
            _pressed = pressed;
        }

        public void Execute()
        {
            if (_playerIndex == 1) _ctx.SetP1Interact(_pressed);
            else _ctx.SetP2Interact(_pressed);
        }
    }
}