﻿using System.Linq;
using System.Text.RegularExpressions;

namespace Nager.ArticleNumber
{
    public static class ArticleNumberHelper
    {
        /// <summary>
        /// Get ArticleNumberType
        /// </summary>
        /// <param name="articleNumber">ASIN, EAN8, EAN13, GTIN, ISBN10, ISBN13, UPC</param>
        /// <returns></returns>
        public static ArticleNumberType GetArticleNumberType(string articleNumber)
        {
            var articleNumberType = AnalyzeArticleNumberType(articleNumber);
            if (articleNumberType != ArticleNumberType.UNKNOWN)
            {
                return articleNumberType;
            }

            if (IsValidIsbn10(articleNumber))
            {
                return ArticleNumberType.ISBN10;
            }
            else if (IsValidIsbn13(articleNumber))
            {
                return ArticleNumberType.ISBN13;
            }
            else if (IsValidAsin(articleNumber))
            {
                return ArticleNumberType.ASIN;
            }

            return ArticleNumberType.UNKNOWN;
        }

        /// <summary>
        /// Validate ASIN
        /// </summary>
        /// <param name="asin"></param>
        /// <returns></returns>
        public static bool IsValidAsin(string asin)
        {
            if (string.IsNullOrEmpty(asin))
            {
                return false;
            }

            if (asin.Length != 10)
            {
                return false;
            }

            var regex = new Regex("^B\\d{2}\\w{7}|\\d{9}(X|\\d)$");
            return regex.IsMatch(asin);
        }

        private static ArticleNumberType AnalyzeArticleNumberType(string code)
        {
            var articleNumberType = ArticleNumberType.UNKNOWN;

            long temp;
            if (!long.TryParse(code, out temp))
            {
                return articleNumberType;
            }

            switch (code.Length)
            {
                case 8://EAN-8
                    articleNumberType = ArticleNumberType.EAN8;
                    break;
                case 12://UPC
                    articleNumberType = ArticleNumberType.UPC;
                    break;
                case 13://EAN-13
                    articleNumberType = ArticleNumberType.EAN13;
                    break;
                case 14://GTIN
                    articleNumberType = ArticleNumberType.GTIN;
                    break;
                default:
                    // wrong number of digits
                    return articleNumberType;
            }

            code = string.Format("{0:00000000000000}", temp);

            var a = new int[13];
            a[0] = (code[0] - '0') * 3;
            a[1] = code[1] - '0';
            a[2] = (code[2] - '0') * 3;
            a[3] = code[3] - '0';
            a[4] = (code[4] - '0') * 3;
            a[5] = code[5] - '0';
            a[6] = (code[6] - '0') * 3;
            a[7] = code[7] - '0';
            a[8] = (code[8] - '0') * 3;
            a[9] = code[9] - '0';
            a[10] = (code[10] - '0') * 3;
            a[11] = code[11] - '0';
            a[12] = (code[12] - '0') * 3;
            var sum = a.Sum();
            var check = (10 - (sum % 10)) % 10;
            // last is check digit
            if (check == code[13] - '0')
            {
                return articleNumberType;
            }

            return ArticleNumberType.UNKNOWN;
        }

        /// <summary>
        /// Validate Global Trade Item Number
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsValidGtin(string code)
        {
            var articleNumberType = AnalyzeArticleNumberType(code);
            if (articleNumberType == ArticleNumberType.GTIN)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Validate European Article Number
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsValidEan(string code)
        {
            var articleNumberType = AnalyzeArticleNumberType(code);
            if (articleNumberType == ArticleNumberType.EAN8 || articleNumberType == ArticleNumberType.EAN13)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Validate ISBN10
        /// </summary>
        /// <param name="isbn10"></param>
        /// <returns></returns>
        public static bool IsValidIsbn10(string isbn10)
        {
            if (string.IsNullOrEmpty(isbn10))
            {
                return false;
            }

            if (isbn10.Contains("-"))
            {
                isbn10 = isbn10.Replace("-", "");
            }

            if (isbn10.Length != 10)
            {
                return false;
            }

            long temp;
            if (!long.TryParse(isbn10.Substring(0, isbn10.Length - 1), out temp))
            {
                return false;
            }

            var sum = 0;
            for (var i = 0; i < 9; i++)
            {
                sum += (isbn10[i] - '0') * (i + 1);
            }

            var result = false;
            var remainder = sum % 11;
            var lastChar = isbn10[isbn10.Length - 1];

            if (lastChar == 'X')
            {
                result = (remainder == 10);
            }
            else if (int.TryParse(lastChar.ToString(), out sum))
            {
                result = (remainder == lastChar - '0');
            }

            return result;
        }

        /// <summary>
        /// Validate ISBN13
        /// </summary>
        /// <param name="isbn13"></param>
        /// <returns></returns>
        public static bool IsValidIsbn13(string isbn13)
        {
            if (string.IsNullOrEmpty(isbn13))
            {
                return false;
            }

            if (isbn13.Contains("-"))
            {
                isbn13 = isbn13.Replace("-", "");
            }

            if (isbn13.Length != 13)
            {
                return false;
            }

            long temp;
            if (!long.TryParse(isbn13, out temp))
            {
                return false;
            }

            var sum = 0;
            for (var i = 0; i < 12; i++)
            {
                sum += (isbn13[i] - '0') * (i % 2 == 1 ? 3 : 1);
            }

            var remainder = sum % 10;
            var checkDigit = 10 - remainder;
            if (checkDigit == 10)
            {
                checkDigit = 0;
            }
            var result = (checkDigit == isbn13[12] - '0');
            return result;
        }
    }
}
