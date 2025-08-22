using ForestQuest.Entities.Player;

namespace ForestQuest.Input
{
    public sealed class GameInputContext
    {
        public Player Player1 { get; }
        public Player Player2 { get; }
        public bool IsMultiplayer { get; }

        private readonly System.Action<bool> _setP1Interact;
        private readonly System.Action<bool> _setP2Interact;

        public GameInputContext(Player p1,
                                Player p2,
                                bool isMultiplayer,
                                System.Action<bool> setP1Interact,
                                System.Action<bool> setP2Interact)
        {
            Player1 = p1;
            Player2 = p2;
            IsMultiplayer = isMultiplayer;
            _setP1Interact = setP1Interact;
            _setP2Interact = setP2Interact;
        }

        public void SetP1Interact(bool pressed) => _setP1Interact?.Invoke(pressed);
        public void SetP2Interact(bool pressed) { if (IsMultiplayer) _setP2Interact?.Invoke(pressed); }
    }
}