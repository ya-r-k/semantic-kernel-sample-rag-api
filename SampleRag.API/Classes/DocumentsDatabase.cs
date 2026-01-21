using SampleRag.Domain.Models;

namespace SampleRagAPI.RagAPI.Classes;

public static class DocumentsDatabase
{
    public static List<DocumentData> GetDocuments()
    {
        var documentsData = new List<DocumentData>()
        {
            new()
            {
                Name = "Трудовой кодекс Российской Федерации",
                LocalLink = "/docs/trudovoy-kodeks-rf.html",
                OriginalLink = "https://www.consultant.ru/document/cons_doc_LAW_34683/",
                BriefDescription = "Основной закон, регулирующий трудовые отношения, права и обязанности работников и работодателей, порядок заключения и расторжения трудовых договоров, оплату труда, отпуска, охрану труда." [web:31][web:34]
            },
            new()
            {
                Name = "Федеральный закон 323-ФЗ Об основах охраны здоровья граждан",
                LocalLink = "/docs/323-fz-osnovy-zdorovya.html",
                OriginalLink = "https://www.consultant.ru/document/cons_doc_LAW_121895/",
                BriefDescription = "Регулирует оказание медицинской помощи, права пациентов, обязанности медорганизаций, лицензирование, страховую медицину и государственные гарантии." [web:32]
            },
            new()
            {
                Name = "Федеральный закон 152-ФЗ О персональных данных",
                LocalLink = "/docs/152-fz-personal-data.html",
                OriginalLink = "https://www.consultant.ru/document/cons_doc_LAW_61801/",
                BriefDescription = "Определяет принципы обработки персональных данных, права субъектов, обязанности операторов, меры защиты информации и ответственность за нарушения." [web:31]
            },
            new()
            {
                Name = "Федеральный закон 44-ФЗ О контрактной системе закупок",
                LocalLink = "/docs/44-fz-zakupki.html",
                OriginalLink = "https://www.consultant.ru/document/cons_doc_LAW_144624/",
                BriefDescription = "Регулирует закупки для государственных и муниципальных нужд: планирование, способы определения поставщиков, исполнение контрактов, контроль." [web:31]
            },
            new()
            {
                Name = "Федеральный закон 273-ФЗ Об образовании в РФ",
                LocalLink = "/docs/273-fz-obrazovanie.html",
                OriginalLink = "https://www.consultant.ru/document/cons_doc_LAW_140174/",
                BriefDescription = "Основы образовательной системы, уровни образования, права обучающихся, аккредитация, финансирование и государственный контроль."
            },
            new()
            {
                Name = "Гражданский кодекс РФ Часть первая",
                LocalLink = "/docs/gk-rf-chast1.html",
                OriginalLink = "https://www.consultant.ru/document/cons_doc_LAW_5142/",
                BriefDescription = "Регулирует гражданско-правовые отношения: физические и юридические лица, сделки, собственность, обязательства, наследование." [web:31]
            },
            new()
            {
                Name = "ГОСТ ISO 9001-2015 Системы менеджмента качества",
                LocalLink = "/docs/gost-iso-9001-2015.html",
                OriginalLink = "https://docs.cntd.ru/document/1200123690",
                BriefDescription = "Требования к системам менеджмента качества организаций: процессы, руководство, планирование, поддержка, анализ и улучшение." [web:33]
            },
            new()
            {
                Name = "Федеральный закон 426-ФЗ О специальной оценке условий труда",
                LocalLink = "/docs/426-fz-sout.html",
                OriginalLink = "https://www.consultant.ru/document/cons_doc_LAW_156615/",
                BriefDescription = "Порядок проведения СОУТ, классификация условий труда, права работников, ответственность работодателей за вредные условия." [web:34]
            },
            new()
            {
                Name = "ГОСТ 12.0.230-2007 Система стандартов безопасности труда",
                LocalLink = "/docs/gost-12-0-230-2007.html",
                OriginalLink = "https://docs.cntd.ru/document/1200064479",
                BriefDescription = "Общие требования к разработке мероприятий по охране труда, идентификация рисков, меры контроля опасностей." [web:37]
            },
            new()
            {
                Name = "Приказ Минздрава России 107н Порядок оказания скорой помощи",
                LocalLink = "/docs/prikaz-minzdrav-107n.html",
                OriginalLink = "https://www.consultant.ru/document/cons_doc_LAW_347399/",
                BriefDescription = "Организация скорой медицинской помощи: показания, оснащение бригад, маршрутизация, формы меддокументации." [web:32]
            },
            new()
            {
                Name = "Технический регламент ТР ТС 010/2011 Безопасность машин",
                LocalLink = "/docs/tr-ts-010-2011.html",
                OriginalLink = "https://docs.cntd.ru/document/902320560",
                BriefDescription = "Требования безопасности к машинам и оборудованию: конструкция, маркировка, инструкции, испытания." [web:33]
            },
            new()
            {
                Name = "Федеральный закон 125-ФЗ Об обязательном страховании от несчастных случаев",
                LocalLink = "/docs/125-fz-strahovanie.html",
                OriginalLink = "https://www.consultant.ru/document/cons_doc_LAW_19208/",
                BriefDescription = "Страхование работников от профзаболеваний и травм на производстве, тарифы, выплаты, ФСС." [web:34]
            },
            new()
            {
                Name = "Приказ Минздрава 183н Медицинская помощь по акушерству",
                LocalLink = "/docs/prikaz-minzdrav-183n.html",
                OriginalLink = "https://www.garant.ru/products/ipo/prime/doc/70517002/",
                BriefDescription = "Стандарты медпомощи при беременности, родах, гинекологии: объемы, формы, условия оказания."
            },
            new()
            {
                Name = "СанПиН 2.2.1/2.1.1.1200-03 Санитарно-эпидемиологические требования",
                LocalLink = "/docs/sanpin-2-2-1-1200-03.html",
                OriginalLink = "https://docs.cntd.ru/document/1200004462",
                BriefDescription = "К помещениям, зданиям, сооружениям: микроклимат, освещение, шум, вибрация, инсоляция."
            },
            new()
            {
                Name = "Федеральный закон 384-ФЗ Технический регламент о безопасности зданий",
                LocalLink = "/docs/384-fz-zdaniya.html",
                OriginalLink = "https://www.consultant.ru/document/cons_doc_LAW_112559/",
                BriefDescription = "Требования пожарной, конструктивной безопасности зданий и сооружений, экспертиза проектов."
            },
            new()
            {
                Name = "ГОСТ Р ИСО 45001-2020 Системы менеджмента охраны труда",
                LocalLink = "/docs/gost-iso-45001-2020.html",
                OriginalLink = "https://docs.cntd.ru/document/566545183",
                BriefDescription = "Международный стандарт СМ ОТ: лидерство, планирование рисков, поддержка, эксплуатация, оценка."
            },
            new()
            {
                Name = "Уголовно-процессуальный кодекс РФ",
                LocalLink = "/docs/upk-rf.html",
                OriginalLink = "https://www.consultant.ru/document/cons_doc_LAW_34481/",
                BriefDescription = "Процесс расследования и рассмотрения уголовных дел: дознание, следствие, судопроизводство."
            },
            new()
            {
                Name = "Федеральный закон 218-ФЗ О государственной регистрации недвижимости",
                LocalLink = "/docs/218-fz-registratsiya.html",
                OriginalLink = "https://www.consultant.ru/document/cons_doc_LAW_156196/",
                BriefDescription = "ЕГРН, кадастровый учет, регистрация прав на недвижимость, Росреестр."
            },
            new()
            {
                Name = "Приказ Минтруда 772н Классификация объектов риска",
                LocalLink = "/docs/prikaz-mintrud-772n.html",
                OriginalLink = "https://normativ.kontur.ru/document?moduleId=1&documentId=383859",
                BriefDescription = "Категорирование юридических лиц по рискам для проверок Роспотребнадзора и других регуляторов."
            },
            new()
            {
                Name = "Федеральный закон 402-ФЗ О бухгалтерском учете",
                LocalLink = "/docs/402-fz-buh-uchet.html",
                OriginalLink = "https://www.consultant.ru/document/cons_doc_LAW_162577/",
                BriefDescription = "Правила ведения бухучета, отчетности, первичных документов, аудит."
            }
        };
        return documentsData;
    }
}
