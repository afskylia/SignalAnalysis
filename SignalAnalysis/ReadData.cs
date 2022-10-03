﻿namespace SignalAnalysis;

partial class FrmMain
{
    /// <summary>
    /// Reads data from an elux-formatted file and stores it into _signalData.
    /// </summary>
    /// <param name="FileName">Path (including name) of the elux file</param>
    /// <returns><see langword="True"/> if successful, <see langword="false"/> otherwise</returns>
    private bool ReadELuxData(string FileName)
    {
        int nPoints = 0;
        bool result = true;
        string? strLine;

        try
        {
            using var fs = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs, System.Text.Encoding.UTF8);
            
            strLine = sr.ReadLine();    // ErgoLux data
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader00));
            if (!strLine.Contains($"{StringResources.FileHeader00} (", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader00));
            System.Globalization.CultureInfo fileCulture = new(strLine[(strLine.IndexOf("(") + 1)..^1]);

            strLine = sr.ReadLine();    // Start time
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader02));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader02", fileCulture) ?? "Start time"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader02));
            string fullPattern = fileCulture.DateTimeFormat.FullDateTimePattern;
            fullPattern = System.Text.RegularExpressions.Regex.Replace(fullPattern, "(:ss|:s)", ClassSettings.GetMillisecondsFormat(fileCulture));
            if (strLine == null || !DateTime.TryParseExact(strLine[(strLine.IndexOf(":") + 2)..], fullPattern, fileCulture, System.Globalization.DateTimeStyles.None, out nStart))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader02));

            strLine = sr.ReadLine();    // End time
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader03));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader03", fileCulture) ?? "End time"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader03));

            strLine = sr.ReadLine();    // Total measuring time
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader04));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader04", fileCulture) ?? "Total measuring time"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader04));

            strLine = sr.ReadLine();    // Number of sensors
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader18));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader18", fileCulture) ?? "Number of sensors"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader18));
            if (!int.TryParse(strLine[(strLine.IndexOf(":") + 1)..], out nSeries))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader18));
            if (nSeries == 0)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader18));
            nSeries += 6;

            strLine = sr.ReadLine();    // Number of data points
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader05));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader05", fileCulture) ?? "Number of data points"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader05));
            if (!int.TryParse(strLine[(strLine.IndexOf(":") + 1)..], out nPoints))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader05));
            if (nPoints == 0)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader05));

            strLine = sr.ReadLine();    // Sampling frequency
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader06));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader06", fileCulture) ?? "Sampling frequency"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader06));
            if (!double.TryParse(strLine[(strLine.IndexOf(":") + 1)..], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, fileCulture, out nSampleFreq))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader06));
            if (nSampleFreq <= 0)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader06));

            strLine = sr.ReadLine();    // Empty line
            if (strLine is null)
                throw new FormatException(StringResources.FileHeader19);
            if (strLine != string.Empty)
                throw new FormatException(StringResources.FileHeader19);

            strLine = sr.ReadLine();    // Column header lines
            if (strLine is null)
                throw new FormatException(StringResources.FileHeader20);
            seriesLabels = strLine.Split('\t');
            if (seriesLabels == Array.Empty<string>())
                throw new FormatException(StringResources.FileHeader20);

            result = InitializeDataArrays(sr, nPoints, fileCulture);
        }
        catch (System.Globalization.CultureNotFoundException ex)
        {
            result = false;
            using (new CenterWinDialog(this))
                MessageBox.Show(String.Format(StringResources.ReadDataErrorCulture, ex.Message),
                    StringResources.ReadDataErrorCultureTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
        }
        catch (FormatException ex)
        {
            result = false;
            using (new CenterWinDialog(this))
                MessageBox.Show(String.Format(StringResources.ReadDataError, ex.Message),
                    StringResources.ReadDataErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            result = false;
            using (new CenterWinDialog(this))
            {
                MessageBox.Show(String.Format(StringResources.MsgBoxErrorOpenData, ex.Message),
                    StringResources.MsgBoxErrorOpenDataTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        return result;
    }

    /// <summary>
    /// Readas data from a signal-formatted file and stores it into _signalData.
    /// </summary>
    /// <param name="FileName">Path (including name) of the sig file</param>
    /// <returns><see langword="True"/> if successful, <see langword="false"/> otherwise</returns>
    private bool ReadSigData(string FileName)
    {
        int nPoints = 0;
        bool result = false;
        string? strLine;

        try
        {
            using var fs = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs, System.Text.Encoding.UTF8);

            strLine = sr.ReadLine();    // SignalAnalysis data
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader01));
            if (!strLine.Contains($"{StringResources.FileHeader01} (", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader01));
            System.Globalization.CultureInfo fileCulture = new(strLine[(strLine.IndexOf("(") + 1)..^1]);

            strLine = sr.ReadLine();    // Number of series
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader17));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader17", fileCulture) ?? "Number of series"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader17));
            if (!int.TryParse(strLine[(strLine.IndexOf(":") + 1)..], out nSeries))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader17));
            if (nSeries == 0)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader17));

            strLine = sr.ReadLine();    // Number of data points
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader05));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader05", fileCulture) ?? "Number of data points"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader05));
            if (!int.TryParse(strLine[(strLine.IndexOf(":") + 1)..], out nPoints))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader05));
            if (nPoints == 0)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader05));

            strLine = sr.ReadLine();    // Sampling frequency
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader06));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader06", fileCulture) ?? "Sampling frequency"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader06));
            if (!double.TryParse(strLine[(strLine.IndexOf(":") + 1)..], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, fileCulture, out nSampleFreq))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader06));
            if (nSampleFreq <= 0)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader06));

            strLine = sr.ReadLine();    // Empty line
            if (strLine is null)
                throw new FormatException(StringResources.FileHeader19);
            if (strLine != string.Empty)
                throw new FormatException(StringResources.FileHeader19);

            strLine = sr.ReadLine();    // Column header names
            if (strLine is null)
                throw new FormatException(StringResources.FileHeader20);
            seriesLabels = strLine.Split('\t');
            if (seriesLabels == Array.Empty<string>())
                throw new FormatException(StringResources.FileHeader20);

            result = InitializeDataArrays(sr, nPoints, fileCulture);
        }
        catch (System.Globalization.CultureNotFoundException ex)
        {
            result = false;
            using (new CenterWinDialog(this))
                MessageBox.Show(String.Format(StringResources.ReadDataErrorCulture, ex.Message),
                    StringResources.ReadDataErrorCultureTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
        }
        catch (FormatException ex)
        {
            result = false;
            using (new CenterWinDialog(this))
                MessageBox.Show(String.Format(StringResources.ReadDataError, ex.Message),
                    StringResources.ReadDataErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            result = false;
            using (new CenterWinDialog(this))
            {
                MessageBox.Show(String.Format(StringResources.MsgBoxErrorOpenData, ex.Message),
                    StringResources.MsgBoxErrorOpenDataTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        return result;
    }

    /// <summary>
    /// Reads data from a text-formatted file and stores it into _signalData.
    /// </summary>
    /// <param name="FileName">Path (including name) of the text file</param>
    /// <param name="results">Numeric results read from the file</param>
    /// <returns><see langword="True"/> if successful, <see langword="false"/> otherwise</returns>
    private bool ReadTextData(string FileName, Stats? results)
    {
        double readValue;
        int nPoints = 0;
        bool result = false;
        string? strLine;

        if (results is null) results = new();

        try
        {
            using var fs = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs, System.Text.Encoding.UTF8);

            strLine = sr.ReadLine();    // SignalAnalysis data
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader01));
            if (!strLine.Contains($"{StringResources.FileHeader01} (", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader01));
            System.Globalization.CultureInfo fileCulture = new(strLine[(strLine.IndexOf("(") + 1)..^1]);

            strLine = sr.ReadLine();    // Start time
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader02));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader02", fileCulture) ?? "Start time"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader02));
            string fullPattern = fileCulture.DateTimeFormat.FullDateTimePattern;
            fullPattern = System.Text.RegularExpressions.Regex.Replace(fullPattern, "(:ss|:s)", ClassSettings.GetMillisecondsFormat(fileCulture));
            if (!DateTime.TryParseExact(strLine[(strLine.IndexOf(":") + 2)..], fullPattern, fileCulture, System.Globalization.DateTimeStyles.None, out nStart))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader02));

            strLine = sr.ReadLine();    // End time
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader03));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader03", fileCulture) ?? "End time"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader03));

            strLine = sr.ReadLine();    // Total measuring time
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader04));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader04", fileCulture) ?? "Total measuring time"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader04));

            strLine = sr.ReadLine();    // Number of series
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader17));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader17", fileCulture) ?? "Number of series"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader17));
            if (!int.TryParse(strLine[(strLine.IndexOf(":") + 1)..], out nPoints))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader17));
            if (nPoints <= 0)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader17));

            strLine = sr.ReadLine();    // Number of data points
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader05));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader05", fileCulture) ?? "Number of data points"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader05));
            if (!int.TryParse(strLine[(strLine.IndexOf(":") + 1)..], out nPoints))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader05));
            if (nPoints <= 0)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader05));

            strLine = sr.ReadLine();    // Sampling frequency
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader06));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader06", fileCulture) ?? "Sampling frequency"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader06));
            if (!double.TryParse(strLine[(strLine.IndexOf(":") + 1)..], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, fileCulture, out nSampleFreq))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader06));
            if (nSampleFreq <= 0)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader06));

            strLine = sr.ReadLine();    // Average illuminance
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader07));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader07", fileCulture) ?? "Average"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader07));
            if (!double.TryParse(strLine[(strLine.IndexOf(":") + 1)..], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, fileCulture, out readValue))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader07));
            results.Average = readValue;

            strLine = sr.ReadLine();    // Maximum illuminance
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader08));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader08", fileCulture) ?? "Maximum"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader08));
            if (!double.TryParse(strLine[(strLine.IndexOf(":") + 1)..], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, fileCulture, out readValue))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader08));
            results.Maximum = readValue;

            strLine = sr.ReadLine();    // Minimum illuminance
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader09));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader09", fileCulture) ?? "Minimum"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader09));
            if (!double.TryParse(strLine[(strLine.IndexOf(":") + 1)..], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, fileCulture, out readValue))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader09));
            results.Minimum = readValue;

            strLine = sr.ReadLine();    // Fractal dimension
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader10));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader10", fileCulture) ?? "Fractal dimension"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader10));
            if (!double.TryParse(strLine[(strLine.IndexOf(":") + 1)..], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, fileCulture, out readValue))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader10));
            results.FractalDimension = readValue;

            strLine = sr.ReadLine();    // Fractal variance
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader11));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader11", fileCulture) ?? "Fractal variance"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader11));
            if (!double.TryParse(strLine[(strLine.IndexOf(":") + 1)..], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, fileCulture, out readValue))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader11));
            results.FractalVariance = readValue;

            strLine = sr.ReadLine();    // Approximate entropy
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader12));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader12", fileCulture) ?? "Approximate entropy"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader12));
            if (!double.TryParse(strLine[(strLine.IndexOf(":") + 1)..], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, fileCulture, out readValue))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader12));
            results.ApproximateEntropy = readValue;

            strLine = sr.ReadLine();    // Sample entropy
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader13));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader13", fileCulture) ?? "Sample entropy"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader13));
            if (!double.TryParse(strLine[(strLine.IndexOf(":") + 1)..], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, fileCulture, out readValue))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader13));
            results.SampleEntropy = readValue;

            strLine = sr.ReadLine();    // Shannnon entropy
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader14));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader14", fileCulture) ?? "Shannon entropy"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader14));
            if (!double.TryParse(strLine[(strLine.IndexOf(":") + 1)..], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, fileCulture, out readValue))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader14));
            results.ShannonEntropy = readValue;

            strLine = sr.ReadLine();    // Entropy bit
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader15));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader15", fileCulture) ?? "Entropy bit"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader15));
            if (!double.TryParse(strLine[(strLine.IndexOf(":") + 1)..], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, fileCulture, out readValue))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader15));
            results.EntropyBit = readValue;

            strLine = sr.ReadLine();    // Ideal entropy
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader16));
            if (!strLine.Contains($"{StringsRM.GetString("strFileHeader16", fileCulture) ?? "Ideal entropy"}: ", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader16));
            if (!double.TryParse(strLine[(strLine.IndexOf(":") + 1)..], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, fileCulture, out readValue))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader16));
            results.IdealEntropy = readValue;

            strLine = sr.ReadLine();    // Empty line
            if (strLine is null)
                throw new FormatException(StringResources.FileHeader19);
            if (strLine != string.Empty)
                throw new FormatException(StringResources.FileHeader19);

            strLine = sr.ReadLine();    // Column header names
            if (strLine is null)
                throw new FormatException(StringResources.FileHeader20);
            seriesLabels = strLine.Split('\t');
            if (seriesLabels == Array.Empty<string>())
                throw new FormatException(StringResources.FileHeader20);
            seriesLabels = seriesLabels[1..];
            nSeries = seriesLabels.Length;

            result = InitializeDataArrays(sr, nPoints, fileCulture, true);
        }
        catch (System.Globalization.CultureNotFoundException ex)
        {
            result = false;
            using (new CenterWinDialog(this))
                MessageBox.Show(String.Format(StringResources.ReadDataErrorCulture, ex.Message),
                    StringResources.ReadDataErrorCultureTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
        }
        catch (FormatException ex)
        {
            result = false;
            using (new CenterWinDialog(this))
                MessageBox.Show(String.Format(StringResources.ReadDataError, ex.Message),
                    StringResources.ReadDataErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            result = false;
            using (new CenterWinDialog(this))
            {
                MessageBox.Show(String.Format(StringResources.MsgBoxErrorOpenData, ex.Message),
                    StringResources.MsgBoxErrorOpenDataTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        return result;
    }

    /// <summary>
    /// Reads data from a binary-formatted file and stores it into _signalData.
    /// </summary>
    /// <param name="FileName">Path (including name) of the text file</param>
    /// <param name="results">Numeric results read from the file</param>
    /// <returns><see langword="True"/> if successful, <see langword="false"/> otherwise</returns>
    /// <exception cref="FormatException"></exception>
    private bool ReadBinData(string FileName, Stats? results)
    {
        int nPoints;
        bool result = true;

        if (results is null) results = new();

        try
        {
            using var fs = File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var br = new BinaryReader(fs, System.Text.Encoding.UTF8);

            string strLine = br.ReadString();   // SignalAnalysis data
            if (strLine is null)
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader01));
            if (!strLine.Contains($"{StringResources.FileHeader01} (", StringComparison.Ordinal))
                throw new FormatException(String.Format(StringResources.FileHeaderSection, StringResources.FileHeader01));
            //System.Globalization.CultureInfo fileCulture = new(strLine[(strLine.IndexOf("(") + 1)..^1]);

            nStart = br.ReadDateTime();     // start time
            br.ReadDateTime();              // end time
            int dummy = br.ReadInt32();     // days
            dummy = br.ReadInt32();         // hours
            dummy = br.ReadInt32();         // minutes
            dummy = br.ReadInt32();         // seconds
            dummy = br.ReadInt32();         // milliseconds
            nPoints = br.ReadInt32();       // number of series
            nPoints = br.ReadInt32();       // number of data points
            nSampleFreq = br.ReadDouble();  // sampling frequency
            results.Average = br.ReadDouble();
            results.Maximum = br.ReadDouble();
            results.Minimum = br.ReadDouble();
            results.FractalDimension = br.ReadDouble();
            results.FractalVariance = br.ReadDouble();
            results.ApproximateEntropy = br.ReadDouble();
            results.SampleEntropy = br.ReadDouble();
            results.ShannonEntropy = br.ReadDouble();
            results.EntropyBit = br.ReadDouble();
            results.IdealEntropy = br.ReadDouble();

            strLine = br.ReadString();      // Column header names
            if (strLine is null)
                throw new FormatException(StringResources.FileHeader20);
            seriesLabels = strLine.Split('\t');
            if (seriesLabels == Array.Empty<string>())
                throw new FormatException(StringResources.FileHeader20);
            seriesLabels = seriesLabels[1..];
            nSeries = seriesLabels.Length;

            // Read data into array
            _settings.IndexStart = 0;
            _settings.IndexEnd = nPoints - 1;

            // Initialize data arrays
            _signalData = new double[nSeries][];
            for (int i = 0; i < nSeries; i++)
                _signalData[i] = new double[nPoints];

            // Read data into _signalData
            for (int row = 0; row < nSeries; row++)
            {
                for (int col = 0; col < _signalData[row].Length; col++)
                {
                    br.ReadDateTime();
                    _signalData[row][col] = br.ReadDouble();
                }
            }

        }
        catch (EndOfStreamException)
        {

        }
        catch (Exception ex)
        {
            result = false;
            using (new CenterWinDialog(this))
            {
                MessageBox.Show(String.Format(StringResources.MsgBoxErrorOpenData, ex.Message),
                    StringResources.MsgBoxErrorOpenDataTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        return result;
    }

    /// <summary>
    /// Default not implemented file-read function showing an error message
    /// </summary>
    /// <param name="FileName">Path (including name and extension) of the text file</param>
    /// <returns><see langword="True"/> if successful, <see langword="false"/> otherwise</returns>
    private bool ReadNotImplemented(string FileName)
    {
        bool result = false;

        using (new CenterWinDialog(this))
            MessageBox.Show(String.Format(StringResources.ReadNotimplementedError, Path.GetExtension(FileName).ToUpper()),
                StringResources.ReadNotimplementedErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

        return result;
    }

    /// <summary>
    /// Reads and parse the data into a numeric format.
    /// </summary>
    /// <param name="sr">This reader should be pointing to the beginning of the numeric data section</param>
    /// <param name="culture">Culture to parse the read data into numeric values</param>
    /// <param name="IsFirstColumDateTime"><see langword="True"/> if successfull</param>
    /// <returns><see langword="True"/> if the first data-column is a DateTime value and thus it will be ingnores, <see langword="false"/> otherwise</returns>
    private bool InitializeDataArrays(StreamReader sr, int points, System.Globalization.CultureInfo culture, bool IsFirstColumDateTime = false)
    {
        bool result = true;
        string? strLine;

        try {
            _settings.IndexStart = 0;
            _settings.IndexEnd = points - 1;

            // Initialize data arrays
            _signalData = new double[nSeries][];
            for (int i = 0; i < nSeries; i++)
                _signalData[i] = new double[points];

            // Read data into _signalData
            for (int i = 0; i < _signalData.Length; i++)
            {
                _signalData[i] = new double[points];
            }
            string[] data;
            int col = 0, row = 0;
            while ((strLine = sr.ReadLine()) != null)
            {
                data = strLine.Split("\t");
                for (col = IsFirstColumDateTime ? 1 : 0; col < data.Length; col++)
                {
                    if (!double.TryParse(data[col], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, culture, out _signalData[col - (IsFirstColumDateTime ? 1 : 0)][row]))
                        throw new FormatException(data[col].ToString());
                }
                row++;
            }
        }
        catch (FormatException ex)
        {
            result = false;
            using (new CenterWinDialog(this))
                MessageBox.Show(String.Format(StringResources.ReadDataErrorNumber, ex.Message),
                    StringResources.ReadDataErrorNumberTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            result = false;
            using (new CenterWinDialog(this))
            {
                MessageBox.Show( String.Format(StringResources.MsgBoxInitArray, ex.Message),
                    StringResources.MsgBoxInitArrayTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        return result;
    }

}