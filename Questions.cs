using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplicationCS
{


    public class Info
    {
        public bool has_more { get; set; }
        public int quota_max { get; set; }
        public int quota_remaining { get; set; }
    }


    public class Owner
    {
        public int reputation { get; set; }
        public int user_id { get; set; }
        public string user_type { get; set; }
        public int accept_rate { get; set; }
        public string profile_image { get; set; }
        public string display_name { get; set; }
        public string link { get; set; }
        public Info info { get; set; }

        public override string ToString()
        {
            return string.Format("display_name:{0}", display_name);
        }

    }

    public class Answer
    {
        public IList<Comment> comments { get; set; }
        public Owner owner { get; set; }
        public bool is_accepted { get; set; }
        public int score { get; set; }
        public int last_activity_date { get; set; }
        public int last_edit_date { get; set; }
        public int creation_date { get; set; }
        public int answer_id { get; set; }
        public int question_id { get; set; }
        public string body { get; set; }
        public Info info { get; set; }
    }

    public class AnswerInfo
    {
        public IList<Answer> items { get; set; }
        public Info info { get; set; }
    }
         


    public class ReplyToUser
    {
        public int reputation { get; set; }
        public int user_id { get; set; }
        public string user_type { get; set; }
        public int accept_rate { get; set; }
        public string profile_image { get; set; }
        public string display_name { get; set; }
        public string link { get; set; }
        public Info info { get; set; }
    }


    public class Comment
    {
        public Owner owner { get; set; }
        public bool edited { get; set; }
        public int score { get; set; }
        public int creation_date { get; set; }
        public int post_id { get; set; }
        public int comment_id { get; set; }
        public string body { get; set; }
    }

    public class Question
    {
        public IList<Answer> answers { get; set; }
        public IList<string> tags { get; set; }
        public Owner owner { get; set; }
        public IList<Comment> comments { get; set; }
        public bool is_answered { get; set; }
        public int view_count { get; set; }
        public int answer_count { get; set; }
        public int score { get; set; }
        public int last_activity_date { get; set; }
        public int creation_date { get; set; }
        public int last_edit_date { get; set; }
        public int question_id { get; set; }
        public string link { get; set; }
        public string title { get; set; }
        public string body { get; set; }


        public override string ToString()
        {

            DateTime creation_dt = Utils.ConvertFromUnix(creation_date);
            return string.Format(@"Owner:{0}\n,
                            creation_date:{1}\n,
                            title:{2}\n
                            question_id:{3}", 
                                            owner.ToString(), 
                                            Utils.Elapsed(creation_dt), 
                                            title,
                                            question_id).Replace("\\n", Environment.NewLine);
        }

    }

    public class CommentInfo
    {
        public IList<Comment> items { get; set; }
        public Info info { get; set; }
    }

    public class QuestionInfo
    {
        public IList<Question> items { get; set; }
        public Info info { get; set; }
 


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach(var item in items)
            {
                sb.Append(item.ToString());
            }

            return sb.ToString();
        }
    } 
}
