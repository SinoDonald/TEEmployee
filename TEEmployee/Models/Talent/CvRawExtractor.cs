using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Talent
{
    public class CvRawExtractor
    {
        private static readonly string[] ChineseDigits = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };

        public static string ExtractAge(List<CvRaw> inputs)
        {
            string age = inputs.FirstOrDefault(x => x.datatype == "基本資料")?.dataitem;
            return age;
        }

        public static string ExtractEducation(List<CvRaw> inputs)
        {
            var texts = inputs.Where(x => x.datatype == "學歷").Select(x => x.dataitem).ToList();

            //var parsedList = texts.Select(words =>
            //{
            //    var p = words.Split('|');
            //    var yearPart = p[1].Replace("年畢業", "");
            //    int graduationYear = Int32.Parse(yearPart);

            //    return new
            //    {
            //        FormatedEduString = $"{p[0]} ({p[1]}) {p[2]} {p[3]}",
            //        GraduationYear = graduationYear
            //    };
            //}).ToList();

            //var sortedList = parsedList.OrderByDescending(x => x.GraduationYear).Select(x => x.FormatedEduString).ToList();
            //var combinedString = string.Join("\n", sortedList);
            var combinedString = string.Join("\n", texts);

            return combinedString;
        }

        public static string ExtractExpertise(List<CvRaw> inputs)
        {
            var texts = inputs.Where(x => x.datatype == "專長").Select(x => x.dataitem).ToList();

            var parsedList = texts.Select((x, index) => $"{index + 1}.{x}").ToList();

            var combinedString = string.Join("\n", parsedList);

            return combinedString;
        }


        public static string ExtractTreatise(List<CvRaw> inputs)
        {
            var texts = inputs.Where(x => x.datatype == "論著").Select(x => x.dataitem).ToList();

            var parsedList = texts.Select((words, index) =>
            {
                var p = words.Split('|');

                return $"{index + 1}.{p[0]}\n({p[1]})";

            }).ToList();

            var combinedString = string.Join("\n", parsedList);

            return combinedString;
        }

        public static string ExtractLanguage(List<CvRaw> inputs)
        {
            var texts = inputs.Where(x => x.datatype == "語文能力").Select(x => x.dataitem).ToList();

            var parsedList = texts.Select((x, index) => $"{index + 1}.{x}").ToList();

            var combinedString = string.Join("\n", parsedList);

            return combinedString;
        }

        public static string ExtractAcademic(List<CvRaw> inputs)
        {
            var texts = inputs.Where(x => x.datatype == "學術組織").Select(x => x.dataitem).ToList();

            var parsedList = texts.Select((words, index) =>
            {
                var p = words.Split('|');

                return $"{index + 1}.{p[0]} {p[1]}";

            }).ToList();

            var combinedString = string.Join("\n", parsedList);

            return combinedString;
        }


        public static string ExtractLicense(List<CvRaw> inputs)
        {
            var texts = inputs.Where(x => x.datatype == "專業證照").Select(x => x.dataitem).ToList();

            var parsedList = texts.Select((words, index) =>
            {
                var p = words.Split('|');

                if (string.IsNullOrEmpty(p[1]))
                    return $"{index + 1}.{p[0]} ({p[2]})";
                else
                    return $"{index + 1}.{p[0]} ({p[1]}、{p[2]})";

            }).ToList();

            var combinedString = string.Join("\n", parsedList);

            return combinedString;
        }


        public static string ExtractTraining(List<CvRaw> inputs)
        {
            var texts = inputs.Where(x => x.datatype == "技術訓練").Select(x => x.dataitem).ToList();

            var parsedList = texts.Select((words, index) =>
            {
                var p = words.Split('|');

                return $"{index + 1}.{p[0]} ({p[1]}) {p[2]}\n({p[3]})";

            }).ToList();

            var combinedString = string.Join("\n", parsedList);

            return combinedString;
        }


        public static string ExtractHonor(List<CvRaw> inputs)
        {
            var texts = inputs.Where(x => x.datatype == "榮譽").Select(x => x.dataitem).ToList();

            var parsedList = texts.Select((words, index) =>
            {
                var p = words.Split('|');

                return $"{index + 1}.{p[0]} {p[1]} ({p[2]})";

            }).ToList();

            var combinedString = string.Join("\n", parsedList);

            return combinedString;
        }


        public static string ExtractExperience(List<CvRaw> inputs)
        {
            var texts = inputs.Where(x => x.datatype == "經歷概要").Select(x => x.dataitem).ToList();

            var parsedList = texts.Select((x, index) => $"{index + 1}.{x}").ToList();

            var combinedString = string.Join("\n", parsedList);

            return combinedString;
        }

        public static string ExtractSeniority(List<CvRaw> inputs)
        {
            var texts = inputs.Where(x => x.datatype == "職等歷程").Select(x => x.dataitem).Reverse().ToList();

            var parsedList = texts.Select((x, index) => $"{index + 1}.{x}").ToList();

            var combinedString = string.Join("\n", parsedList);

            return combinedString;
        }


        public static string ExtractProject(List<CvRaw> inputs)
        {
            var texts = inputs.Where(x => x.datatype == "經歷").Select(x => x.dataitem).ToList();

            string combinedString = "";
            int outerCount = 1;
            int innerCount = 1;

            foreach (var words in texts)
            {
                var p = words.Split('|');

                // Outer (W|W|E|E)
                if (string.IsNullOrEmpty(p[2]) && string.IsNullOrEmpty(p[3]) && !string.IsNullOrEmpty(p[1]))
                {
                    combinedString += $"({ChineseDigits[outerCount]}) {p[0]} {p[1]}\n";
                    outerCount++;
                    innerCount = 1;
                    continue;
                }

                // Inner Case 1(W|W|W|E)
                if (!string.IsNullOrEmpty(p[2]) && string.IsNullOrEmpty(p[3]))
                {
                    combinedString += $"({innerCount}) {p[0]} {p[1]} {p[2]}\n";
                    innerCount++;
                    continue;
                }

                // Inner Case 2(W|E|E|E)
                if (string.IsNullOrEmpty(p[2]) && string.IsNullOrEmpty(p[3]))
                {
                    combinedString += $"({innerCount}) {p[0]} {p[1]} {p[2]}\n";
                    innerCount++;
                    continue;
                }

                // Bullet Case 1 (W|E|E|W)
                if (!string.IsNullOrEmpty(p[0]) && !string.IsNullOrEmpty(p[3]))
                {
                    combinedString += $"． {p[3]}({p[0]})\n";
                    continue;
                }

                // Bullet Case 2 (E|E|E|W)
                if (string.IsNullOrEmpty(p[0]) && !string.IsNullOrEmpty(p[3]))
                {
                    combinedString += $"． {p[3]}\n";
                    continue;
                }
            }

            return combinedString;
        }
    }
}