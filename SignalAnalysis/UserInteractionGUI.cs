﻿namespace SignalAnalysis;

partial class FrmMain
{
    private void Exit_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void Open_Click(object? sender, EventArgs e)
    {
        DialogResult result;
        string filePath;

        using OpenFileDialog openDlg = new()
        {
            Filter = StringResources.OpenDlgFilter,
            FilterIndex = 5,
            InitialDirectory = _settings.RememberFileDialogPath ? _settings.UserOpenPath : _settings.DefaultOpenPath,
            RestoreDirectory = true,
            Title = StringResources.OpenDlgTitle,
        };

        using (new CenterWinDialog(this))
            result = openDlg.ShowDialog(this);

        if (result == DialogResult.OK && openDlg.FileName != "")
        {
            // Show a waiting cursor
            var cursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            //Get the path of specified file and store the directory for future calls
            filePath = openDlg.FileName;
            if (_settings.RememberFileDialogPath) _settings.UserOpenPath = Path.GetDirectoryName(filePath) ?? string.Empty;

            // Read the data file in the corresponding format
            Stats? results = null;
            bool boolRead = Path.GetExtension(filePath).ToLower() switch
            {
                ".elux" => ReadELuxData(filePath),
                ".sig" => ReadSigData(filePath),
                ".txt" => ReadTextData(filePath, results = new()),
                ".bin" => ReadBinData(filePath, results = new()),
                _ => ReadNotImplemented(filePath)
            };

            if (boolRead)
            {
                PopulateComboSeries();
                SetFormTitle(this, openDlg.FileName);

                if (results is not null)
                {
                    Results = results;
                    txtStats.Text = Results.ToString(_settings.AppCulture);
                }
            }

            // Restore the cursor
            Cursor.Current = cursor;
        }

    }

