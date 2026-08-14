using codex.Services;
using static codex.Services.CsvReader;

namespace test
{
    public class CsvReaderShould
    {
        [Fact]
        public void ReturnEmptyListOnNonValid()
        {
            string data = ",,,";
            CsvReader r = new CsvReader(new StringReader(data));

            var result = r.Read();// возвращает колелекцию из пустых элементов , например [[]]

            Assert.Collection(result, Assert.Empty);
        }
        [Fact]
        public void ReturnValidListOnNonValid()
        {
            string data = "fine,90,\"text,with comas\",,,";
            string[] expected = ["fine", "90", "\"text,with comas\""];
            CsvReader r = new CsvReader(new StringReader(data));

            var result = r.Read();
            
            Assert.Collection(result, item => Assert.Equal(expected, item));

        }
        //Наборы верных (валидных) данных
        public static IEnumerable<object[]> GetValidTestData()
        {
            yield return new object[] {//Набор 1
                "fine,90,\"text,with comas\"",//data
                CsvConfig.Default,//config
                new List<string[]>{
                    new string[]{ "fine", "90", "\"text,with comas\"" }
                } //expected
            };
            yield return new object[] {//Набор 2. Символ-разделитель ячейки - (;) 
                "fine;90;\"text;with comas\"\r\nNext;line , with coma,;and \"Quotation, Mark's\"",//data
                new CsvConfig(';','"'),//config
                new List<string[]>{
                    new string[]{ "fine", "90", "\"text;with comas\"" },
                    new string[]{ "Next", "line , with coma,", "and \"Quotation, Mark's\"" }
                } //expected
            };
            yield return new object[] {//Набор 3. Экранирующий символ - ($) 
                "fine;90;$text;with comas$\r\nNext;line , with coma,;and $Quotation, Mark's$",//data
                new CsvConfig(';','$'),//config
                new List<string[]>{
                    new string[]{ "fine", "90", "$text;with comas$" },
                    new string[]{ "Next", "line , with coma,", "and $Quotation, Mark's$" }
                } //expected
            };
        }

        [Theory]
        [MemberData(nameof(GetValidTestData))]
        public void ReturnCorrectOnValid(string data,CsvConfig config, List<string[]> expectedOutput)
        {
            //Arrange
            CsvReader r = new CsvReader(new StringReader(data), config);
            var actualOutput = new List<string[]>();

            //Act
            foreach (var line in r.Read()) {
                actualOutput.Add(line);
            }

            //Assert
            Assert.Equal(expectedOutput.Count, actualOutput.Count);
            foreach (var line in expectedOutput)
            {
                Assert.Contains(line, actualOutput);
            }
        }
        //Наборы неверных данных
        public static IEnumerable<object[]> GetInvalidTestData()
        {
            yield return new object[] {//Набор 1 пустой файл
                "",//data
                CsvConfig.Default,//config
                typeof(FileNotFoundException) //expected
            };          
        }
        [Theory]
        [MemberData(nameof(GetInvalidTestData))]
        public void ThrowsOnInvalid(string data, CsvConfig config, Type expected)
        {
            //Arrange & Act
            var actualOutput = new List<string[]>();

            var e = Record.Exception(() =>
            {
                CsvReader r = new CsvReader(new StringReader(data), config);

                foreach (var line in r.Read())  // ← Исключение здесь
                {
                    actualOutput.Add(line);
                }
            });

            //Assert
            Assert.IsType(expected, e);
        }
        [Fact]
        public void ThrowsOnDirectoryNotExist()
        {
            var actualOutput = new List<string[]>();

            var e = Record.Exception(() =>
            {
                CsvReader r = new CsvReader("path/to/file.txt");// ← Исключение здесь
            });

            //Assert
            Assert.IsType<DirectoryNotFoundException>(e);
        }
        [Fact]
        public void ThrowsOnFileNotExist()
        {
            var actualOutput = new List<string[]>();

            var e = Record.Exception(() =>
            {
                CsvReader r = new CsvReader("file.txt");// ← Исключение здесь
            });

            //Assert
            Assert.IsType<FileNotFoundException>(e);
        }
        
    }
}
