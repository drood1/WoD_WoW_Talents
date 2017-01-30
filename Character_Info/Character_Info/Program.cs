/*
  Copyright (c) 2017 Karl Olsen @ droodicus at http://gmail.com 
  Please see "license.txt" in the top-most folder for more information.
 
WRITTEN BY KARL OLSEN 
droodicus@gmail.com
twitter.com/Droodicus
github.com/drood1
 
IMPORTANT: This was written in August of 2016 during the Warlords of Draenor WoW expansion.
As such, this version is currently obsolete. While it will print the correct talents for characters,
with the new talent system in the Legion expansion, the formatting is inconsistent.

I am currently working on an updated version of this program to function with Legion's new talent system.
Feel free to keep an eye out on my github page! github.com/drood1
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Character_Info
{
    public class Talent
    {
        public int tier;
        public string name;
        public string description;
        public string icon;

        public Talent()
        {
            name = "";
        }

        public Talent(int t, string n, string d, string i) 
        {
            tier = t;
            name = n;
            description = d;
            icon = i;
        }

        public void printInfo()
        {
            Console.WriteLine("Tier: {0}", tier);
            Console.WriteLine("Name: {0}", name);
            Console.WriteLine("Description: {0}", description);
            //Console.WriteLine("Icon: {0}\n", icon);
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            //url-formation data
            string char_name;
            string server;
            string temp_server = "";
            string base_item_url = "https://us.api.battle.net/wow/character/";
            //string locale = "?locale=en_US";
            string talent_url = "?fields=talents&locale=en_US";
            string api_key = "&apikey=hrc5as59s2rmhyx4xeqh62p4f2b7evdw";
            string full_url;
            string full_information;

            //talent/spec information storage
            List<Talent> talents_main = new List<Talent>();
            List<Talent> talents_secondary = new List<Talent>();
            bool primary_talents_done = false;
            string primary_spec = "";
            string secondary_spec = "";
            //temp storage variables for new talent class objects
            int temp_tier;
            string temp_name = "";
            string temp_desc = "";
            string temp_icon = "";

            //characters to "ignore" while reading the item's information
            char[] delimiters = { '}', '{', ':', '[', ']', ',', '\"', ';', '(', ')', '\n' };
            //storage for the words that are "read" in the item's information
            string[] word_array;
            //"cleaner" version of word_array
            List<string> words = new List<string>();


            //infinitely loop to allow for testing on any number of items
            while (true)
            {
                Console.WriteLine("Please give a server or type 'exit' to exit.");
                server = Console.ReadLine();
                if (server == "exit")
                    break;

                Console.WriteLine("Please give a character name.");
                char_name = Console.ReadLine();
                Console.WriteLine("\nNAME: {0}\nSERVER:{1}", char_name, server);

                //**********VULNERABILITY: COULD RUN INTO ISSUES WITH SERVERS WITH MORE THAN ONE SPACE IN THE NAME**********
                for (int i = 0; i < server.Length; i++)
                {
                    //if there's a space in the server name (i.e. "Area 52")
                    if (server[i].ToString() == " ")
                    {
                        for (int j = 0; j < server.Length; j++)
                        {
                            //need to turn the space into "%20"
                            if (server[j].ToString() == " ")
                                temp_server = temp_server + "%20";
                            else
                                temp_server = temp_server + server[j];
                        }
                        server = temp_server;
                    }
                }

                ////FORM URL 
                full_url = base_item_url + server + "/" + char_name + talent_url + api_key;
                //Console.WriteLine("URL: {0}\n", full_url);

                ////CALL API WITH FORMED URL
                WebRequest request = WebRequest.Create(full_url);
                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = ex.Response as HttpWebResponse;
                }
                ////check that the id provided is valid
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine("That character/server combination does not exist");
                }
                else
                {
                    //COLLECT RELEVANT DATA FROM THE API
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    full_information = reader.ReadToEnd();

                    word_array = full_information.Split(delimiters);

                    //cleaning up the blank spots that tokenizing the original string created
                    for (int i = 0; i < word_array.Length; i++)
                    {
                        if (word_array[i] != "")
                            words.Add(word_array[i]);
                    }

                    //Console.WriteLine("EXTRACTED INFORMATION:");
                    //foreach (string w in words)
                    //{
                    //    Console.WriteLine(w);
                    //}

                    //TOKENIZE THE FULL_INFORMATION STRING TO FIND THE RELEVANT "KEYWORDS" FOR TESTING
                    for (int i = 0; i < words.Count(); i++)
                    {
                        //PRIMARY TALENT INFORMATION EXTRACTION

                        //each talent's information starts with the word "tier"
                        if (words[i] == "tier" && primary_talents_done == false)
                        {
                            temp_tier = Convert.ToInt32(words[i + 1]);
                            int k = i + 1;


                            //
                            while (words[k] != "tier" && words[k] != "glyphs" && words[k] != "spec")
                            {
                                if (words[k] == "name")
                                    temp_name = words[k + 1];
                                if (words[k] == "icon")
                                    temp_icon = words[k + 1];

                                if(words[k] == "description")  {
                                    int j = k + 1;
                                    while (words[j] != "range" && words[j] != "castTime")
                                    {
                                        temp_desc = temp_desc + words[j] + " ";
                                        j++;
                                    }
                                    //temp_desc = words[i + 12];
                                }

                                k++;
                            }

                            Talent temp = new Talent(temp_tier, temp_name, temp_desc, temp_icon);
                            talents_main.Add(temp);
                            //need to "empty out" temp_desc between talents since it collects data through concatenation
                            temp_desc = "";
                        }
                        else if (words[i] == "tier" && primary_talents_done == true)
                        {
                            temp_tier = Convert.ToInt32(words[i + 1]);
                            int k = i + 1;


                            //
                            while (words[k] != "tier" && words[k] != "glyphs" && words[k] != "spec")
                            {
                                if (words[k] == "name")
                                    temp_name = words[k + 1];
                                if (words[k] == "icon")
                                    temp_icon = words[k + 1];

                                if (words[k] == "description")
                                {
                                    int j = k + 1;
                                    while (words[j] != "range" && words[j] != "castTime" && words[j] != "order")
                                    {
                                        temp_desc = temp_desc + words[j] + " ";
                                        j++;
                                    }
                                    //temp_desc = words[i + 12];
                                }

                                k++;
                            }

                            Talent temp = new Talent(temp_tier, temp_name, temp_desc, temp_icon);
                            talents_secondary.Add(temp);
                            temp_desc = "";
                        }
                        //The talents for primary and secondary specs are separated by the word "calcSpec"
                        else if (words[i] == "spec" && primary_talents_done == false)
                        {
                            primary_spec = words[i + 2];

                            int k = i + 1;
                            while (words[k] != "tier" && words[k] != "totalHonorableKills")
                            {
                                if (words[k] == "calcSpec")
                                {
                                    primary_talents_done = true;
                                }
                                k++;
                            }
                        }
                        else if (words[i] == "spec" && primary_talents_done == true)
                        {
                            secondary_spec = words[i + 2];
                        }
                    }

                    //sort talent lists based on tier
                    talents_main.Sort(delegate(Talent x, Talent y)
                    {
                        return x.tier.CompareTo(y.tier);
                    });

                    talents_secondary.Sort(delegate(Talent x, Talent y)
                    {
                        return x.tier.CompareTo(y.tier);
                    });

                    //output talent lists
                    Console.WriteLine("\nPRIMARY SPEC: {0}\nTALENTS:\n", primary_spec);
                    for (int i = 0; i < talents_main.Count; i++)
                    {
                        talents_main[i].printInfo();
                    }
                    Console.WriteLine("\nSECONDARY SPEC: {0}\nTALENTS:\n", secondary_spec);
                    for (int i = 0; i < talents_secondary.Count; i++)
                    {
                        talents_secondary[i].printInfo();
                    }


                    words.Clear();

                }
                //reset information
                char_name = "";
                server = "";
                temp_server = "";
                full_url = "";

                talents_main.Clear();
                talents_secondary.Clear();
                primary_talents_done = false;

                Console.WriteLine("\n");
                //end of infinite loop
            }

            Console.WriteLine("Goodbye.");

        }
    }
}
