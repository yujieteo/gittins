using System;
using System.IO;
using System.Windows.Forms;

public class GittinsForm : Form
{
    private Label gammaLabel;
    private TextBox gammaTextBox;
    private Label TLabel;
    private TextBox TTextBox;
    private Button calculateButton;
    private TextBox outputTextBox;

    public GittinsForm()
    {
        // Initialize the form elements
        gammaLabel = new Label { Text = "Gamma (discount rate):", Dock = DockStyle.Top };
        gammaTextBox = new TextBox { Text = "0.9", Dock = DockStyle.Top };
        TLabel = new Label { Text = "Time horizon (T):", Dock = DockStyle.Top };
        TTextBox = new TextBox { Text = "6", Dock = DockStyle.Top };
        calculateButton = new Button { Text = "Calculate Gittins Indices", Dock = DockStyle.Top };
        outputTextBox = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };

        calculateButton.Click += CalculateButton_Click;

        Controls.Add(outputTextBox);
        Controls.Add(calculateButton);
        Controls.Add(TTextBox);
        Controls.Add(TLabel);
        Controls.Add(gammaTextBox);
        Controls.Add(gammaLabel);

        Text = "Gittins Indices Calculator";
        Width = 800;
        Height = 600;
    }

    private void CalculateButton_Click(object sender, EventArgs e)
    {
        double gamma;
        int T;

        if (!double.TryParse(gammaTextBox.Text, out gamma))
        {
            MessageBox.Show("Invalid value for gamma. Please enter a valid number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!int.TryParse(TTextBox.Text, out T) || T <= 1)
        {
            MessageBox.Show("Invalid value for T. Please enter an integer greater than 1.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        double step = 0.0001;
        double[,] R = new double[T - 1, T - 1];
        double[,] Gittins = new double[T - 1, T - 1];

        for (int alpha = 1; alpha < T; alpha++)
        {
            R[alpha - 1, T - alpha - 1] = (double)alpha / T;
        }

        for (double p = step / 2; p < 1; p += step)
        {
            double safe = p / (1 - gamma);

            for (int t = T - 1; t > 1; t--)
            {
                for (int alpha = 1; alpha < t; alpha++)
                {
                    double risky = (double)alpha / t *
                                   (1 + gamma * R[alpha, t - alpha - 1]) +
                                   (t - alpha) / (double)t * (gamma * R[alpha - 1, t - alpha]);

                    if (Gittins[alpha - 1, t - alpha - 1] == 0 && safe >= risky)
                    {
                        Gittins[alpha - 1, t - alpha - 1] = p - step / 2;
                    }

                    R[alpha - 1, t - alpha - 1] = Math.Max(safe, risky);
                }
            }
        }

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string fileName = string.Format("GittinsIndices_{0:yyyyMMddHHmmss}.txt", DateTime.Now);
        string filePath = Path.Combine(desktopPath, fileName);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int i = 0; i < Gittins.GetLength(0); i++)
            {
                for (int j = 0; j < Gittins.GetLength(1); j++)
                {
                    double roundedValue = RoundToSignificantDigits(Gittins[i, j], 3);
                    writer.Write(roundedValue + " ");
                    outputTextBox.AppendText(roundedValue + " ");
                }
                writer.WriteLine();
                outputTextBox.AppendText(Environment.NewLine);
            }
        }

        MessageBox.Show("Calculation complete. Results saved to " + filePath, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static double RoundToSignificantDigits(double value, int digits)
    {
        if (value == 0)
            return 0;

        double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(value))) + 1);
        return scale * Math.Round(value / scale, digits);
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new GittinsForm());
    }
}
