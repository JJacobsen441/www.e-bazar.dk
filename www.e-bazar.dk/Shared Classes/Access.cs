using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using www.e_bazar.dk.Statics;

namespace www.e_bazar.dk.SharedClasses
{
    public class Elem
    {
        private int counter = 100;
        private bool first = true;
        public string Guid { get; set; }
        public string Place { get; set; }
        private Elem()
        {
        }
        public Elem(string place, string guid)
        {
            Guid = guid;
            Place = place;
        }
        public bool Wait()
        {
            Line l = Access.GetLine(this.Place);

            if (l == null)//burde ikke kunne ske!
            {
                l?.GetThrough(this);
                AdminHelper.Notification.Run(SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), "Queue > line NULL", "");
                return false;
            }

            if (!l.IsInLine(this))
                return false;
            
            if (counter <= 0)
            {
                //l.Reset();
                l?.GetThrough(this);
                return false;
            }

            if (l.IsFirst(this.Guid))
                return true;
            
            
            Thread.Sleep(100);
            if (first)
                AdminHelper.Notification.Run(SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), "Queue > " + this.Guid, "");
            first = false;
            counter--;
            
            return Wait();
            
        }
        public void Pass()
        {
            Line l = Access.GetLine(this.Place);
            l?.GetThrough(this);
        }
    }
    public class Line
    {
        //public DateTime last;
        public List<string> q = new List<string>();
        public bool IsFirst(string guid)
        {
            if (q.Count == 0 || q[0] == guid)
                return true;
            return false;
        }
        public void PutInLine(Elem elem)
        {
            q.Add(elem.Guid);
            //last = DateTime.Now;
        }
        public void GetThrough(Elem elem)
        {
            if (q.Contains(elem.Guid))
                q.Remove(elem.Guid);
        }
        /*public void Reset()
        {
            //if (last == DateTime.MinValue)
            //    return true;
            //DateTime now = DateTime.Now;
            //if (now.Subtract(last).TotalSeconds > 30.0f)
            //{
                q = new List<string>();
            //    return false;
            //}
            //return true;
        }*/
        public bool IsInLine(Elem elem)
        {
            return q.Contains(elem.Guid);
        }
        public bool IsEmpty()
        {
            return q.Count == 0;
        }
    }
    public class Access
    {
        public static Dictionary<string, Line> qs = new Dictionary<string, Line>();
        public Elem e;
        private string place;
        private Access() { }
        public Access(string place, string guid)
        {
            e = new Elem(place, guid);
            if (!qs.ContainsKey(place))
                qs.Add(place, new Line());
            this.place = place;
        }

        public bool Queue()
        {
            Line l = Access.GetLine(this.place);

            if (l == null)//burde ikke kunne ske!
            {
                l?.GetThrough(e);
                AdminHelper.Notification.Run(SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), SettingsHelper.Basic.EMAIL_MAIL(), "Queue > line NULL", "");
                return false;
            }

            if (!l.IsInLine(e))
                l.PutInLine(e);

            //Task t = Task.Run(async () =>
            //{
            return e.Wait();// Line.PutInLine(e);
            //});
        }

        public void UnQueue()
        {
            e.Pass();// Line.GetThrough(e);
            Line l = GetLine(e.Place);
            if (l != null)
            {
                if (l.IsEmpty())
                    qs.Remove(e.Place);
            }
        }

        public static Line GetLine(string place)
        {
            if (!qs.ContainsKey(place))
                qs[place] = new Line();
            return qs[place];
            //return null;
        }
    }
}
