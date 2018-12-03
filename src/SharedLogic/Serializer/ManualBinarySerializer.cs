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
                    case AssemblyResolveAnswer msg:
                        stream.WriteByte( 1 );
                        stream.WriteInt( msg.Number );
                        stream.WriteBool( msg.Handled );
                        stream.WriteString( msg.Location );
                        stream.WriteInt( msg.AnswerTo );
                        break;
                    case AssemblyResolveMessage msg:
                        stream.WriteByte( 2 );
                        stream.WriteInt( msg.Number );
                        stream.WriteString( msg.Name );
                        stream.WriteString( msg.RequestingAssemblyFullName );
                        break;
                    case CreateObjectOfTypeCommand msg:
                        stream.WriteByte( 3 );
                        stream.WriteInt( msg.Number );
                        stream.WriteString( msg.AssemblyPath );
                        stream.WriteString( msg.TypeFullName );
                        break;

                    case EventInvokeCommand msg:
                        stream.WriteByte( 4 );
                        stream.WriteInt( msg.Number );
                        stream.WriteString( msg.EventName );
                        stream.WriteObject( _formatter, msg.Arguments );

                        break;
                    case MethodCallCommand msg:
                        stream.WriteByte( 5 );
                        stream.WriteInt( msg.Number );
                        stream.WriteString( msg.MethodName );
                        stream.WriteString( msg.MethodId );
                        stream.WriteObject( _formatter, msg.Arguments );

                        break;
                    case MethodCallResultAnswer msg:
                        stream.WriteByte( 6 );
                        stream.WriteInt( msg.Number );
                        stream.WriteInt( msg.AnswerTo );
                        stream.WriteObject( _formatter, msg.Result );
                        stream.WriteObject( _formatter, msg.Exception );
                        break;
                    case SubscribeToEventCommand msg:
                        stream.WriteByte( 7 );
                        stream.WriteInt( msg.Number );
                        stream.WriteString( msg.EventName );
                        break;
                    case UnexpectedExceptionMessage msg:
                        stream.WriteByte( 8 );
                        stream.WriteInt( msg.Number );
                        stream.WriteObject( _formatter, msg.Exception );
                        break;
                    case UnsubscribeFromEventCommand msg:
                        stream.WriteByte( 9 );
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
                        var assemblyResolveAnswer = new AssemblyResolveAnswer( stream.ReadInt() );
                        assemblyResolveAnswer.Handled = stream.ReadBool();
                        assemblyResolveAnswer.Location = stream.ReadString();
                        assemblyResolveAnswer.AnswerTo = stream.ReadInt();
                        return assemblyResolveAnswer;

                    case 2:
                        var assemblyResolveMessage = new AssemblyResolveMessage( stream.ReadInt() );
                        assemblyResolveMessage.Name = stream.ReadString();
                        assemblyResolveMessage.RequestingAssemblyFullName = stream.ReadString();
                        return assemblyResolveMessage;
                    case 3:
                        var createObjectOfTypeCommand = new CreateObjectOfTypeCommand( stream.ReadInt() );
                        createObjectOfTypeCommand.AssemblyPath = stream.ReadString();
                        createObjectOfTypeCommand.TypeFullName = stream.ReadString();
                        return createObjectOfTypeCommand;

                    case 4:
                        var invokeCommand = new EventInvokeCommand( stream.ReadInt() );
                        invokeCommand.EventName = stream.ReadString();
                        invokeCommand.Arguments = ( object[] ) stream.ReadObject( _formatter );
                        return invokeCommand;
                    case 5:
                        var methodCallCommand = new MethodCallCommand( stream.ReadInt() );
                        methodCallCommand.MethodName = stream.ReadString();
                        methodCallCommand.MethodId = stream.ReadString();
                        methodCallCommand.Arguments = ( object[] ) stream.ReadObject( _formatter );
                        return methodCallCommand;

                    case 6:
                        var methodCallResultAnswer = new MethodCallResultAnswer( stream.ReadInt() );
                        methodCallResultAnswer.AnswerTo = stream.ReadInt();
                        methodCallResultAnswer.Result = stream.ReadObject( _formatter );
                        methodCallResultAnswer.Exception = ( Exception ) stream.ReadObject( _formatter );
                        return methodCallResultAnswer;

                    case 7:
                        var subscribeToEventCommand = new SubscribeToEventCommand( stream.ReadInt() );
                        subscribeToEventCommand.EventName = stream.ReadString();
                        return subscribeToEventCommand;
                    case 8:
                        return new UnexpectedExceptionMessage( stream.ReadInt() ) { Exception = ( Exception ) stream.ReadObject( _formatter ) };
                    case 9:
                        return new UnsubscribeFromEventCommand( stream.ReadInt() ) { EventName = stream.ReadString() };

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}