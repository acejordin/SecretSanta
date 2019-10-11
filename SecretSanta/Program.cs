using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SecretSanta
{
    class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random((int)DateTime.Now.Ticks);

            int iterationCount = 0;

            while (true)
            {
                iterationCount++;
                var participants = GetParticipants();
                var shuffledParticipants = participants.OrderBy(x => random.Next()).ToList();

                bool failed = false;
                foreach (var participant in shuffledParticipants)
                {
                    var eligibleParticipants = GetEligibleParticipants(participant, participants);
                    if (eligibleParticipants.Count == 0) { failed = true; break; }

                    int randInt = random.Next(0, eligibleParticipants.Count - 1);
                    var giftee = eligibleParticipants[randInt];
                    participant.Giftee = giftee;
                    giftee.BeenPicked = true;
                }

                Console.Write($"Attempts: {iterationCount}\r");

                //Auto run until success
                if (failed) continue;

                Console.WriteLine();

                PrintResults(participants);

                Console.Write("Run again? [y]/n: ");
                var key = Console.ReadKey();
                Console.WriteLine();
                if(key.Key != ConsoleKey.Y && key.Key != ConsoleKey.Enter)
                {
                    break;
                }
                Console.WriteLine();
            }
            Console.WriteLine("Done. Press Enter to quit...");
            Console.ReadLine();
        }

        static void PrintResults(List<Participant> participants)
        {
            Console.WriteLine("Results");
            Console.WriteLine("-------");
            foreach (var participant in participants)
            {
                Console.WriteLine(participant.Name + " -> " + participant.Giftee?.Name);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="participant"></param>
        /// <param name="participants"></param>
        /// <returns></returns>
        static List<Participant> GetEligibleParticipants(Participant participant, List<Participant> participants)
        {
            //to keep from starving out participants toward the end of picking giftees (commonly this means the last person has nobody to pick)
            //the first whack at getting a list of eligible participants includes an extra criteria that excludes participants that already have a giftee
            //and then when everyone has a giftee then that criteria is dropped to match more. this seems to be effective at avoiding a failed run

            var eligibleParticipants = new List<Participant>();
            var histories = GetHistories();

            foreach(var p in participants)
            {
                //don't match participant to people on their never match list
                if (participant.NeverMatchList.Contains(p))
                {
                    p.Disqualified = true;

                    //ALSO don't match with someone if they are on the never match list of a giftee that is on THEIR never match list
                    //eg if Jack has Jane on  
                    if(p.Giftee != null)
                    {
                        foreach(var pp in p.Giftee.NeverMatchList)
                        {
                            pp.Disqualified = true;
                        }
                    }
                }

                //check against histories
                foreach(var history in histories)
                {
                    //don't match someone if they've had them in the past
                    var found = history.HistoryParticipants.FindAll(x => x.Gifter.ToLower() == participant.Name.ToLower());
                    if (found.Count == 0) { continue; }
                    if (found.Count != 1) { throw new Exception(); }
                    var giftee = participants.Find(x => x.Name == found[0].Giftee);
                    giftee.Disqualified = true;

                    //TODO: if Jack and Jill are together, and John and Jane are together, and Jack got Jane in a previous year, should Jill be able to get Jane this year?
                }
            }

            eligibleParticipants = participants.Where(x => 
                x.BeenPicked == false && 
                x != participant && 
                x.Giftee == null &&
                !x.Disqualified
            ).ToList();

            if (eligibleParticipants.Count == 0)
            {
                eligibleParticipants = participants.Where(x =>
                   x.BeenPicked == false &&
                   x != participant &&
                   !x.Disqualified
               ).ToList();
            }

            participants.ForEach(x => x.Disqualified = false);

            return eligibleParticipants;
        }

        static List<Participant> GetParticipants()
        {
            var obj = (JArray)JsonConvert.DeserializeObject(File.ReadAllText("participants.json"));
            List<Participant> jsonParticipants = new List<Participant>();
            foreach(var child in obj.Children())
            {
                jsonParticipants.Add(new Participant { Name = child["Name"].ToString() });
            }
            foreach(var child in obj.Children())
            {
                var name = child["Name"].ToString();
                var participant = jsonParticipants.Find(x => x.Name == name);
                foreach(var neverMatch in child["NeverMatchList"])
                {
                    var nmParticipant = jsonParticipants.Find(x => x.Name == neverMatch["Name"].ToString());
                    participant.NeverMatchList.Add(nmParticipant);
                }
            }

            return jsonParticipants;
        }

        static List<History> GetHistories()
        {
            var obj = (JArray)JsonConvert.DeserializeObject(File.ReadAllText("histories.json"));
            List<History> histories = new List<History>();
            foreach(var childHist in obj.Children())
            {
                var history = new History { Name = childHist["Name"].ToString() };
                histories.Add(history);
                foreach(var histParticipant in childHist["HistoryParticipants"])
                {
                    history.HistoryParticipants.Add(new HistoryParticipant { Gifter = histParticipant["Gifter"].ToString(), Giftee = histParticipant["Giftee"].ToString() });
                }
            }
            
            return histories;
        }
    }

    [DebuggerDisplay("Name = {Name}, Giftee = {Giftee}, NeverMatchCount = {NeverMatchList.Count}, DQ'd = {Disqualified}, Picked = {BeenPicked}")]
    class Participant
    {
        public string Name { get; set; }
        public bool Disqualified { get; set; }
        public bool BeenPicked { get; set; }
        public List<Participant> NeverMatchList { get; set; } = new List<Participant>();
        public Participant Giftee { get; set; }
    }

    [DebuggerDisplay("Name = {Name}")]
    class History
    {
        public string Name { get; set; }
        public List<HistoryParticipant> HistoryParticipants { get; set; } = new List<HistoryParticipant>();
    }

    [DebuggerDisplay("Gifter = {Gifter}, Giftee = {Giftee}")]
    class HistoryParticipant
    {
        public string Gifter { get; set; }
        public string Giftee { get; set; }
    }
}
