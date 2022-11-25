using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;

namespace GetTvShowTotalLength
{
    internal class Program
    {
        static int Main(string[] args)
        {
            int res = 0;
            string tvShows = "";


            try
            {

                tvShows = string.Join(" ", args);
                res = outputResult(tvShows);
                if(res <= 10)
                {
                    System.Environment.Exit(res);
                }
                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return 0;

        }


        public static int outputResult(string tvShows)
        {
            
            int tempRuntime = 0;



            tempRuntime = getRuntimeData(tvShows);
            return tempRuntime;

        }


        public static int getRuntimeData(string str)
        {
            
            int showId = 0;
            int runTime = 0;
            int totaRunlTime = 0;


            try
            {
                //pulling the id show by name of the show (beacuse using 2 different api's)
                showId = getShowId(str);
                //pulling the runtime of the show - the runtime is the window time of one episode
                runTime = getrunTime(str);
                //calculate the total runtime of the show
                totaRunlTime = getTotalRuntime(showId,runTime);
            
                return totaRunlTime;

            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return 0;

            

        }

        public static int getShowId(string str)
        {
            //fetching json data from the api 
            var client = new RestClient("https://api.tvmaze.com");
            var request = new RestRequest("/search/shows?q=" + str);
            var response = client.Execute(request);
            int showId = 0;

            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK && str.Length > 0)
                {
                    //deserialize the json data and make array of "movies" using movies class
                    string rawresponse = response.Content;
                    List<Movies> mov = JsonConvert.DeserializeObject<List<Movies>>(rawresponse);
                    string currDate = mov[0].show.ended;
                    var mostRecent = DateTime.Now;
                    if (currDate != null)
                    {
                        mostRecent = DateTime.Parse(currDate);
                    }
                    showId = (int)mov[0].show.id;
                    //if there is more then one tv show with the same name, the newer one will be provided 
                    foreach (var movie in mov)
                    {
                        if (movie.show.name == str)
                        {
                            //if the show not ended, it's null end date so it's newer
                            if (movie.show.ended == null)
                            {
                                showId = (int)movie.show.id;
                                break;
                            }
                            var tempDate = DateTime.Parse(movie.show.ended);
                            if (tempDate.Date > mostRecent.Date)
                            {
                                showId = (int)movie.show.id;

                            }
                        }
                    }
                    if (showId != null)
                    {
                        return showId;
                    }
                    else return 0;
                }
                else
                {
                    System.Environment.Exit(10);

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
   
            return 0;
            

        }

        public static int getrunTime(string str)
        {
            var client = new RestClient("https://api.tvmaze.com");
            var request = new RestRequest("/search/shows?q=" + str);
            var response = client.Execute(request);
            int runTime = 0;
            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK && str.Length > 0)
                {
                    string rawresponse = response.Content;
                    List<Movies> mov = JsonConvert.DeserializeObject<List<Movies>>(rawresponse);

                    if (mov[0].show.runtime != null)
                    {
                        runTime = (int)mov[0].show.runtime;

                    }
                    else if (mov[0].show.averageRuntime != null)
                    {
                        runTime = (int)mov[0].show.averageRuntime;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    System.Environment.Exit(10);

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);  
            }

            
            return runTime;

        }

        public static int getTotalRuntime(int id,int runTime)
        {

            int totaRunlTime = 0;
            var client = new RestClient("https://api.tvmaze.com");
            var request = new RestRequest("/shows/" + id + "/episodes");
            var response = client.Execute(request);

            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string rawresponse = response.Content;
                    List<Movies> mov = JsonConvert.DeserializeObject<List<Movies>>(rawresponse);

                    totaRunlTime = (mov.Count * runTime);
                    //If there is not aired episode but its counted in the api,then subing the extra run time for correct runtime
                    foreach (var movie in mov)
                    {
                        DateTime d1 = DateTime.Parse(movie.airdate);
                        DateTime d2 = DateTime.Now;
                        if (d1 > d2)
                        {
                            totaRunlTime = totaRunlTime - runTime;
                        }
                    }
                    if (totaRunlTime > 0)
                    {
                        return totaRunlTime;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return 0;
        }

        class Movies
        {
            public float? score { get; set; }
            public _Show? show { get; set; }
            public string? airdate { get; set; }
        }

        class _Show
        {
            public int? id { get; set; }
            public string? url { get; set; }
            public string? name { get; set; }
            public string? type { get; set; }
            public string? language { get; set; }   
            public string[]? genres { get; set; }
            public string? status { get; set; }
            public int? runtime { get; set; }
            public int? averageRuntime { get; set; }
            public string? ended { get; set; }


        }


    }

    
}
