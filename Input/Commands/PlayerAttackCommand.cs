using ForestQuest.Entities.Player;

namespace ForestQuest.Input.Commands
{
    public sealed class PlayerAttackCommand : ICommand
    {
        private readonly GameInputContext _ctx;
        private readonly int _playerIndex;

        public PlayerAttackCommand(GameInputContext ctx, int playerIndex)
        {
            _ctx = ctx;
            _playerIndex = playerIndex;
        }

        public void Execute()
        {
            Player p = _playerIndex == 1 ? _ctx.Player1 : _ctx.Player2;
            if (p != null)
                p.StartAttack(); // Safe: StartAttack heeft cooldown/locks
        }
    }
}