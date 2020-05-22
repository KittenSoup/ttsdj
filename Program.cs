using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using Newtonsoft.Json.Linq;
using System.Speech.AudioFormat;

namespace KeepTalking
{
    public class NowPlaying
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string TrackId { get; set; }
        public bool IsPlaying { get; set; }
    }
    class Program
    {
        static readonly HttpClient client = new HttpClient();

        // static async Task Main(string[] args)
        static async Task Main(string[] args)
        {
            var synthesizer = new SpeechSynthesizer
            {
                Rate = -1,
                Volume = 80
            };
            string lastTrackId = "";
            if (args.Length == 0)
            {
                System.Console.WriteLine("Usage: ttsdj.exe \"spotify oauth token\" [\"voice\"]");
                Console.WriteLine("");
                System.Console.WriteLine("Example: ttsdj.exe \"BQCqYdp7Ex0p...e94mXB38L\"");
                Console.WriteLine("");
                Console.WriteLine("");

                
                // Output information about all of the installed voices.   
                Console.WriteLine("Installed voices -");
                foreach (InstalledVoice voice in synthesizer.GetInstalledVoices())
                {
                    VoiceInfo info = voice.VoiceInfo;
                    string AudioFormats = "";
                    foreach (SpeechAudioFormatInfo fmt in info.SupportedAudioFormats)
                    {
                        AudioFormats += String.Format("{0}\n",
                        fmt.EncodingFormat.ToString());
                    }

                    Console.WriteLine(" Name:          " + info.Name);
                }
                
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            else
            {
                while(true) {
                    string voice = synthesizer.GetInstalledVoices()[0].VoiceInfo.Name;
                     
                    if (args.Length == 2)
                    {
                        voice = args[1];
                    }
                    // Call asynchronous network methods in a try/catch block to handle exceptions.
                    try
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", args[0]);
                        HttpResponseMessage response = await client.GetAsync("https://api.spotify.com/v1/me/player/currently-playing");
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        try
                        {
                            JObject s = JObject.Parse(responseBody);
                            NowPlaying nowPlaying = new NowPlaying
                            {
                                Artist = (string)s["item"]["artists"][0]["name"],
                                Title = (string)s["item"]["name"],
                                TrackId = (string)s["item"]["id"],
                                IsPlaying = (bool)s["is_playing"]
                            };
                            string output = "Now listening to" + nowPlaying.Title + " by " + nowPlaying.Artist;
                            synthesizer.SetOutputToDefaultAudioDevice();
                            synthesizer.SelectVoice(voice);
                            if (nowPlaying.TrackId != lastTrackId && nowPlaying.IsPlaying == true)
                            {
                                synthesizer.Speak(output);
                                lastTrackId = nowPlaying.TrackId;
                            }
                            
                        } catch (Newtonsoft.Json.JsonReaderException)
                        {
                            Console.WriteLine("Not playing anything");
                        }
                        // Above three lines can be replaced with new helper method below
                        // string responseBody = await client.GetStringAsync(uri);

                        // Console.WriteLine(responseBody);
                   
                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine("\nException Caught!");
                        Console.WriteLine("Message :{0} ", e.Message);
                        Console.WriteLine("Likely issue: wrong/invalid auth token :(");
                    }
                    System.Threading.Thread.Sleep(6000);
                }
            }
        }
    }
}