using System;
using System.Drawing;
using System.Media;
using System.Threading;
using System.Windows.Forms;

namespace PomodoroTimer
{
    public class MainForm : Form
    {
        private Label timerLabel = null!;
        private Label statusLabel = null!;
        private Button startBtn = null!;
        private Button pauseBtn = null!;
        private Button stopBtn = null!;
        private Button resetBtn = null!;
        private Button workBtn = null!;
        private Button breakBtn = null!;
        private FlowLayoutPanel workTimePanel = null!;
        private FlowLayoutPanel breakTimePanel = null!;
        private System.Windows.Forms.Timer timer = null!;

        private int workSeconds = 1500;
        private int breakSeconds = 300;
        private int timeLeft;
        private bool isRunning = false;
        private bool isWorkMode = true;

        // 12 кнопок в каждом массиве = 6 в ряд × 2 ряда
        private readonly int[] workTimes = { 1, 60, 180, 300, 600, 900, 1200, 1500, 1800, 2700, 3600, 5400 };
        private readonly string[] workLabels = { "1с", "1м", "3м", "5м", "10м", "15м", "20м", "25м", "30м", "45м", "60м", "90м" };

        private readonly int[] breakTimes = { 1, 60, 180, 300, 600, 900, 1200, 1500, 1800, 2700, 3600, 5400 };
        private readonly string[] breakLabels = { "1с", "1м", "3м", "5м", "10м", "15м", "20м", "25м", "30м", "45м", "60м", "90м" };

        // Размеры кнопок времени
        private const int TimeBtnWidth = 34;
        private const int TimeBtnHeight = 22;
        private const int TimeBtnMargin = 2;

        public MainForm()
        {
            Text = "Помодоро Таймер";
            ClientSize = new Size(260, 420);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 47);

            timeLeft = workSeconds;
            BuildUI();
            UpdateDisplay();
        }

        private void BuildUI()
        {
            // Заголовок
            var titleLabel = new Label
            {
                Text = "⏱ Помодоро",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 5),
                Size = new Size(240, 25)
            };
            Controls.Add(titleLabel);

