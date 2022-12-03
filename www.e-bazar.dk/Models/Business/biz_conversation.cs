using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.SharedClasses;
using www.e_bazar.dk.Statics;

namespace www.e_bazar.dk.Models.DTOs
{
    public class biz_conversation
    {
        public biz_conversation()
        {            
        }
        
        public dto_person GetPersonStart(dto_conversation dto)
        {
            return dto.comment_dtos.FirstOrDefault().dto_salesman != null ? 
                (dto_person)dto.comment_dtos.FirstOrDefault().dto_salesman : 
                (dto_person)dto.comment_dtos.FirstOrDefault().dto_customer;
        }

        public dto_person GetPersonOther(dto_conversation dto)
        {
            if (dto.comment_dtos.Count() == 0)
                return null;

            dto_salesman salesman_poco = dto.product_dto != null ? dto.product_dto.booth_dto.salesman_dto :
                                            dto.collection_dto != null ? dto.collection_dto.booth_dto.salesman_dto :
                                            dto.booth_dto.salesman_dto;
            if (CurrentUser.Inst().CurrentUserID == salesman_poco.person_id)
                return GetPersonStart(dto);//vil altid være den første
            else
                return salesman_poco;
        }

        public bool Viewed(dto_conversation dto, bool is_product_owner)
        {
            bool viewed = true;
            foreach(dto_comment com in dto.comment_dtos)
            {
                if (!com.product_viewed_owner && is_product_owner)
                    viewed = false;
                if (!com.viewed_other && !is_product_owner)
                    viewed = false;
            }
            return viewed;
        }

        public void SetViewed(dto_conversation dto, bool is_owner)
        {
            using (EbazarDB db = new EbazarDB())
            {
                conversation conn = db.conversation.Where(c => c.Id == dto.conversation_id).FirstOrDefault();
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

                biz_salesman person_poco = new biz_salesman();
                bool is_salesman = person_poco.IsSalesman(person_id, CurrentUser.Inst().CurrentType);

                IQueryable<conversation> _c = _db.conversation
                                                        .Include("comment")
                                                        .Include("comment.person")
                                                        .Include("product")
                                                        .Include("collection")
                                                        .Include("booth")
                                                        .Include("booth.person")
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

        public dto_conversation GetConversationDTO(long product_id, int collection_id, int booth_id, string person_id, TYPE type)
        {
            biz_conversation biz = new biz_conversation();
            dto_conversation dto = new dto_conversation();
            conversation con = GetConversation(product_id, collection_id, booth_id, person_id, type);
            if (con == null)
                return new dto_conversation();
                
            dto = biz.ToDTO(con);
            return dto;            
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
                                        .Include("booth.person")
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

        public List<dto_conversation> GetConversationsPersonDTO(string person_id, bool is_salesman, bool withboothsalesman)
        {
            List<dto_conversation> list = new List<dto_conversation>();
            List<conversation> cons = GetConversationsPerson(person_id, is_salesman, withboothsalesman);
            
            list = this.ToDTOList(cons);
            return list;            
        }

        public string SaveMessage(long? conversation_id, long id, string person_id, string message, TYPE type)
        {
            if (conversation_id == null)
                throw new Exception("A-OK, Check");

            //EbazarDB _db = DAL.GetInstance().GetContext();
            using (EbazarDB _db = new EbazarDB())
            {
                CurrentUser user = CurrentUser.Inst();
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

                    biz_comment poco_com = new biz_comment();
                    comment com = poco_com.CreateComment(con, person_id, message);

                _db.comment.Add(com);
                    con.modified = now;
                _db.SaveChanges();
                
                return con.person_id;            
            }
        }

        public List<dto_conversation> ToDTOList(ICollection<conversation> cons)
        {
            if (cons == null)
                throw new Exception("A-OK, Check");

            List<dto_conversation> list = new List<dto_conversation>();
            foreach (conversation c in cons.ToList())
            {
                biz_conversation biz = new biz_conversation();
                dto_conversation _dto = biz.ToDTO(c);
                list.Add(_dto);
            }
            return list;
        }

        public dto_conversation ToDTO(conversation con)
        {
            if (con == null)
                throw new Exception("A-OK, Check");

            dto_conversation dto = new dto_conversation();

            biz_comment com_biz = new biz_comment();
            dto.conversation_id = con.Id;
            dto.created_on = con.created_on;
            dto.modified = con.modified;
            dto.product_id = con.product_id;
            dto.collection_id = con.collection_id;
            dto.booth_id = con.booth_id;
            dto.person_id = con.person_id;
            
            if(con.comment != null)
                dto.comment_dtos = com_biz.ToDTOList(con.comment.ToList());

            if (con.product != null)
            {
                biz_product biz = new biz_product();
                dto.product_dto = new dto_product();
                dto.product_dto = biz.ToDTO(NullHelper.ConNull(con.product), null, "");
            }
            
            if (con.collection != null)
            {
                biz_collection biz = new biz_collection();
                dto.collection_dto = new dto_collection();
                dto.collection_dto = biz.ToDTO(NullHelper.ConNull(con.collection), null, "");
            }
            
            if (con.booth != null)
            {
                biz_booth biz = new biz_booth();
                dto.booth_dto = new dto_booth();
                dto.booth_dto = biz.ToDTO(NullHelper.ConNull(con.booth), null);
            }
            
            //dto.other = this.GetPersonOther(dto);
            
            return dto;
        }
    }
}