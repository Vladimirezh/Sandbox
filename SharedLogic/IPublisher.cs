namespace Sandbox
{
    public interface IPublisher<in T>
    {
        void Publish(T message);
    }
}