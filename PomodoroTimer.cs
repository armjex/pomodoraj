using System;
using System.Drawing;
using System.Windows.Forms;

namespace PomodoroTimer
{
    public class MainForm : Form
    {
        private Label timerLabel;
        private Label statusLabel;
        private Button startBtn, pauseBtn, stopBtn, resetBtn;
        private Button workBtn, breakBtn;
        private FlowLayoutPanel workTimePanel, breakTimePanel;
        private System.Windows.Forms.Timer timer;

        private int workSeconds = 1500;  // 25 минут
        private int breakSeconds = 300;  // 5 минут
        private int timeLeft;
        private bool isRunning = false;
        private bool isWorkMode = true;

        // Значения для кнопок времени работы
        private int[] workTimes = { 1, 60, 180, 300, 600, 900, 1200, 1500, 1800, 2700, 3600 };
        private string[] workLabels = { "1с", "1м", "3м", "5м", "10м", "15м", "20м", "25м", "30м", "45м", "60м" };

        // Значения для кнопок времени перерыва
        private int[] breakTimes = { 1, 60, 180, 300, 600, 900, 1200, 1500, 1800, 2700, 3000, 5400 };
        private string[] breakLabels = { "1с", "1м", "3м", "5м", "10м", "15м", "20м", "25м", "30м", "45м", "50м", "90м" };

        public MainForm()
        {
            this.Text = "Помодоро Таймер";
            this.ClientSize = new Size(260, 400);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 47);

            timeLeft = workSeconds;
            InitializeComponents();
            UpdateDisplay();
        }

        private void InitializeComponents()
        {
            // Заголовок
            Label title = new Label
            {
                Text = "⏱ Помодоро",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 5),
                Size = new Size(240, 25)
            };
            this.Controls.Add(title);

            // Дисплей таймера
            Label = new Label
            {
                Text = "25:00",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 36),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 35),
                Size = new Size(240, 60)
            };
            this.Controls.Add(Label);

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
            this.Controls.Add(workBtn);

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
            this.Controls.Add(breakBtn);

            // Метка времени работы
            Label workLabel = new Label
            {
                Text = "Время работы:",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Location = new Point(10, 135),
                Size = new Size(100, 15)
            };
            this.Controls.Add(workLabel);

            // Панель кнопок времени работы
            workTimePanel = new FlowLayoutPanel
            {
                Location = new Point(10, 152),
                Size = new Size(240, 50),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent
            };
            CreateTimeButtons(workTimePanel, workTimes, workLabels, true);
            this.Controls.Add(workTimePanel);

            // Метка времени перерыва
            Label breakLabel = new Label
            {
                Text = "Время перерыва:",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Location = new Point(10, 208),
                Size = new Size(120, 15)
            };
            this.Controls.Add(breakLabel);

            // Панель кнопок времени перерыва
            breakTimePanel = new FlowLayoutPanel
            {
                Location = new Point(10, 225),
                Size = new Size(240, 55),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent
            };
            CreateTimeButtons(breakTimePanel, breakTimes, breakLabels, false);
            this.Controls.Add(breakTimePanel);

            // Кнопки управления
            startBtn = CreateControlButton("▶", 15, 290, Color.FromArgb(76, 175, 80));
            startBtn.Click += Start;
            this.Controls.Add(startBtn);

            pauseBtn = CreateControlButton("⏸", 70, 290, Color.FromArgb(255, 152, 0));
            pauseBtn.Click += Pause;
            this.Controls.Add(pauseBtn);

            stopBtn = CreateControlButton("⏹", 125, 290, Color.FromArgb(244, 67, 54));
            stopBtn.Click += Stop;
            this.Controls.Add(stopBtn);

            resetBtn = CreateControlButton("↺", 180, 290, Color.FromArgb(156, 39, 176));
            resetBtn.Click += ResetAll;
            this.Controls.Add(resetBtn);

            // Статус
            statusLabel = new Label
            {
                Text = "Готов к работе",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 8),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(10, 325),
                Size = new Size(240, 20)
            };
            this.Controls.Add(statusLabel);

            // Таймер
            timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += Timer_Tick;
        }

        private Button CreateControlButton(string text, int x, int y, Color color)
        {
            Button btn = new Button
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
                int index = i;
                Button btn = new Button
                {
                    Text = labels[i],
                    Tag = times[i],
                    Size = new Size(32, 22),
                    BackColor = (isWork && times[i] == 1500) || (!isWork && times[i] == 300)
                        ? Color.FromArgb(33, 150, 243)
                        : Color.FromArgb(60, 60, 70),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 7)
                };
                btn.FlatAppearance.BorderSize = 0;

                btn.Click += (s, e) =>
                {
                    foreach (Control c in panel.Controls)
                        c.BackColor = Color.FromArgb(60, 60, 70);
                    btn.BackColor = Color.FromArgb(33, 150, 243);

                    int seconds = (int)btn.Tag;
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

        private void Timer_Tick(object sender, EventArgs e)
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
                System.Media.SystemSounds.Beep.Play();
                System.Threading.Thread.Sleep(200);
            }
        }

        private void StartTimer(object sender, EventArgs e)
        {
            if (isRunning) return;
            if (timeLeft <= 0)
                timeLeft = isWorkMode ? workSeconds : breakSeconds;
            isRunning = true;
            statusLabel.Text = isWorkMode ? "⚡ Работа..." : "☕ Отдых...";
            timer.Start();
        }

        private void PauseTimer(object sender, EventArgs e)
        {
            timer.Stop();
            isRunning = false;
            statusLabel.Text = "⏸ Пауза";
        }

        private void StopTimer(object sender, EventArgs e)
        {
            timer.Stop();
            isRunning = false;
            statusLabel.Text = "⏹ Остановлено";
        }

        private void ResetAll(object sender, EventArgs e)
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

            // Сброс выделения кнопок времени
            foreach (Control c in workTimePanel.Controls)
                c.BackColor = Color.FromArgb(60, 60, 70);
            foreach (Control c in breakTimePanel.Controls)
                c.BackColor = Color.FromArgb(60, 60, 70);

            // Выделяем стандартные значения
            foreach (Control c in workTimePanel.Controls)
                if ((int)c.Tag == 1500) c.BackColor = Color.FromArgb(33, 150, 243);
            foreach (Control c in breakTimePanel.Controls)
                if ((int)c.Tag == 300) c.BackColor = Color.FromArgb(33, 150, 243);
        }

        private void SetWorkMode(bool autoStart)
        {
            isWorkMode = true;
            timeLeft = workSeconds;
            workBtn.BackColor = Color.FromArgb(76, 175, 80);
            breakBtn.BackColor = Color.FromArgb(60, 60, 70);
            UpdateDisplay();
            statusLabel.Text = "📋 Режим работы";
            if (autoStart) StartTimer(null, null);
        }

        private void SetBreakMode(bool autoStart)
        {
            isWorkMode = false;
            timeLeft = breakSeconds;
            breakBtn.BackColor = Color.FromArgb(76, 175, 80);
            workBtn.BackColor = Color.FromArgb(60, 60, 70);
            UpdateDisplay();
            statusLabel.Text = "☕ Режим перерыва";
            if (autoStart) StartTimer(null, null);
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
