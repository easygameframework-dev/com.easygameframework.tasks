using System;
using System.Collections.Generic;
using System.Net;
using Cysharp.Threading.Tasks;
using EasyGameFramework.Core.Event;
using EasyGameFramework.Core.Network;

namespace EasyGameFramework.Tasks
{
    public static class NetworkExtensions
    {
        class ChannelContext
        {
            public INetworkChannel Channel { get; }
            public UniTaskCompletionSource ConnectedTcs;

            public readonly Dictionary<Type, UniTaskCompletionSource<object>> MessageReceivedTcsByMessageType =
                new Dictionary<Type, UniTaskCompletionSource<object>>();

            public ChannelContext(INetworkChannel channel)
            {
                Channel = channel;
            }

            public void FailAllPendingReceives(Exception exception)
            {
                foreach (var tcs in MessageReceivedTcsByMessageType.Values)
                {
                    tcs.TrySetException(exception);
                }

                MessageReceivedTcsByMessageType.Clear();
            }
        }

        private static readonly Dictionary<INetworkChannel, ChannelContext> ChannelContexts =
            new Dictionary<INetworkChannel, ChannelContext>();

        static NetworkExtensions()
        {
            var eventComponent = GameEntry.GetComponent<EventComponent>();
            eventComponent.Subscribe<NetworkConnectedEventArgs>(OnEvent);
            eventComponent.Subscribe<NetworkErrorEventArgs>(OnEvent);
            eventComponent.Subscribe<NetworkMessageEventArgs>(OnEvent);
        }

        private static ChannelContext GetChannelContext(INetworkChannel channel)
        {
            if (ChannelContexts.TryGetValue(channel, out var context))
            {
                return context;
            }

            context = new ChannelContext(channel);
            ChannelContexts.Add(channel, context);
            return context;
        }

        public static UniTask ConnectAsync(this INetworkChannel channel,
            IPAddress ipAddress,
            int port)
        {
            var context = GetChannelContext(channel);

            if (context.ConnectedTcs != null)
            {
                return context.ConnectedTcs.Task;
            }

            context.ConnectedTcs = new UniTaskCompletionSource();
            channel.Connect(ipAddress, port);
            return context.ConnectedTcs.Task;
        }

        public static async UniTask<T> ReceiveAsync<T>(this INetworkChannel channel)
        {
            return (T)await ReceiveAsync(channel, typeof(T));
        }

        public static UniTask<object> ReceiveAsync(this INetworkChannel channel, Type messageType)
        {
            var context = GetChannelContext(channel);
            if (context.MessageReceivedTcsByMessageType.TryGetValue(messageType, out var messageReceivedTcs))
            {
                return messageReceivedTcs.Task;
            }

            messageReceivedTcs = new UniTaskCompletionSource<object>();
            context.MessageReceivedTcsByMessageType.Add(messageType, messageReceivedTcs);
            return messageReceivedTcs.Task;
        }

        private static void OnEvent(object sender, NetworkConnectedEventArgs e)
        {
            if (ChannelContexts.Remove(e.NetworkChannel, out var context))
            {
                context.ConnectedTcs?.TrySetResult();
            }
        }

        private static void OnEvent(object sender, NetworkMessageEventArgs e)
        {
            var context = GetChannelContext(e.NetworkChannel);
            if (context.MessageReceivedTcsByMessageType.Remove(e.Message.GetType(), out var messageReceivedTcs))
            {
                messageReceivedTcs.TrySetResult(e.Message);
            }
        }

        private static void OnEvent(object sender, NetworkErrorEventArgs e)
        {
            switch (e.ErrorCode)
            {
                case NetworkErrorCode.Unknown:
                    break;
                case NetworkErrorCode.AddressFamilyError:
                    break;
                case NetworkErrorCode.SocketError:
                    break;
                case NetworkErrorCode.ConnectError:
                    if (ChannelContexts.Remove(e.NetworkChannel, out var context))
                    {
                        context.ConnectedTcs?.TrySetException(new Exception(e.ErrorMessage));
                    }

                    break;
                case NetworkErrorCode.SendError:
                    break;
                case NetworkErrorCode.ReceiveError:
                    break;
                case NetworkErrorCode.SerializeError:
                    break;
                case NetworkErrorCode.DeserializePacketHeaderError:
                    break;
                case NetworkErrorCode.DeserializePacketError:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            {
                // In the current design, we can only fail all pending receive tasks
                // associated with the corresponding network channel
                // when any network exception occurs.
                // Because we have no way to determine which specific message caused the exception.
                if (ChannelContexts.TryGetValue(e.NetworkChannel, out var context))
                {
                    context.FailAllPendingReceives(new Exception(e.ErrorMessage));
                }
            }
        }
    }
}