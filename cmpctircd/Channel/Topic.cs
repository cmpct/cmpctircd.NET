﻿using System;

namespace cmpctircd {
    public class Topic {
        // Hold our topic metadata: The topic itself, user who set it, date & time set, channel set on
        public string TopicText { get; set; }
        public string Setter { get; set; }
        public Channel Channel { get; set; }
        public long Date { get; set; }

        // TODO: Revise function definition (could be properties?)
        public void GetTopic(Client client, string target, bool onJoin = false) {
            if (String.IsNullOrWhiteSpace(TopicText) && !onJoin) {
                client.Write(String.Format(":{0} {1} {2} {3} :No topic is set.", client.IRCd.Host, IrcNumeric.RPL_NOTOPIC.Printable(), client.Nick, target));
            }
            else if (!String.IsNullOrWhiteSpace(TopicText)) {
                client.Write(String.Format(":{0} {1} {2} {3} :{4}", client.IRCd.Host, IrcNumeric.RPL_TOPIC.Printable(), client.Nick, target, TopicText));
                client.Write(String.Format(":{0} {1} {2} {3} {4} {5}", client.IRCd.Host, IrcNumeric.RPL_TOPICWHOTIME.Printable(), client.Nick, target, Setter, Date));
            }
        }

        public void SetTopic(Client client, string target, string topic) {
            Channel = client.IRCd.ChannelManager[target];
            if (Channel.Inhabits(client)) {
                var userRank = Channel.Status(client);
                var isOp     = userRank.CompareTo(ChannelPrivilege.Op);

                try {
                    Channel.Modes["t"].GetValue();
                    if (isOp < 0) {
                        throw new IrcErrChanOpPrivsNeededException(client, Channel.Name);
                    }
                } catch (IrcModeNotEnabledException) {
                    if((isOp < 0) && Channel.Modes["b"].Has(client)) {
                        throw new IrcErrCannotSendToChanException(client, Channel.Name, "Cannot send to channel (You're banned)");
                    }
                }

                TopicText = topic;
                Date   = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Setter = client.Nick;
                Channel.SendToRoom(client, String.Format(":{0} TOPIC {1} :{2}", client.Mask, Channel.Name, TopicText), true);
            }
        }
    }
}
