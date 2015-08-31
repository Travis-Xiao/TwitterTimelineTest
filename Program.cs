using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using Tweetinvi;
using Tweetinvi.Core;
using Tweetinvi.Core.Credentials;
using Tweetinvi.Core.Enum;
using Tweetinvi.Core.Extensions;
using Tweetinvi.Core.Interfaces;
using Tweetinvi.Core.Interfaces.Controllers;
using Tweetinvi.Core.Interfaces.DTO;
using Tweetinvi.Core.Interfaces.Models;
using Tweetinvi.Core.Interfaces.Parameters;
using Tweetinvi.Core.Interfaces.Streaminvi;
using Tweetinvi.Core.Parameters;
using Tweetinvi.Json;
using Geo = Tweetinvi.Geo;
using SavedSearch = Tweetinvi.SavedSearch;
using Stream = Tweetinvi.Stream;

namespace SearchTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //int totalCredentials = TimelineTest.Credentials.Code.Length / 5;
            //for (int i = 0; i < totalCredentials; i++)
            //{
            //    //Stream_SampleStreamExample(i);
            //    SetCredentials(i);
            //    Search_SearchTweet();
            //}
            CredentialTest();
            LoadLabeledUserTimeline();
            //GetUsersFollowersFriends();
            Console.WriteLine("Fin");
            Console.ReadKey();
        }

        static void CredentialTest()
        {
            string [] tokens = {
                "adctzaZOlOAS4bc7XBKGjw",
                "B7oZsHHDKcWsleIuFCHmAyVdu9XPcrCZiWE0iJsM",
                "2269815608-xJ2SyvCzaDPcyUWmOGxInHPyPIHPoMDcIKzZ1ha",
                "hKXxDJ6AY9Uu6HpEzf9QLqgNfJMg7nbzpkeiiBD5LIpOw",
                "Auq2Lx06hkl8pJke2iUh3w",
                "p9nhm4pJBfDtw0TZJwXh1XRemLeYqPrxPytu0ncdo",
                "2269823232-AFkuSS4cHjauK2wGCdmrUX8ocCDXFMK3PDUYts2",
                "IXv530n3XRmWVk48YuZzJlCB1DC1RJsd2DhZLFxihy0Tg",
            };

            var pass = Auth.SetUserCredentials(tokens[0],tokens[1], tokens[2], tokens[3]);
        }

        static void LoadLabeledUserTimeline()
        {
            //string usernamesFile = "../../../Data/HouseRepresentatives/names.txt";
            string usernamesFile = "../../names.txt";
            string outputDir = "../../Output/";
            string[] usernames = File.ReadAllLines(usernamesFile);

            StringBuilder builder = new StringBuilder();
            //foreach (string name in usernames)
            int progress = 0;
            //System.Threading.Tasks.Parallel.For(0, usernames.Count(), (i) =>
            for (int i = 0; i < usernames.Count(); i ++)
            {
                string line = usernames[i];
                string[] tokens = line.Split('\t');
                Console.WriteLine(tokens[1]);
                if (tokens.Count() < 2) return;
                string username = tokens[1];
                if (String.IsNullOrEmpty(username))
                {
                    return;
                }
                string timeline = GetUserTimeline(username);
                if (timeline == null)
                {
                    return;
                }
                string prefix = "";
                if (tokens[0] == "D")
                    prefix = "0 ";
                else
                    prefix = "1 ";
                SaveToFile(outputDir + prefix + username, timeline);
                Console.WriteLine(prefix + username + "-" + progress);
                progress++;
            };
            //Search_SearchTweet();
        }

        static void GetUsersFollowersFriends()
        {
            string usernamesFile = "../../LabeledUsers.txt";
            string output = "../../FriendsFollowers.txt";
            string[] usernames = File.ReadAllLines(usernamesFile);

            StringBuilder builder = new StringBuilder();
            //foreach (string name in usernames)

            System.Threading.Tasks.Parallel.For(0, usernames.Count(), (i) =>
            {
                string name = usernames[i];
                if (String.IsNullOrEmpty(name))
                {
                    return;
                }
                string content = name + "\n" + GetUserFollowers(name) + "\n" + GetUserFriends(name) + "\n";
                lock (builder)
                {
                    builder.Append(content);
                }
            });

            SaveToFile(output, builder.ToString());
        }

        static void SaveToFile(string storage, string content)
        {
            System.IO.StreamWriter writer = new StreamWriter(storage);
            writer.WriteLine(content);
            writer.Close();
        }

        static string GetUserFollowers(string username)
        {
            var user = User.GetUserFromScreenName(username);
            if (user == null)
            {
                Console.WriteLine(username);
                return "";
            }
            var followers = user.GetFollowers(1000);

            if (followers == null)
            {
                Console.WriteLine(username);
                return "";
            }

            StringBuilder builder = new StringBuilder();
            foreach (var follower in followers)
            {
                builder.Append(follower.ScreenName).Append(" ");
            }
            return builder.ToString();
        }

        static string GetUserFriends(string username)
        {
            var user = User.GetUserFromScreenName(username);
            if (user == null)
            {
                Console.WriteLine(username);
                return "";
            }
            var followers = user.GetFriends(1000);

            if (followers == null)
            {
                Console.WriteLine(username);
                return "";
            }

            StringBuilder builder = new StringBuilder();
            foreach (var follower in followers)
            {
                builder.Append(follower.ScreenName).Append(" ");
            }
            return builder.ToString();
        }

        static string GetUserTimeline(string username)
        {
            var user = User.GetUserFromScreenName(username);

            if (user == null)
            {
                Console.WriteLine(username);
                return "";
            }

            var timelineTweets = user.GetUserTimeline(200);

            if (timelineTweets == null)
            {
                Console.WriteLine(username);
                return "";
            }

            StringBuilder builder = new StringBuilder();
            foreach (var tweet in timelineTweets)
            {
                builder.Append(tweet.CreatedAt).Append("\t");
                builder.Append(tweet.Text).Append("\n");
            }
            return builder.ToString();
        }

        static void SetCredentials(int index)
        {
            string [] codes = SearchTest.Credentials.Code;
            var pass = Auth.SetUserCredentials(codes[5*index+ 3],
                codes[5 * index + 4],
                codes[5 * index + 1],
                codes[5 * index + 2]);
            Console.WriteLine(pass);
        }

        public static void Search_SearchTweet()
        {
            // IF YOU DO NOT RECEIVE ANY TWEET, CHANGE THE PARAMETERS!

            var searchParameter = Search.CreateTweetSearchParameter("obama");

            searchParameter.SetGeoCode(Geo.GenerateCoordinates(-122.398720, 37.781157), 1, DistanceMeasure.Miles);
            searchParameter.Lang = Language.English;
            searchParameter.SearchType = SearchResultType.Popular;
            searchParameter.MaximumNumberOfResults = 100;
            searchParameter.Since = new DateTime(2013, 12, 1);
            searchParameter.Until = new DateTime(2013, 12, 11);
            searchParameter.SinceId = 399616835892781056;
            searchParameter.MaxId = 405001488843284480;
            searchParameter.TweetSearchType = TweetSearchType.OriginalTweetsOnly;
            searchParameter.Filters = TweetSearchFilters.Videos;

            var tweets = Search.SearchTweets(searchParameter);

            // if auth is not successful
            if (tweets == null)
            {
                Console.WriteLine(ExceptionHandler.GetLastException().TwitterDescription);
                return;
            }

            StringBuilder builder = new StringBuilder();
            //tweets.ForEach(t => builder.Append(t.Text));
            PropertyInfo [] props = typeof(ITweet).GetProperties();
            foreach (var tweet in tweets)
            {
                builder.Append(tweet.CreatedAt);
                foreach (var prop in props)
                {
                    if (prop.CanRead)
                    {
                        builder.Append(prop.GetValue(tweet, null)).Append("\t");
                    }
                }
                builder.Append("\n");
            }

            System.IO.StreamWriter file = new StreamWriter("a.txt");
            file.Write(builder.ToString());
            file.Close();
        }

        private static void Stream_SampleStreamExample(int index)
        {
            int count = 0;
            string[] codes = SearchTest.Credentials.Code;
            var pass = Auth.SetUserCredentials(codes[5 * index + 3],
                codes[5 * index + 4],
                codes[5 * index + 1],
                codes[5 * index + 2]);

            var stream = Stream.CreateSampleStream();
            stream.TweetReceived += (sender, args) =>
            {
                var tweet = args.Tweet;
                if (tweet.Language == Language.English)
                {
                    StringBuilder b = new StringBuilder();
                    b.Append(tweet.CreatedAt);
                    b.Append("\t");
                    b.Append(tweet.Text.Replace('\n', ' '));
                    b.Append("\t");
                    b.Append(tweet.IsRetweet);
                    b.Append("\t");
                    b.Append(tweet.Retweeted);
                    b.Append("\t");
                    b.Append(tweet.RetweetCount);
                    b.Append("\t");
                    //if (count++ % 10000 == 0)
                    Console.Out.WriteLine(count++);
                    DateTime time = DateTime.Now;
                    string fileName = time.ToShortDateString().Replace('/', '-');
                    File.AppendAllText(@"..\..\Data\" + fileName + "_" + index + ".txt", b.ToString());
                }
                //Console.WriteLine(args.Tweet.Text);
            };

            stream.LimitReached += (sender, args) =>
            {
                Console.Out.WriteLine("LimitReached" + DateTime.Now);
            };
            stream.DisconnectMessageReceived += (sender, args) =>
            {
                Console.Out.WriteLine("DisconnectMessageReceived" + DateTime.Now);
            };
            stream.StreamStopped += (sender, args) =>
            {
                if (args.DisconnectMessage != null)
                    Console.Out.WriteLine(args.DisconnectMessage.Reason);
            };

            while (true)
            {
                stream.StartStream();
                Console.Out.WriteLine("End" + DateTime.Now);
            }
        }
    }
}
