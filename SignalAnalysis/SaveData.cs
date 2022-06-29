﻿namespace SignalAnalysis;

partial class FrmMain
{
    /// <summary>
    /// Saves data into a text-formatted file.
    /// </summary>
    /// <param name="FileName">Path (including name) of the text file</param>
    /// <param name="Data">Array of values to be saved</param>
    /// <param name="ArrIndexInit">Offset index of _signalData (used to compute the time data field)</param>
    /// <param name="SeriesName">Name of the serie data to be saved</param>
    /// <returns><see langword="True"/> if successful, <see langword="false"/> otherwise</returns>
    private bool SaveTextData(string FileName, double[] Data, int ArrIndexInit, string? SeriesName)
    {
        bool result = false;

        try
        {
            using var fs = File.Open(FileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

            // Append millisecond pattern to current culture's full date time pattern
            string fullPattern = _settings.AppCulture.DateTimeFormat.FullDateTimePattern;
            fullPattern = System.Text.RegularExpressions.Regex.Replace(fullPattern, "(:ss|:s)", _settings.MillisecondsFormat);

            // Save the header text into the file
            string content = string.Empty;
            TimeSpan nTime = nStart.AddSeconds((Data.Length - 1) / nSampleFreq) - nStart; // At least there should be 1 point

            sw.WriteLine($"{(StringsRM.GetString("strFileHeader01", _settings.AppCulture) ?? "SignalAnalysis data")} ({_settings.AppCultureName})");
            //sw.WriteLine("Start time: {0}", nStart.AddSeconds(ArrIndexInit / nSampleFreq).ToString(fullPattern, _settings.AppCulture));
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader02", _settings.AppCulture) ?? "Start time")}: {nStart.AddSeconds(ArrIndexInit / nSampleFreq).ToString(fullPattern, _settings.AppCulture)}");
            //sw.WriteLine("End time: {0}", nStart.AddSeconds((Data.Length - 1 + ArrIndexInit) / nSampleFreq).ToString(fullPattern, _settings.AppCulture));
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader03", _settings.AppCulture) ?? "End time")}: {nStart.AddSeconds((Data.Length - 1 + ArrIndexInit) / nSampleFreq).ToString(fullPattern, _settings.AppCulture)}");
            //sw.WriteLine("Total measuring time: {0} days, {1} hours, {2} minutes, {3} seconds, and {4} milliseconds", nTime.Days, nTime.Hours, nTime.Minutes, nTime.Seconds, nTime.Milliseconds);
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader04", _settings.AppCulture) ?? "Total measuring time")}: " +
                $"{nTime.Days} {(StringsRM.GetString("strFileHeader22", _settings.AppCulture) ?? "days")}, " +
                $"{nTime.Hours} {(StringsRM.GetString("strFileHeader23", _settings.AppCulture) ?? "hours")}, " +
                $"{nTime.Minutes} {(StringsRM.GetString("strFileHeader24", _settings.AppCulture) ?? "minutes")}, " +
                $"{nTime.Seconds} {(StringsRM.GetString("strFileHeader25", _settings.AppCulture) ?? "seconds")} " +
                $"{(StringsRM.GetString("strFileHeader26", _settings.AppCulture) ?? "and")} " +
                $"{nTime.Milliseconds} {(StringsRM.GetString("strFileHeader27", _settings.AppCulture) ?? "milliseconds")}");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader17", _settings.AppCulture) ?? "Number of data series")}: 1");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader05", _settings.AppCulture) ?? "Number of data points")}: {Data.Length.ToString(_settings.AppCulture)}");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader06", _settings.AppCulture) ?? "Sampling frequency")}: {nSampleFreq.ToString(_settings.AppCulture)}");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader07", _settings.AppCulture) ?? "Average")}: {Results.Average.ToString(_settings.AppCulture)}");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader08", _settings.AppCulture) ?? "Maximum")}: {Results.Maximum.ToString(_settings.AppCulture)}");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader09", _settings.AppCulture) ?? "Minimum")}: {Results.Minimum.ToString(_settings.AppCulture)}");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader10", _settings.AppCulture) ?? "Fractal dimension")}: {Results.FractalDimension.ToString(_settings.AppCulture)}");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader11", _settings.AppCulture) ?? "Fractal variance")}: {Results.FractalVariance.ToString(_settings.AppCulture)}");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader12", _settings.AppCulture) ?? "Approximate entropy")}: {Results.ApproximateEntropy.ToString(_settings.AppCulture)}");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader13", _settings.AppCulture) ?? "Sample entropy")}: {Results.SampleEntropy.ToString(_settings.AppCulture)}");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader14", _settings.AppCulture) ?? "Shannon entropy")}: {Results.ShannonEntropy.ToString(_settings.AppCulture)}");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader15", _settings.AppCulture) ?? "Entropy bit")}: {Results.EntropyBit.ToString(_settings.AppCulture)}");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader16", _settings.AppCulture) ?? "Ideal entropy")}: {Results.IdealEntropy.ToString(_settings.AppCulture)}");
            sw.WriteLine();
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader21", _settings.AppCulture) ?? "Time")}\t{SeriesName}");

            string time;
            // Save the numerical values
            for (int j = 0; j < Data.Length; j++)
            {
                time = nStart.AddSeconds((j+ ArrIndexInit) / nSampleFreq).ToString(fullPattern, _settings.AppCulture);
                content = $"{time}\t{Data[j].ToString(_settings.DataFormat, _settings.AppCulture)}";
                
                //trying to write data to csv
                sw.WriteLine(content);
            }

            // Success!
            result = true;
        }
        catch (Exception ex)
        {
            // Show error message
            using (new CenterWinDialog(this))
            {
                MessageBox.Show(String.Format(StringsRM.GetString("strMsgBoxErrorSaveData", _settings.AppCulture) ?? "An unexpected error happened while saving file data.\nPlease try again later or contact the software engineer." + Environment.NewLine + "{0}", ex.Message),
                    StringsRM.GetString("strMsgBoxErrorSaveDataTitle", _settings.AppCulture) ?? "Error saving data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        return result;
    }

    /// <summary>
    /// Saves data into a SignalAnalysis-formatted file.
    /// </summary>
    /// <param name="FileName">Path (including name) of the sig file</param>
    /// <param name="Data">Array of values to be saved</param>
    /// <param name="ArrIndexInit">Offset index of _signalData (used to compute the time data field)</param>
    /// <param name="SeriesName">Name of the serie data to be saved</param>
    /// <returns><see langword="True"/> if successful, <see langword="false"/> otherwise</returns>
    private bool SaveSigData(string FileName, double[] Data, int ArrIndexInit, string? SeriesName)
    {
        bool result = false;

        try
        {
            using var fs = File.Open(FileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

            // Append millisecond pattern to current culture's full date time pattern
            string fullPattern = _settings.AppCulture.DateTimeFormat.FullDateTimePattern;
            fullPattern = System.Text.RegularExpressions.Regex.Replace(fullPattern, "(:ss|:s)", _settings.MillisecondsFormat);

            // Save the header text into the file
            string content = string.Empty;

            sw.WriteLine($"{(StringsRM.GetString("strFileHeader01", _settings.AppCulture) ?? "SignalAnalysis data")} ({_settings.AppCultureName})");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader17", _settings.AppCulture) ?? "Number of data series")}: 1");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader05", _settings.AppCulture) ?? "Number of data points")}: {Data.Length.ToString(_settings.AppCulture)}");
            sw.WriteLine($"{(StringsRM.GetString("strFileHeader06", _settings.AppCulture) ?? "Sampling frequency")}: {nSampleFreq.ToString(_settings.AppCulture)}");
            sw.WriteLine();
            sw.WriteLine($"{SeriesName}");

            // Save the numerical values
            for (int j = 0; j < Data.Length; j++)
                sw.WriteLine(Data[j].ToString(_settings.DataFormat, _settings.AppCulture));

            // Success!
            result = true;
        }
        catch (Exception ex)
        {
            // Show error message
            using (new CenterWinDialog(this))
            {
                MessageBox.Show(String.Format(StringsRM.GetString("strMsgBoxErrorSaveData", _settings.AppCulture) ?? "An unexpected error happened while saving file data.\nPlease try again later or contact the software engineer." + Environment.NewLine + "{0}", ex.Message),
                    StringsRM.GetString("strMsgBoxErrorSaveDataTitle", _settings.AppCulture) ?? "Error saving data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        return result;
    }

    /// <summary>
    /// Saves data into a binary-formatted file. Adapts the text-format to a binary format.
    /// </summary>
    /// <param name="FileName">Path (including name) of the sig file</param>
    /// <param name="Data">Array of values to be saved</param>
    /// <param name="ArrIndexInit">Offset index of _signalData (used to compute the time data field)</param>
    /// <param name="SeriesName">Name of the serie data to be saved</param>
    /// <returns><see langword="True"/> if successful, <see langword="false"/> otherwise</returns>
    private bool SaveBinaryData(string FileName, double[] Data, int ArrIndexInit, string? SeriesName)
    {
        bool result = false;
        
        try
        {
            using var fs = File.Open(FileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var bw = new BinaryWriter(fs, System.Text.Encoding.UTF8, false);
            
            string content = string.Empty;
            TimeSpan nTime = nStart.AddSeconds((Data.Length - 1) / nSampleFreq) - nStart; // At least there should be 1 point

            // Save the header text into the file
            bw.Write($"{(StringsRM.GetString("strFileHeader01", _settings.AppCulture) ?? "SignalAnalysis data")} ({_settings.AppCultureName})");
            bw.Write(nStart.AddSeconds(ArrIndexInit / nSampleFreq));
            bw.Write(nStart.AddSeconds((Data.Length - 1 + ArrIndexInit) / nSampleFreq));
            bw.Write(nTime.Days);
            bw.Write(nTime.Hours);
            bw.Write(nTime.Minutes);
            bw.Write(nTime.Seconds);
            bw.Write(nTime.Milliseconds);
            bw.Write(1);
            bw.Write(Data.Length);
            bw.Write(nSampleFreq);
            bw.Write(Results.Average);
            bw.Write(Results.Maximum);
            bw.Write(Results.Minimum);
            bw.Write(Results.FractalDimension);
            bw.Write(Results.FractalVariance);
            bw.Write(Results.ApproximateEntropy);
            bw.Write(Results.SampleEntropy);
            bw.Write(Results.ShannonEntropy);
            bw.Write(Results.EntropyBit);
            bw.Write(Results.IdealEntropy);

            bw.Write($"{(StringsRM.GetString("strFileHeader21", _settings.AppCulture) ?? "Time")}\t{SeriesName}");

            // Save the numerical values
            for (int j = 0; j < Data.Length; j++)
            {
                bw.Write(nStart.AddSeconds((j + ArrIndexInit) / nSampleFreq));
                bw.Write(Data[j]);   
            }

            // Success!
            result = true;
        }
        catch (Exception ex)
        {
            // Show error message
            using (new CenterWinDialog(this))
            {
                MessageBox.Show(String.Format(StringsRM.GetString("strMsgBoxErrorSaveData", _settings.AppCulture) ?? "An unexpected error happened while saving file data.\nPlease try again later or contact the software engineer." + Environment.NewLine + "{0}", ex.Message),
                    StringsRM.GetString("strMsgBoxErrorSaveDataTitle", _settings.AppCulture) ?? "Error saving data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        return result;
    }

    /// <summary>
    /// Default saving, reverting to SaveTextData function.
    /// </summary>
    /// <param name="FileName">Path (including name) of the sig file</param>
    /// <param name="Data">Array of values to be saved</param>
    /// <param name="ArrIndexInit">Offset index of _signalData</param>
    /// <param name="SeriesName">Name of the serie data to be saved</param>
    /// <returns><see langword="True"/> if successful, <see langword="false"/> otherwise</returns>
    private bool SaveDefaultData(string FileName, double[] Data, int ArrIndexInit, string? SeriesName)
    {
        return SaveTextData(FileName, Data, ArrIndexInit, SeriesName);
    }


    private bool SaveResultsData(string FileName)
    {
        bool result = false;

        try
        {
            using var fs = File.Open(FileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

            sw.WriteLine($"{(StringsRM.GetString("strFileHeader01", _settings.AppCulture) ?? "SignalAnalysis data")} ({_settings.AppCultureName})");
            sw.WriteLine(Results.ToString(_settings.AppCulture));
            sw.WriteLine();
            sw.WriteLine($"{(StringsRM.GetString("strPlotFFTXLabel", _settings.AppCulture) ?? "Frequency (Hz)")}\t{StringsRM.GetString("strPlotFFTYLabelMag", _settings.AppCulture) ?? "Magnitude (RMS²)"}\t{StringsRM.GetString("strPlotFFTYLabelPow", _settings.AppCulture) ?? "Power (dB)"}");

            // Save the numerical values
            for (int j = 0; j < Results.FFTfrequencies.Length; j++)
            {
                //trying to write data to text file
                sw.WriteLine($"{Results.FFTfrequencies[j].ToString("0.########", _settings.AppCulture)}\t" +
                    $"{Results.FFTmagnitude[j].ToString("0.########", _settings.AppCulture)}\t" +
                    $"{Results.FFTpower[j].ToString("0.########", _settings.AppCulture)}"
                    );
            }

            // Success!
            result = true;

        }
        catch (Exception ex)
        {
            // Show error message
            using (new CenterWinDialog(this))
            {
                MessageBox.Show(String.Format(StringsRM.GetString("strMsgBoxErrorSaveData", _settings.AppCulture) ?? "An unexpected error happened while saving file data.\nPlease try again later or contact the software engineer." + Environment.NewLine + "{0}", ex.Message),
                    StringsRM.GetString("strMsgBoxErrorSaveDataTitle", _settings.AppCulture) ?? "Error saving data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        return result;
    }
}