            // Дисплей
            timerLabel = new Label
            {
                Text = "25:00",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 36),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 35),
                Size = new Size(240, 60)
            };
            Controls.Add(timerLabel);

            // Кнопки режимов
            workBtn = new Button
            {
                Text = "📋 Работа",
                Location = new Point(25, 100),
                Size = new Size(100, 28),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };
            workBtn.FlatAppearance.BorderSize = 0;
            workBtn.Click += (s, e) => SetWorkMode(false);
            Controls.Add(workBtn);

            breakBtn = new Button
            {
                Text = "☕ Перерыв",
                Location = new Point(135, 100),
                Size = new Size(100, 28),
                BackColor = Color.FromArgb(60, 60, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };
            breakBtn.FlatAppearance.BorderSize = 0;
            breakBtn.Click += (s, e) => SetBreakMode(false);
            Controls.Add(breakBtn);

            // Метка работы
            var workLabel = new Label
            {
                Text = "Время работы:",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Location = new Point(10, 135),
                Size = new Size(120, 15)
            };
            Controls.Add(workLabel);

            // Панель работы — 6 кнопок в ряд × 2 ряда
            workTimePanel = new FlowLayoutPanel
            {
                Location = new Point(10, 152),
                Size = new Size(240, 55),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            CreateTimeButtons(workTimePanel, workTimes, workLabels, true);
            Controls.Add(workTimePanel);

            // Метка перерыва
            var breakLabel = new Label
            {
                Text = "Время перерыва:",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Location = new Point(10, 213),
                Size = new Size(120, 15)
            };
            Controls.Add(breakLabel);

            // Панель перерыва — 6 кнопок в ряд × 2 ряда
            breakTimePanel = new FlowLayoutPanel
            {
                Location = new Point(10, 230),
                Size = new Size(240, 55),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            CreateTimeButtons(breakTimePanel, breakTimes, breakLabels, false);
            Controls.Add(breakTimePanel);

            // Управление
            startBtn = CreateControlButton("▶", 15, 300, Color.FromArgb(76, 175, 80));
            startBtn.Click += StartTimer;
            Controls.Add(startBtn);

            pauseBtn = CreateControlButton("⏸", 70, 300, Color.FromArgb(255, 152, 0));
            pauseBtn.Click += PauseTimer;
            Controls.Add(pauseBtn);

            stopBtn = CreateControlButton("⏹", 125, 300, Color.FromArgb(244, 67, 54));
            stopBtn.Click += StopTimer;
            Controls.Add(stopBtn);

            resetBtn = CreateControlButton("↺", 180, 300, Color.FromArgb(156, 39, 176));
            resetBtn.Click += ResetAll;
            Controls.Add(resetBtn);

            // Статус
            statusLabel = new Label
            {
                Text = "Готов к работе",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 8),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 340),
                Size = new Size(240, 20)
            };
            Controls.Add(statusLabel);

            // Таймер
            timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += TimerTick;
        }

        private Button CreateControlButton(string text, int x, int y, Color color)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(45, 30),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void CreateTimeButtons(FlowLayoutPanel panel, int[] times, string[] labels, bool isWork)
        {
            for (int i = 0; i < times.Length; i++)
            {
                int seconds = times[i];
                bool isDefault = (isWork && seconds == 1500) || (!isWork && seconds == 300);

                var btn = new Button
                {
                    Text = labels[i],
                    Tag = seconds,
                    Size = new Size(TimeBtnWidth, TimeBtnHeight),
                    Margin = new Padding(TimeBtnMargin),
                    BackColor = isDefault ? Color.FromArgb(33, 150, 243) : Color.FromArgb(60, 60, 70),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 7, FontStyle.Regular),
                    Padding = new Padding(0)
                };
                btn.FlatAppearance.BorderSize = 0;

                btn.Click += (s, e) =>
                {
                    foreach (Control c in panel.Controls)
                        c.BackColor = Color.FromArgb(60, 60, 70);
                    btn.BackColor = Color.FromArgb(33, 150, 243);

                    if (isWork)
                    {
                        workSeconds = seconds;
                        if (isWorkMode && !isRunning)
                        {
                            timeLeft = workSeconds;
                            UpdateDisplay();
                        }
                    }
                    else
                    {
                        breakSeconds = seconds;
                        if (!isWorkMode && !isRunning)
                        {
                            timeLeft = breakSeconds;
                            UpdateDisplay();
                        }
                    }
                };

                panel.Controls.Add(btn);
            }
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            timeLeft--;
            UpdateDisplay();

            if (timeLeft <= 0)
            {
                timer.Stop();
                isRunning = false;
                PlaySound();

                if (isWorkMode) SetBreakMode(true);
                else SetWorkMode(true);
            }
        }

        private void UpdateDisplay()
        {
            int minutes = timeLeft / 60;
            int seconds = timeLeft % 60;
            timerLabel.Text = $"{minutes:D2}:{seconds:D2}";
        }

        private void PlaySound()
        {
            for (int i = 0; i < 3; i++)
            {
                SystemSounds.Beep.Play();
                Thread.Sleep(200);
            }
        }

        private void StartTimer(object? sender, EventArgs e)
        {
            if (isRunning) return;
            if (timeLeft <= 0)
                timeLeft = isWorkMode ? workSeconds : breakSeconds;
            isRunning = true;
            statusLabel.Text = isWorkMode ? "⚡ Работа..." : "☕ Отдых...";
            timer.Start();
        }

        private void PauseTimer(object? sender, EventArgs e)
        {
            timer.Stop();
            isRunning = false;
            statusLabel.Text = "⏸ Пауза";
        }

        private void StopTimer(object? sender, EventArgs e)
        {
            timer.Stop();
            isRunning = false;
            statusLabel.Text = "⏹ Остановлено";
        }

        private void ResetAll(object? sender, EventArgs e)
        {
            timer.Stop();
            isRunning = false;
            workSeconds = 1500;
            breakSeconds = 300;
            isWorkMode = true;
            timeLeft = workSeconds;
            UpdateDisplay();
            statusLabel.Text = "🔄 Готов к работе";

            workBtn.BackColor = Color.FromArgb(76, 175, 80);
            breakBtn.BackColor = Color.FromArgb(60, 60, 70);

            foreach (Control c in workTimePanel.Controls)
                c.BackColor = (int)(c.Tag ?? 0) == 1500
                    ? Color.FromArgb(33, 150, 243)
                    : Color.FromArgb(60, 60, 70);

            foreach (Control c in breakTimePanel.Controls)
                c.BackColor = (int)(c.Tag ?? 0) == 300
                    ? Color.FromArgb(33, 150, 243)
                    : Color.FromArgb(60, 60, 70);
        }

        private void SetWorkMode(bool autoStart)
        {
            isWorkMode = true;
            timeLeft = workSeconds;
            workBtn.BackColor = Color.FromArgb(76, 175, 80);
            breakBtn.BackColor = Color.FromArgb(60, 60, 70);
            UpdateDisplay();
            statusLabel.Text = "📋 Режим работы";
            if (autoStart) StartTimer(null, EventArgs.Empty);
        }

        private void SetBreakMode(bool autoStart)
        {
            isWorkMode = false;
            timeLeft = breakSeconds;
            breakBtn.BackColor = Color.FromArgb(76, 175, 80);
            workBtn.BackColor = Color.FromArgb(60, 60, 70);
            UpdateDisplay();
            statusLabel.Text = "☕ Режим перерыва";
            if (autoStart) StartTimer(null, EventArgs.Empty);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
