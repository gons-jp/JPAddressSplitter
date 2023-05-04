using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace JPAddressSplitter
{
    /// <summary>
    /// 全ての数字表記を纏めて作成するクラス
    /// </summary>
    public class NumTransform
    {
        private List<TroubleWordList> TRBList = new List<TroubleWordList>();

        public NumTransform()
        {
            InitTRBList();
        }

        /// <summary>
        /// 各数字の表記のリストを作成
        /// </summary>
        private void InitTRBList()
        {
            TRBList.Add(new TroubleWordList("0", "0", CharacterClass.CLS_Num, CharacterType.C_Han));
            TRBList.Add(new TroubleWordList("0", "０", CharacterClass.CLS_Num, CharacterType.C_Kan));
            TRBList.Add(new TroubleWordList("0", "０", CharacterClass.CLS_Num, CharacterType.C_Zen));
            TRBList.Add(new TroubleWordList("1", "1", CharacterClass.CLS_Num, CharacterType.C_Han));
            TRBList.Add(new TroubleWordList("1", "１", CharacterClass.CLS_Num, CharacterType.C_Zen));
            TRBList.Add(new TroubleWordList("1", "一", CharacterClass.CLS_Num, CharacterType.C_Kan));
            TRBList.Add(new TroubleWordList("2", "2", CharacterClass.CLS_Num, CharacterType.C_Han));
            TRBList.Add(new TroubleWordList("2", "２", CharacterClass.CLS_Num, CharacterType.C_Zen));
            TRBList.Add(new TroubleWordList("2", "二", CharacterClass.CLS_Num, CharacterType.C_Kan));
            TRBList.Add(new TroubleWordList("3", "3", CharacterClass.CLS_Num, CharacterType.C_Han));
            TRBList.Add(new TroubleWordList("3", "３", CharacterClass.CLS_Num, CharacterType.C_Zen));
            TRBList.Add(new TroubleWordList("3", "三", CharacterClass.CLS_Num, CharacterType.C_Kan));
            TRBList.Add(new TroubleWordList("4", "4", CharacterClass.CLS_Num, CharacterType.C_Han));
            TRBList.Add(new TroubleWordList("4", "４", CharacterClass.CLS_Num, CharacterType.C_Zen));
            TRBList.Add(new TroubleWordList("4", "四", CharacterClass.CLS_Num, CharacterType.C_Kan));
            TRBList.Add(new TroubleWordList("5", "5", CharacterClass.CLS_Num, CharacterType.C_Han));
            TRBList.Add(new TroubleWordList("5", "５", CharacterClass.CLS_Num, CharacterType.C_Zen));
            TRBList.Add(new TroubleWordList("5", "五", CharacterClass.CLS_Num, CharacterType.C_Kan));
            TRBList.Add(new TroubleWordList("6", "6", CharacterClass.CLS_Num, CharacterType.C_Han));
            TRBList.Add(new TroubleWordList("6", "６", CharacterClass.CLS_Num, CharacterType.C_Zen));
            TRBList.Add(new TroubleWordList("6", "六", CharacterClass.CLS_Num, CharacterType.C_Kan));
            TRBList.Add(new TroubleWordList("7", "7", CharacterClass.CLS_Num, CharacterType.C_Han));
            TRBList.Add(new TroubleWordList("7", "７", CharacterClass.CLS_Num, CharacterType.C_Zen));
            TRBList.Add(new TroubleWordList("7", "七", CharacterClass.CLS_Num, CharacterType.C_Kan));
            TRBList.Add(new TroubleWordList("8", "8", CharacterClass.CLS_Num, CharacterType.C_Han));
            TRBList.Add(new TroubleWordList("8", "８", CharacterClass.CLS_Num, CharacterType.C_Zen));
            TRBList.Add(new TroubleWordList("8", "八", CharacterClass.CLS_Num, CharacterType.C_Kan));
            TRBList.Add(new TroubleWordList("9", "9", CharacterClass.CLS_Num, CharacterType.C_Han));
            TRBList.Add(new TroubleWordList("9", "９", CharacterClass.CLS_Num, CharacterType.C_Zen));
            TRBList.Add(new TroubleWordList("9", "九", CharacterClass.CLS_Num, CharacterType.C_Kan));
        }

        /// <summary>
        /// 文字列の中ある数字標識の形式を取得する。
        /// </summary>
        /// <param name="NumStr">数字入りの文字列</param>
        /// <returns>全半漢字のどの表記かをCharacterTypeで返す</returns>
        private CharacterType GetNumType(string NumStr)
        {
            CharacterType ret = CharacterType.C_None;

            foreach (TroubleWordList twl in TRBList)
            {
                if (NumStr.Contains(twl.Value))
                {
                    ret = twl.CharType;
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// 数字を渡すと全半角、漢字、漢字桁表記の全種別をListにして返す。
        /// </summary>
        /// <param name="NumStr">数字文字列</param>
        /// <returns>各種表記を追加したList</returns>
        public List<string> GetTransformNum(string NumStr)
        {
            string KanjiNum = "";
            List<string> ret = new List<string>();

            string tmp_NumStr = (GetNumType(NumStr) == CharacterType.C_Kan) ? KanjiUnitToNumeric(NumStr) : NumStr;
            tmp_NumStr = TransformNum(tmp_NumStr, () => TRBList.Where(n => n.CharType == CharacterType.C_Kan || n.CharType == CharacterType.C_Zen), false);
            ret.Add(tmp_NumStr);

            string tmp_NumStr_Zen = TransformNum(tmp_NumStr, () => TRBList.Where(n => n.CharType == CharacterType.C_Zen), true);
            if (!tmp_NumStr.Equals(tmp_NumStr_Zen))
                ret.Add(tmp_NumStr_Zen);

            KanjiNum = TransformNum(tmp_NumStr, () => TRBList.Where(n => n.CharType == CharacterType.C_Kan), true);
            if (!tmp_NumStr.Equals(KanjiNum))
                ret.Add(KanjiNum);

            string KanjiAdded = AddKanjiUnit(KanjiNum);
            if (!KanjiNum.Equals(KanjiAdded))
                ret.Add(KanjiAdded);

            return ret;
        }

        /// <summary>
        /// 文字列中の数字の表記を、対応する単語に変換します。
        /// </summary>
        /// <param name="numStr">変換対象の文字列</param>
        /// <param name="query">変換に使用する数字表記のリストを返すメソッド</param>
        /// <param name="KeyToVal">true の場合、Key を Value に変換する。false の場合、Value を Key に変換する。</param>
        /// <returns>変換後の文字列</returns>
        private string TransformNum(string numStr, Func<IEnumerable<TroubleWordList>> query, bool KeyToVal)
        {
            var listTrb = query();
            string ret = numStr;

            foreach (var trb in listTrb)
            {
                ret = KeyToVal ? ret.Replace(trb.Key, trb.Value) : ret.Replace(trb.Value, trb.Key);
            }

            return ret;
        }

        /// <summary>
        /// 漢字表記の数値を、数字のみの表記に変換します。
        /// </summary>
        /// <param name="NumStr">変換対象の漢字表記の数値</param>
        /// <returns>数字のみの表記に変換された文字列</returns>
        private string KanjiUnitToNumeric(string NumStr)
        {
            return NumStr.Replace("十", "0").Replace("百", "00").Replace("千", "000").Replace("万", "0000");
        }

        /// <summary>
        /// 数値の文字列に漢字数字の単位を追加するメソッド
        /// </summary>
        /// <param name="NumStr">単位を追加したい漢字表記の数字</param>
        /// <returns>漢字数字の単位を追加した漢字表記の数字</returns>
        private string AddKanjiUnit(string NumStr)
        {
            string ret = "";
            string Tmp_Num = NumStr;
            string[] KanjiUnit = { "", "十", "百", "千", "万", "十万", "百万", "千万" };

            try
            {
                if (NumStr.Length == 1 || !(GetNumType(NumStr) == CharacterType.C_Kan))
                    return NumStr;

                Match m = Regex.Match(Tmp_Num, "[一二三四五六七八九０十百千万]+");
                if (m.Success)
                {
                    Tmp_Num = m.Value;
                }

                char[] NumAry = Tmp_Num.ToArray();
                string numchar = "";
                int LenNum = NumAry.Length;
                int UnitCnter = LenNum - 1;
                string UnitStr = KanjiUnit[UnitCnter];

                for (int i = 0; i < LenNum; i++)
                {
                    numchar = NumAry[i].ToString();
                    if (numchar.Equals("一"))
                    {
                        if (i < LenNum - 1)
                        {
                            numchar = UnitStr;
                        }
                    }
                    else if (numchar.Equals("０") || numchar.Equals("0"))
                    {
                        numchar = "";
                    }
                    else
                    {
                        numchar += UnitStr;
                    }

                    ret += numchar;
                    UnitCnter = UnitCnter > 0 ? UnitCnter -= 1 : 0;
                    UnitStr = KanjiUnit[UnitCnter];
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return NumStr.Replace(Tmp_Num, ret);
        }
    }
}