    private void Export_Click(object? sender, EventArgs e)
    {
        DialogResult result;
        string filePath;

        // Extract the values to be exported
        double[] signal = Array.Empty<double>();
        if (_signalData.Length > 0)
            signal = _signalData[stripComboSeries.SelectedIndex][_settings.IndexStart..(_settings.IndexEnd + 1)];

        // Exit if there is no data to be saved
        if (signal.Length == 0)
        {
            // Exit if no data has been received or the matrices are still un-initialized
            using (new CenterWinDialog(this))
            {
                MessageBox.Show(StringResources.MsgBoxNoDataTitle,
                    StringResources.MsgBoxNoDataTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            return;
        }

        // Displays a SaveFileDialog, so the user can save the data into a file  
        SaveFileDialog SaveDlg = new()
        {
            DefaultExt = "*.txt",
            Filter = StringResources.SaveDlgFilter,
            FilterIndex = 1,
            InitialDirectory = _settings.RememberFileDialogPath ? _settings.UserSavePath : _settings.DefaultSavePath,
            OverwritePrompt = true,
            Title = StringResources.SaveDlgTitle
        };

        using (new CenterWinDialog(this))
            result = SaveDlg.ShowDialog(this.Parent);

        // If the file name is not an empty string, call the corresponding routine to save the data into a file.  
        if (result == DialogResult.OK && SaveDlg.FileName != "")
        {
            // Show a waiting cursor
            var cursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            //Get the path of specified file and store the directory for future calls
            filePath = SaveDlg.FileName;
            if (_settings.RememberFileDialogPath) _settings.UserSavePath = Path.GetDirectoryName(filePath) ?? string.Empty;
            
            var boolSave = Path.GetExtension(filePath).ToLower() switch
            {
                ".txt" => SaveTextData(SaveDlg.FileName, signal, _settings.IndexStart, stripComboSeries.SelectedItem.ToString()),
                ".sig" => SaveSigData(SaveDlg.FileName, signal, _settings.IndexStart, stripComboSeries.SelectedItem.ToString()),
                ".bin" => SaveBinaryData(SaveDlg.FileName, signal, _settings.IndexStart, stripComboSeries.SelectedItem.ToString()),
                ".results" => SaveResultsData(SaveDlg.FileName),
                _ => SaveDefaultData(SaveDlg.FileName, signal, _settings.IndexStart, stripComboSeries.SelectedItem.ToString()),
            };

            // Restore the cursor
            Cursor.Current = cursor;

            if (boolSave)
            {
                // Show OK save data
                using (new CenterWinDialog(this))
                {
                    MessageBox.Show(StringResources.MsgBoxOKSaveData,
                        StringResources.MsgBoxOKSaveDataTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }
    }

    private void Settings_Click(object? sender, EventArgs e)
    {
        FrmSettings frm = new(_settings);
        frm.ShowDialog();
        if (frm.DialogResult == DialogResult.OK)
        {
            _settings = frm.Settings;
            ComboSeries_SelectedIndexChanged(this, EventArgs.Empty);

            statusStripLabelExPower.Checked = _settings.PowerSpectra;
            statusStripLabelExCumulative.Checked = _settings.CumulativeDimension;
            statusStripLabelExEntropy.Checked = _settings.Entropy;
            statusStripLabelExCrossHair.Checked = _settings.CrossHair;

            UpdateUI_Language();
        }

    }

    private void About_Click(object? sender, EventArgs e)
    {
        FrmAbout frmAbout = new();
        frmAbout.ShowDialog();
    }

    private void Language_Click(object? sender, EventArgs e)
    {
        FrmLanguage frm = new(_settings);
        frm.ShowDialog();

        if (frm.DialogResult == DialogResult.OK)
            UpdateUI_Language();
    }

    private void LabelEx_CheckedChanged(object? sender, EventArgs e)
    {
        //if (sender is not null && sender is ToolStripStatusLabelEx LabelEx)
        //{
        //    var label = LabelEx;
        //    // Change the text color
        //    if (label.Checked)
        //        label.ForeColor = Color.Black;
        //    else
        //        label.ForeColor = Color.LightGray;
        //}
    }

    private void LabelEx_Click(object? sender, EventArgs e)
    {
        if (sender is not null && sender is ToolStripStatusLabelEx LabelEx)
        {
            var label = LabelEx;
            label.Checked = !label.Checked;

            // Change the text color
            if (label.Checked)
                label.ForeColor = Color.Black;
            else
                label.ForeColor = Color.LightGray;

            // Update the settings
            switch (label.Name)
            {
                case "LabelExPower":
                    _settings.PowerSpectra = label.Checked;
                    ComboWindow_SelectedIndexChanged(null, EventArgs.Empty);
                    break;
                case "LabelExCumulative":
                    _settings.CumulativeDimension = label.Checked;
                    if (_settings.CumulativeDimension)
                        UpdateStatsPlots(stripComboSeries.SelectedIndex);
                    if (!label.Checked && statsTask is not null && statsTask.Status == TaskStatus.Running)
                        FrmMain_KeyPress(sender, new KeyPressEventArgs((char)Keys.Escape));
                    break;
                case "LabelExEntropy":
                    _settings.Entropy = label.Checked;
                    if (_settings.Entropy)
                        UpdateStatsPlots(stripComboSeries.SelectedIndex);
                    if (!label.Checked && statsTask is not null && statsTask.Status == TaskStatus.Running)
                        FrmMain_KeyPress(sender, new KeyPressEventArgs((char)Keys.Escape));
                    break;
                case "LabelExCrossHair":
                    _settings.CrossHair = label.Checked;
                    if (plotOriginal is not null && plotOriginal.Plot.GetPlottables().Length > 0)
                    {
                        plotOriginal.ShowCrossHair = label.Checked;
                        plotOriginal.Refresh();
                    }
                    if (plotWindow is not null && plotWindow.Plot.GetPlottables().Length > 0)
                    {
                        plotWindow.ShowCrossHair = label.Checked;
                        plotWindow.Refresh();
                    }
                    if (plotApplied is not null && plotApplied.Plot.GetPlottables().Length > 0)
                    {
                        plotApplied.ShowCrossHair = label.Checked;
                        plotApplied.Refresh();
                    }
                    if (plotFractal is not null && plotFractal.Plot.GetPlottables().Length > 0)
                    {
                        plotFractal.ShowCrossHair = label.Checked;
                        plotFractal.Refresh();
                    }
                    if (plotFFT is not null && plotFFT.Plot.GetPlottables().Length > 0)
                    {
                        plotFFT.ShowCrossHair = label.Checked;
                        plotFFT.Refresh();
                    }
                    break;
            }
        }
    }

}
