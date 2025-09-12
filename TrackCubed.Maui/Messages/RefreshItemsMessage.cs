using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackCubed.Maui.Messages
{
    // A simple message to signal that the items list should be refreshed.
    public class RefreshItemsMessage : ValueChangedMessage<bool>
    {
        public RefreshItemsMessage(bool value) : base(value) { }
    }
}
