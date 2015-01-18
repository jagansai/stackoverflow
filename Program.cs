using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Configuration;


namespace ConsoleApplicationCS
{

    // Performance is not that good now. It's taking ~4.2 secs to get all the data for an answer.
    // The async methods are not adding to any performance benefit.
    

    class Program
    {
        static string site_ = ConfigurationManager.AppSettings["sitename"];
        static string version_ = ConfigurationManager.AppSettings["version"];


        private static async Task<Type> ProcessStackOverFlowQueryAsync<Type>(string url) where Type : new()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                using (var response = await (Task<WebResponse>)request.GetResponseAsync())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        return serializer.Deserialize<Type>(reader.ReadToEnd());
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception raised when querying with the request:{0}", url));
                throw ex;
            }
        }
        private static Type ProcessStackOverFlowQuery<Type>(string url) where Type : new()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        return serializer.Deserialize<Type>(reader.ReadToEnd());
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception raised when querying with the request:{0}", url));
                throw ex;
            }
        }

        public static void QueryQuestionWithId()
        {
            int id = 124797;
            string url = "https://api.stackexchange.com/{0}/questions/{1}?site={2}";
            QuestionInfo info = ProcessStackOverFlowQuery<QuestionInfo>(string.Format(url, version_, id, site_));
            Console.WriteLine(info.ToString());
        }

        public static void QueryTaggedQuestions()
        {
            int i = 1;
            string url = "https://api.stackexchange.com/{0}/questions?page={1}&pagesize=30&order=desc&sort=creation&tagged={2}&site={3}";

            do
            {
                QuestionInfo info = ProcessStackOverFlowQuery<QuestionInfo>(string.Format(url, version_, i++, "c++", site_));
                Console.WriteLine(info.ToString());
                if (info.info.has_more)
                {
                    Console.WriteLine("Got more");
                    Console.ReadKey();
                }
                else
                {
                    break;
                }
            }while(true);
        }


        private static async Task<AnswerInfo> AnswerWithFiltersAsync(int id)
        {
            string answerFilter = ConfigurationManager.AppSettings["answerfilter"];
            string url = "https://api.stackexchange.com/{0}/answers/{1}?order=desc&sort=creation&site={2}&filter={3}";

            return await ProcessStackOverFlowQueryAsync<AnswerInfo>(string.Format(url, version_, id, site_, answerFilter));
        }

        private static Answer AnswerWithFilters(int id)
        {
            string answerFilter = ConfigurationManager.AppSettings["answerfilter"];
            string url = "https://api.stackexchange.com/{0}/answers/{1}?order=desc&sort=creation&site={2}&filter={3}";

            return ProcessStackOverFlowQuery<AnswerInfo>(string.Format(url, version_, id, site_, answerFilter)).items[0];
        }

        private static async Task<CommentInfo> CommentWithFiltersAsync(int id)
        {
            string commentfilter = ConfigurationManager.AppSettings["commentfilter"];
            string url = "https://api.stackexchange.com/{0}/comments/{1}?order=desc&sort=creation&site={2}&filter={3}";
            return await ProcessStackOverFlowQueryAsync<CommentInfo>(string.Format(url, version_, id, site_, commentfilter));
        }

        private static Comment CommentWithFilters(int id)
        {
            string commentfilter = ConfigurationManager.AppSettings["commentfilter"];
            string url = "https://api.stackexchange.com/{0}/comments/{1}?order=desc&sort=creation&site={2}&filter={3}";
            return ProcessStackOverFlowQuery<CommentInfo>(string.Format(url, version_, id, site_, commentfilter)).items[0];
        }

        

        private static void WriteCommentsToStream(IList<Task<Comment>> comments, Stream stream, Action<Stream, byte[]> writeToStream)
        {
            foreach(var comment in comments)
            {
                writeToStream(stream, Encoding.UTF8.GetBytes(comment.Result.body + "\n"));
            }
        }


        private static void WriteCommentsToStream(IList<Comment> comments, Stream stream, Action<Stream, byte[]> writeToStream)
        {
            foreach(var comment in comments)
            {
                writeToStream(stream, Encoding.UTF8.GetBytes(comment.body + "\n"));
            }
        }


        private static void WriteAnswersToStream(IList<Answer> answers, Stream stream, Action<Stream, byte[]> writeToStream)
        {
            foreach(var answer in answers)
            {
                writeToStream(stream, Encoding.UTF8.GetBytes(answer.body + "\n"));
                WriteCommentsToStream(answer.comments, stream, writeToStream);
            }
        }

        private static IList<Task<Comment>> GetComments(IList<Comment> comments)
        {
            var commentsWithBody = new List<Task<Comment>>();
            
            if (comments != null)
            {
                foreach (var comment in comments)
                {
                    //CommentWithFilters(comment.comment_id).ContinueWith<CommentInfo>( commentWithBody => {
                    //    commentsWithBody.Add(commentWithBody.Result.items[0].body);
                    var commentWithBody = Task.Factory.StartNew<Comment>(() => { return CommentWithFilters(comment.comment_id); });
                    commentsWithBody.Add(commentWithBody);
                }
            }
            return commentsWithBody;
        }

       
        private static async Task<IList<Comment>> GetCommentsAsync(IList<Comment> comments)
        {
            var commentsWithBody = new List<Comment>();
            if (comments != null)
            {
                foreach (var comment in comments)
                {
                    //CommentWithFilters(comment.comment_id).ContinueWith<CommentInfo>( commentWithBody => {
                    //    commentsWithBody.Add(commentWithBody.Result.items[0].body);
                    CommentInfo commentWithBody = await CommentWithFiltersAsync(comment.comment_id);
                    commentsWithBody.Add(commentWithBody.items[0]);
                }
            }
            return commentsWithBody;
        }

        private static void QuestionWithFilters()
        {
            string questionfilter = ConfigurationManager.AppSettings["questionfilter"];
            string url = "https://api.stackexchange.com/{0}/questions/23010736?order=desc&sort=activity&site={1}&filter={2}";

            QuestionInfo info = ProcessStackOverFlowQuery<QuestionInfo>(string.Format(url, version_, site_, questionfilter));
            
            // Query the question and if it has answers, then get the answers.
            // if it has comments, then get the comments


            FileInfo f = new FileInfo(@"output.txt");
            using (var stream = f.OpenWrite())
            {

                Func<Stream, byte[], Task> writeToStreamAsync = (outstream, byteArr) => 
                {
                    return outstream.WriteAsync(byteArr, 0, byteArr.Length); 
                };

                Action<Stream, byte[]> writeToStream = (outStream, byteArr) =>
                    {
                        outStream.Write(byteArr, 0, byteArr.Length);
                    };
                    


                // Get the data asynchronously and then write to stream.
                // This would not be huge hence writing to stream later would not be a
                // performance issue.

                int length = info.items[0].body.Length;
                writeToStream(stream, Encoding.UTF8.GetBytes(info.items[0].body + "\n"));
                //var commentsWithBody = await GetCommentsAsync(info.items[0].comments);
                //var answersWithBody = await GetAnswersAsync(info.items[0].answers);


                var commentsWithBody = GetComments(info.items[0].comments);
                var answersWithBody = GetAnswers(info.items[0].answers);
                

                // Now write everything to a file.
                WriteCommentsToStream(commentsWithBody, stream, writeToStream);
                WriteAnswersToStream(answersWithBody, stream, writeToStream);
                Console.WriteLine("Done");
            }
        }

        private static async Task<IList<Answer>> GetAnswersAsync(IList<Answer> answers)
        {
            List<Answer> answersWithBody = new List<Answer>();
            int index = 0;

            if (answers != null)
            {
                foreach (var answer in answers)
                {
                    AnswerInfo answerWithBody = await AnswerWithFiltersAsync(answer.answer_id);
                    answersWithBody.Add(answerWithBody.items[0]);
                    var commentsWithBody = await GetCommentsAsync(answer.comments);
                    answersWithBody[index++].comments = commentsWithBody;
                }
            }
            return answersWithBody;
        }

        private static IList<Answer> GetAnswers(IList<Answer> answers)
        {
            List<Answer> answersWithBody = new List<Answer>();
            var commentTasks = new List<Task<List<Comment>>>();
            
            int index = 0;

            if (answers != null)
            {
                foreach (var answer in answers)
                {
                    var task = Task.Factory.StartNew<Answer>(() =>
                    { 
                        Answer answerWithBody = AnswerWithFilters(answer.answer_id);
                        answersWithBody.Add(answerWithBody);
                        return answerWithBody;
                    }).ContinueWith<List<Comment>>((continuation) =>
                    {
                        var commentWithBody = new List<Comment>();
                        foreach(var commentTask in GetComments(continuation.Result.comments))
                        {
                            commentWithBody.Add(commentTask.Result); 
                        }
                        return commentWithBody;
                    });
                    commentTasks.Add(task);
                }
            }

            foreach(var commentTask in commentTasks)
            {
                answersWithBody[index++].comments = commentTask.Result;
            }

            return answersWithBody;
        }

        static void Main(string[] args)
        {
            try
            {
                Dictionary<int, string> methodNamesMap = new Dictionary<int, string>();
                Dictionary<int, Action> methodMaps = new Dictionary<int, Action>();

                methodNamesMap.Add(1, "QueryTaggedQuestions");
                methodNamesMap.Add(2, "QueryQuestionWithId");
                methodNamesMap.Add(3, "QuestionWithFilters");



                methodMaps.Add(1, QueryTaggedQuestions);
                methodMaps.Add(2, QueryQuestionWithId);
                methodMaps.Add(3, QuestionWithFilters);


                foreach (var method in methodNamesMap)
                {
                    Console.WriteLine("{0}:{1}", method.Key, method.Value);
                }

                int choice = Int32.Parse(Console.ReadLine());

                if (methodMaps.ContainsKey(choice))
                {

                    Utils.TimeTaken(() =>
                    {
                        methodMaps[choice]();
                    }, string.Format("Executing {0}", methodNamesMap[choice]));
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Press Enter");
            Console.ReadKey();
        }
    }
}
