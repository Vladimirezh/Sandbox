namespace Sandbox
{
    public interface INamedPipeStreamFactory
    {
        INamedPipeStream CreateStream( string address );
    }
}