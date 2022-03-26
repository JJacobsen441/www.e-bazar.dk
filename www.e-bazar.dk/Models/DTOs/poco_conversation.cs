using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.Models.DataAccess;
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
            EbazarDB _db = DAL.GetInstance().GetContext();

            //using (EbazarDB db = new EbazarDB())
            //{
            poco_comment comment_poco = new poco_comment();
            poco_product product_poco = new poco_product(false);
            poco_product product_forowner = new poco_product(false);
            poco_collection collection_poco = new poco_collection();
            poco_collection collection_forowner = new poco_collection();
            poco_booth booth_poco = new poco_booth();
            poco_salesman person_poco = new poco_salesman();
            bool is_salesman = person_poco.IsSalesman(person_id, CurrentUser.GetInstance().CurrentType);
            
                conversation con = (from c in _db.conversation
                                    where c.product_id == product_id && (c.person_id == person_id || is_salesman) ||
                                          c.collection_id == collection_id && (c.person_id == person_id || is_salesman) ||
                                          c.booth_id == booth_id && (c.person_id == person_id || is_salesman)
                                    select c).AsEnumerable()
                                    .Select(x => new conversation
                                    {
                                        Id = x.Id,
                                        created_on = x.created_on,
                                        product_id = x.product_id,
                                        collection_id = x.collection_id,
                                        booth_id = x.booth_id,
                                        person_id = x.person_id,
                                        modified = x.modified,
                                        comment = comment_poco.GetComments(x.Id),
                                        product = x.product_id != null ? product_poco.GetProduct((int)x.product_id, true, false, false, false) : null,
                                        collection = x.collection_id != null ? collection_poco.GetCollection(x.collection_id, false, true, false, false, false) : null,
                                        booth = x.booth_id != null ? booth_poco.GetBooth(x.booth_id, "", "", true, false, false, false, false, false, true) : null

                                    })/*.Cast<conversation>()/*.AsEnumerable()
                                        .Select(c => new conversation {
                                            Id = c.Id,
                                            created_on = c.created_on,
                                            product_id = product_id,
                                            collection_id = collection_id,
                                            booth_id = booth_id,
                                            person_id = c.person_id,
                                            modified = c.modified,
                                            comment = comment_poco.GetComments(c.Id),
                                            product = type == TYPE.PRODUCT ? product_poco.GetProduct((int)c.product_id, true, false, false, false) : null,
                                            collection = type == TYPE.COLLECTION ? collection_poco.GetCollection((int)c.collection_id, false, true, false, false, false) : null,
                                            booth = type == TYPE.BOOTH ? booth_poco.GetBooth((int)c.booth_id, "", "", true, false, false, false, false, false, true) : null
                                        })*/.FirstOrDefault();
                if (con == null)
                    return new conversation();
                return con;
            //}
        }
        public poco_conversation GetConversationPOCO(long product_id, int collection_id, int booth_id, string person_id, TYPE type)
        {
            //using (EbazarDB db = new EbazarDB())
            //{
                poco_conversation con_poco = new poco_conversation();
                conversation con = GetConversation(product_id, collection_id, booth_id, person_id, type);
                if (con == null)
                    return new poco_conversation();
                
                con_poco.ToPoco(con);
                return con_poco;                
            //}
        }
        
        public List<conversation> GetConversations(long product_id, int collection_id, int booth_id, TYPE type)
        {
            EbazarDB _db = DAL.GetInstance().GetContext();

            //using (EbazarDB db = new EbazarDB())
            //{

            poco_comment comment_poco = new poco_comment();
                poco_product product_poco = new poco_product(false);
                poco_collection collection_poco = new poco_collection();
                poco_booth booth_poco = new poco_booth();

                List<conversation> conversations = (from c in _db.conversation
                                                    where (type == TYPE.PRODUCT && c.product_id == product_id) ||
                                                          (type == TYPE.COLLECTION && c.collection_id == collection_id) ||
                                                          (type == TYPE.BOOTH && c.booth_id == booth_id)
                                                    select c).AsEnumerable()
                                    .Select(x => new conversation
                                    {
                                        Id = x.Id,
                                        created_on = x.created_on,
                                        product_id = x.product_id,
                                        collection_id = x.collection_id,
                                        booth_id = x.booth_id,
                                        person_id = x.person_id,
                                        modified = x.modified,
                                        comment = comment_poco.GetComments(x.Id),
                                        product = x.product_id != null ? product_poco.GetProduct((int)x.product_id, true, false, false, false) : null,
                                        collection = x.collection_id != null ? collection_poco.GetCollection(x.collection_id, false, true, false, false, false) : null,
                                        booth = x.booth_id != null ? booth_poco.GetBooth(x.booth_id, "", "", true, false, false, false, false, false, true) : null

                                    })/*.Cast<conversation>()/*.AsEnumerable()
                                               .Select(c=>new conversation {
                                                   Id = c.Id,
                                                   created_on = c.created_on,
                                                   product_id = c.product_id,
                                                   collection_id = c.collection_id,
                                                   booth_id = c.booth_id,
                                                   person_id = c.person_id,
                                                   modified = c.modified,
                                                   comment = comment_poco.GetComments(c.Id),
                                                   product = type == TYPE.PRODUCT ? product_poco.GetProduct((int)c.product_id, true, false, false, false) : null,
                                                   collection = type == TYPE.COLLECTION ? collection_poco.GetCollection((int)c.collection_id, false, true, false, false, false) : null,
                                                   booth = type == TYPE.BOOTH ? booth_poco.GetBooth((int)c.booth_id, "", "", true, false, false, false, false, false, true) : null
                                               })*/.ToList();
                if (conversations == null)
                    throw new Exception("A-OK, Handled.");
                return conversations;
            //}
        }
        public List<poco_conversation> GetConversationsPOCO(long product_id, int collection_id, int booth_id, TYPE type)
        {
            List<poco_conversation> list = new List<poco_conversation>();
            List<conversation> conversations = GetConversations(product_id, collection_id, booth_id, type);
            
            list = this.ToPocoList(conversations);
            return list;
        }
        
        public List<conversation> GetConversationsPerson(string person_id, bool is_salesman, bool withboothsalesman)
        {
            if (string.IsNullOrEmpty(person_id))
                throw new Exception("A-OK, Check");

            EbazarDB _db = DAL.GetInstance().GetContext();
            //using (EbazarDB db = new EbazarDB())
            //{

            poco_comment comment_poco = new poco_comment();
            poco_product product_poco = new poco_product(false);
            poco_collection collection_poco = new poco_collection();
            poco_booth booth_poco = new poco_booth();
            //poco_person person_poco = CurrentUser.GetInstance().GetCurrentUser(false, true, true);

            List<conversation> con = (from c in _db.conversation
                                      where (c.person_id == person_id) ||
                                      (c.booth != null && c.booth.person_id == person_id && is_salesman) ||
                                      (c.collection != null && c.collection.booth.person_id == person_id && is_salesman) ||
                                      (c.product != null && c.product.booth.person_id == person_id && is_salesman)
                                      select c).AsEnumerable()
                                    .Select(x => new conversation
                                    {
                                          Id = x.Id,
                                          created_on = x.created_on,
                                          product_id = x.product_id,
                                          collection_id = x.collection_id,
                                          booth_id = x.booth_id,
                                          person_id = x.person_id,
                                          modified = x.modified,
                                            comment = comment_poco.GetComments(x.Id),
                                            product = x.product_id != null ? product_poco.GetProduct((int)x.product_id, true, false, false, false) : null,
                                            collection = x.collection_id != null ? collection_poco.GetCollection(x.collection_id, false, true, false, withboothsalesman, false) : null,
                                            booth = x.booth_id != null ? booth_poco.GetBooth(x.booth_id, "", "", true, false, false, false, false, false, true) : null

                                    })/*.Cast<conversation>()/*.AsEnumerable()
                                      .Select(c=> new conversation{
                                          Id = c.Id,
                                          created_on = c.created_on,
                                          product_id = c.product_id,
                                          collection_id = c.collection_id,
                                          booth_id = c.booth_id,
                                          person_id = c.person_id,
                                          modified = c.modified,
                                          comment = comment_poco.GetComments(c.Id),
                                          product = c.product_id != null ? product_poco.GetProduct((int)c.product_id, true, false, false, false) : null,
                                          collection = c.collection_id != null ? collection_poco.GetCollection(c.collection_id, false, true, false, withboothsalesman, false) : null,
                                          booth = c.booth_id != null ? booth_poco.GetBooth(c.booth_id, "", "", true, false, false, false, false, false, true) : null
                                      })*/.ToList();
                if (con == null)
                    throw new Exception("A-OK, Handled.");
                
                return con;
            //}
        }

        public List<poco_conversation> GetConversationsPersonPOCO(string person_id, bool is_salesman, bool withboothsalesman)
        {
            if (string.IsNullOrEmpty(person_id))
                throw new Exception("A-OK, Check");

            List<poco_conversation> list = new List<poco_conversation>();
            List<conversation> cons = GetConversationsPerson(person_id, is_salesman, withboothsalesman);
            
            list = this.ToPocoList(cons);
            return list;            
        }

        public string SaveMessage(long? conversation_id, long id, string person_id, string message, TYPE type)
        {
            if (conversation_id == null)
                throw new Exception("A-OK, Check");

            EbazarDB _db = DAL.GetInstance().GetContext();
            //using (EbazarDB db = new EbazarDB())
            //{

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
            _db.Dispose();

                return con.person_id;
            //}
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
        private person Null(person per)
        {
            if (per.booth != null)
                per.booth = null;
            if (per.boothrating != null)
                per.boothrating = null;
            if (per.comment != null)
                per.comment = null;
            if (per.conversation != null)
                per.conversation = null;
            if (per.favorites_collection != null)
                per.favorites_collection = null;
            if (per.favorites_product != null)
                per.favorites_product = null;
            if (per.following != null)
                per.following = null;
            return per;
        }
        private booth Null(booth b)
        {
            if (b.boothrating != null)
                b.boothrating = null;
            if (b.category_main != null)
                b.category_main = null;
            if (b.collection != null)
                b.collection = null;
            if (b.conversation != null)
                b.conversation = null;
            if (b.foldera != null)
                b.foldera = null;
            if (b.followers != null)
                b.followers = null;
            if (b.person != null)
                b.person = Null(b.person);
            if (b.product != null)
                b.product = null;
            if (b.region != null)
                b.region = null;
            return b;
        }
        private product Null(product p)
        {
            //if (p.booth != null)
            //    p.booth = null;
            if (p.category_main != null)
                p.category_main = null;
            if (p.category_second != null)
                p.category_second = null;
            if (p.collection != null)
                p.collection = null;
            if (p.conversation != null)
                p.conversation = null;
            if (p.favorites != null)
                p.favorites = null;
            if (p.foldera != null)
                p.foldera = null;
            if (p.folderb != null)
                p.folderb = null;
            if (p.image != null)
                p.image = null;
            if (p.product_param != null)
                p.product_param = null;
            return p;
        }
        private collection Null(collection c)
        {
            //if (c.booth != null)
            //    c.booth = null;
            if (c.category_main != null)
                c.category_main = null;
            if (c.category_second != null)
                c.category_second = null;
            if (c.product != null)
                c.product = null;
            if (c.conversation != null)
                c.conversation = null;
            if (c.favorites != null)
                c.favorites = null;
            if (c.foldera != null)
                c.foldera = null;
            if (c.folderb != null)
                c.folderb = null;
            if (c.image != null)
                c.image = null;
            if (c.collection_param != null)
                c.collection_param = null;
            return c;
        }
        public void ToPoco(conversation con)
        {
            if (con == null)
                throw new Exception("A-OK, Check");

            //using (EbazarDB db = new EbazarDB())
            //{

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
                this.product_poco.ToPoco(Null(con.product), null, "");
            }
            if (con.collection != null)
            {
                this.collection_poco = new poco_collection();
                this.collection_poco.ToPoco(Null(con.collection), null, "");
            }
            if (con.booth != null)
            {
                this.booth_poco = new poco_booth();
                this.booth_poco.ToPoco(Null(con.booth), null);
            }
            //}
        }
    }
}