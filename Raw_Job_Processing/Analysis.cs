using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Analytics.Constants;

namespace Raw_Job_Processing
{
    public static class JobAnalysis
    {
        public static List<JDAttribute> ALL_JD_ATTRIBUTES;

        public static List<JDAttribute> GetAllJDAttributes()
        {
            var ret = new List<JDAttribute>();

            ret.Add(new JDAttribute("Help Desk", AttributeType.Keyword));

            ret.Add(new JDAttribute("Windows", AttributeType.Keyword));
            ret.Add(new JDAttribute("Linux", AttributeType.Keyword));
            ret.Add(new JDAttribute("OSX", AttributeType.Keyword));
            ret.Add(new JDAttribute("Docker", AttributeType.Keyword));

            var a = new List<string> { "Kubernetes", "K8" };
            ret.Add(new JDAttribute("Kubernetes", AttributeType.Keyword, a));

             a = new List<string> { "Machine Learning", "ML" };
            ret.Add(new JDAttribute("Machine Learning", AttributeType.Keyword, a));

             a = new List<string> { "Artificial Intelligence", "AI" };
            ret.Add(new JDAttribute("Artificial Intelligence", AttributeType.Keyword, a));

            a = new List<string>{"AWS","Amazon Web" };
            ret.Add(new JDAttribute("AWS", AttributeType.Keyword, a));

            a = new List<string> { "DevOps", "DevSecOps" };
            ret.Add(new JDAttribute("DevOps", AttributeType.Keyword, a));

            a = new List<string> { "Azure", "Microsoft Cloud" };
            ret.Add(new JDAttribute("Azure", AttributeType.Keyword, a));

            a = new List<string> { "GCP", "Google Cloud" };
            ret.Add(new JDAttribute("GCP", AttributeType.Keyword, a));

            a = new List<string> { "PMP", "Project Management Professional" };
            ret.Add(new JDAttribute("Project Management Professional (PMP)", AttributeType.Certification, a));

            ret.Add(new JDAttribute("ITIL", AttributeType.Certification));

            a = new List<string> { "Certified Scrum Master", "CSM" };
            ret.Add(new JDAttribute("Certified Scrum Master (CSM)", AttributeType.Certification, a));

            a = new List<string> { "CEH", "Certified Ethical Hacker" };
            ret.Add(new JDAttribute("Certified Ethical Hacker", AttributeType.Certification, a));

            a = new List<string> { "CISSP", "Certified Information Systems Security Professional" };
            ret.Add(new JDAttribute("Certified Information Systems Security Professional (CISSP)", AttributeType.Certification, a));

            a = new List<string> { "Certified in Risk and Information Systems Control", "CRISC" };
            ret.Add(new JDAttribute("Certified in Risk and Information Systems Control (CRISC)", AttributeType.Certification, a));

            a = new List<string> { "Certified Information Systems Auditor", "CISA" };
            ret.Add(new JDAttribute("Certified Information Systems Auditor (CISA)", AttributeType.Certification, a));

            a = new List<string> { "CEH", "Certified Ethical Hacker" };
            ret.Add(new JDAttribute("Certified Ethical Hacker", AttributeType.Certification, a));

            a = new List<string> { "AWS Certified Solutions Architect", "AWS Certified Developer", "AWS Certified" };
            ret.Add(new JDAttribute("AWS Cert", AttributeType.Certification, a));

            a = new List<string> { "Google Certified Professional Cloud Architect", "" };
            ret.Add(new JDAttribute("GCP Cert", AttributeType.Certification, a));

            a = new List<string> { "Citrix Certified", "Citrix Certified Professional – Virtualization (CCP-V)", "Citrix Certified Associate – Networking (CCA-N)" };
            ret.Add(new JDAttribute("Citrix Certs", AttributeType.Certification, a));

            a = new List<string> { "CCNP Routing and Switching", "CCNP Enterprise Certification", "Cisco Certified Networking Professional" };
            ret.Add(new JDAttribute("CCNP Enterprise Certification", AttributeType.Certification, a));

            a = new List<string> { "Microsoft Certified: Azure"};
            ret.Add(new JDAttribute("Azure Certs", AttributeType.Certification, a));

            ret.Add(new JDAttribute("Certified Information Security Manager (CISM)", AttributeType.Certification));

            a = new List<string> { "TOGAF", "The Open Group Architecture Framework" };
            ret.Add(new JDAttribute("TOGAF", AttributeType.Certification, a));

            a = new List<string> { "CompTIA Security+", "CompTIA Network+" };
            ret.Add(new JDAttribute("CompTIA", AttributeType.Certification, a));



            ret.Add(new JDAttribute("C#", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("JavaScript", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Java ", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("C", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("C++", AttributeType.ProgrammingLanguage));
            a = new List<string> { "Go", "Golang" };
            ret.Add(new JDAttribute("Go", AttributeType.ProgrammingLanguage, a));
            ret.Add(new JDAttribute("R", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Swift", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("PHP", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("SQL", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Mongo", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Dart", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Kotlin", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Scala", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Groovy", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Jenkins", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Ruby", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Perl", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("MATLAB", AttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Python", AttributeType.ProgrammingLanguage));


            return ret;
        }

        public static void AnalyzeJobs()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            ALL_JD_ATTRIBUTES = GetAllJDAttributes();

            MongoClient dbClient = new MongoClient(MongoStrings.CONNECTION);
            IMongoDatabase database = dbClient.GetDatabase(MongoStrings.JOB_DB);

            var raw_collection = database.GetCollection<BsonDocument>(MongoStrings.JOB_COLLECTION);
            var kpi_collection = database.GetCollection<BsonDocument>(MongoStrings.JOB_KPI_COLLECTION);

            //find total number of documents
            long docsInCollection = raw_collection.CountDocuments(new BsonDocument());

            //figure out what the chunk indices will be
            long num_chunks = docsInCollection / MongoStrings.CHUNK_SIZE;

            if (num_chunks > 0)
            {
                int chunk_remainder = (int)(docsInCollection % MongoStrings.CHUNK_SIZE);

                int start_incrementer = 0;
                int chunk_counter = 0;

                var db_chunks = new List<Tuple<int, int>>();

                for (int i = 0; i < num_chunks; i++)
                {
                    db_chunks.Add(new Tuple<int, int>(start_incrementer, start_incrementer + MongoStrings.CHUNK_SIZE));
                    start_incrementer += MongoStrings.CHUNK_SIZE;
                }
                db_chunks.Add(new Tuple<int, int>(start_incrementer, start_incrementer + chunk_remainder));

                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = watch.Elapsed;

                // Format and display the TimeSpan value.

                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine($"Setup Complete: {elapsedTime}");

                var tmp_i = 0;

                //do we want to start in the middle?
                var chunks_to_skip = 0;

                if (chunks_to_skip > 0 && chunks_to_skip < db_chunks.Count)
                {
                    tmp_i = (chunks_to_skip * MongoStrings.CHUNK_SIZE) + 1;
                    chunk_counter = chunks_to_skip;
                    db_chunks = db_chunks.Skip(chunks_to_skip).ToList();
                }

                foreach (var chunk in db_chunks)
                {
                    // get the chunk
                    var bsonDocs = MongoExport.getSomeJDs(chunk.Item1, chunk.Item2);

                    if (bsonDocs.Count > 0)
                    {
                        foreach (var b in bsonDocs)
                        {
                            //convert to C# class object
                            var jd = BsonSerializer.Deserialize<RawJobDescription>(b);

                            //use class function to generate job kpi report
                            var jd_kpi = new JobKPI(jd);

                            //clean up dates (and search terms?)
                            //jd_kpi.Clean();

                            //add it to the list
                            var filter = Builders<BsonDocument>.Filter.Eq("_id", jd_kpi.ID);
                            var options = new ReplaceOptions { IsUpsert = true };
                            var result = kpi_collection.ReplaceOne(filter, jd_kpi.ToBsonDocument(), options);

                            //keep track of how many we've done
                            tmp_i++;
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR - EMPTY CHUNK!!!!");
                    }

                    // Get the elapsed time as a TimeSpan value.
                    ts = watch.Elapsed;

                    // Format and display the TimeSpan value.
                    elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);

                    chunk_counter++;
                    Console.WriteLine(chunk_counter.ToString() + " of " + db_chunks.Count.ToString() + " in " + elapsedTime + ". " + tmp_i.ToString() + " Jobs Analyzed.");
                }

            }
            else
            {
                Console.WriteLine("NO CHUNKS!!!");
            }

        }
    }
}
