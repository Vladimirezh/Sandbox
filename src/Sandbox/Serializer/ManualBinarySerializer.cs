using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Sandbox.Commands;
using Sandbox.Common;

namespace Sandbox.Serializer
{
    public sealed class ManualBinarySerializer : ISerializer
    {
        private readonly BinaryFormatter _formatter = new BinaryFormatter();

        public byte[] Serialize( Message message )
        {
            using ( var stream = new MemoryStream() )
            {
                switch ( message )
                {
                    case CreateObjectOfTypeCommand msg:
                        stream.WriteByte( 1 );
                        stream.WriteInt( msg.Number );
                        stream.WriteString( msg.AssemblyPath );
                        stream.WriteString( msg.TypeFullName );
                        break;

                    case EventInvokeCommand msg:
                        stream.WriteByte( 2 );
                        stream.WriteInt( msg.Number );
                        stream.WriteString( msg.EventName );
                        stream.WriteObject( _formatter, msg.Arguments );

                        break;
                    case MethodCallCommand msg:
                        stream.WriteByte( 3 );
                        stream.WriteInt( msg.Number );
                        stream.WriteString( msg.MethodName );
                        stream.WriteString( msg.MethodId );
                        stream.WriteObject( _formatter, msg.Arguments );

                        break;
                    case MethodCallResultAnswer msg:
                        stream.WriteByte( 4 );
                        stream.WriteInt( msg.Number );
                        stream.WriteInt( msg.AnswerTo );
                        stream.WriteObject( _formatter, msg.Result );
                        stream.WriteObject( _formatter, msg.Exception );
                        break;
                    case SubscribeToEventCommand msg:
                        stream.WriteByte( 5 );
                        stream.WriteInt( msg.Number );
                        stream.WriteString( msg.EventName );
                        break;
                    case UnexpectedExceptionMessage msg:
                        stream.WriteByte( 6 );
                        stream.WriteInt( msg.Number );
                        stream.WriteObject( _formatter, msg.Exception );
                        break;
                    case UnsubscribeFromEventCommand msg:
                        stream.WriteByte( 7 );
                        stream.WriteInt( msg.Number );
                        stream.WriteString( msg.EventName );
                        break;
                    default:
                        throw new NotImplementedException();
                }

                return stream.ToArray();
            }
        }

        public Message Deserialize( byte[] bytes )
        {
            using ( var stream = new MemoryStream( bytes ) )
            {
                switch ( stream.ReadByte() )
                {
                    case 1:
                        var createObjectOfTypeCommand = new CreateObjectOfTypeCommand( stream.ReadInt() );
                        createObjectOfTypeCommand.AssemblyPath = stream.ReadString();
                        createObjectOfTypeCommand.TypeFullName = stream.ReadString();
                        return createObjectOfTypeCommand;

                    case 2:
                        var invokeCommand = new EventInvokeCommand( stream.ReadInt() );
                        invokeCommand.EventName = stream.ReadString();
                        invokeCommand.Arguments = ( object[] ) stream.ReadObject( _formatter );
                        return invokeCommand;
                    case 3:
                        var methodCallCommand = new MethodCallCommand( stream.ReadInt() );
                        methodCallCommand.MethodName = stream.ReadString();
                        methodCallCommand.MethodId = stream.ReadString();
                        methodCallCommand.Arguments = ( object[] ) stream.ReadObject( _formatter );
                        return methodCallCommand;

                    case 4:
                        var methodCallResultAnswer = new MethodCallResultAnswer( stream.ReadInt() );
                        methodCallResultAnswer.AnswerTo = stream.ReadInt();
                        methodCallResultAnswer.Result = stream.ReadObject( _formatter );
                        methodCallResultAnswer.Exception = ( Exception ) stream.ReadObject( _formatter );
                        return methodCallResultAnswer;

                    case 5:
                        var subscribeToEventCommand = new SubscribeToEventCommand( stream.ReadInt() );
                        subscribeToEventCommand.EventName = stream.ReadString();
                        return subscribeToEventCommand;
                    case 6:
                        return new UnexpectedExceptionMessage( stream.ReadInt() ) { Exception = ( Exception ) stream.ReadObject( _formatter ) };
                    case 7:
                        return new UnsubscribeFromEventCommand( stream.ReadInt() ) { EventName = stream.ReadString() };

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}