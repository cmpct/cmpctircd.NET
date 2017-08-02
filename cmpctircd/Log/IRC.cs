using System;
using System.Collections.Generic;
using System.IO;

namespace cmpctircd {
    public class IRC : BaseLogger {

        Channel channel;

        public IRC(IRCd ircd, Log.LogType type) : base(ircd, type) {}

        override public void Create(Dictionary<string, string> arguments) {
            var channelName = arguments["channel"];

            try {
                // Use the channel if it already exists (very unlikely)
                channel = IRCd.ChannelManager.Channels[channelName];
            } catch(KeyNotFoundException) {
                channel = IRCd.ChannelManager.Create(channelName);
            }
        }

        override public void Close() {
            // TODO: tell channel we're going
            // kill channel if empty?
        }

        override public string Prepare(string msg, Log.LogType Type) {
            return $":{IRCd.Host} PRIVMSG {channel.Name} :{msg}";
        }

        override public void WriteLine(string msg, Log.LogType type, bool prepared = true) {
            if(!prepared) msg = Prepare(msg, type);
            channel.SendToRoom(null, msg);
        }


    }
}