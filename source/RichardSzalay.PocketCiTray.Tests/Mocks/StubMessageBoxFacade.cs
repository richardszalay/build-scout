using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using RichardSzalay.PocketCiTray.Services;
using System.Collections.Generic;

namespace RichardSzalay.PocketCiTray.Tests.Mocks
{
    public class StubMessageBoxFacade : IMessageBoxFacade
    {
        private MessageBoxResult result;

        public StubMessageBoxFacade(MessageBoxResult result)
        {
            this.result = result;
        }

        public MessageBoxResult Show(string message, string description, MessageBoxButton buttons)
        {
            recordedMessageBoxes.Add(new RecordedMessageBox(message, description, buttons));

            return result;
        }

        private List<RecordedMessageBox> recordedMessageBoxes = new List<RecordedMessageBox>();

        public List<RecordedMessageBox> RecordedMessageBoxes
        {
            get { return recordedMessageBoxes; }
        }

        public class RecordedMessageBox
        {
            public RecordedMessageBox(string message, string description, MessageBoxButton buttons)
            {
                this.Message = message;
                this.Description = description;
                this.Buttons = buttons;
            }

            public string Message { get; private set; }
            public string Description { get; private set; }
            public MessageBoxButton Buttons { get; private set; }
        }
    }
}
