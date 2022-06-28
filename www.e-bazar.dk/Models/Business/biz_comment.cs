using System;
using System.Collections.Generic;
using System.Linq;
using www.e_bazar.dk.Statics;

namespace www.e_bazar.dk.Models.DTOs
{
    public class biz_comment
    {
        public biz_comment()
        {            
        }
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

        public List<dto_comment> ToDTOList(ICollection<comment> comments)
        {
            if (comments == null)
                throw new Exception("A-OK, Check");

            List<dto_comment> list = new List<dto_comment>();
            foreach(comment c in comments.ToList())
            {
                biz_comment biz = new biz_comment();
                dto_comment _dto = new dto_comment();
                _dto = biz.ToDTO(c);
                list.Add(_dto);
            }
            return list;
        }
        
        public dto_comment ToDTO(comment com)
        {
            if (com == null)
                throw new Exception("A-OK, Check");

            dto_comment dto = new dto_comment();

            dto.created_on = com.created_on;
            dto.text = com.text;
            dto.bid = com.bid;
            dto.type = com.type;
            dto.conversation_id = com.conversation_id;
            dto.product_viewed_owner = com.viewed_owner;
            dto.viewed_other = com.viewed_other;

            if (com.person.descriminator == "Salesman")
            {
                biz_salesman biz = new biz_salesman();
                dto.dto_salesman = biz.ToDTO<dto_salesman>(NullHelper.ComNull(com.person));
            }
            else
            {
                biz_customer biz = new biz_customer();
                dto.dto_customer = biz.ToDTO<dto_customer>(NullHelper.ComNull(com.person));
            }
            dto.person_id = com.person_id;

            return dto;
        }
    }    
}