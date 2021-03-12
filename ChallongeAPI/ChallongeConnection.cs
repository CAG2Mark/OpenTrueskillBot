using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;

namespace OpenSkillBot.ChallongeAPI {
    public class ChallongeConnection {

        const string url = "https://api.challonge.com/v1/";
        
        /// <summary>
        /// The token of the bot.
        /// </summary>
        private string token;

        /// <summary>
        /// Constructor for the challonge connection.
        /// </summary>
        /// <param name="token">The token of the bot.</param>
        public ChallongeConnection(string token) {
            this.token = token;
        }

        #region endpoints

        /// <summary>
        /// Gets the list of tournaments of the current user.
        /// </summary>
        /// <returns>The list of tournaments as a dynamic object. <br/>For the implementation, see https://api.challonge.com/v1/documents/tournaments/index.</returns>
        public async Task<List<ChallongeTournament>> GetTournaments() {
            var res = await httpGet("tournaments.json");

            Console.WriteLine(res);

            List<ChallongeTournament> returns = new List<ChallongeTournament>();

            try {
                List<Dictionary<string, ChallongeTournament>> des = 
                    JsonConvert.DeserializeObject<List<Dictionary<string, ChallongeTournament>>>(res);
                foreach (var dict in des) {
                    returns.Add(dict["tournament"]);
                }

                return returns;
            }
            catch (JsonException) {
                // todo: reimplement error
                ChallongeError err = JsonConvert.DeserializeObject<ChallongeError>(res);
                throw new ChallongeException(err.Errors);
            }

        }

        #endregion

        #region http api

        private static string encodeParams(Dictionary<string,string> parameters) {
            // Source: https://stackoverflow.com/questions/23518966/convert-a-dictionary-to-string-of-url-parameters
            return string.Join("&", parameters.Select(kvp => $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}"));
        }

        HttpClient client = new HttpClient();
        /// <summary>
        /// Makes an HTTP GET request.
        /// </summary>
        /// <param name="parameters">The parameters to send to the API endpoint.</param>
        /// <param name="endpoint">The Challonge API endpoint, starting after /v1/.</param>
        /// <returns></returns>
        private async Task<string> httpGet(string endpoint, Dictionary<string,string> parameters = null) {

            if (parameters == null) parameters = new Dictionary<string, string>();
            parameters.Add("api_key", token);

            var resp = await client.GetAsync(url + endpoint + "?" + encodeParams(parameters));
            var content = await resp.Content.ReadAsStringAsync();

            return content;
        } 

        /// <summary>
        /// Makes an HTTP POST request.
        /// </summary>
        /// <param name="parameters">The parameters to send to the API endpoint.</param>
        /// <param name="endpoint">The Challonge API endpoint, starting after /v1/.</param>
        /// <returns></returns>
        private async Task<string> httpPost(string endpoint, Dictionary<string,string> parameters = null) {

            if (parameters == null) parameters = new Dictionary<string, string>();
            parameters.Add("api_key", token);

            HttpContent strContent = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");

            var resp = await client.PostAsync(url + endpoint, strContent);
            var content = await resp.Content.ReadAsStringAsync();

            return content;
        } 

        #endregion

        #region helpers


        /// <summary>
        /// Helps parse the date time format given by Challonge, ie 2015-01-19T16:57:17-05:00
        /// </summary>
        /// <param name="time">The time given by Challonge.</param>
        /// <returns>The properly structured DateTime.</returns>
        public static DateTime ParseChallongeTime(string time) {
            var dtSpl = time.Split("T");
            var dateSpl = dtSpl[0].Split("-");
            var timeSpl = dtSpl[1].Substring(0, dtSpl[1].Length - 6).Split(":");
            var offsetSpl = dtSpl[1].Substring(dtSpl[1].Length - 5).Split(":");

            int multiplier = (dtSpl[1])[dtSpl[1].Length - 6] != '+' ? 1 : -1;

            var t = new DateTime(
                Convert.ToInt32(dateSpl[0]),
                Convert.ToInt32(dateSpl[1]),
                Convert.ToInt32(dateSpl[2]),
                Convert.ToInt32(timeSpl[0]),
                Convert.ToInt32(timeSpl[1]),
                Convert.ToInt32(timeSpl[2].Split(".")[0])
                );

            t = t
                .AddHours(Convert.ToInt32(offsetSpl[0]) * multiplier)
                .AddMinutes(Convert.ToInt32(offsetSpl[1]) * multiplier);

            return t;
        }
        #endregion
    }

    // technically these can be combined, but keep separate for sake of clarity
    public class ChallongeError {
        [JsonProperty("errors")]
        public List<string> Errors { get; }
    }

    public class ChallongeException : Exception {
        public List<string> Errors { get; private set; }

        public ChallongeException(List<string> errors) {
            this.Errors = errors;
        }
    }
}