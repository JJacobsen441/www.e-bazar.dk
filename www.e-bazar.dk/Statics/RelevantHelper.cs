using System.Linq;
using www.e_bazar.dk.SharedClasses;

namespace www.e_bazar.dk.Statics
{
    public class RelevantHelper
    {
        private RelevantHelper()
        {

        }

        public static RelevantHelper Create(bool setup)
        {
            /*
             * how to make this function run only once per request???
             * */
            if(ThisSession.Relevant.IsNull())
                ThisSession.Relevant = new RelevantHelper();
            if (setup)
                ThisSession.Relevant.SetupRelevant();
            return ThisSession.Relevant;
        }

        public void GetVals(out string[] opt, out string op1, out string op2, out string op3, out string op4, out string op5, out string op6, out string[] cats, out string cat, out int fra, out int til, out int zip)
        {
            opt = this.opt;
            op1 = this.opt[0]; op2 = this.opt[1]; op3 = this.opt[2]; op4 = this.opt[3]; op5 = this.opt[4]; op6 = this.opt[5];
            cats = this.cats;
            cat = this.cat;
            fra = this.fra; til = this.til; zip = this.zip;
        }

        private int zip { get; set; }
        private int fra { get; set; }
        private int til { get; set; }
        private bool kun_med_fast { get; set; }

        private string search = "";
        private string categorys = "";

        private string[] cats;
        private string cat = "";

        private string[] opt = new string[6];
        public enum OTYPE { BASIC, TYPE2, TYPE3 }

        private string OPTION(string[] arr, int index, int len, OTYPE type)
        {
            string res = "";
            switch (type)
            {
                //index: 0 & 1 & 2
                case OTYPE.BASIC:
                    foreach (string s in arr)
                        res += s != "" ? s.Trim().ToLower() + " " : "";
                    break;
                //index: 0 | 1 | 2
                case OTYPE.TYPE2:
                    if (len <= arr.Length && !string.IsNullOrEmpty(arr[index]))
                        res = arr[index].Trim().ToLower();
                    break;
                //index: 0 & 1 | 1 & 2
                case OTYPE.TYPE3:
                    if (len <= arr.Length && !string.IsNullOrEmpty(arr[index]) && !string.IsNullOrEmpty(arr[index + 1]))
                        res = arr[index].Trim().ToLower() + " " + arr[index + 1].Trim().ToLower();
                    break;
                default:
                    break;
            }
            return res;
        }

        public void SetupRelevant()
        {
            search = ThisSession.Search;
            categorys = ThisSession.Category;
            zip = ThisSession.Zip;
            fra = ThisSession.Fra;
            til = ThisSession.Til;
            kun_med_fast = ThisSession.FastPris;

            string[] options = ThisSession.Search.Trim().ToLower().Split(' ');
            opt[0] = OPTION(options, -1, -1, OTYPE.BASIC);
            opt[1] = OPTION(options, 0, 1, OTYPE.TYPE2);
            opt[2] = OPTION(options, 1, 2, OTYPE.TYPE2);
            opt[3] = OPTION(options, 2, 3, OTYPE.TYPE2);
            opt[4] = OPTION(options, 0, 2, OTYPE.TYPE3);
            opt[5] = OPTION(options, 1, 3, OTYPE.TYPE3);

            //example categorys: "beklædning-fodtøj-" 
            cats = !string.IsNullOrEmpty(categorys) ? categorys.Split('-') : new string[] { "" };
            cats = cats.Where(x => x != "").ToArray();
            
            cat = (cats.Count() > 0) ? cats[0] : "alle";
        }
    }
}