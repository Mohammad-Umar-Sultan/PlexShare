﻿using PlexShareContent.DataModels;
using PlexShareContent.Enums;
using PlexShareContent.Server;
using PlexShareContent;
using PlexShareNetwork;

namespace PlexShareTests.ContentTests.Server
{
    public class ContentServerNotificationHandlerTest
    {
        private FakeCommunicator communicator;
        private ContentServer contentServer;
        private FakeContentListener listener;
        private INotificationHandler notificationHandler;
        private IContentSerializer serializer;

        private int sleeptime;
        private Utility utils;

        public void Setup()
        {
            utils = new Utility();
            contentServer = ContentServerFactory.GetInstance() as ContentServer;
            contentServer.Reset();
            notificationHandler = new ContentServerNotificationHandler(contentServer);
            serializer = new ContentSerializer();
            listener = new FakeContentListener();
            communicator = new FakeCommunicator();
            contentServer.Communicator = communicator;
            contentServer.ServerSubscribe(listener);
            sleeptime = 50;
        }

        [Fact]
        public void OnDataReceived_ChatDataIsReceived_CallReceiveMethodOfContentDatabase()
        {
            Setup();
            var messageData = utils.GenerateContentData(data: "Hello");

            var serializedMessage = serializer.Serialize(messageData);

            notificationHandler.OnDataReceived(serializedMessage);

            Thread.Sleep(sleeptime);

            var notifiedMessage = listener.GetReceivedMessage();

            Assert.Equal("Hello", notifiedMessage.Data);
            Assert.Equal(messageData.Type, notifiedMessage.Type);
            Assert.Equal(messageData.Event, notifiedMessage.Event);
            Assert.Equal(messageData.SenderID, notifiedMessage.SenderID);
            Assert.Equal(messageData.Starred, notifiedMessage.Starred);
            Assert.Equal(messageData.ReceiverIDs, notifiedMessage.ReceiverIDs);

            var sentMessage = communicator.GetSentData();

            var deserializesSentMessage = serializer.Deserialize<ContentData>(sentMessage);

            Assert.Equal("Hello", deserializesSentMessage.Data);
            Assert.Equal(messageData.Type, deserializesSentMessage.Type);
            Assert.Equal(messageData.Event, deserializesSentMessage.Event);
            Assert.Equal(messageData.SenderID, deserializesSentMessage.SenderID);
            Assert.Equal(messageData.Starred, deserializesSentMessage.Starred);
            Assert.Equal(messageData.ReceiverIDs, deserializesSentMessage.ReceiverIDs);
            Assert.True(communicator.IsBroadcast());
        }

        [Fact]
        public void OnDataReceived_FileDataIsReceived_CallReceiveMethodOfContentDatabase()
        {
            Setup();
            var CurrentDirectory = Directory.GetCurrentDirectory();
            var path = CurrentDirectory.Split(new[] { "\\PlexShareTests" }, StringSplitOptions.None);
            var pathA = path[0] + "\\PlexShareTests\\ContentTests\\Test_File.pdf";

            var file = new ContentData
            {
                Data = "Test_File.pdf",
                Type = MessageType.File,
                FileData = new SendFileData(pathA),
                SenderID = 1,
                ReplyThreadID = -1,
                Event = MessageEvent.New,
                ReceiverIDs = new int[0]
            };

            var serializedMessage = serializer.Serialize(file);

            notificationHandler.OnDataReceived(serializedMessage);

            Thread.Sleep(sleeptime);

            var notifiedMessage = listener.GetReceivedMessage();
            Assert.Equal("Test_File.pdf", notifiedMessage.Data);
            Assert.Equal(file.Type, notifiedMessage.Type);
            Assert.Equal(file.Event, notifiedMessage.Event);
            Assert.Equal(file.SenderID, notifiedMessage.SenderID);
            Assert.Equal(file.Starred, notifiedMessage.Starred);
            Assert.Equal(file.ReceiverIDs, notifiedMessage.ReceiverIDs);

            var sentMessage = communicator.GetSentData();

            var deserializesSentMessage = serializer.Deserialize<ContentData>(sentMessage);

            Assert.Equal("Test_File.pdf", deserializesSentMessage.Data);
            Assert.Equal(file.Type, deserializesSentMessage.Type);
            Assert.Equal(file.Event, deserializesSentMessage.Event);
            Assert.Equal(file.SenderID, deserializesSentMessage.SenderID);
            Assert.Equal(file.Starred, deserializesSentMessage.Starred);
            Assert.Equal(file.ReceiverIDs, deserializesSentMessage.ReceiverIDs);
            Assert.True(communicator.IsBroadcast());
        }

        [Fact]
        public void OnDataReceived_InvalidDataIsReceived_CallReceiveMethodOfContentDatabase()
        {
            Setup();
            var previousMessageToSubsribers = listener.GetReceivedMessage();
            var previousMessageToCommunicator = communicator.GetSentData();

            var garbageData = " adfasfasfsadf";
            notificationHandler.OnDataReceived(garbageData);

            Thread.Sleep(sleeptime);

            var currentMessageToSubscribers = listener.GetReceivedMessage();

            Assert.Equal(currentMessageToSubscribers, previousMessageToSubsribers);
            Assert.Equal(communicator.GetSentData(), previousMessageToCommunicator);
        }
    }
}
