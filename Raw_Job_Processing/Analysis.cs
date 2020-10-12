using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using static Analytics.Constants;

namespace Raw_Job_Processing
{
    public static class JobAnalysis
    {
        public static List<JDAttribute> ALL_JD_ATTRIBUTES;

        public static List<JDAttribute> GetAllJDAttributes()
        {
            var ret = new List<JDAttribute>();

            ret.Add(new JDAttribute("Help Desk", JDAttributeType.Keyword));

            ret.Add(new JDAttribute("Windows", JDAttributeType.Keyword));
            ret.Add(new JDAttribute("Linux", JDAttributeType.Keyword));
            ret.Add(new JDAttribute("OSX", JDAttributeType.Keyword));
            ret.Add(new JDAttribute("Docker", JDAttributeType.Keyword));

            var a = new List<string> { "Kubernetes", "K8" };
            ret.Add(new JDAttribute("Kubernetes", JDAttributeType.Keyword, a));

             a = new List<string> { "Machine Learning" };
            ret.Add(new JDAttribute("Machine Learning", JDAttributeType.Keyword, a));

             a = new List<string> { "Artificial Intelligence" };
            ret.Add(new JDAttribute("Artificial Intelligence", JDAttributeType.Keyword, a));

            a = new List<string>{"AWS","Amazon Web" };
            ret.Add(new JDAttribute("AWS", JDAttributeType.Keyword, a));

            a = new List<string> { "DevOps", "DevSecOps" };
            ret.Add(new JDAttribute("DevOps", JDAttributeType.Keyword, a));

            a = new List<string> { "Azure", "Microsoft Cloud" };
            ret.Add(new JDAttribute("Azure", JDAttributeType.Keyword, a));

            a = new List<string> { "GCP", "Google Cloud" };
            ret.Add(new JDAttribute("GCP", JDAttributeType.Keyword, a));

            a = new List<string> { "PMP", "Project Management Professional" };
            ret.Add(new JDAttribute("Project Management Professional (PMP)", JDAttributeType.Certification, a));

            ret.Add(new JDAttribute("ITIL", JDAttributeType.Certification));

            a = new List<string> { "Certified Scrum Master", "CSM" };
            ret.Add(new JDAttribute("Certified Scrum Master (CSM)", JDAttributeType.Certification, a));

            a = new List<string> { "CEH", "Certified Ethical Hacker" };
            ret.Add(new JDAttribute("Certified Ethical Hacker", JDAttributeType.Certification, a));

            a = new List<string> { "CISSP", "Certified Information Systems Security Professional" };
            ret.Add(new JDAttribute("Certified Information Systems Security Professional (CISSP)", JDAttributeType.Certification, a));

            a = new List<string> { "Certified in Risk and Information Systems Control", "CRISC" };
            ret.Add(new JDAttribute("Certified in Risk and Information Systems Control (CRISC)", JDAttributeType.Certification, a));

            a = new List<string> { "Certified Information Systems Auditor", "CISA" };
            ret.Add(new JDAttribute("Certified Information Systems Auditor (CISA)", JDAttributeType.Certification, a));

            a = new List<string> { "CEH", "Certified Ethical Hacker" };
            ret.Add(new JDAttribute("Certified Ethical Hacker", JDAttributeType.Certification, a));

            a = new List<string> { "AWS Certified Solutions Architect", "AWS Certified Developer", "AWS Certified" };
            ret.Add(new JDAttribute("AWS Cert", JDAttributeType.Certification, a));

            a = new List<string> { "Google Certified Professional Cloud Architect", "Google Certified" };
            ret.Add(new JDAttribute("GCP Cert", JDAttributeType.Certification, a));

            a = new List<string> { "Citrix Certified", "Citrix Certified Professional – Virtualization (CCP-V)", "Citrix Certified Associate – Networking (CCA-N)" };
            ret.Add(new JDAttribute("Citrix Certs", JDAttributeType.Certification, a));

            a = new List<string> { "CCNP Routing and Switching", "CCNP Enterprise Certification", "Cisco Certified Networking Professional" };
            ret.Add(new JDAttribute("CCNP Enterprise Certification", JDAttributeType.Certification, a));

            a = new List<string> { "Microsoft Certified: Azure"};
            ret.Add(new JDAttribute("Azure Certs", JDAttributeType.Certification, a));

            ret.Add(new JDAttribute("Certified Information Security Manager (CISM)", JDAttributeType.Certification));

            a = new List<string> { "TOGAF", "The Open Group Architecture Framework" };
            ret.Add(new JDAttribute("TOGAF", JDAttributeType.Certification, a));

            a = new List<string> { "CompTIA Security+", "CompTIA Network+" };
            ret.Add(new JDAttribute("CompTIA", JDAttributeType.Certification, a));



            ret.Add(new JDAttribute("C#", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("JavaScript", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Java ", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("C", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("C++", JDAttributeType.ProgrammingLanguage));
            a = new List<string> { "Go", "Golang" };
            ret.Add(new JDAttribute("Go", JDAttributeType.ProgrammingLanguage, a));
            ret.Add(new JDAttribute("R", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Swift", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("PHP", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("SQL", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Mongo", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Dart", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Kotlin", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Scala", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Groovy", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Jenkins", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Ruby", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Perl", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("MATLAB", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Python", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Flask", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Django", JDAttributeType.ProgrammingLanguage));
            ret.Add(new JDAttribute("Tensor", JDAttributeType.ProgrammingLanguage));


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
                if (chunk_remainder > 0)
                    db_chunks.Add(new Tuple<int, int>(start_incrementer, start_incrementer + chunk_remainder));

                Helpers.printTimeStatus(watch.Elapsed, "Setup Complete:");

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
                    var bsonDocs = MongoExport.getBSONDocs(chunk.Item1, chunk.Item2, MongoStrings.JOB_DB, MongoStrings.JOB_COLLECTION);

                    if (bsonDocs.Count > 0)
                    {
                        foreach (var b in bsonDocs)
                        {
                            //convert to C# class object
                            var jd = BsonSerializer.Deserialize<RawJobDescription>(b);

                            //use class function to generate job kpi report
                            var jd_kpi = new JobKPI(jd);

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
                    chunk_counter++;
                    Helpers.printTimeStatus(watch.Elapsed, chunk_counter.ToString() + " of " + db_chunks.Count.ToString() + " in", tmp_i.ToString() + " Jobs Analyzed.");
                }
            }
            else
            {
                Console.WriteLine("NO CHUNKS!!!");
            }

        }
    }
}
