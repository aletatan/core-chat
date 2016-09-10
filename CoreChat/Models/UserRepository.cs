﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreChat.Models
{
    public class UserRepository : IUserRepository
    {
        private int _id;
        private static List<User> _users = new List<User>();

        public UserRepository()
        {
            _id = 0;
        }

        public User Add(User user)
        {
            user.ID = _id;
            user.Token = Guid.NewGuid().ToString();

            _users.Add(user);
            _id++;

            return user;
        }

        public User FindByToken(string token)
        {
            return _users.Find(x => x.Token == token);
        }

        public User FindByEmail(string email)
        {
            return _users.Find(x => x.Email == email);
        }

        public void Update(User user)
        {
            var storedUser = _users.Find(x => x.Email == user.Email);
            storedUser = user;
        }
    }
}
