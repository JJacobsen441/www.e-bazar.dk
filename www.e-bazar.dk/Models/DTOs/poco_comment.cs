using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.Extensions;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_comment
    {
        public poco_comment()
        {
            
        }
        
        public DateTime created_on { get; set; }
        public string text { get; set; }
        public float? bid { get; set; }
        public string type { get; set; }
        public long? conversation_id { get; set; }
        public string person_id { get; set; }
        public bool product_viewed_owner { get; set; }
        public bool viewed_other { get; set; }

        public virtual poco_conversation conversation_poco { get; set; }

        public poco_salesman poco_salesman { get; set; }
        public poco_customer poco_customer { get; set; }

        /*public List<comment> _GetComments(long? conversation_id)
        {
            if (conversation_id == null)
                throw new Exception("A-OK, Check");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                _db.Configuration.ProxyCreationEnabled = false;
                _db.Configuration.LazyLoadingEnabled = false;

                IQueryable<comment> _c = _db.comment
                                                .Include("person")
                                                .Where(x=>x.conversation_id == conversation_id);

                IEnumerable<comment> c = _c.AsEnumerable().ToList();

                /*poco_salesman salesman_poco = new poco_salesman();
                poco_customer customer_poco = new poco_customer();
                person per = new person();
            
                List<comment> comments = (from c in _db.comment.Include("person")
                                          where c.conversation_id == conversation_id
                                          select c)/*new
                                          {
                                              created_on = c.created_on,
                                              text = c.text,
                                              bid = c.bid,
                                              type = c.type,
                                              conversation_id = c.conversation_id,
                                              //person = c.person,//salesman_poco.GetPerson<poco_salesman>(c.person_id, true, false, false) != null ? salesman_poco.GetPerson<poco_salesman>(c.person_id, true, false, false) : customer_poco.GetPerson<poco_customer>(c.person_id, true, false, false),
                                              //pocostomer = customer_poco.GetPersonPOCO<poco_customer>(c.person_id, true, false, false),
                                              person_id = c.person_id,
                                              person = c.person,
                                              viewed_owner = c.viewed_owner,
                                              viewed_other = c.viewed_other
                                          })/.AsEnumerable()
                                          .Select(c => new comment {
                                              created_on = c.created_on,
                                              text = c.text,
                                              bid = c.bid,
                                              type = c.type,
                                              conversation_id = c.conversation_id,
                                              person = c.person, 
                                              person_id = c.person_id,
                                              viewed_owner = c.viewed_owner,
                                              viewed_other = c.viewed_other
                                          }).OrderBy(c => c.created_on).ToList();//
                if (c.IsNull())
                    throw new Exception("A-OK, handled.");
                else
                {

                }
                return c.ToList();
            
            }
        }

        public List<poco_comment> _GetCommentPOCOs(long? conversation_id)
        {
            List<comment> comments = _GetComments(conversation_id);
            List<poco_comment> list = new List<poco_comment>();
            foreach (comment com in comments)
            {
                poco_comment com_poco = new poco_comment();
                com_poco.ToPoco(com);
                list.Add(com_poco);
            }
            return list;
        }/**/

        public comment CreateComment(conversation conversation, string person_id, string message)
        {
            if (conversation == null)
                throw new Exception("A-OK, Check");
            if (conversation.Id == 0 || conversation.Id == -1)
                throw new Exception("A-OK, Check");

            comment comment = new comment();
            comment.created_on = DateTime.Now;
            comment.bid = null;
            comment.text = message;
            comment.type = "Dont Know";
            comment.conversation_id = conversation.Id;
            comment.viewed_owner = false;
            comment.viewed_other = false;
            comment.person_id = person_id;
            return comment;
        }

        public List<poco_comment> ToPocoList(ICollection<comment> comments)
        {
            if (comments == null)
                throw new Exception("A-OK, Check");

            List<poco_comment> list = new List<poco_comment>();
            foreach(comment c in comments.ToList())
            {
                poco_comment com_poco = new poco_comment();
                com_poco.ToPoco(c);
                list.Add(com_poco);
            }
            return list;
        }
        
        public void ToPoco(comment com)
        {
            if (com == null)
                throw new Exception("A-OK, Check");

            this.created_on = com.created_on;
            this.text = com.text;
            this.bid = com.bid;
            this.type = com.type;
            this.conversation_id = com.conversation_id;
            this.product_viewed_owner = com.viewed_owner;
            this.viewed_other = com.viewed_other;

            if (com.person.descriminator == "Salesman")
            {
                this.poco_salesman = new poco_salesman();
                this.poco_salesman.ToPoco<poco_salesman>(NullHelper.ComNull(com.person));
            }
            else
            {
                this.poco_customer = new poco_customer();
                this.poco_customer.ToPoco<poco_customer>(NullHelper.ComNull(com.person));
            }
            this.person_id = com.person_id;
        }
    }    
}