using Combat.Component.Ship;

namespace Combat.Ai
{
    public interface IController
    {
	    void Update(float deltaTime);
	    bool IsAlive { get; }
        bool ControllerChangeToAi { get; }
        bool ControllerChangeToPlayer { get; }
    }

    public interface IControllerFactory
    {
        IController Create(IShip ship);
    }
}
