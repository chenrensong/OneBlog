﻿using Ganss.XSS;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OneBlog.Helpers
{
    public static class StringHelper
    {

        #region Extension Methods
        /// <summary>
        /// Checks whether the string is Null Or Empty
        /// </summary>
        /// <param name="theInput"></param>
        /// <returns></returns>
        public static bool IsNullEmpty(this string theInput)
        {
            return string.IsNullOrEmpty(theInput);
        }

        /// <summary>
        /// Converts the string to Int32
        /// </summary>
        /// <param name="theInput"></param>
        /// <returns></returns>
        public static int ToInt32(this string theInput)
        {
            return !string.IsNullOrEmpty(theInput) ? Convert.ToInt32(theInput) : 0;
        }

        /// <summary>
        /// Removes all line breaks from a string
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static string RemoveLineBreaks(this string lines)
        {
            return lines.Replace("\r\n", "")
                        .Replace("\r", "")
                        .Replace("\n", "");
        }

        // Gets the full url including 
        public static string ReturnCurrentDomain()
        {
            //Microsoft.AspNetCore.Http.Features.IHttpConnectionFeature ss;
            //Microsoft.AspNet.Http.Features.IHttpConnectionFeature ss1;
            var r = AspNetCoreHelper.HttpContext.Request;
            var builder = new UriBuilder(r.Scheme, r.Host.Host, r.Host.Port.Value);
            return builder.Uri.ToString().TrimEnd('/');
        }

        /// <summary>
        /// Removes all line breaks from a string and replaces them with specified replacement
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string ReplaceLineBreaks(this string lines, string replacement)
        {
            return lines.Replace(Environment.NewLine, replacement);
        }

        /// <summary>
        /// Does a case insensitive contains
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ContainsCaseInsensitive(this string source, string value)
        {
            var results = source.IndexOf(value, StringComparison.CurrentCultureIgnoreCase);
            return results != -1;
        }
        #endregion


        #region Validation

        public static string md5HashString(string toHash)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            var md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(toHash));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();  // Return the hexadecimal string.
        }

        /// <summary>
        /// Checks to see if the string passed in is a valid email address
        /// </summary>
        /// <param name="strIn"></param>
        /// <returns></returns>
        public static bool IsValidEmail(string strIn)
        {
            if (strIn.IsNullEmpty())
            {
                return false;
            }

            // Return true if strIn is in valid e-mail format.
            return Regex.IsMatch(strIn,
                   @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
                   @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$");
        }

        #endregion

        #region Misc

        /// <summary>
        /// Create a salt for the password hash (just makes it a bit more complex)
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string CreateSalt(int size)
        {
            // Generate a cryptographic random number.
            var rng = RandomNumberGenerator.Create();
            var buff = new byte[size];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }

        /// <summary>
        /// Generate a hash for a password, adding a salt value
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string GenerateSaltedHash(string plainText, string salt)
        {
            // http://stackoverflow.com/questions/2138429/hash-and-salt-passwords-in-c-sharp

            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var saltBytes = Encoding.UTF8.GetBytes(salt);

            // Combine the two lists
            var plainTextWithSaltBytes = new List<byte>(plainTextBytes.Length + saltBytes.Length);
            plainTextWithSaltBytes.AddRange(plainTextBytes);
            plainTextWithSaltBytes.AddRange(saltBytes);
            // Produce 256-bit hashed value i.e. 32 bytes
            var algorithm = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            algorithm.AppendData(plainTextWithSaltBytes.ToArray());
            var byteHash = algorithm.GetHashAndReset();
            return Convert.ToBase64String(byteHash);
        }

        public static async Task<string> PostForm(string url, string poststring)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded";

            var bytedata = Encoding.UTF8.GetBytes(poststring);
            httpRequest.Headers["Content-Length"] = bytedata.Length.ToString();

            using (var requestStream = await httpRequest.GetRequestStreamAsync())
            {
                requestStream.Write(bytedata, 0, bytedata.Length);
            }

            var httpWebResponse = (HttpWebResponse)(await httpRequest.GetResponseAsync());
            var responseStream = httpWebResponse.GetResponseStream();

            var sb = new StringBuilder();

            if (responseStream != null)
            {
                using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        sb.Append(line);
                    }
                }
            }

            return sb.ToString();

        }


        #endregion

        #region Conversion

        /// <summary>
        /// Converts a csv list of string guids into a real list of guids
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        public static List<string> CsvIdConverter(string csv)
        {
            return csv.TrimStart(',').TrimEnd(',').Split(',').ToList();
        }


        #endregion

        #region Numeric Helpers
        /// <summary>
        /// Strips numeric charators from a string
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string StripNonNumerics(string source)
        {
            var digitRegex = new Regex(@"[^\d]");
            return digitRegex.Replace(source, "");
        }

        /// <summary>
        /// Checks to see if the object is numeric or not
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool IsNumeric(object expression)
        {
            double retNum;
            var isNum = Double.TryParse(Convert.ToString(expression), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }
        #endregion

        #region String content helpers

        private static readonly Random _rng = new Random();
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string RandomString(int size)
        {
            var buffer = new char[size];
            for (var i = 0; i < size; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
            }
            return new string(buffer);
        }

        /// <summary>
        /// Returns the number of occurances of one string within another
        /// </summary>
        /// <param name="text"></param>
        /// <param name="stringToFind"></param>
        /// <returns></returns>
        public static int NumberOfOccurrences(string text, string stringToFind)
        {
            if (text == null || stringToFind == null)
            {
                return 0;
            }

            var reg = new Regex(stringToFind, RegexOptions.IgnoreCase);

            return reg.Matches(text).Count;
        }

        /// <summary>
        /// reverses a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringReverse(string str)
        {
            var len = str.Length;
            var arr = new char[len];
            for (var i = 0; i < len; i++)
            {
                arr[i] = str[len - 1 - i];
            }
            return new string(arr);
        }

        /// <summary>
        /// Returns a capitalised version of words in the string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string CapitalizeWords(string value)
        {
            if (value == null)
                return null;
            if (value.Length == 0)
                return value;

            var result = new StringBuilder(value);
            result[0] = char.ToUpper(result[0]);
            for (var i = 1; i < result.Length; ++i)
            {
                if (char.IsWhiteSpace(result[i - 1]))
                    result[i] = char.ToUpper(result[i]);
                else
                    result[i] = char.ToLower(result[i]);
            }
            return result.ToString();
        }


        /// <summary>
        /// Returns the amount of individual words in a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int CountWordsInString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }
            var tmpStr = text.Replace("\t", " ").Trim();
            tmpStr = tmpStr.Replace("\n", " ");
            tmpStr = tmpStr.Replace("\r", " ");
            while (tmpStr.IndexOf("  ") != -1)
                tmpStr = tmpStr.Replace("  ", " ");
            return tmpStr.Split(' ').Length;
        }

        /// <summary>
        /// 按文本内容长度截取html字符串(支持截取带html代码样式的字符串)
        /// </summary>
        /// <param name="html">将要截取的字符串参数</param>
        /// <param name="len">截取的字节长度</param>
        /// <param name="endstring">字符串末尾补上的字符串</param>
        /// <returns>返回截取后的字符串</returns>
        public static string HtmlSubstring(string html, int len, string endstring)
        {
            if (string.IsNullOrEmpty(html) || html.Length <= len) return html;
            MatchCollection mcentiry, mchtmltag;
            List<string> inputhtmltag = new List<string>();
            string r = "", tmpvalue;
            int rwordcount = 0, wordnum = 0, i = 0;
            Regex rxsingle = new Regex("^<(br|hr|img|input|param|meta|link)", RegexOptions.Compiled | RegexOptions.IgnoreCase)//是否单标签正则
                , rxendtag = new Regex("</[^>]+>", RegexOptions.Compiled)//是否结束标签正则
                , rxtagname = new Regex("<([a-z]+)[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase)//获取标签名正则
                , rxhtmltag = new Regex("<[^>]+>", RegexOptions.Compiled)//html标签正则
                , rxentity = new Regex("&[a-z]{1,9};", RegexOptions.Compiled | RegexOptions.IgnoreCase)//实体正则
                , rxentityreverse = new Regex("§", RegexOptions.Compiled)//反向替换实体正则
                ;
            html = html.Replace("§", "§");//替换字符§为他的实体“§”，以便进行下一步替换
            mcentiry = rxentity.Matches(html);//收集实体对象到匹配数组中
            html = rxentity.Replace(html, "§");//替换实体为特殊字符§，这样好控制一个实体占用一个字符
            mchtmltag = rxhtmltag.Matches(html);//收集html标签到匹配数组中
            html = rxhtmltag.Replace(html, "__htmltag__");//替换为特殊标签
            string[] arrword = html.Split(new string[] { "__htmltag__" }, StringSplitOptions.None);//通过特殊标签进行拆分
            wordnum = arrword.Length;
            //获取指定内容长度及html标签
            for (; i < wordnum; i++)
            {
                if (rwordcount + arrword[i].Length >= len) r += arrword[i].Substring(0, len - rwordcount) + endstring;
                else r += arrword[i];
                rwordcount += arrword[i].Length;//计算已经获取到的字符长度
                if (rwordcount >= len) break;
                //搜集已经添加的非单标签，以便封闭html标签对
                if (i < wordnum - 1)
                {
                    tmpvalue = mchtmltag[i].Value;
                    if (!rxsingle.IsMatch(tmpvalue))
                    { //不是单标签
                        if (rxendtag.IsMatch(tmpvalue) && inputhtmltag.Count > 0) inputhtmltag.RemoveAt(inputhtmltag.Count - 1);
                        else inputhtmltag.Add(tmpvalue);
                    }
                    r += tmpvalue;
                }
            }
            //替换回实体
            for (i = 0; i < mcentiry.Count; i++) r = rxentityreverse.Replace(r, mcentiry[i].Value, 1);
            //封闭标签
            for (i = inputhtmltag.Count - 1; i >= 0; i--) r += "</" + rxtagname.Match(inputhtmltag[i].ToString()).Groups[1].Value + ">";
            return r;
        }


        /// <summary> /// 提取摘要，是否清除HTML代码 /// </summary> 
        /// /// <param name="content"></param>
        /// /// <param name="length"></param> 
        /// /// <param name="StripHTML"></param> 
        /// /// <returns></returns> 
        public static string GetContentSummary(string content, int length, bool StripHTML)
        {
            if (string.IsNullOrEmpty(content) || length == 0)
                return "";

            if (StripHTML)
            {
                Regex re = new Regex("<[^>]*>");
                content = re.Replace(content, "");
                content = content.Replace("　", "").Replace(" ", "");
                if (content.Length <= length) return content;
                else
                    return content.Substring(0, length) + "……";
            }
            else
            {
                if (content.Length <= length) return content;
                int pos = 0, npos = 0, size = 0; bool firststop = false,
                    notr = false, noli = false; StringBuilder sb = new StringBuilder();
                while (true)
                {
                    if (pos >= content.Length) break;
                    string cur = content.Substring(pos, 1); if (cur == "<")
                    {
                        string next = content.Substring(pos + 1, 3).ToLower();
                        if (next.IndexOf("p") == 0 && next.IndexOf("pre") != 0) { npos = content.IndexOf(">", pos) + 1; }
                        else if (next.IndexOf("/p") == 0 && next.IndexOf("/pr") != 0)
                        {
                            npos = content.IndexOf(">", pos) + 1;
                            if (size < length) sb.Append("<br/>");
                        }
                        else if (next.IndexOf("br") == 0)
                        {
                            npos = content.IndexOf(">", pos) + 1; if (size < length) sb.Append("<br/>");
                        }
                        else if (next.IndexOf("img") == 0)
                        {
                            npos = content.IndexOf(">", pos) + 1;
                            if (size < length) { sb.Append(content.Substring(pos, npos - pos)); size += npos - pos + 1; }
                        }
                        else if (next.IndexOf("li") == 0 || next.IndexOf("/li") == 0)
                        {
                            npos = content.IndexOf(">", pos) + 1;
                            if (size < length) { sb.Append(content.Substring(pos, npos - pos)); }
                            else
                            {
                                if (!noli && next.IndexOf("/li") == 0)
                                {
                                    sb.Append(content.Substring(pos, npos - pos)); noli = true;
                                }
                            }
                        }
                        else if (next.IndexOf("tr") == 0 || next.IndexOf("/tr") == 0) { npos = content.IndexOf(">", pos) + 1; if (size < length) { sb.Append(content.Substring(pos, npos - pos)); } else { if (!notr && next.IndexOf("/tr") == 0) { sb.Append(content.Substring(pos, npos - pos)); notr = true; } } } else if (next.IndexOf("td") == 0 || next.IndexOf("/td") == 0) { npos = content.IndexOf(">", pos) + 1; if (size < length) { sb.Append(content.Substring(pos, npos - pos)); } else { if (!notr) { sb.Append(content.Substring(pos, npos - pos)); } } } else { npos = content.IndexOf(">", pos) + 1; sb.Append(content.Substring(pos, npos - pos)); }
                        if (npos <= pos) npos = pos + 1; pos = npos;
                    }
                    else
                    {
                        if (size < length) { sb.Append(cur); size++; }
                        else
                        {
                            if (!firststop)
                            {
                                sb.Append("……");
                                firststop = true;
                            }
                        }
                        pos++;
                    }
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Returns a specified amount of words from a string （有bug）
        /// </summary>
        /// <param name="text"></param>
        /// <param name="wordAmount"></param>
        /// <returns></returns>
        public static string ReturnAmountWordsFromString(string text, int wordAmount)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            int length = wordAmount;
            string tmpStr;
            string[] stringArray;
            var tmpStrReturn = "";
            tmpStr = text.Replace("\t", " ").Trim();
            tmpStr = tmpStr.Replace("\n", " ");
            tmpStr = tmpStr.Replace("\r", " ");

            while (tmpStr.IndexOf("  ") != -1)
            {
                tmpStr = tmpStr.Replace("  ", " ");
            }
            stringArray = tmpStr.Split(' ');

            if (stringArray.Length < wordAmount)
            {
                wordAmount = stringArray.Length;
            }
            for (int i = 0; i < wordAmount; i++)
            {
                tmpStrReturn += stringArray[i] + " ";
            }

            if (tmpStrReturn.Length > length)
            {
                return tmpStrReturn.Substring(0, length);
            }

            return tmpStrReturn;
        }

        /// <summary>
        /// Returns a string to do a related question/search lookup
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public static string ReturnSearchString(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return searchTerm;
            }

            // Lower case
            searchTerm = searchTerm.ToLower();

            // Firstly strip non alpha numeric charactors out
            searchTerm = Regex.Replace(searchTerm, @"[^\w\.@\- ]", "");

            // Now strip common words out and retun the final result
            return string.Join(" ", searchTerm.Split().Where(w => !CommonWords().Contains(w)).ToArray());
        }

        /// <summary>
        /// Returns a list of the most common english words
        /// TODO: Need to put this in something so people can add other language lists of common words
        /// </summary>
        /// <returns></returns>
        public static IList<string> CommonWords()
        {
            return new List<string>
                {
                    "the", "be",  "to",
                    "of",
                    "and",
                    "a",
                    "in",
                    "that",
                    "have",
                    "i",
                    "it",
                    "for",
                    "not",
                    "on",
                    "with",
                    "he",
                    "as",
                    "you",
                    "do",
                    "at",
                    "this",
                    "but",
                    "his",
                    "by",
                    "from",
                    "they",
                    "we",
                    "say",
                    "her",
                    "she",
                    "or",
                    "an",
                    "will",
                    "my",
                    "one",
                    "all",
                    "would",
                    "there",
                    "their",
                    "what",
                    "so",
                    "up",
                    "out",
                    "if",
                    "about",
                    "who",
                    "get",
                    "which",
                    "go",
                    "me",
                    "when",
                    "make",
                    "can",
                    "like",
                    "time",
                    "no",
                    "just",
                    "him",
                    "know",
                    "take",
                    "people",
                    "into",
                    "year",
                    "your",
                    "good",
                    "some",
                    "could",
                    "them",
                    "see",
                    "other",
                    "than",
                    "then",
                    "now",
                    "look",
                    "only",
                    "come",
                    "its",
                    "over",
                    "think",
                    "also",
                    "back",
                    "after",
                    "use",
                    "two",
                    "how",
                    "our",
                    "work",
                    "first",
                    "well",
                    "way",
                    "even",
                    "new",
                    "want",
                    "because",
                    "any",
                    "these",
                    "give",
                    "day",
                    "most",
                    "cant",
                    "us"
                };
        }

        #endregion

        #region Sanitising

        /// <summary>
        /// Strips all non alpha/numeric charators from a string
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="replaceWith"></param>
        /// <returns></returns>
        public static string StripNonAlphaNumeric(string strInput, string replaceWith)
        {
            strInput = Regex.Replace(strInput, "[^\\w]", replaceWith);
            strInput = strInput.Replace(string.Concat(replaceWith, replaceWith, replaceWith), replaceWith)
                                .Replace(string.Concat(replaceWith, replaceWith), replaceWith)
                                .TrimStart(Convert.ToChar(replaceWith))
                                .TrimEnd(Convert.ToChar(replaceWith));
            return strInput;
        }

        /// <summary>
        /// Get the current users IP address
        /// </summary>
        /// <returns></returns>
        public static string GetUsersIpAddress()
        {
            var context = AspNetCoreHelper.HttpContext;
            return context?.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress.ToString();

            //  var serverVariablesFeature = context.Features.Get<IServerVariablesFeature>();
            //var serverName = AspCoreUtils.GetHeaderValueAs<string>("SERVER_NAME");
            //if (serverName.ToLower().Contains("localhost"))
            //{
            //    return serverName;
            //}
            //var ipList = AspCoreUtils.GetHeaderValueAs<string>("HTTP_X_FORWARDED_FOR");
            //return !string.IsNullOrEmpty(ipList) ? ipList.Split(',')[0] : AspCoreUtils.GetHeaderValueAs<string>("REMOTE_ADDR");
        }

        /// <summary>
        /// Used to pass all string input in the system  - Strips all nasties from a string/html
        /// </summary>
        /// <param name="html"></param>
        /// <param name="useXssSantiser"></param>
        /// <returns></returns>
        public static string GetSafeHtml(string html, bool useXssSantiser = false)
        {
            // Scrub html
            html = ScrubHtml(html, useXssSantiser);

            // remove unwanted html
            html = RemoveUnwantedTags(html);

            return html;
        }


        /// <summary>
        /// Takes in HTML and returns santized Html/string
        /// </summary>
        /// <param name="html"></param>
        /// <param name="useXssSantiser"></param>
        /// <returns></returns>
        public static string ScrubHtml(string html, bool useXssSantiser = false)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            // clear the flags on P so unclosed elements in P will be auto closed.
            HtmlNode.ElementsFlags.Remove("p");

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var finishedHtml = html;

            // Embed Urls
            if (doc.DocumentNode != null)
            {
                // Get all the links we are going to ////a[contains(@href, 'coolxap.com')]|//a[contains(@href, 'iqiyi.com')]|//a[contains(@href, 'youku.com')]|
                var tags = doc.DocumentNode.SelectNodes("//a[contains(@href, 'youtube.com')]|//a[contains(@href, 'youtu.be')]|//a[contains(@href, 'vimeo.com')]|//a[contains(@href, 'screenr.com')]|//a[contains(@href, 'instagram.com')]");

                if (tags != null)
                {
                    // find formatting tags
                    foreach (var item in tags)
                    {
                        if (item.PreviousSibling == null)
                        {
                            // Prepend children to parent node in reverse order
                            foreach (var node in item.ChildNodes.Reverse())
                            {
                                item.ParentNode.PrependChild(node);
                            }
                        }
                        else
                        {
                            // Insert children after previous sibling
                            foreach (var node in item.ChildNodes)
                            {
                                item.ParentNode.InsertAfter(node, item.PreviousSibling);
                            }
                        }

                        // remove from tree
                        item.Remove();
                    }
                }


                //Remove potentially harmful elements
                var nc = doc.DocumentNode.SelectNodes("//script|//link|//iframe|//frameset|//frame|//applet|//object");//|//embed
                if (nc != null)
                {
                    foreach (var node in nc)
                    {
                        node.ParentNode.RemoveChild(node, false);

                    }
                }

                //remove hrefs to java/j/vbscript URLs
                nc = doc.DocumentNode.SelectNodes("//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'javascript')]|//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'jscript')]|//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'vbscript')]");
                if (nc != null)
                {

                    foreach (var node in nc)
                    {
                        node.SetAttributeValue("href", "#");
                    }
                }

                //remove img with refs to java/j/vbscript URLs
                nc = doc.DocumentNode.SelectNodes("//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'javascript')]|//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'jscript')]|//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'vbscript')]");
                if (nc != null)
                {
                    foreach (var node in nc)
                    {
                        node.SetAttributeValue("src", "#");
                    }
                }

                //remove on<Event> handlers from all tags
                nc = doc.DocumentNode.SelectNodes("//*[@onclick or @onmouseover or @onfocus or @onblur or @onmouseout or @ondblclick or @onload or @onunload or @onerror]");
                if (nc != null)
                {
                    foreach (var node in nc)
                    {
                        node.Attributes.Remove("onFocus");
                        node.Attributes.Remove("onBlur");
                        node.Attributes.Remove("onClick");
                        node.Attributes.Remove("onMouseOver");
                        node.Attributes.Remove("onMouseOut");
                        node.Attributes.Remove("onDblClick");
                        node.Attributes.Remove("onLoad");
                        node.Attributes.Remove("onUnload");
                        node.Attributes.Remove("onError");
                    }
                }

                // remove any style attributes that contain the word expression (IE evaluates this as script)
                nc = doc.DocumentNode.SelectNodes("//*[contains(translate(@style, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'expression')]");
                if (nc != null)
                {
                    foreach (var node in nc)
                    {
                        node.Attributes.Remove("stYle");
                    }
                }

                // build a list of nodes ordered by stream position
                var pos = new NodePositions(doc);

                // browse all tags detected as not opened
                foreach (var error in doc.ParseErrors.Where(e => e.Code == HtmlParseErrorCode.TagNotOpened))
                {
                    // find the text node just before this error
                    var last = pos.Nodes.OfType<HtmlTextNode>().LastOrDefault(n => n.StreamPosition < error.StreamPosition);
                    if (last != null)
                    {
                        // fix the text; reintroduce the broken tag
                        last.Text = error.SourceText.Replace("/", "") + last.Text + error.SourceText;
                    }
                }

                finishedHtml = doc.DocumentNode.WriteTo();
            }


            // The reason we have this option, is using the santiser with the MarkDown editor 
            // causes problems with line breaks.
            if (useXssSantiser)
            {
                var sanitizer = new HtmlSanitizer();
                var sanitized = sanitizer.Sanitize(finishedHtml);
                return SanitizerCompatibleWithForiegnCharacters(sanitized);
            }

            return finishedHtml;
        }

        public static string RemoveUnwantedTags(string html)
        {

            var unwantedTagNames = new List<string>
            {
                "div",
                "font",
                "table",
                "tbody",
                "tr",
                "td",
                "th",
                "thead"
            };

            return RemoveUnwantedTags(html, unwantedTagNames);
        }

        public static string RemoveUnwantedTags(string html, List<string> unwantedTagNames)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            var htmlDoc = new HtmlDocument();

            // load html
            htmlDoc.LoadHtml(html);

            var tags = (from tag in htmlDoc.DocumentNode.Descendants()
                        where unwantedTagNames.Contains(tag.Name)
                        select tag).Reverse();


            // find formatting tags
            foreach (var item in tags)
            {
                if (item.PreviousSibling == null)
                {
                    // Prepend children to parent node in reverse order
                    foreach (var node in item.ChildNodes.Reverse())
                    {
                        item.ParentNode.PrependChild(node);
                    }
                }
                else
                {
                    // Insert children after previous sibling
                    foreach (var node in item.ChildNodes)
                    {
                        item.ParentNode.InsertAfter(node, item.PreviousSibling);
                    }
                }

                // remove from tree
                item.Remove();
            }

            // return transformed doc
            return htmlDoc.DocumentNode.WriteContentTo().Trim();
        }

  
        /// <summary>
        /// decode a chunk of html or url
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string HtmlDecode(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return WebUtility.HtmlDecode(input);
            }
            return input;
        }

        /// <summary>
        /// Uses regex to strip HTML from a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripHtmlFromString(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                input = Regex.Replace(input, @"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", string.Empty, RegexOptions.Singleline);
                input = Regex.Replace(input, @"\[[^]]+\]", "");
                input = input.Replace("&nbsp;", " ");
            }
            return input;
        }

        /// <summary>
        /// Returns safe plain text using XSS library
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SafePlainText(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                input = StripHtmlFromString(input);
                input = GetSafeHtml(input, true);
            }
            return input;
        }
        #endregion

        #region Html Element Helpers

        public static string AppendDomainToImageUrlInHtml(string html, string domain)
        {
            var htmlDocument = new HtmlDocument();
            try
            {
                htmlDocument.LoadHtml(html);
                var nodes = htmlDocument.DocumentNode.SelectNodes("//img");
                if (nodes != null && nodes.Any())
                {
                    foreach (var image in nodes)
                    {
                        HtmlAttribute imageUrl = image?.Attributes[@"src"];
                        if (imageUrl != null && !imageUrl.Value.Contains("http"))
                        {
                            imageUrl.Value = string.Concat(domain, imageUrl.Value);
                        }
                    }

                    return htmlDocument.DocumentNode.WriteTo();
                }
            }
            catch
            {
                // Do nothing
            }

            return html;
        }

        public static IList<string> GetAmountOfImagesUrlFromHtml(this string html, int amount = 1)
        {
            var images = new List<string>();
            try
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);
                var nodes = htmlDocument.DocumentNode.SelectNodes("//img");
                if (nodes != null && nodes.Any())
                {
                    foreach (var image in nodes.Take(amount))
                    {
                        var imageUrl = image?.Attributes[@"src"];
                        if (imageUrl != null)
                        {
                            images.Add(imageUrl.Value);
                        }
                    }
                }
            }
            catch
            {
                // Do nothing
            }

            return images;
        }

        /// <summary>
        /// Returns a HTML link
        /// </summary>
        /// <param name="href"></param>
        /// <param name="anchortext"></param>
        /// <param name="openinnewwindow"></param>
        /// <returns></returns>
        public static string ReturnHtmlLink(string href, string anchortext, bool openinnewwindow = false)
        {
            return string.Format(openinnewwindow ? "<a rel='nofollow' target='_blank' href=\"{0}\">{1}</a>" : "<a rel='nofollow' href=\"{0}\">{1}</a>", href, anchortext);
        }

        public static string CheckLinkHasHttp(string url)
        {
            return !url.Contains("http://") ? string.Concat("http://", url) : url;
        }

        /// <summary>
        /// Returns a HTML image tag
        /// </summary>
        /// <param name="url"></param>
        /// <param name="alt"></param>
        /// <returns></returns>
        public static string ReturnImageHtml(string url, string alt)
        {
            return $"<img src=\"{url}\" alt=\"{alt}\" />";
        }
        #endregion

        #region Urls / Webpages
        /// <summary>
        /// Downloads a web page and returns the HTML as a string
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<HttpWebResponse> DownloadWebPage(string url)
        {
            var ub = new UriBuilder(url);
            var request = (HttpWebRequest)WebRequest.Create(ub.Uri);
            request.Proxy = null;
            return (HttpWebResponse)(await request.GetResponseAsync());
        }

        /// <summary>
        /// Creates a URL freindly string, good for SEO
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="replaceWith"></param>
        /// <returns></returns>
        public static string CreateUrl(string strInput, string replaceWith)
        {
            // Doing this to stop the urls having amp from &amp;
            strInput = WebUtility.HtmlDecode(strInput);
            // Doing this to stop the urls getting encoded
            var url = RemoveAccents(strInput);
            return StripNonAlphaNumeric(url, replaceWith).ToLower();
        }

        public static string RemoveAccents(string input)
        {
            // Replace accented characters for the closest ones:
            //var from = "ÂÃÄÀÁÅÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝàáâãäåçèéêëìíîïðñòóôõöøùúûüýÿ".ToCharArray();
            //var to = "AAAAAACEEEEIIIIDNOOOOOOUUUUYaaaaaaceeeeiiiidnoooooouuuuyy".ToCharArray();
            //for (var i = 0; i < from.Length; i++)
            //{
            //    input = input.Replace(from[i], to[i]);
            //}

            //// Thorn http://en.wikipedia.org/wiki/%C3%9E
            //input = input.Replace("Þ", "TH");
            //input = input.Replace("þ", "th");

            //// Eszett http://en.wikipedia.org/wiki/%C3%9F
            //input = input.Replace("ß", "ss");

            //// AE http://en.wikipedia.org/wiki/%C3%86
            //input = input.Replace("Æ", "AE");
            //input = input.Replace("æ", "ae");

            //return input;


            var stFormD = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var t in stFormD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(t);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(t);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));

        }

        #endregion

        #region Rich Text Formatting

        /// <summary>
        /// Returns UK formatted amount from int
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static string FormatCurrency(int? amount)
        {
            return amount != null ? $"{amount:C}" : "n/a";
        }


        /// <summary>
        /// 暂时废弃
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EmbedVideosInPosts(string str)
        {

            if (str.IndexOf("youtube.com", StringComparison.CurrentCultureIgnoreCase) >= 0 || str.IndexOf("youtu.be", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                const string pattern = @"(?:https?:\/\/)?(?:www\.)?(?:(?:(?:youtube.com\/watch\?[^?]*v=|youtu.be\/)([\w\-]+))(?:[^\s?]+)?)";
                const string replacement = "<div class=\"video-container\" itemscope itemtype=\"//schema.org/VideoObject\"><meta itemprop=\"embedURL\" content=\"//www.youtube.com/embed/$1\"><iframe title='YouTube video player' width='500' height='281' src='//www.youtube.com/embed/$1' frameborder='0' allowfullscreen='1'></iframe></div>";

                var rgx = new Regex(pattern);
                str = rgx.Replace(str, replacement);
            }

            if (str.IndexOf("vimeo.com", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                const string pattern = @"(?:https?:\/\/)?vimeo\.com/(?:.*#|.*/videos/)?([0-9]+)";
                const string replacement = "<div class=\"video-container\" itemscope itemtype=\"//schema.org/VideoObject\"><meta itemprop=\"embedURL\" content=\"//player.vimeo.com/video/$1?portrait=0\"><iframe src=\"//player.vimeo.com/video/$1?portrait=0\" width=\"500\" height=\"281\" frameborder=\"0\"></iframe></div>";

                var rgx = new Regex(pattern);
                str = rgx.Replace(str, replacement);
            }

            if (str.IndexOf("screenr", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                const string pattern = @"(?:https?:\/\/)?(?:www\.)screenr\.com/([a-zA-Z0-9]+)";
                const string replacement = "<div class=\"video-container\" itemscope itemtype=\"//schema.org/VideoObject\"><meta itemprop=\"embedURL\" content=\"//www.screenr.com/embed/$1\"><iframe src=\"//www.screenr.com/embed/$1\" width=\"500\" height=\"281\" frameborder=\"0\"></iframe></div>";

                var rgx = new Regex(pattern);
                str = rgx.Replace(str, replacement);
            }

            if (str.IndexOf("instagr", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                var reg = new Regex(@"(?:https?:\/\/)?(?:www\.)?instagr\.?am(?:\.com)?/\S*", RegexOptions.Compiled);
                var idRegex = new Regex(@"(?<=p/).*?(?=/)", RegexOptions.Compiled);
                var result = new StringBuilder();
                using (var reader = new StringReader(str))
                {
                    while (reader.Peek() > 0)
                    {
                        var line = reader.ReadLine();
                        if (line != null && reg.IsMatch(line))
                        {
                            var url = reg.Match(line).ToString();

                            // Find links 

                            result.AppendLine(reg.Replace(line, $"<p><img src=\"//instagram.com/p/{idRegex.Match(url)}/media/?size=l\" class=\"img-responsive\" /></p>"));
                        }
                        else
                        {
                            result.AppendLine(line);
                        }

                    }
                }

                str = result.ToString();
            }

            return str;
        }

        /// <summary>
        /// A method to convert basic BBCode to HTML
        /// </summary>
        /// <param name="str">A string formatted in BBCode</param>
        /// <param name="replaceLineBreaks">Whether or not to replace line breaks with br's</param>
        /// <returns>The HTML representation of the BBCode string</returns>
        public static string ConvertBbCodeToHtml(string str, bool replaceLineBreaks = true)
        {
            if (replaceLineBreaks)
            {
                // As this is a BBEditor we need to replace line breaks
                // or they won't show in the front end
                str = ReplaceLineBreaks(str, "<br>");
            }

            // format the bold tags: [b][/b]
            // becomes: <strong></strong>
            var exp = new Regex(@"\[b\](.+?)\[/b\]");
            str = exp.Replace(str, "<strong>$1</strong>");

            // format the italic tags: [i][/i]
            // becomes: <em></em>
            exp = new Regex(@"\[i\](.+?)\[/i\]");
            str = exp.Replace(str, "<em>$1</em>");

            // format the underline tags: [u][/u]
            // becomes: <u></u>
            exp = new Regex(@"\[u\](.+?)\[/u\]");
            str = exp.Replace(str, "<u>$1</u>");

            // format the underline tags: [ul][/ul]
            // becomes: <ul></ul>
            exp = new Regex(@"\[ul\](.+?)\[/ul\]");
            str = exp.Replace(str, "<ul>$1</ul>");

            // format the underline tags: [ol][/ol]
            // becomes: <ol></ol>
            exp = new Regex(@"\[ol\](.+?)\[/ol\]");
            str = exp.Replace(str, "<ol>$1</ol>");

            // format the underline tags: [li][/li]
            // becomes: <li></li>
            exp = new Regex(@"\[li\](.+?)\[/li\]");
            str = exp.Replace(str, "<li>$1</li>");

            // format the code tags: [code][/code]
            // becomes: <pre></pre>
            exp = new Regex(@"\[code\](.+?)\[/code\]");
            str = exp.Replace(str, "<pre>$1</pre>");

            // format the code tags: [quote][/quote]
            // becomes: <blockquote></blockquote>
            exp = new Regex(@"\[quote\](.+?)\[/quote\]");
            str = exp.Replace(str, "<blockquote>$1</blockquote>");

            // format the strike tags: [s][/s]
            // becomes: <strike></strike>
            exp = new Regex(@"\[s\](.+?)\[/s\]");
            str = exp.Replace(str, "<strike>$1</strike>");

            //### Before this replace links without http ###
            str.Replace("[url=www.", "[url=http://www.");
            // format the url tags: [url=www.website.com]my site[/url]
            // becomes: <a href="www.website.com">my site</a>
            exp = new Regex(@"\[url\=([^\]]+)\]([^\]]+)\[/url\]");
            str = exp.Replace(str, "<a rel=\"nofollow\" href=\"$1\">$2</a>");

            // format the img tags: [img]www.website.com/img/image.jpeg[/img]
            // becomes: <img src="www.website.com/img/image.jpeg" />
            exp = new Regex(@"\[img\]([^\]]+)\[/img\]");
            str = exp.Replace(str, "<img src=\"$1\" />");

            // format img tags with alt: [img=www.website.com/img/image.jpeg]this is the alt text[/img]
            // becomes: <img src="www.website.com/img/image.jpeg" alt="this is the alt text" />
            exp = new Regex(@"\[img\=([^\]]+)\]([^\]]+)\[/img\]");
            str = exp.Replace(str, "<img src=\"$1\" alt=\"$2\" />");

            // format the size tags: [size=1.2][/size]
            // becomes: <span style="font-size:1.2em;"></span>
            exp = new Regex(@"\[size\=([^\]]+)\]([^\]]+)\[/size\]");
            str = exp.Replace(str, "<span style=\"font-size:$1em;\">$2</span>");

            return str;
        }
        #endregion

        private static Regex _identifierForDisplayRgx = new Regex(
    @"  (?<=[A-Z])(?=[A-Z][a-z])    # UC before me, UC lc after me
             |  (?<=[^A-Z])(?=[A-Z])        # Not UC before me, UC after me
             |  (?<=[A-Za-z])(?=[^A-Za-z])  # Letter before me, non letter after me
            ", RegexOptions.IgnorePatternWhitespace
);
        /// <summary>
        /// Format's an identifier (e.g. variable, enum field value) for display purposes,
        /// by adding a space before each capital letter.  A value like EditOwnUser becomes
        /// "Edit Own User".  If multiple capital letters are in identifier, these letters
        /// will be treated as one word (e.g. XMLEditor becomes "XML Editor", not
        /// "X M L Editor").
        /// </summary>
        /// <param name="fieldName">An identifier ready to be formatted for display.</param>
        /// <returns>The identifier for display purposes.</returns>
        /// <remarks>Credit: http://stackoverflow.com/questions/3103730</remarks>
        public static string FormatIdentifierForDisplay(string fieldName)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string part in _identifierForDisplayRgx.Split(fieldName))
            {
                if (sb.Length > 0) { sb.Append(" "); }
                sb.Append(part);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 从字符串的指定位置截取指定长度的子字符串
        /// </summary>
        /// <param name="str">原字符串</param>
        /// <param name="startIndex">子字符串的起始位置</param>
        /// <param name="length">子字符串的长度</param>
        /// <returns>子字符串</returns>
        public static string CutString(string str, int startIndex, int length)
        {
            if (startIndex >= 0)
            {
                if (length < 0)
                {
                    length = length * -1;
                    if (startIndex - length < 0)
                    {
                        length = startIndex;
                        startIndex = 0;
                    }
                    else
                        startIndex = startIndex - length;
                }

                if (startIndex > str.Length)
                    return "";
            }
            else
            {
                if (length < 0)
                    return "";
                else
                {
                    if (length + startIndex > 0)
                    {
                        length = length + startIndex;
                        startIndex = 0;
                    }
                    else
                        return "";
                }
            }

            if (str.Length - startIndex < length)
                length = str.Length - startIndex;

            return str.Substring(startIndex, length);
        }

        /// <summary>
        /// 从字符串的指定位置开始截取到字符串结尾的了符串
        /// </summary>
        /// <param name="str">原字符串</param>
        /// <param name="startIndex">子字符串的起始位置</param>
        /// <returns>子字符串</returns>
        public static string CutString(string str, int startIndex)
        {
            return CutString(str, startIndex, str.Length);
        }


        #region Sanitizer Compatible With Chinese Characters
        private static readonly Dictionary<string, string> HbjDictionaryFx = new Dictionary<string, string>();
        /// <summary>
        /// 微软的AntiXSS v4.0 让部分汉字乱码,这里将乱码部分汉字转换回来
        /// Microsoft AntiXSS Library Sanitizer causes some Chinese characters become "encoded",
        /// use this function to replace them back.
        /// source:http://www.zhaoshu.net/bbs/read10.aspx?TieID=b1745a9c-03a6-4367-93e0-114707aff3e3
        /// </summary>
        /// <returns></returns>
        public static string SanitizerCompatibleWithForiegnCharacters(string originalString)
        {
            var returnString = originalString;

            //returnString = returnString.Replace("\r\n", "");
            if (returnString.Contains("&#"))
            {
                //Initialize the dictionary, if it doesn't contain anything. 
                if (HbjDictionaryFx.Keys.Count == 0)
                {
                    lock (HbjDictionaryFx)
                    {
                        if (HbjDictionaryFx.Keys.Count == 0)
                        {
                            HbjDictionaryFx.Clear();
                            HbjDictionaryFx.Add("&#20028;", "丼");
                            HbjDictionaryFx.Add("&#20284;", "似");
                            HbjDictionaryFx.Add("&#20540;", "值");
                            HbjDictionaryFx.Add("&#20796;", "儼");
                            HbjDictionaryFx.Add("&#21052;", "刼");
                            HbjDictionaryFx.Add("&#21308;", "匼");
                            HbjDictionaryFx.Add("&#21564;", "吼");
                            HbjDictionaryFx.Add("&#21820;", "唼");
                            HbjDictionaryFx.Add("&#22076;", "嘼");
                            HbjDictionaryFx.Add("&#22332;", "圼");
                            HbjDictionaryFx.Add("&#22588;", "堼");
                            HbjDictionaryFx.Add("&#23612;", "尼");
                            HbjDictionaryFx.Add("&#26684;", "格");
                            HbjDictionaryFx.Add("&#22844;", "夼");
                            HbjDictionaryFx.Add("&#23100;", "娼");
                            HbjDictionaryFx.Add("&#23356;", "嬼");
                            HbjDictionaryFx.Add("&#23868;", "崼");
                            HbjDictionaryFx.Add("&#24124;", "帼");
                            HbjDictionaryFx.Add("&#24380;", "弼");
                            HbjDictionaryFx.Add("&#24636;", "怼");
                            HbjDictionaryFx.Add("&#24892;", "愼");
                            HbjDictionaryFx.Add("&#25148;", "戼");
                            HbjDictionaryFx.Add("&#25404;", "挼");
                            HbjDictionaryFx.Add("&#25660;", "搼");
                            HbjDictionaryFx.Add("&#25916;", "攼");
                            HbjDictionaryFx.Add("&#26172;", "昼");
                            HbjDictionaryFx.Add("&#26428;", "朼");
                            HbjDictionaryFx.Add("&#26940;", "椼");
                            HbjDictionaryFx.Add("&#27196;", "樼");
                            HbjDictionaryFx.Add("&#27452;", "欼");
                            HbjDictionaryFx.Add("&#27708;", "氼");
                            HbjDictionaryFx.Add("&#27964;", "洼");
                            HbjDictionaryFx.Add("&#28220;", "渼");
                            HbjDictionaryFx.Add("&#28476;", "漼");
                            HbjDictionaryFx.Add("&#28732;", "瀼");
                            HbjDictionaryFx.Add("&#28988;", "焼");
                            HbjDictionaryFx.Add("&#29244;", "爼");
                            HbjDictionaryFx.Add("&#29500;", "猼");
                            HbjDictionaryFx.Add("&#29756;", "琼");
                            HbjDictionaryFx.Add("&#30012;", "甼");
                            HbjDictionaryFx.Add("&#30268;", "瘼");
                            HbjDictionaryFx.Add("&#30524;", "眼");
                            HbjDictionaryFx.Add("&#30780;", "砼");
                            HbjDictionaryFx.Add("&#31036;", "礼");
                            HbjDictionaryFx.Add("&#31292;", "稼");
                            HbjDictionaryFx.Add("&#31548;", "笼");
                            HbjDictionaryFx.Add("&#31804;", "簼");
                            HbjDictionaryFx.Add("&#32060;", "紼");
                            HbjDictionaryFx.Add("&#32316;", "縼");
                            HbjDictionaryFx.Add("&#32572;", "缼");
                            HbjDictionaryFx.Add("&#32828;", "耼");
                            HbjDictionaryFx.Add("&#33084;", "脼");
                            HbjDictionaryFx.Add("&#33340;", "舼");
                            HbjDictionaryFx.Add("&#33596;", "茼");
                            HbjDictionaryFx.Add("&#33852;", "萼");
                            HbjDictionaryFx.Add("&#34108;", "蔼");
                            HbjDictionaryFx.Add("&#36156;", "贼");
                            HbjDictionaryFx.Add("&#39740;", "鬼");

                            // Also add russion
                            HbjDictionaryFx.Add("&#1084;", "м");
                        }
                    }

                }

                //start to replace "encoded" Chinese characters.
                lock (HbjDictionaryFx)
                {
                    foreach (string key in HbjDictionaryFx.Keys)
                    {
                        if (returnString.Contains(key))
                        {
                            returnString = returnString.Replace(key, HbjDictionaryFx[key]);
                        }
                    }
                }
            }

            return returnString;
        }
        #endregion
    }


}