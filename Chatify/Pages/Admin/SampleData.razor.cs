using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using ChatifyLibrary.Models;
using ChatifyLibrary.BasicModel;

namespace Chatify.Pages.Admin;

public partial class SampleData
{
    private bool conversationsCreated = false;
    private bool messagesCreated = false;
    private bool userCreated = false;
    private bool categoriesCreated = false;
    private bool userUpdated = false;
    private bool friendRequestCreated = false;
    private async Task GenerateSampleData()
    {
        UserModel user = new()
        {
            ObjectIdentifier = "abc-123",
            FriendCode = await oidGenerator.GenerateOidAsync(),
            FirstName = "Arthur",
            LastName = "Morgan",
            Email = "ArthurMorgan@gmail.com"
        };
        await userData.CreateUser(user);
        user = new()
        {
            ObjectIdentifier = "def-456",
            FriendCode = await oidGenerator.GenerateOidAsync(),
            FirstName = "John",
            LastName = "Martston",
            Email = "JohnMarston@gmail.com"
        };
        await userData.CreateUser(user);
        user = new()
        {
            ObjectIdentifier = "hij-789",
            FriendCode = await oidGenerator.GenerateOidAsync(),
            FirstName = "Hosea",
            LastName = "Matthews",
            Email = "HoseaMatthews@gmail.com"
        };
        await userData.CreateUser(user);
        userCreated = true;
    }

    private async Task CreateCategories()
    {
        var categories = await categoryData.GetAllCategoriesAsync();
        if (categories?.Count > 0)
        {
            return;
        }

        CategoryModel c = new()
        {
            CategoryName = "Gaming",
            CategoryDescription = "Gamers chat about their favorite games and strategies while playing together online.",
        };
        await categoryData.CreateCategory(c);
        c = new()
        {
            CategoryName = "Music",
            CategoryDescription = "Musicians music enthusiasts discuss their favorite artists, albums, and songs, sharing their opinions and discovering new music recommendations from each other.",
        };
        await categoryData.CreateCategory(c);
        c = new()
        {
            CategoryName = "Sports",
            CategoryDescription = "Sports fans talk about the latest games and scores, debate the strengths and weaknesses of their favorite teams and players, and share their predictions for upcoming matches.",
        };
        await categoryData.CreateCategory(c);
        c = new()
        {
            CategoryName = "Sciences",
            CategoryDescription = "Science enthusiasts discuss the latest scientific discoveries, theories, and breakthroughs, exchanging ideas and perspectives on a variety of topics such as physics, biology, chemistry, and astronomy.",
        };
        await categoryData.CreateCategory(c);
        c = new()
        {
            CategoryName = "Technology",
            CategoryDescription = "Tech enthusiasts talk about the latest gadgets, software, and innovations in the tech industry, sharing their thoughts on the latest trends and advancements, and discussing how technology is shaping the world around us."
        };
        await categoryData.CreateCategory(c);
        categoriesCreated = true;
    }

    private async Task CreateConversation()
    {
        var foundUser = await userData.GetUserFromAuthenticationAsync("abc-123");
        var categories = await categoryData.GetAllCategoriesAsync();
        var participants = new List<BasicUserModel>();
        participants.Add(new BasicUserModel(foundUser));
        ConversationModel c = new()
        {
            Participants = participants,
            IsGroupChat = true,
            GroupName = "The Martians",
            Archived = false,
            Category = categories[0],
            Owner = new BasicUserModel(foundUser)
        };
        await conversationData.CreateConversation(c);
        var user = await userData.GetUserFromAuthenticationAsync("def-456");
        participants.Add(new BasicUserModel(user));
        c = new()
        {
            Participants = participants,
            IsGroupChat = false,
            GroupName = "The Nerds",
            Archived = false,
            Category = categories[1],
            Owner = new BasicUserModel(foundUser)
        };
        await conversationData.CreateConversation(c);
        conversationsCreated = true;
    }

    private async Task CreateMessage()
    {
        var firstUser = await userData.GetUserFromAuthenticationAsync("abc-123");
        var secondUser = await userData.GetUserFromAuthenticationAsync("def-456");
        var conversation = await conversationData.GetConversationAsync("64172486ed6abded2dac7841");
        var conversation2 = await conversationData.GetConversationAsync("64172487ed6abded2dac7843");
        var users = await userData.GetAllUsersAsync();
        var receivers = new List<BasicUserModel>();
        foreach (var r in users)
        {
            receivers.Add(new BasicUserModel(r));
        }

        MessageModel m = new()
        {
            Sender = new BasicUserModel(firstUser),
            Text = "Howdy! There!",
            Conversation = conversation,
            Archived = false,
        };
        await messageData.CreateMessage(m);
        m = new()
        {
            Sender = new BasicUserModel(firstUser),
            Text = "Howdy! There!",
            Conversation = conversation2,
            Archived = false,
        };
        await messageData.CreateMessage(m);
        messagesCreated = true;
    }

    private async Task UpdateUser()
    {
        var foundUser = await userData.GetUserFromAuthenticationAsync("abc-123");
        var friend = await userData.GetUserFromAuthenticationAsync("hij-789");
        var friends = new List<BasicUserModel>
        {
            new BasicUserModel(friend),
        };
        foundUser.Friends.Add(new BasicUserModel(friend));
        await userData.UpdateUser(foundUser);
        userUpdated = true;
    }

    private async Task CreateFriendRequest()
    {
        var foundUser = await userData.GetUserFromAuthenticationAsync("abc-123");
        var otherUser = await userData.GetUserFromAuthenticationAsync("hij-789");
        var request = new FriendRequestModel
        {
            Sender = new BasicUserModel(otherUser),
            Receiver = new BasicUserModel(foundUser)
        };
        await requestData.CreateFriendRequest(request);
        friendRequestCreated = true;
    }
}