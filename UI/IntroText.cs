using System.Text;

namespace ForestQuest.UI
{
    public static class IntroText
    {
        public static string Build(bool isMultiplayer)
        {
            const string sp =
                "Lina: Welcome to Forest Quest!" + "\n" +
                "Your mission: collect every coin and defeat all enemies to clear the level." + "\n" +
                "Enemies in this forest: Cats, Dogs, and the Shadow Wolf (boss)." + "\n" +
                "Controls (Player 1) - Move: Z/Q/S/D, Attack: Space, Interact: E." + "\n" +
                "Good luck!";

            const string mp =
                "Lina: Welcome to Forest Quest (Multiplayer)!" + "\n" +
                "Work together: collect every coin and defeat all enemies to clear the level." + "\n" +
                "Enemies in this forest: Cats, Dogs, and the Shadow Wolf (boss)." + "\n" +
                "Controls (Player 1) - Move: Z/Q/S/D, Attack: Space, Interact: E." + "\n" +
                "Controls (Player 2) - Move: Arrow Keys, Attack: K, Interact: L." + "\n" +
                "Good luck, heroes!";

            return isMultiplayer ? mp : sp;
        }
    }
}