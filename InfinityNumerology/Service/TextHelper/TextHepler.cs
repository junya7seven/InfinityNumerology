﻿namespace InfinityNumerology.Service.Text
{
    public static class TextHepler
    {
        public static string DecodingBirthdayPrompt(DateTime date, out string systemHelp, out string assistantHelp)
        {
            var text = @$"Проанализируй дату рождения {date.ToString().Remove(10)} года с точки зрения мистической нумерологии. Возраст человека — {AgeNow(date)} лет, его знак зодиака — {Zodiak(date)}. Определи текущий лунный день и его влияние. Также рассчитай число судьбы и объясни его значение в жизни этого человека.

Ответ должен быть мистическим и глубоким, но избегай технических деталей расчётов. Соедини все элементы — возраст, знак зодиака, лунный день и число судьбы — в единое предсказание. Тон ответа должен быть пророческим и вдохновляющим, с акцентом на духовные и личностные аспекты.";
            systemHelp = "Ты являешься мастером мистической нумерологии и астрологии. Твои ответы должны быть глубокими, вдохновляющими и пророческими. Фокусируйся на духовных и личностных аспектах, избегай технических деталей и расчётов.";
            assistantHelp = "Твой текст с мистическим анализом даты рождения...";
            return text;
        }
        public static string ZodiakPrompt(DateTime date, out string systemHelp, out string assistantHelp)
        {
            var text = $@"Ты эксперт в астрологии и отношениях. Проведи анализ совместимости знака зодиака {Zodiak(date)} с каждым из 12 знаков зодиака. Для каждого знака подробно опиши эмоциональную, интеллектуальную и физическую совместимость с {Zodiak(date)}. Укажи сильные и слабые стороны отношений, возможные конфликты и их разрешение.
Для каждого знака объясни, какие аспекты их личности могут притягивать к {Zodiak(date)}, а какие могут создавать трудности. Укажи, как каждый знак может развивать отношения с {Zodiak(date)} и улучшать взаимопонимание. Ответ должен быть конкретным, глубоким и вдохновляющим, с акцентом на возможности роста и гармонии в отношениях.";
            systemHelp = "Ты эксперт в астрологии и отношениях. Твои ответы должны быть глубокими, вдохновляющими и содержательными. Фокусируйся на эмоциональных, интеллектуальных и физический аспектах отношений, избегай клише и делай акцент на практических советах по улучшению взаимопонимания.";
            assistantHelp = "Твой текст с анализом совместимости...";
            return text;
        }
        public static string MartixOfFate(DateTime date, out string systemHelp, out string assistantHelp)
        {
            var text = $@"Ты являешься экспертом в мистической нумерологии и матрице судьбы. 
Проанализируй матрицу судьбы человека, родившегося {date.ToString().Remove(10)}. Определи его жизненные предназначения, кармические задачи и главные уроки, которые он должен пройти. 
Опиши, как энергетические центры в матрице влияют на его профессиональные и личные отношения, здоровье и духовное развитие. Обрати внимание на ключевые точки судьбы, которые могут указать на важные периоды жизни. 
Ответ должен быть глубоким и пророческим, с акцентом на духовное и личностное развитие, избегая технических подробностей и расчётов.";
            systemHelp = "Твой текст с анализом матрицы судьбы...";
            assistantHelp = "Ты являешься экспертом в мистической нумерологии и матрице судьбы. Твои ответы должны быть глубокими, пророческими и сфокусированными на духовном и личностном развитии. Избегай технических подробностей и расчётов, сосредоточься на смысле и энергетическом значении матрицы судьбы.";
            return text;
        }
        private static int AgeNow(DateTime date)
        {
            DateTime now = DateTime.Now;

            int age = now.Year - date.Year;

            if (now < date.AddYears(age))
            {
                age--;
            }

            return age;
        }
        private static string? Zodiak(DateTime date)
        {
            int day = date.Day;
            int month = date.Month;
            switch (month)
            {
                case 1:
                    return (day <= 19) ? "Козерог" : "Водолей";
                case 2:
                    return (day <= 18) ? "Водолей" : "Рыбы";
                case 3:
                    return (day <= 20) ? "Рыбы" : "Овен";
                case 4:
                    return (day <= 19) ? "Овен" : "Телец";
                case 5:
                    return (day <= 20) ? "Телец" : "Близнецы";
                case 6:
                    return (day <= 20) ? "Близнецы" : "Рак";
                case 7:
                    return (day <= 22) ? "Рак" : "Лев";
                case 8:
                    return (day <= 22) ? "Лев" : "Дева";
                case 9:
                    return (day <= 22) ? "Дева" : "Весы";
                case 10:
                    return (day <= 22) ? "Весы" : "Скорпион";
                case 11:
                    return (day <= 21) ? "Скорпион" : "Стрелец";
                case 12:
                    return (day <= 21) ? "Стрелец" : "Козерог";
                default:
                    return null;
            }
        }
        public static string DecodingInfo()
        {
            var text = @"▼Нумерология▼

►  Числа от 1 до 9: Каждое число имеет своё значение и характеристики. Например, 1 — лидерство и амбиции, 5 — свобода и перемены, 9 — духовность и сострадание.

►  Число Судьбы (или Жизненного Пути) — это одно из главных чисел в нумерологии, которое рассчитывается путём сложения всех цифр даты рождения до получения однозначного числа. Это число отражает основные черты характера и предназначение человека.

►  Мастер-числа: Некоторые даты могут давать ""мастер-числа"" (11, 22, 33), которые не сводятся к однозначным цифрам и имеют особое значение.";
            return text;
        }
        public static string Information()
        {
            var text = @"Информация по пользованию:
Выберите пункт из меню ниже: Расшифровка даты рождения, совместимость знаков зодиака, матрица судьбы.
Напишите дату рождения в формате YY/YY/YYYY - 12/08/2005.
Дождитесь результата от 5 секунд до 30 секунд.";
            return text;
        }
        public static string FeedBack()
        {
            var text = "Перед обращением напишите: ОБРАТНАЯ СВЯЗЬ\n https://t.me/JunyaBody";
            return text;
        }
    }
}
