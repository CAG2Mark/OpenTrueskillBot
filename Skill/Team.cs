using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace OpenSkillBot.Skill {
    public class Team {

        // For challonge. Will be populated when needed
        public ulong ChallongeId { get; set; }

        /// <summary>
        /// The ranking of this team in a tournament. By default is set to 2^32.
        /// </summary>
        /// <value></value>
        public uint Ranking { get; set; } = uint.MaxValue;

        private List<Player> players;


        [JsonProperty]
        private IEnumerable<string> playerUUIDs { get; set; } = new List<string>();

        [JsonIgnoreAttribute]
        public List<Player> Players {
            get {
                if (players == null) {
                    players = MatchAction.UUIDListToTeam(playerUUIDs).Players;
                }
                return players;
            }
            set {
                this.players = value;
                playerUUIDs = players.Select(p => p.UUId);
            }
        } 
        public bool IsSameTeam(Team t) {
            // O(n) solution for checking equality
            // Idea from: https://stackoverflow.com/questions/14236672/fastest-way-to-check-if-two-listt-are-equal
            Dictionary<string, int> hash = new Dictionary<string, int>();
            foreach (var p in Players) {
                if (hash.ContainsKey(p.UUId)) ++hash[p.UUId];
                else hash.Add(p.UUId, 1);
            } 
            foreach (var p in t.Players) {
                if (!hash.ContainsKey(p.UUId) || hash[p.UUId] == 0) return false;
                --hash[p.UUId];
            } 
            return true;
        }

        public string GetPodiumString() {
            var str = this.ToString();
            if (this.Ranking != uint.MaxValue) return this.Ranking + getPrefix(this.Ranking) + ": " + str;
            else return str;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //
            
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return IsSameTeam((Team)obj);
        }
        
        // override object.GetHashCode
        public override int GetHashCode()
        {
            return 23 * Players.GetHashCode();
        }

        public override string ToString()
        {
            return String.Join(", ", Players.Select(p => p.IGN));
        }

        static string getPrefix(uint num) {
            string[] prefixes = new string[] { "th", "st", "nd", "rd", "th" };
            if ((num / 10) % 10 == 1) return "th";
            else return prefixes[Math.Min(4, num % 10)];
        }
    }
}