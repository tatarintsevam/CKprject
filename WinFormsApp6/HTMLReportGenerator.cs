using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoltageAnalyzer;

namespace WinFormsApp6
{
    public class HTMLReportGenerator
    {
        private readonly ThreePhaseVoltageAnalyzer _voltageAnalyzer;
        private readonly ThreePhaseVoltageAnalyzer _voltageAnalyzerFreq;
        private readonly Quality_Calculator _quality_Calculator;
        private readonly string _outputPath;

        public HTMLReportGenerator(ThreePhaseVoltageAnalyzer voltageAnalyzer,
                                   ThreePhaseVoltageAnalyzer voltageAnalyzerFreq,
                                   Quality_Calculator quality_Calculator,
                                   string outputPath = "report.html")
        {
            _voltageAnalyzer = voltageAnalyzer;
            _voltageAnalyzerFreq = voltageAnalyzerFreq;
            _outputPath = outputPath;
            _quality_Calculator = quality_Calculator;
        }

        public void GenerateReport()
        {
            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='ru'>");
            html.AppendLine("<head>");
            html.AppendLine(" <meta charset='UTF-8'>");
            html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("<title>Анализ напряжения - Отчет</title>");
            html.AppendLine(" <style>");
            html.AppendLine("body { font-family: 'Franklin Gothic Demi', Arial, Times New Roman, GOST type B, sans-serif; margin: 20px; background-color: #ebd68d; }");
            html.AppendLine(".container { max-width: 1200px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            html.AppendLine(" h1 { margin: 30px; }");
            html.AppendLine("h2 { margin: 30px; }");
            html.AppendLine(" h3 { color: #555; margin-top: 20px; }");
            html.AppendLine(" table { border-collapse: collapse; width: 100%; margin: 15px 0; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }");
            html.AppendLine(" th { background-color: #3498db; color: white; font-weight: bold; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
            html.AppendLine("tr:hover { background-color: #f5f5f5; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div class='container'>");

            // Заголовок
            html.AppendLine($" <h1>Анализ показателей качесттва электроэнергии</h1>");
            html.AppendLine($"<p>Станция: {_voltageAnalyzer.GetStationName()}</p>");
            html.AppendLine($" <p> Номинальная частота:  {_voltageAnalyzer.GetNominalFrequency():F2} Гц</p>");
            html.AppendLine($" <p> Частота дискретизации: {_voltageAnalyzer.GetSamplingRate():F2} Гц</p>");
            html.AppendLine($" <p> Дата генерации отчета:{DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>");

            // Статистика по частоте
            GenerateFrequencyStatistics(html);

            // Статистика по напряжению
            GenerateVoltageStatistics(html);

            // Гармонический анализ
            GenerateHarmonicAnalysis(html);

            // Основные параметры качества
            GeneratePowerQualityMetrics(html);

            html.AppendLine("    </div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            File.WriteAllText(_outputPath, html.ToString(), Encoding.UTF8);
        }

        private void GenerateFrequencyStatistics(StringBuilder html)
        {
            var frequencies = _voltageAnalyzerFreq.PhaseAFreq.ToList();
            if (frequencies.Count == 0)
            {
                html.AppendLine(" <h3>Пока что нет </h3>");


                return; 
            }
            double nominalFreq = _voltageAnalyzer.GetNominalFrequency();
            double avgFreq = _quality_Calculator.Calculate_Frequency_Quality();
            double TimeFrequencyOutOf02 = _quality_Calculator.CountDiff_df_02 * 10;
            double TimeFrequencyOutOf04 = _quality_Calculator.CountDiff_df_04 * 10;
            double AverageDifference = avgFreq - nominalFreq;


            //выводим  отклонение частоты(все 66 значений? ) усредненное, а так же информацию о том сколько времени было превышение и тд и тп, потом сколько в процентах времени было превышение, также потом будет график
          

            html.AppendLine(" <h2>Анализ частоты</h2>");
            html.AppendLine("<table>");
            html.AppendLine(" <tr><th>Параметр</th><th>Фаза A</th></tr>");
            html.AppendLine($"<tr><td>Средняя частота</td><td>{avgFreq:F6} Гц</td></tr>");
            html.AppendLine($"<tr><td>Отклонение частоты</td><td>{avgFreq- nominalFreq:F6} Гц</td></tr>");
            html.AppendLine($"<tr><td>Отклонение частоты было больше чем +-0.2 Гц:</td><td>{TimeFrequencyOutOf02:F1} секунд</td></tr>");
            html.AppendLine($"<tr><td>Отклонение частоты было больше чем +-0.4 Гц:</td><td>{TimeFrequencyOutOf04:F1} секунд</td></tr>");
            html.AppendLine(" </table>");


        }

        private void GenerateVoltageStatistics(StringBuilder html)
        {
           
            var voltages = _voltageAnalyzer.PhaseARms.ToList();
            if (voltages.Count == 0) return;

            var AvgVoltage = _quality_Calculator.Calculate_Voltage_quality();
            double nominalVoltage = _quality_Calculator.nominalU;
            double minVoltage = voltages.Min();
            double maxVoltage = voltages.Max();
            double DifferencePercent = ((AvgVoltage - nominalVoltage) / nominalVoltage) * 100;
            //выведем среднеквадратичное отклонение напряжение на 10 минутах, сколько 10 минутных участков отклонение было больше 10%

            html.AppendLine("<h2>Анализ действующих значений напряжения</h2>");
            html.AppendLine("<table>");
            html.AppendLine(" <tr><th>Параметр</th><th>Фаза A</th></tr>");
            html.AppendLine($"<tr><td>Среднее значение</td><td>{AvgVoltage:F2} В</td></tr>");
            html.AppendLine($"<tr><td>Минимальное значение</td><td>{minVoltage:F2} В</td></tr>");
            html.AppendLine($" <tr><td>Максимальное значение</td><td>{maxVoltage:F2} В</td></tr>");
            html.AppendLine($" <tr><td>Отклонение от номинала</td><td>{DifferencePercent:F2} %</td></tr>");
            html.AppendLine("</table>");


        }

        private void GenerateHarmonicAnalysis(StringBuilder html)
        {
            var harmonicsA = _voltageAnalyzer.PhaseAHarmonicsAmplitudes;
            if (harmonicsA == null || harmonicsA.Count == 0)

            {
                html.AppendLine("<h2>Гармонический анализ:</h2>");
                html.AppendLine(" <h2>не</h3>");

                return;

            }


            html.AppendLine(" <h2>Гармонический анализ</h2>");


          
            // Таблица гармноник
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Гармоника</th><th>Амплитуда (Фаза A)</th><th>THD коэффициент</th></tr>");




        }

        private void GeneratePowerQualityMetrics(StringBuilder html)
        {
            //html.AppendLine(" <h2>Соответствие полученных данных требованием ГОСТ</h2>");
            //вывести итоговые значения и сравнить с труьебъъ==
        }

        public void OpenInBrowser()
        {
            try
            {
                string fullPath = Path.GetFullPath(_outputPath);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fullPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"не полуичилось отркрыть бразуеер{ex.Message}");
            }
        }
    }
}
