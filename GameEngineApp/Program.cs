using System;

namespace GameEngineApp
{
    class Program
    {
        static void Main(string[] args)
        {
            GameEngine engine = GameEngine.GetInstance();

            engine.AddGameObject(new GameObjects.Player("Player"))
                  .AddGameObject(new GameObjects.Panel("Panel"))
                  .AddGameObject(new GameObjects.Root("Root"))
                  .Run();
        }
    }
}