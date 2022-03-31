using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.Extensions;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Models.DTOs
{
    public class poco_conversation
    {
        //private EbazarDB db;
        public long? conversation_id { get; set; }
        public DateTime created_on { get; set; }
        public DateTime modified { get; set; }

        public long? product_id { get; set; }
        public long? collection_id { get; set; }
        public int? booth_id { get; set; }
        public string person_id { get; set; }

        public virtual List<poco_comment> comment_pocos { get; set; }
        public virtual poco_product product_poco { get; set; }
        public virtual poco_collection collection_poco { get; set; }
        public virtual poco_booth booth_poco { get; set; }
        

        public poco_conversation()
        {
            //db = new EbazarDB();
            conversation_id = null;
        }
        /*~poco_conversation()
        {
            db?.Dispose();
        }*/

        public poco_person GetPersonStart()
        {
            return comment_pocos.FirstOrDefault().poco_salesman != null ? (poco_person)comment_pocos.FirstOrDefault().poco_salesman : (poco_person)comment_pocos.FirstOrDefault().poco_customer;
        }

        public poco_person GetPersonOther()
        {
            poco_person other = null;            
            poco_salesman salesman_poco =   product_poco != null ? product_poco.booth_poco.salesman_poco : 
                                            collection_poco != null ? collection_poco.booth_poco.salesman_poco :
                                            booth_poco.salesman_poco;
            if (comment_pocos != null && CurrentUser.GetInstance().CurrentUserID == salesman_poco.person_id)
                return GetPersonStart();//vil altid være den første
            else
                return salesman_poco;
        }

        public bool Viewed(bool is_product_owner)
        {
            bool viewed = true;
            foreach(poco_comment com in comment_pocos)
            {
                if (!com.product_viewed_owner && is_product_owner)
                    viewed = false;
                if (!com.viewed_other && !is_product_owner)
                    viewed = false;
            }
            return viewed;
        }

        public void SetViewed(bool is_owner)
        {
            using (EbazarDB db = new EbazarDB())
            {
                conversation conn = db.conversation.Where(c => c.Id == this.conversation_id).FirstOrDefault();
                if (conn == null)
                    return;
                if (conn.comment == null)
                    return;
                
                foreach (comment com in conn.comment)
                {
                    if (is_owner)
                        com.viewed_owner = true;
                    if (!is_owner)
                        com.viewed_other = true;
                }
                
                db.SaveChanges();
            }
        }

        public conversation GetConversation(long product_id, int collection_id, int booth_id, string person_id, TYPE type)
        {
            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                _db.Configuration.ProxyCreationEnabled = false;
                _db.Configuration.LazyLoadingEnabled = false;

                poco_salesman person_poco = new poco_salesman();
                bool is_salesman = person_poco.IsSalesman(person_id, CurrentUser.GetInstance().CurrentType);

                IQueryable<conversation> _c = _db.conversation
                                                        .Include("comment")
                                                        .Include("comment.person")
                                                        .Include("product")
                                                        .Include("collection")
                                                        .Include("booth")
                                                        .Where(x=> x.product_id == product_id && (x.person_id == person_id || is_salesman) ||
                                          x.collection_id == collection_id && (x.person_id == person_id || is_salesman) ||
                                          x.booth_id == booth_id && (x.person_id == person_id || is_salesman));

                conversation c = _c.AsEnumerable().FirstOrDefault();
                                
                if (c.IsNull())
                    return new conversation();
                else
                {
                    foreach (comment com in c.comment)
                        NullHelper.ComNull(com.person);
                }
                return c;            
            }
        }

        public poco_conversation GetConversationPOCO(long product_id, int collection_id, int booth_id, string person_id, TYPE type)
        {
            poco_conversation con_poco = new poco_conversation();
            conversation con = GetConversation(product_id, collection_id, booth_id, person_id, type);
            if (con == null)
                return new poco_conversation();
                
            con_poco.ToPoco(con);
            return con_poco;            
        }        
        
        public List<conversation> GetConversationsPerson(string person_id, bool is_salesman, bool withboothsalesman)
        {
            if (string.IsNullOrEmpty(person_id))
                throw new Exception("A-OK, Check");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                _db.Configuration.ProxyCreationEnabled = false;
                _db.Configuration.LazyLoadingEnabled = false;

                IQueryable<conversation> _c = _db.conversation
                                        .Include("comment")
                                        .Include("comment.person")
                                        .Include("product")
                                        .Include("collection")
                                        .Include("booth")
                                        .Where(x=>(x.person_id == person_id) ||
                                      (x.booth != null && x.booth.person_id == person_id && is_salesman) ||
                                      (x.collection != null && x.collection.booth.person_id == person_id && is_salesman) ||
                                      (x.product != null && x.product.booth.person_id == person_id && is_salesman));

                IEnumerable<conversation> c = _c.AsEnumerable().ToList();
                                
                if (c == null)
                    throw new Exception("A-OK, Handled.");
                else
                {
                    foreach(conversation __c in c)
                    {
                        foreach(comment com in __c.comment)
                            NullHelper.ComNull(com.person);
                    }
                }
                
                return c.ToList();            
            }
        }

        public List<poco_conversation> GetConversationsPersonPOCO(string person_id, bool is_salesman, bool withboothsalesman)
        {
            List<poco_conversation> list = new List<poco_conversation>();
            List<conversation> cons = GetConversationsPerson(person_id, is_salesman, withboothsalesman);
            
            list = this.ToPocoList(cons);
            return list;            
        }

        public string SaveMessage(long? conversation_id, long id, string person_id, string message, TYPE type)
        {
            if (conversation_id == null)
                throw new Exception("A-OK, Check");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                CurrentUser user = CurrentUser.GetInstance();
                DateTime now = DateTime.Now;
                bool new_conversation = false;
                conversation con = conversation_id != null ? _db.conversation.Where(c => c.Id == conversation_id).FirstOrDefault() : null;
                if (con == null)
                {
                    new_conversation = true;
                    con = new conversation();
                    con.created_on = now;
                    con.modified = now;
                    //con.product_id = id;
                    if (type == TYPE.PRODUCT)
                    {
                        if (user.OwnsProduct(id))
                            throw new Exception("A-OK, handled.");
                        con.product_id = id;
                        con.collection_id = null;
                        con.booth_id = null;
                        product p = _db.product.Where(x => x.Id == id).FirstOrDefault();
                        if (p == null)
                            throw new Exception("dao_conversation.SaveMessage > c NULL");
                        con.product = p;
                    }
                    else if (type == TYPE.COLLECTION)
                    {
                        if (user.OwnsCollection((int)id))
                            throw new Exception("A-OK, handled.");
                        con.product_id = null;
                        con.collection_id = (int)id;
                        con.booth_id = null;
                        collection c = _db.collection.Where(x => x.Id == id).FirstOrDefault();
                        if (c == null)
                            throw new Exception("dao_conversation.SaveMessage > c NULL");
                        con.collection = c;
                    }
                    else
                    {
                        if (user.OwnsBooth((int)id))
                            throw new Exception("A-OK, handled.");
                        con.product_id = null;
                        con.collection_id = null;
                        con.booth_id = (int)id;
                        booth b = _db.booth.Where(x => x.Id == id).FirstOrDefault();
                        if (b == null)
                            throw new Exception("dao_conversation.SaveMessage > b NULL");
                        con.booth = b;
                    }

                    person per = _db.person.Where(x => x.Id == person_id).FirstOrDefault();
                    if (per == null)
                        throw new Exception("dao_conversation.SaveMessage > per NULL");
                    con.person_id = person_id;
                    con.person = per;
                }
            
                if (new_conversation)
                    con = _db.conversation.Add(con);
                _db.SaveChanges();

                    poco_comment poco_com = new poco_comment();
                    comment com = poco_com.CreateComment(con, person_id, message);

                _db.comment.Add(com);
                    con.modified = now;
                _db.SaveChanges();
                
                return con.person_id;            
            }
        }
        public List<poco_conversation> ToPocoList(ICollection<conversation> cons)
        {
            if (cons == null)
                throw new Exception("A-OK, Check");

            List<poco_conversation> list = new List<poco_conversation>();
            foreach (conversation c in cons.ToList())
            {
                poco_conversation con_poco = new poco_conversation();
                con_poco.ToPoco(c);
                list.Add(con_poco);
            }
            return list;
        }

        public void ToPoco(conversation con)
        {
            if (con == null)
                throw new Exception("A-OK, Check");

            poco_comment com_poco = new poco_comment();
            this.conversation_id = con.Id;
            this.created_on = con.created_on;
            this.modified = con.modified;
            this.product_id = con.product_id;
            this.collection_id = con.collection_id;
            this.booth_id = con.booth_id;
            this.person_id = con.person_id;
            if(con.comment != null)
                this.comment_pocos = com_poco.ToPocoList(con.comment.ToList());
            if (con.product != null)
            {
                this.product_poco = new poco_product(false);
                this.product_poco.ToPoco(NullHelper.ConNull(con.product), null, "");
            }
            if (con.collection != null)
            {
                this.collection_poco = new poco_collection();
                this.collection_poco.ToPoco(NullHelper.ConNull(con.collection), null, "");
            }
            if (con.booth != null)
            {
                this.booth_poco = new poco_booth();
                this.booth_poco.ToPoco(NullHelper.ConNull(con.booth), null);
            }            
        }
    }
}