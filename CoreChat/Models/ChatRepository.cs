﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreChat.Models
{
    public class ChatRepository : IChatRepository
    {
        private IUserRepository Users;
        private List<Chat> _chats = new List<Chat>();
        private List<Message> _messages = new List<Message>();
        private int _chatId = 0;
        private int _messageId = 0;

        public ChatRepository(IUserRepository users)
        {
            Users = users;

            // Add test chats.

            _chats.Add(new Chat
            {
                ID = _chatId++,
                Created = DateTime.Now,
                LastMessage = null,
                Name = "Alpha",
                UserID = 0
            });

            _chats.Add(new Chat
            {
                ID = _chatId++,
                Created = DateTime.Now,
                LastMessage = null,
                Name = "Bravo",
                UserID = 1
            });

            _messages.Add(new Message
            {
                ID = _messageId++,
                Created = DateTime.Now,
                ChatID = 0,
                UserID = 1,
                Content = "Hey Alice, it's Bob. How's it going?"
            });

            _messages.Add(new Message
            {
                ID = _messageId++,
                Created = DateTime.Now,
                ChatID = 0,
                UserID = 0,
                Content = "Good. Just chilling."
            });
        }

        public Chat AddChat(Chat newChat)
        {
            var chat = new Chat
            {
                ID = _chatId++,
                Name = newChat.Name,
                UserID = newChat.UserID,
                Created = DateTime.Now,
                LastMessage = null
            };

            _chats.Add(chat);

            return chat;
        }

        public Message AddMessage(Message newMessage)
        {
            var message = new Message
            {
                ID = _messageId++,
                Content = newMessage.Content,
                UserID = newMessage.UserID,
                Created = DateTime.Now
            };

            _messages.Add(message);

            return message;
        }

        public PagedResults<Chat> GetChats(string query, int page, int limit)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (limit < 1)
            {
                limit = 0;
            }

            IEnumerable<Chat> allRecords = _chats.OrderByDescending(x => x.ID);

            // Filter based on name.
            if (!String.IsNullOrEmpty(query))
            {
                allRecords = allRecords.Where(x => x.Name.ToLower().Contains(query.ToLower()));
            }

            // Set some defaults for pagination (assumes limit = 0).
            var allCount = allRecords.Count();
            var pageCount = 1;
            var hasNextPage = false;
            var hasPrevPage = false;
            var pagedRecords = allRecords;

            if (limit != 0)
            {
                pageCount = (int)Math.Ceiling((double)allCount / limit);
                hasNextPage = pageCount > page;
                hasPrevPage = page > 1;
                pagedRecords = allRecords
                    .Skip(limit * (page - 1))
                    .Take(limit);
            }

            foreach(var chat in pagedRecords)
            {
                // Populate the chat user.
                var chatUser = Users.FindByID(chat.UserID);
                chat.User = new SimpleUser
                {
                    ID = chatUser.ID,
                    Name = chatUser.Name
                };

                // Populate the last message.
                var lastMessage = _messages.Where(x => x.ChatID == chat.ID)
                    .OrderBy(x => x.Created)
                    .LastOrDefault();
                chat.LastMessage = lastMessage;

                // Populate last message user.
                if (chat.LastMessage != null)
                {
                    var lastMessageUser = Users.FindByID(lastMessage.UserID);
                    chat.LastMessage.User = new SimpleUser
                    {
                        ID = lastMessageUser.ID,
                        Name = lastMessageUser.Name
                    };
                }
            }

            var pagination = new Pagination
            {
                PageCount = pageCount,
                CurrentPage = page,
                HasNextPage = hasNextPage,
                HasPrevPage = hasPrevPage,
                Count = allCount,
                Limit = limit
            };

            return new PagedResults<Chat>
            {
                Data = pagedRecords,
                Pagination = pagination
            };
        }

        public PagedResults<Message> GetMessages(int chatId, int page, int limit)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (limit < 1)
            {
                limit = 0;
            }

            IEnumerable<Message> allRecords = _messages.OrderByDescending(x => x.ID)
                .Where(x => x.ChatID == chatId);

            // Set some defaults for pagination (assumes limit = 0).
            var allCount = allRecords.Count();
            var pageCount = 1;
            var hasNextPage = false;
            var hasPrevPage = false;
            var pagedRecords = allRecords;

            if (limit != 0)
            {
                pageCount = (int)Math.Ceiling((double)allCount / limit);
                hasNextPage = pageCount > page;
                hasPrevPage = page > 1;
                pagedRecords = allRecords
                    .Skip(limit * (page - 1))
                    .Take(limit);
            }

            foreach (var record in pagedRecords)
            {
                var user = Users.FindByID(record.UserID);
                record.User = new SimpleUser
                {
                    ID = user.ID,
                    Name = user.Name
                };
            }

            var pagination = new Pagination
            {
                PageCount = pageCount,
                CurrentPage = page,
                HasNextPage = hasNextPage,
                HasPrevPage = hasPrevPage,
                Count = allCount,
                Limit = limit
            };

            return new PagedResults<Message>
            {
                Data = pagedRecords,
                Pagination = pagination
            };
        }
    }
}
