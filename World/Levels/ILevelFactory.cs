namespace ForestQuest.World.Levels
{
    public interface ILevelFactory
    {
        LevelData Create(int level);
    }
}