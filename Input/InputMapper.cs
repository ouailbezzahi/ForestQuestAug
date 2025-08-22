using System.Collections.Generic;
using ForestQuest.Input.Commands;
using Microsoft.Xna.Framework.Input;

namespace ForestQuest.Input
{
    public sealed class InputMapper
    {
        public List<ICommand> Map(KeyboardState kb, GameInputContext ctx)
        {
            var cmds = new List<ICommand>();

            // Interact (coins)
            if (kb.IsKeyDown(ctx.Player1.Controls.Interact))
                cmds.Add(new PlayerInteractCommand(ctx, playerIndex: 1, pressed: true));
            if (ctx.IsMultiplayer && kb.IsKeyDown(ctx.Player2.Controls.Interact))
                cmds.Add(new PlayerInteractCommand(ctx, playerIndex: 2, pressed: true));

            // Optional: Attack via command (veilig; Player.Update roept dit ook aan)
            if (kb.IsKeyDown(ctx.Player1.Controls.Attack))
                cmds.Add(new PlayerAttackCommand(ctx, playerIndex: 1));
            if (ctx.IsMultiplayer && kb.IsKeyDown(ctx.Player2.Controls.Attack))
                cmds.Add(new PlayerAttackCommand(ctx, playerIndex: 2));

            return cmds;
        }
    }
}