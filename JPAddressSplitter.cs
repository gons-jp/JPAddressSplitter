using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JPAddressSplitter
{
    public enum ValueType
    {
        TypTodofuken,
        TypShikuchoson,
        TypChoiki,
        TypBanchi,
        TypTatemono,
        TypBanchiTatemono,
        TypMiss
    }

    public enum CharacterType { C_None, C_Han, C_Zen, C_Kan };
    public enum CharacterClass { CLS_None, CLS_Num, CLS_Moji };

    public class AddressSpliter
    {
        private string _KENALLFILEPATH = "";
        public List<TroubleWordList> _TROUBLEWORD = null;
        public DataTable _DT_ADDRESS = null;

        public AddressSpliter(string kenall_path)
        {
            _KENALLFILEPATH = kenall_path;
            CreateAddressTable();
            CreateTroubleWordList();
        }

        /// <summary>
        /// 住所判別用の内部DataTableの_DT_ADDRESS準備
        /// </summary>
        private void CreateAddressTable()
        {
            _DT_ADDRESS = new DataTable();
            _DT_ADDRESS.Columns.AddRange(
                new DataColumn[]
                {
                    new DataColumn("todofuken",typeof(string)),
                    new DataColumn("shikuchoson",typeof(string)),
                    new DataColumn("chouiki",typeof(string)),
                    new DataColumn("org_shikuchoson",typeof(string)),
                    new DataColumn("org_chouiki",typeof(string))
                });

        }

        /// <summary>
        /// トラブルを起こしやすい文字のパターンDataTableの_TROUBLEWORD作成
        /// </summary>
        private void CreateTroubleWordList()
        {
            _TROUBLEWORD = new List<TroubleWordList>();
            _TROUBLEWORD.Add(new TroubleWordList("ツ", "ﾂ", CharacterClass.CLS_Moji, CharacterType.C_Han));
            _TROUBLEWORD.Add(new TroubleWordList("ツ", "つ", CharacterClass.CLS_Moji, CharacterType.C_Zen));
            _TROUBLEWORD.Add(new TroubleWordList("ツ", "ツ", CharacterClass.CLS_Moji, CharacterType.C_Zen));
            _TROUBLEWORD.Add(new TroubleWordList("ケ", "ヶ", CharacterClass.CLS_Moji, CharacterType.C_Han));
            _TROUBLEWORD.Add(new TroubleWordList("ケ", "ケ", CharacterClass.CLS_Moji, CharacterType.C_Zen));
            _TROUBLEWORD.Add(new TroubleWordList("ケ", "が", CharacterClass.CLS_Moji, CharacterType.C_Zen));
            _TROUBLEWORD.Add(new TroubleWordList("ケ", "か", CharacterClass.CLS_Moji, CharacterType.C_Zen));
            _TROUBLEWORD.Add(new TroubleWordList("ケ", "ガ", CharacterClass.CLS_Moji, CharacterType.C_Zen));
            _TROUBLEWORD.Add(new TroubleWordList("ノ", "ノ", CharacterClass.CLS_Moji, CharacterType.C_Zen));
            _TROUBLEWORD.Add(new TroubleWordList("ノ", "の", CharacterClass.CLS_Moji, CharacterType.C_Zen));
            _TROUBLEWORD.Add(new TroubleWordList("ノ", "之", CharacterClass.CLS_Moji, CharacterType.C_Zen));
        }

        /// <summary>
        /// 住所を割る為に使用する、日本郵政の住所CSV「KEN_ALL.CSV」をインポートする。
        /// </summary>
        private void ImportKenAll()
        {
            string tmp_address = "";
            string wk_ken = "";
            string wk_shikuchoson = "";
            string wk_chou = "";
            string tmp_chou = "";
            bool sw_addchou = false;
            NumTransform NT = new NumTransform();

            try
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(_KENALLFILEPATH, Encoding.GetEncoding("SHIFT_JIS")))
                {
                    while (!sr.EndOfStream)
                    {
                        string Line = sr.ReadLine();
                        string[] LineCol = Line.Split(',');

                        wk_ken = LineCol[6].Replace("\"", "");
                        wk_shikuchoson = LineCol[7].Replace("\"", "");
                        tmp_chou = LineCol[8].Replace("\"", "").Replace("以下に掲載がない場合", "").Replace("次のビルを除く", "").Replace("（番地）","").Replace("（丁目）","");
                        wk_chou = sw_addchou ? wk_chou + tmp_chou : tmp_chou;

                        if (sw_addchou)
                        {
                            Match m_add = Regex.Match(wk_chou, ".+[）)]");
                            if (m_add.Success)
                            {
                                sw_addchou = false;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            //複数行に渡って(hogehoge、hugahuga)になっている場合
                            Match m = Regex.Match(wk_chou, "[(（].+[^）)]$");
                            if (m.Success)
                            {
                                sw_addchou = true;
                                continue;
                            }
                        }

                        wk_chou = Regex.Replace(wk_chou, "「.+?」", "");
                        List<string> chous = GetChouikiStrings(wk_chou);
                        foreach (string chou in chous)
                        {
                            if (!tmp_address.Equals(wk_ken + wk_shikuchoson + chou))
                            {
                                tmp_address = wk_ken + wk_shikuchoson + chou;
                                //数字を含む場合に数字(半角、全角)、漢数字の複数種に置き換えたパターンを作りヒット率を上げる。
                                List<string> strnums = NT.GetTransformNum(chou);
                                foreach (string strnum in strnums)
                                {
                                    AddDataRow(wk_ken, wk_shikuchoson, strnum);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception)
            {
                _DT_ADDRESS = null;
                throw;
            }
        }

        /// <summary>
        /// KENALLから町域部分のインポート用文字列を抽出する。
        /// カッコ内に町域名が入っている場合は繋げて返す。複数の場合も複数で返す。
        /// 番地情報は削除する。
        /// </summary>
        /// <param name="ChouikiStrs">KenAllの町域項目の値</param>
        /// <returns>各町域情報</returns>
        private List<string> GetChouikiStrings(string ChouikiStrs)
        {
            List<string> ret = new List<string>();
            string cho = "", etc = "";

            //Hoge町(hogehoge)等のカッコで複数町名が含まれている場合
            Match m = Regex.Match(ChouikiStrs, "(?<cho>.+)(?<etc>（.+)");
            if (m.Success)
            {
                cho = m.Groups["cho"].Value;
                etc = m.Groups["etc"].Value;

                Match mc = Regex.Match(etc, "[0-9０-９]+|.+～.+");
                if (mc.Success)
                {
                    ret.Add(cho);
                    return ret;
                }

                etc = etc.Replace("（", "").Replace("）", "").Replace("、", ",").Replace("その他", "");
                string[] etcList = etc.Split(',');
                foreach (string etcadd in etcList)
                {
                    ret.Add(cho + etcadd);
                }
            }
            else
            {
                Match m2 = Regex.Match(ChouikiStrs, "^[0-9０-９、-ー－]+");
                if (m2.Success)
                {
                    ret.Add("");
                    return ret;
                }
                ret.Add(ChouikiStrs);
            }

            return ret;
        }

        /// <summary>
        /// 都道府県、市区町村、町域の3つの値を渡し内部DataTableの_DT_ADDRESSに追加する。
        /// 追加する際に_TROUBLEWORDに登録された問題となりやすい文字(ケｹガ等)が入っている場合
        /// 対象の文字パターンを全て置き換えて登録する。(例：市ヶ谷、市ケ谷、市ガ谷)
        /// </summary>
        /// <param name="Todofuken">都道府県</param>
        /// <param name="Shikuchoson">市区町村</param>
        /// <param name="Choiki">町域</param>
        private void AddDataRow(string Todofuken, string Shikuchoson, string Choiki)
        {
            string[] AddParts = new string[] { Todofuken, Shikuchoson, Choiki };
            string findwrd = "";
            string findval = "";
            CharacterClass cc = CharacterClass.CLS_None;
            CharacterType ct = CharacterType.C_None;
            bool Added = false;

            for (int i = 0; i < 3; i++)
            {
                foreach (var wrds in _TROUBLEWORD)
                {
                    if (AddParts[i].Contains(wrds.Value))
                    {
                        findwrd = wrds.Key;
                        findval = wrds.Value;
                        cc = wrds.CharClass;
                        ct = wrds.CharType;

                        var TWords = _TROUBLEWORD.Where(w => w.Key.Equals(findwrd));
                        foreach (var wrd in TWords)
                        {
                            _DT_ADDRESS.Rows.Add(
                                Todofuken,
                                (i == 1 ? Shikuchoson.Replace(findval, wrd.Value) : Shikuchoson),
                                (i == 2 ? Choiki.Replace(findval, wrd.Value) : Choiki),
                                Shikuchoson,
                                Choiki
                                );
                            Added = true;
                        }
                    }

                }
            }

            if (!Added)
                _DT_ADDRESS.Rows.Add(Todofuken, Shikuchoson, Choiki, Shikuchoson, Choiki);
        }

        /// <summary>
        /// 渡した住所情報から市区町村、町域部分を探して返す。
        /// </summary>
        /// <param name="AddressData">_DT_ADDRESSから探すと大変なのでフィルターしたDataRowのコレクションを渡す</param>
        /// <param name="AddressValue">住所。処理後に日本郵政の表記に直して返す</param>
        /// <param name="SearchColName">抽出する対象項目名。「市区町村」→shikuchoson。「町域」→chouiki</param>
        /// <returns>住所からSearchColNameで指定した内容を返す。</returns>
        private string GetAddressPart(OrderedEnumerableRowCollection<DataRow> AddressData, ref string AddressValue, string SearchColName)
        {
            string ColValue, org_ColValue;
            string ret = "";

            foreach (var Add in AddressData)
            {
                ColValue = (string)Add[SearchColName];
                org_ColValue = (string)Add["org_" + SearchColName];

                if (ColValue.Length > 0 && AddressValue.IndexOf(ColValue) == 0)
                {
                    AddressValue = AddressValue.Replace(ColValue, org_ColValue);
                    ret = ColValue.Replace(ColValue, org_ColValue);
                    break;
                }
            }

            return ret;
        }

        /// <summary>
        /// 住所を各項目に分ける。
        /// </summary>
        /// <param name="AddressWord">住所文字列</param>
        /// <returns>分解された住所List</returns>
        public List<SplitInfo> Split(string AddressWord)
        {
            string tdfk = "";
            string skcs = "";
            string chik = "";
            string Add = AddressWord.Replace("(","（").Replace(")","）").Replace("\r","").Replace("\n","");
            List<SplitInfo> retval = new List<SplitInfo>();

            try
            {
                if (_DT_ADDRESS == null || _DT_ADDRESS.Rows.Count == 0)
                    ImportKenAll();

                Match match = Regex.Match(AddressWord, "^\\S{2,3}?[都道府県]");
                if (!match.Success)
                {
                    retval.Add(new SplitInfo(ValueType.TypMiss, "都道府県情報がありませんでした。"));
                    return retval;
                }

                tdfk = match.Value;
                retval.Add(new SplitInfo(ValueType.TypTodofuken, tdfk));

                Add = (new Regex(tdfk)).Replace(Add, "", 1);

                var swords = _DT_ADDRESS.AsEnumerable().Where(s => s["todofuken"].Equals(match.Value)).OrderByDescending(d => d["shikuchoson"].ToString().Length);
                skcs = GetAddressPart(swords, ref Add, "shikuchoson");
                if (skcs.Length == 0)
                {
                    retval = new List<SplitInfo>();
                    retval.Add(new SplitInfo(ValueType.TypMiss, "市区町村情報がありません"));
                    return retval;
                }
                retval.Add(new SplitInfo(ValueType.TypShikuchoson, skcs));
                Add = (new Regex(skcs)).Replace(Add, "", 1);

                var schou = swords.Where(s => s["shikuchoson"].Equals(skcs)).OrderByDescending(o => o["chouiki"].ToString().Length);
                chik = GetAddressPart(schou, ref Add, "chouiki");
                Add = chik.Length > 0 ? (new Regex(chik)).Replace(Add, "", 1) : Add;
                string banchi = GetAddressNoInfo(Add);
                if (Add.IndexOf(banchi) > 0)
                {
                    string resub = Add.Substring(0, Add.IndexOf(banchi));
                    Add = (new Regex(resub)).Replace(Add, "", 1);
                    chik += resub;
                }

                retval.Add(new SplitInfo(ValueType.TypChoiki, chik));

                if (banchi.Length > 0)
                {
                    Add = (new Regex(banchi)).Replace(Add, "", 1).TrimStart();
                }

                retval.Add(new SplitInfo(ValueType.TypBanchi, banchi));
                retval.Add(new SplitInfo(ValueType.TypTatemono, Add));
            }
            catch(Exception ex)
            {
                throw;
            }
            return retval;
        }

        /// <summary>
        /// 住所情報の中から番地情報を抽出する。
        /// </summary>
        /// <param name="address">住所情報（町域）</param>
        /// <returns>抽出された番地情報</returns>
        private string GetAddressNoInfo(string address)
        {
            string ret = "";
            string pat = "(([0-9０-９一二三四五六七八九十壱弐参拾百千万萬億兆〇])+(?!保|島|崎|好|橋|段|条|幡|谷|栄|駄|ノ町|の町|之町|戸|つ橋|ツ橋|道|浦|番町|番館|号室|本木|代田|田|北田|の丸|ノ丸|之丸|騎|葉|反田|甲|折|角|地割|番丁|丁堀)(北|番地|丁目|街区|-|[−ｰ－ーのノ号番F階]|号室| 号|　号| 号室|　号室| F|　F)*)+";
            string str_address = address;

            Match m = Regex.Match(str_address, pat);
            if (m.Success)
                ret = m.Value;

            return ret;
        }
    }
}
