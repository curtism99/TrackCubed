using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackCubed.Maui.Messages
{
    // A simple message to signal that a sign-out has occurred.
    public class SignOutMessage : ValueChangedMessage<bool>
    {
        public SignOutMessage(bool signedOutSuccessfully) : base(signedOutSuccessfully)
        {
        }
    }
}
